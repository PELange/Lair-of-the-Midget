using LOTM.Shared.Engine.Network.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager
    {
        private const int MAX_ACK_TIMEOUT_MS = 1_000;
        private const int ACK_RETRANSMIT_COOLDOWN_MS = 250;

        //Start at 100 to avoid any accidients with default values e.g. -1, 0, 1 etc ...
        private int NEXT_PACKET_ID = 100;

        private UdpSocket Socket { get; }

        private AutoResetEvent SendEvent { get; }
        private Thread SendThread { get; }
        private CancellationTokenSource ShouldShutdown { get; set; }

        private ConcurrentQueue<NetworkPacket> ReceiveQueue { get; }
        private ConcurrentQueue<(NetworkPacket, IPEndPoint)> SendQueue { get; }
        private ConcurrentDictionary<int, AwaitingAckEntry> AwaitingAck { get; }

        public Func<(NetworkPacket, IPEndPoint), bool> PacketSendFailureCallback { get; set; }

        //private Random Random { get; } = new Random();

        private class AwaitingAckEntry
        {
            public NetworkPacket Packet { get; set; }
            public IPEndPoint EndPoint { get; set; }
            public DateTime FirstAttemt { get; set; }
            public DateTime LastAttemt { get; set; }
        }

        private NetworkPacketSerializationProvider NetworkPacketSerializationProvider { get; }

        //private System.Timers.Timer DiagnosticsTimer { get; }
        //public int PacketsReceived { get; set; }
        //public int PacketsSent { get; set; }

        public NetworkManager(
            UdpSocket socket,
            NetworkPacketSerializationProvider networkPacketSerializationProvider)
        {
            Socket = socket;
            SendEvent = new AutoResetEvent(false);
            ShouldShutdown = new CancellationTokenSource();
            ReceiveQueue = new ConcurrentQueue<NetworkPacket>();
            SendQueue = new ConcurrentQueue<(NetworkPacket, IPEndPoint)>();
            AwaitingAck = new ConcurrentDictionary<int, AwaitingAckEntry>();
            NetworkPacketSerializationProvider = networkPacketSerializationProvider;

            //Setup receive listener
            Socket.OnMessageReceived = (socket, packet) =>
            {
                //PacketsReceived++;

                ////Simulate packet loss
                //if (Random.Next(0, 3) == 1)
                //{
                //    return;
                //}


                //Split UDP packet into messages
                using MemoryStream memoryStream = new MemoryStream(packet.data);
                using BinaryReader reader = new BinaryReader(memoryStream);

                while (memoryStream.Position + sizeof(int) < packet.data.Length)
                {
                    var messageLength = reader.ReadInt32();

                    var messageBytes = reader.ReadBytes(messageLength);

                    //Unpack the packet into NetworkPaket C# instance
                    var networkPacket = NetworkPacketSerializationProvider.DeserializePacket(messageBytes, packet.senderEndpoint);

                    //Enqueue the result
                    if (networkPacket != null)
                    {
                        if (networkPacket is PacketAck packetAck)
                        {
                            //System.Console.WriteLine($"Recieved ack for packet id {packetAck.AckPacketId}");
                            AwaitingAck.TryRemove(packetAck.AckPacketId, out var _);
                        }
                        else
                        {
                            ReceiveQueue.Enqueue(networkPacket);

                            //Send back ack packet if requested by the sender
                            if (networkPacket.RequiresAck)
                            {
                                //System.Console.WriteLine($"Sender wants ack for packet id {networkPacket.Id}. Sending ...");
                                SendPacket(new PacketAck { AckPacketId = networkPacket.Id }, packet.senderEndpoint);
                            }
                        }
                    }
                }
            };

            //Start sender thread
            SendThread = new Thread(() => SendWorker());
            SendThread.Start();

            //DiagnosticsTimer = new System.Timers.Timer(1000);
            //DiagnosticsTimer.Start();
            //DiagnosticsTimer.Elapsed += (sender, args) =>
            //{
            //    System.Console.WriteLine($"[NETWORK] Sent {PacketsSent}/s - Received {PacketsReceived}/s");
            //    PacketsSent = 0;
            //    PacketsReceived = 0;
            //};
        }

        public void Shutdown()
        {
            ShouldShutdown.Cancel();
            SendEvent.Set();
            SendThread?.Join();
            Socket.Close();
        }

        private async void SendWorker()
        {
            while (!ShouldShutdown.IsCancellationRequested)
            {
                //Only wait if there are not packets to ack, and we have no tasks to send new packets
                if (AwaitingAck.Count == 0)
                {
                    SendEvent.WaitOne();
                }

                if (ShouldShutdown.IsCancellationRequested) break;

                //Handle packet ack pending states
                var retransmitIds = new List<int>();
                var discardIds = new List<int>();

                foreach (var awaitingAck in AwaitingAck.Values)
                {
                    //Do not attampt to retransmit packets that were not ack within the maximum timeframe
                    if ((DateTime.Now - awaitingAck.FirstAttemt).TotalMilliseconds > MAX_ACK_TIMEOUT_MS)
                    {
                        discardIds.Add(awaitingAck.Packet.Id);
                    }
                    else if ((DateTime.Now - awaitingAck.LastAttemt).TotalMilliseconds > ACK_RETRANSMIT_COOLDOWN_MS)
                    {
                        retransmitIds.Add(awaitingAck.Packet.Id);
                    }
                }

                //Removed discarded packets from ack pending collection
                foreach (var discard in discardIds)
                {
                    //If there is a callback to control if the packet is dropped or continue being waited for, call it. Remove when the callback returns false
                    if (AwaitingAck.TryGetValue(discard, out var discarded) && (PacketSendFailureCallback == null || !PacketSendFailureCallback((discarded.Packet, discarded.EndPoint))))
                    {
                        //System.Console.WriteLine($"{System.DateTime.Now} Ack for packet {discard} did not arrive in time. Discarded ...");
                        AwaitingAck.TryRemove(discard, out var _);
                    }
                }

                //Add packets to retransmit to send queue
                foreach (var retransmit in retransmitIds)
                {
                    if (AwaitingAck.TryGetValue(retransmit, out var data))
                    {
                        //System.Console.WriteLine($"{System.DateTime.Now} Ack pending for packet id {data.Packet.Id}. Retransmitting ... last transmision was at {data.LastAttemt}");

                        data.LastAttemt = DateTime.Now;
                        SendQueue.Enqueue((data.Packet, data.EndPoint));
                    }
                }

                var receivers = new List<IPEndPoint>();
                var seralizedMessages = new List<(byte[], IPEndPoint)>();

                while (SendQueue.TryDequeue(out var result))
                {
                    if (NetworkPacketSerializationProvider.SerializePacket(result.Item1, out var data))
                    {
                        //await Socket.SendAsync(data, result.Item2);

                        //Save seralized message
                        seralizedMessages.Add((data, result.Item2));

                        //Collect all receivers as distinct list
                        if (!receivers.Any(x => x.Equals(result.Item2)))
                        {
                            receivers.Add(result.Item2);
                        }

                        if (result.Item1.RequiresAck)
                        {
                            //Only adds if we were not already waiting for it
                            AwaitingAck.TryAdd(result.Item1.Id, new AwaitingAckEntry
                            {
                                Packet = result.Item1,
                                EndPoint = result.Item2,
                                FirstAttemt = DateTime.Now, //First timestamp = first time we tried sending
                                LastAttemt = DateTime.Now //Second timestamp = last time we tried sending
                            });
                        }

                        //PacketsSent++;
                    }
                }

                ////Go through all receivers to bundle their messages together
                //foreach (var receiver in receivers)
                //{
                //    //Remove messages that we could gather for this sender from generel outbound list
                //    var messages = seralizedMessages.Where(x => x.Item2.Equals(receiver)).ToList();
                //    seralizedMessages.RemoveAll(x => messages.Contains(x));

                //    const int maxPacketSize = 500; //500 bytes is considered "safe enough" for IP layer. Though delivery is not guaranteed it should not be split up or dropped because of size

                //    //Start new buffer
                //    while (messages.Count > 0)
                //    {
                //        var handled = new List<(byte[], IPEndPoint)>();

                //        var memoryStream = new MemoryStream();
                //        var writer = new BinaryWriter(memoryStream);

                //        for (int nMessage = 0; nMessage < messages.Count; nMessage++)
                //        {
                //            var message = messages[nMessage];

                //            //Always write the first one or if there is still room for this message
                //            if (nMessage == 0 || memoryStream.Length + sizeof(int) + message.Item1.Length <= maxPacketSize)
                //            {
                //                writer.Write(message.Item1.Length);
                //                writer.Write(message.Item1);

                //                handled.Add(message);
                //            }
                //            else //No more space, send and repeat process
                //            {
                //                await Socket.SendAsync(memoryStream.ToArray(), receiver);

                //                break;
                //            }
                //        }

                //        //Remove handled messages
                //        messages.RemoveAll(x => handled.Contains(x));
                //    }
                //}

                //Left over messages
                foreach (var message in seralizedMessages)
                {
                    var memoryStream = new MemoryStream();
                    var writer = new BinaryWriter(memoryStream);

                    writer.Write(message.Item1.Length);
                    writer.Write(message.Item1);

                    await Socket.SendAsync(memoryStream.ToArray(), message.Item2);
                }
            }
        }

        public bool TryGetPacket(out NetworkPacket packet)
        {
            return ReceiveQueue.TryDequeue(out packet);
        }

        protected void SendPacket(NetworkPacket packet, IPEndPoint receiver)
        {
            packet.Id = NEXT_PACKET_ID++;
            SendQueue.Enqueue((packet, receiver));
            SendEvent.Set();
        }
    }
}

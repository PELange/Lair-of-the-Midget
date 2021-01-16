using LOTM.Shared.Engine.Network.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager
    {
        private const int MAX_ACK_TIMEOUT_MS = 1_000;
        private const int ACK_RETRANSMIT_COOLDOWN_MS = 250;

        private int NEXT_PACKET_ID = 1;

        private UdpSocket Socket { get; }

        private AutoResetEvent SendEvent { get; }
        private Thread SendThread { get; }
        private CancellationTokenSource ShouldShutdown { get; set; }

        private ConcurrentQueue<NetworkPacket> ReceiveQueue { get; }
        private ConcurrentQueue<(NetworkPacket, IPEndPoint)> SendQueue { get; }
        private ConcurrentDictionary<int, AwaitingAckEntry> AwaitingAck { get; }

        private class AwaitingAckEntry
        {
            public NetworkPacket Packet { get; set; }
            public IPEndPoint EndPoint { get; set; }
            public DateTime FirstAttemt { get; set; }
            public DateTime LastAttemt { get; set; }
        }

        public event EventHandler PacketSendFailure;

        public class PacketSendFailureEventArgs : EventArgs
        {
            public NetworkPacket Packet { get; }
            public IPEndPoint Recipient { get; }

            public PacketSendFailureEventArgs(NetworkPacket packet, IPEndPoint recipient)
            {
                Packet = packet;
                Recipient = recipient;
            }
        }

        private NetworkPacketSerializationProvider NetworkPacketSerializationProvider { get; }

        //private System.Timers.Timer DiagnosticsTimer { get; }
        //public int PacketsReceived { get; set; }
        //public int PacketsSent { get; set; }

        public NetworkManager(UdpSocket socket, NetworkPacketSerializationProvider networkPacketSerializationProvider)
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

                //Unpack the packet into NetworkPaket C# instance
                var networkPacket = NetworkPacketSerializationProvider.DeserializePacket(packet.data, packet.senderEndpoint);

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
                    if (AwaitingAck.TryRemove(discard, out var discarded))
                    {
                        //System.Console.WriteLine($"{System.DateTime.Now} Ack for packet {discard} did not arrive in time. Discarded ...");
                        PacketSendFailure?.Invoke(this, new PacketSendFailureEventArgs(discarded.Packet, discarded.EndPoint));
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

                while (SendQueue.TryDequeue(out var result))
                {
                    if (NetworkPacketSerializationProvider.SerializePacket(result.Item1, out var data))
                    {
                        await Socket.SendAsync(data, result.Item2);

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

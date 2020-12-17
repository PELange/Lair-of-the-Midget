using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager
    {
        private Thread ListenThread { get; }
        private Thread SendThread { get; }
        private bool ShouldShutdown { get; set; }

        private ConcurrentQueue<NetworkPacket> ReceiveQueue { get; }
        private ConcurrentQueue<(NetworkPacket, IPEndPoint)> SendQueue { get; }
        private INetworkPacketSerializationProvider NetworkPacketSerializationProvider { get; }

        public NetworkManager(INetworkPacketSerializationProvider networkPacketSerializationProvider, IPEndPoint bindEndpoint = null)
        {
            NetworkPacketSerializationProvider = networkPacketSerializationProvider;

            ReceiveQueue = new ConcurrentQueue<NetworkPacket>();
            SendQueue = new ConcurrentQueue<(NetworkPacket, IPEndPoint)>();

            if (bindEndpoint == null) //Bind to the next free udp port
            {
                bindEndpoint = new IPEndPoint(IPAddress.Any, 1234); //todo fetch open udp port

                //var client = new UdpClient();
                //client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                //bindEndpoint = (IPEndPoint)client.Client.LocalEndPoint;
                //client.Client.Close();
            }

            ListenThread = new Thread(() => ListenWorker(bindEndpoint));
            ListenThread.Start();

            SendThread = new Thread(() => SendWorker(bindEndpoint));
            SendThread.Start();
        }

        public void Shutdown()
        {
            ShouldShutdown = true;
            ListenThread.Abort();
            SendThread.Abort();
        }

        private void ListenWorker(IPEndPoint bindEndpoint)
        {
            var listener = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(bindEndpoint);

            Console.WriteLine($"Listening on: {bindEndpoint}");

            while (!ShouldShutdown)
            {
                //Create dummy endpoint that the sender data is written into
                var remoteEndpoint = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);

                //Receive the data
                var data = listener.Receive(ref remoteEndpoint);

                //Unpack the packet into NetworkPaket C# instance
                var packet = NetworkPacketSerializationProvider.DeserializePacket(data, remoteEndpoint);

                //Enqueue the result
                if (packet != null) ReceiveQueue.Enqueue(packet);
            }
        }

        private void SendWorker(IPEndPoint bindEndpoint)
        {
            var sender = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.Client.Bind(bindEndpoint);

            Console.WriteLine($"Sending on: {bindEndpoint}");

            while (!ShouldShutdown)
            {
                if (SendQueue.TryDequeue(out var result))
                {
                    var data = NetworkPacketSerializationProvider.SerializePacket(result.Item1);

                    sender.Send(data, data.Length, result.Item2);
                }
            }
        }

        public bool TryGetPacket(out NetworkPacket packet)
        {
            return ReceiveQueue.TryDequeue(out packet);
        }

        protected void SendPacket(NetworkPacket packet, IPEndPoint receiver)
        {
            SendQueue.Enqueue((packet, receiver));
        }
    }
}

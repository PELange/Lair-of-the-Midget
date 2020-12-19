using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager
    {
        private UdpSocket Socket { get; }

        private AutoResetEvent SendEvent { get; }
        private Thread SendThread { get; }
        private CancellationTokenSource ShouldShutdown { get; set; }

        private ConcurrentQueue<NetworkPacket> ReceiveQueue { get; }
        private ConcurrentQueue<(NetworkPacket, IPEndPoint)> SendQueue { get; }

        private INetworkPacketSerializationProvider NetworkPacketSerializationProvider { get; }

        public NetworkManager(UdpSocket socket, INetworkPacketSerializationProvider networkPacketSerializationProvider)
        {
            Socket = socket;

            SendEvent = new AutoResetEvent(false);

            SendThread = new Thread(() => SendWorker());
            SendThread.Start();

            ShouldShutdown = new CancellationTokenSource();

            ReceiveQueue = new ConcurrentQueue<NetworkPacket>();
            SendQueue = new ConcurrentQueue<(NetworkPacket, IPEndPoint)>();

            NetworkPacketSerializationProvider = networkPacketSerializationProvider;

            //Setup receive listener
            Socket.OnMessageReceived = (socket, packet) =>
            {
                //Unpack the packet into NetworkPaket C# instance
                var networkPacket = NetworkPacketSerializationProvider.DeserializePacket(packet.data, packet.senderEndpoint);

                //Enqueue the result
                if (networkPacket != null) ReceiveQueue.Enqueue(networkPacket);
            };
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
                SendEvent.WaitOne();

                if (ShouldShutdown.IsCancellationRequested) break;

                if (SendQueue.TryDequeue(out var result))
                {
                    var data = NetworkPacketSerializationProvider.SerializePacket(result.Item1);

                    await Socket.SendAsync(data, result.Item2);
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
            SendEvent.Set();
        }
    }
}

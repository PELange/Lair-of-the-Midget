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

        private System.Timers.Timer DiagnosticsTimer { get; }
        public int PacketsReceived { get; set; }
        public int PacketsSent { get; set; }


        public NetworkManager(UdpSocket socket, INetworkPacketSerializationProvider networkPacketSerializationProvider)
        {
            Socket = socket;
            SendEvent = new AutoResetEvent(false);
            ShouldShutdown = new CancellationTokenSource();
            ReceiveQueue = new ConcurrentQueue<NetworkPacket>();
            SendQueue = new ConcurrentQueue<(NetworkPacket, IPEndPoint)>();
            NetworkPacketSerializationProvider = networkPacketSerializationProvider;

            //Setup receive listener
            Socket.OnMessageReceived = (socket, packet) =>
            {
                //PacketsReceived++;

                //Unpack the packet into NetworkPaket C# instance
                var networkPacket = NetworkPacketSerializationProvider.DeserializePacket(packet.data, packet.senderEndpoint);

                //Enqueue the result
                if (networkPacket != null) ReceiveQueue.Enqueue(networkPacket);
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
                SendEvent.WaitOne();

                if (ShouldShutdown.IsCancellationRequested) break;

                while (SendQueue.TryDequeue(out var result))
                {
                    var data = NetworkPacketSerializationProvider.SerializePacket(result.Item1);

                    await Socket.SendAsync(data, result.Item2);

                    //PacketsSent++;
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

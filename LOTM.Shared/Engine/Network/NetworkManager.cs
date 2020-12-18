using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager : IDisposable
    {
        private Thread ListenThread { get; }
        private Thread SendThread { get; }
        private CancellationTokenSource ShouldShutdown { get; set; }

        private ConcurrentQueue<NetworkPacket> ReceiveQueue { get; }
        private ConcurrentQueue<(NetworkPacket, IPEndPoint)> SendQueue { get; }
        private INetworkPacketSerializationProvider NetworkPacketSerializationProvider { get; }

        private AutoResetEvent SendEvent { get; }

        public NetworkManager(INetworkPacketSerializationProvider networkPacketSerializationProvider, IPEndPoint bindEndpoint = null)
        {
            ShouldShutdown = new CancellationTokenSource();
            NetworkPacketSerializationProvider = networkPacketSerializationProvider;

            SendEvent = new AutoResetEvent(false);

            ReceiveQueue = new ConcurrentQueue<NetworkPacket>();
            SendQueue = new ConcurrentQueue<(NetworkPacket, IPEndPoint)>();

            if (bindEndpoint == null) //Bind to the next free udp port
            {
                var client = new UdpClient();
                client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                bindEndpoint = (IPEndPoint)client.Client.LocalEndPoint;
                client.Client.Close();
                client.Close();
            }

            ListenThread = new Thread(() => ListenWorker(bindEndpoint));
            ListenThread.Start();

            SendThread = new Thread(() => SendWorker(bindEndpoint));
            SendThread.Start();
        }

        public void Shutdown()
        {
            ShouldShutdown.Cancel();
            SendEvent.Set();
            ListenThread?.Join();
            SendThread?.Join();
        }

        private async void ListenWorker(IPEndPoint bindEndpoint)
        {
            var listener = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(bindEndpoint);

            Console.WriteLine($"Listening on: {bindEndpoint}");

            while (!ShouldShutdown.IsCancellationRequested)
            {
                //Receive the data
                var receiveTask = listener.ReceiveAsync();

                var tcs = new TaskCompletionSource<bool>();

                using (ShouldShutdown.Token.Register(x => tcs.TrySetResult(true), null))
                {
                    var finishedTask = await Task.WhenAny(receiveTask, tcs.Task);

                    if (finishedTask == tcs.Task)
                    {
                        break;
                    }
                }

                var data = await receiveTask;

                //Unpack the packet into NetworkPaket C# instance
                var packet = NetworkPacketSerializationProvider.DeserializePacket(data.Buffer, data.RemoteEndPoint);

                //Enqueue the result
                if (packet != null) ReceiveQueue.Enqueue(packet);
            }

            Console.WriteLine($"Stop listening ...");

            listener.Client.Shutdown(SocketShutdown.Both);
            listener.Client.Close();
            listener.Close();
        }

        private async void SendWorker(IPEndPoint bindEndpoint)
        {
            var sender = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.Client.Bind(bindEndpoint);

            Console.WriteLine($"Sending on: {bindEndpoint}");

            while (!ShouldShutdown.IsCancellationRequested)
            {
                SendEvent.WaitOne();

                if (ShouldShutdown.IsCancellationRequested) break;

                if (SendQueue.TryDequeue(out var result))
                {
                    var data = NetworkPacketSerializationProvider.SerializePacket(result.Item1);

                    await sender.SendAsync(data, data.Length, result.Item2);
                }
            }

            Console.WriteLine($"Stop sending ...");

            sender.Client.Shutdown(SocketShutdown.Both);
            sender.Client.Close();
            sender.Close();
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

        public void Dispose()
        {
            Shutdown();
        }
    }
}

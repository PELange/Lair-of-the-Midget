using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LOTM.Shared.Engine.Network
{
    public class UdpSocket
    {
        protected UdpClient UdpClient { get; }
        protected CancellationTokenSource CancellationTokenSource { get; }

        public EventHandler<(byte[] data, IPEndPoint senderEndpoint)> OnMessageReceived;

        private UdpSocket()
        {
            CancellationTokenSource = new CancellationTokenSource();
            UdpClient = new UdpClient();
        }

        public static UdpSocket CreateServer(IPEndPoint endpoint)
        {
            var udpSocket = new UdpSocket();

            udpSocket.UdpClient.Client.Bind(endpoint);

            Task.Run(async () => await udpSocket.ReceiveAsync());

            return udpSocket;
        }

        public static UdpSocket CreateClient(IPEndPoint endpoint)
        {
            var udpSocket = new UdpSocket();

            //Supress ICMP messages to avoid exception when host is not reachable (local network only)
            udpSocket.UdpClient.Client.IOControl(
                (IOControlCode)(-1744830452),
                new byte[] { 0, 0, 0, 0 },
                null
            );

            udpSocket.UdpClient.Client.Connect(endpoint);

            Task.Run(async () => await udpSocket.ReceiveAsync());

            return udpSocket;
        }

        public async Task SendAsync(byte[] bytes, IPEndPoint endpoint)
        {
            await UdpClient.SendAsync(bytes, bytes.Length, endpoint);
        }

        public void Close()
        {
            CancellationTokenSource.Cancel();
        }

        private async Task ReceiveAsync()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var resultTask = Task<UdpReceiveResult>.Factory.FromAsync((callback, state) => UdpClient.BeginReceive(callback, state), (ar) =>
                {
                    try
                    {
                        IPEndPoint remoteEP = null;
                        var buffer = UdpClient.EndReceive(ar, ref remoteEP);
                        return new UdpReceiveResult(buffer, remoteEP);
                    }
                    catch (ObjectDisposedException)
                    {
                        if (!CancellationTokenSource.IsCancellationRequested) throw; //If we did not intend to shutdown, rethrow

                        return default;
                    }

                }, null);

                var cancelTaskSource = new TaskCompletionSource<bool>();

                using (CancellationTokenSource.Token.Register(() => cancelTaskSource.SetResult(true)))
                {
                    var finishedTask = await Task.WhenAny(cancelTaskSource.Task, resultTask);

                    if (finishedTask == cancelTaskSource.Task)
                    {
                        break;
                    }
                }

                OnMessageReceived?.Invoke(this, (resultTask.Result.Buffer, resultTask.Result.RemoteEndPoint));
            }

            UdpClient.Close();
        }
    }
}

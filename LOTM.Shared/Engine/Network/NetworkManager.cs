using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkManager
    {
        protected Thread Listener { get; }

        protected IPEndPoint NetworkEndpoint { get; }

        public NetworkManager(string networkAddress)
        {
            NetworkEndpoint = IPEndPoint.Parse(networkAddress);
            Listener = new Thread(Listen);
            Listener.Start();
        }

        protected void Listen()
        {
            //UdpClient udpServer = new UdpClient(11000);

            //while (true)
            //{
            //    var remoteEP = new IPEndPoint(IPAddress.Any, 11000);
            //    var data = udpServer.Receive(ref remoteEP); // listen on port 11000
            //    Console.WriteLine("receive data from " + remoteEP.ToString());
            //    udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
            //}
        }
    }
}

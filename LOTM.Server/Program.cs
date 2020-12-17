using LOTM.Server.Game;
using LOTM.Shared.Engine.Math;

namespace LOTM.Server
{
    class Program
    {
        /// <summary>
        /// Lair of the Midget dedicated server
        /// </summary>
        /// <param name="port">The port that the server will listen on. Default is 4297</param>
        static void Main(uint port)
        {
            if (port == 0)
            {
                port = 4297;
            }

            new LotmServer($"0.0.0.0:{port}").Start();

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

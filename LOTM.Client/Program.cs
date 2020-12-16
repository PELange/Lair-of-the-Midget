using LOTM.Client.Game;

namespace LOTM.Client
{
    class Program
    {
        /// <summary>
        /// Lair of the Midget dedicated server
        /// </summary>
        /// <param name="connect">Host ip:port. Default is 127.0.0.1:4297</param>
        static void Main(string connect)
        {
            if (string.IsNullOrEmpty(connect))
            {
                connect = "127.0.0.1:4297";
            }

            //var timer = new Timer
            //{
            //    Interval = 100
            //};
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();

            new LotmClient(500, 500, connect).Start();
        }

        //private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    var client = new UdpClient();
        //    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening
        //    client.Connect(ep);

        //    // send data
        //    client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);

        //    // then receive data
        //    var receivedData = client.Receive(ref ep);

        //    Console.WriteLine("receive data from " + ep.ToString());
        //}
    }
}

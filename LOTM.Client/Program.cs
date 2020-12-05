using LOTM.Client.Game;

namespace LOTM.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //var timer = new Timer
            //{
            //    Interval = 100
            //};
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();

            new LotmClient(500, 500).Start();
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

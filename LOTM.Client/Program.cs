using System;

using LOTM.Client.Game;

namespace LOTM.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");

            //UDPSocket c = new UDPSocket();
            //c.Client("127.0.0.1", 27000);
            //c.Send("TEST!");

            //if (System.Diagnostics.Debugger.IsAttached) Console.ReadLine();

            new LotmGame(500, 500).Start();
        }
    }
}

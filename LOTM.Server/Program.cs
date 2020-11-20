using LOTM.Shared;
using System;

namespace LOTM.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Server!");

            UDPSocket s = new UDPSocket();
            s.Server("127.0.0.1", 27000);

            if (System.Diagnostics.Debugger.IsAttached) Console.ReadLine();
        }
    }
}

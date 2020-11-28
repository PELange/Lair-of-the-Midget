using LOTM.Shared;
using System;
using System.Timers;

namespace LOTM.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = new Timer();
            timer.Interval = 100;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Console.WriteLine("Hello Server!");

            Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId);

            UDPSocket s = new UDPSocket();
            s.Server("127.0.0.1", 27000);

            if (System.Diagnostics.Debugger.IsAttached) Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("lloyd");

            //if (sender is Timer timer)
            //{
            //    timer.Stop();
            //}

            Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }
}

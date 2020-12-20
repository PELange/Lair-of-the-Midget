using LOTM.Client.Game;

namespace LOTM.Client
{
    class Program
    {
        /// <summary>
        /// Lair of the Midget
        /// </summary>
        /// <param name="name">Name of your player that appears in multiplayer. Default is Player</param>
        /// <param name="connect">Host ip:port. Default is 127.0.0.1:4297</param>
        static void Main(string name, string connect)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Player";
            }

            if (string.IsNullOrEmpty(connect))
            {
                connect = "127.0.0.1:4297";
            }
            new LotmClient(720, 720, connect, name).Start();
        }
    }
}

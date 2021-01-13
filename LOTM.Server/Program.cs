using LOTM.Server.Game;

namespace LOTM.Server
{
    class Program
    {
        /// <summary>
        /// Lair of the Midget dedicated server
        /// </summary>
        /// <param name="port">The port that the server will listen on. Default is 4297</param>
        /// <param name="lobbySize">How many players can join the game. Default is 1</param>
        static void Main(uint port, uint lobbySize)
        {
            if (port == 0)
            {
                port = 4297;
            }

            if (lobbySize == 0)
            {
                lobbySize = 1;
            }

            new LotmServer($"0.0.0.0:{port}", lobbySize).Start();
        }
    }
}

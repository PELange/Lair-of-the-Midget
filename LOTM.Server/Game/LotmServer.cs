using LOTM.Server.Game.Network;
using LOTM.Shared.Game.Network.Packets;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        public LotmNetworkManagerServer NetworkServer { get; set; }

        public LotmServer(string listenAddress)
            : base(new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
        }

        protected override void OnInit()
        {
            //World.Seed = System.Guid.NewGuid().GetHashCode();
            World.Seed = 130;
        }

        protected override void OnBeforeUpdate()
        {
            if (NetworkManager.TryGetPacket(out var packet))
            {
                switch (packet)
                {
                    case PlayerJoin playerJoin:
                    {
                        NetworkServer.OnPlayerJoin(playerJoin, CreatePlayerJoinAck);
                        break;
                    }
                }
            }
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
        }

        protected override void OnUpdate(double deltaTime)
        {
        }

        protected override void OnAfterUpdate()
        {
        }

        protected override void OnShutdown()
        {
        }

        public PlayerJoinAck CreatePlayerJoinAck(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            //todo put in the correct values
            return new PlayerJoinAck
            {
                PlayerGameObjectId = -1,
                WorldSeed = World.Seed
            };
        }
    }
}

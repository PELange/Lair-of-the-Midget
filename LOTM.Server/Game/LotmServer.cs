using LOTM.Server.Game.Network;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        public LotmServer(string listenAddress)
            : base(new LotmNetworkManagerServer(listenAddress))
        {
        }

        protected override void OnInit()
        {
        }

        protected override void OnBeforeUpdate()
        {
            if (NetworkManager.TryGetPacket(out var packet))
            {
                System.Console.WriteLine($"Packet arrived -> Type:{packet.GetType()}");
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
    }
}

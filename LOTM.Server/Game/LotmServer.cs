using LOTM.Server.Engine.Network;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        public LotmServer(string listenAddress) : base(new NetworkManagerServer(listenAddress))
        {
        }

        protected override void OnInit()
        {
        }

        protected override void OnBeforeUpdate()
        {
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

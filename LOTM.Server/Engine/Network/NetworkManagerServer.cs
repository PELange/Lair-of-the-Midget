using LOTM.Shared.Engine.Network;

namespace LOTM.Server.Engine.Network
{
    public class NetworkManagerServer : NetworkManager
    {
        public NetworkManagerServer(string listenBindAddress) : base(listenBindAddress)
        {
        }
    }
}

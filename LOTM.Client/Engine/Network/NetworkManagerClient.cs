using LOTM.Shared.Engine.Network;

namespace LOTM.Client.Engine.Network
{
    class NetworkManagerClient : NetworkManager
    {
        public NetworkManagerClient(string serverAddress) : base(serverAddress)
        {
        }
    }
}

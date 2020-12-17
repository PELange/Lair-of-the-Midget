using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using System.Net;

namespace LOTM.Server.Game.Network
{
    public class LotmNetworkManagerServer : NetworkManager
    {
        public LotmNetworkManagerServer(string listenBindAddress)
            : base(new LotmNetworkPacketSerializationProvider(), IPEndPoint.Parse(listenBindAddress)) //Start listen on bind ip and port
        {
        }
    }
}

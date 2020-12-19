using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using System.Net;

namespace LOTM.Server.Game.Network
{
    public class LotmNetworkManagerServer : NetworkManager
    {
        public LotmNetworkManagerServer(string listenBindAddress)
            : base(UdpSocket.CreateServer(IPEndPoint.Parse(listenBindAddress)), new LotmNetworkPacketSerializationProvider()) //Start listen on bind ip and port
        {
        }
    }
}

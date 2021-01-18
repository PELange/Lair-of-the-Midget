using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using System.Collections.Generic;
using System.Net;

namespace LOTM.Server.Game.Network
{
    public class LotmNetworkManagerServer : NetworkManager
    {
        protected Dictionary<string, IPEndPoint> ConnectedPlayers { get; }

        public LotmNetworkManagerServer(string listenBindAddress)
            : base(UdpSocket.CreateServer(IPEndPoint.Parse(listenBindAddress)), new LotmNetworkPacketSerializationProvider()) //Start listen on bind ip and port
        {
            ConnectedPlayers = new Dictionary<string, IPEndPoint>();
        }

        public void AddConnectedPlayer(IPEndPoint player)
        {
            ConnectedPlayers.Add(player.ToString(), player);
        }

        public void Broadcast(NetworkPacket packet)
        {
            foreach (var player in ConnectedPlayers.Values)
            {
                SendPacket(packet, player);
            }
        }

        public void SendPacketTo(NetworkPacket packet, IPEndPoint receiver, bool sendNow = false)
        {
            SendPacket(packet, receiver, sendNow);
        }
    }
}

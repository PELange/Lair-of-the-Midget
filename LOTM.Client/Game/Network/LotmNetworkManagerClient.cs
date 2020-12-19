using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using LOTM.Shared.Game.Network.Packets;
using System.Net;

namespace LOTM.Client.Game.Network
{
    public class LotmNetworkManagerClient : NetworkManager
    {
        protected IPEndPoint ServerEndpoint { get; set; }

        public string PlayerName { get; }

        public bool IsConnected { get; set; }

        public LotmNetworkManagerClient(string serverAddress, string playerName)
            : base(UdpSocket.CreateClient(IPEndPoint.Parse(serverAddress)), new LotmNetworkPacketSerializationProvider())
        {
            ServerEndpoint = IPEndPoint.Parse(serverAddress);
            PlayerName = playerName;
        }

        public void SendPacket(NetworkPacket packet)
        {
            SendPacket(packet, ServerEndpoint);
        }

        public bool OnPlayerJoinAck(PlayerJoinAck playerJoinAck)
        {
            if (IsConnected) return false;

            IsConnected = true;

            return true;
        }

        public void EnsureServerConnection()
        {
            if (!IsConnected)
            {
                SendPacket(new PlayerJoin { PlayerName = PlayerName });
            }
        }
    }
}

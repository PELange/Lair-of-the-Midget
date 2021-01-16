﻿using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using System.Net;

namespace LOTM.Client.Game.Network
{
    public class LotmNetworkManagerClient : NetworkManager
    {
        protected IPEndPoint ServerEndpoint { get; set; }


        public LotmNetworkManagerClient(string serverAddress)
            : base(UdpSocket.CreateClient(IPEndPoint.Parse(serverAddress)), new LotmNetworkPacketSerializationProvider())
        {
            ServerEndpoint = IPEndPoint.Parse(serverAddress);
        }

        public void SendPacket(NetworkPacket packet)
        {
            SendPacket(packet, ServerEndpoint);
        }

        public string CurrentServer => ServerEndpoint.ToString();
    }
}

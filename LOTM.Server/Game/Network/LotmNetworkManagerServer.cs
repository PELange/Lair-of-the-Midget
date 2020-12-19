using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network;
using LOTM.Shared.Game.Network.Packets;
using System;
using System.Collections.Generic;
using System.Net;

namespace LOTM.Server.Game.Network
{
    public class LotmNetworkManagerServer : NetworkManager
    {
        public List<IPEndPoint> ConnectedPlayers { get; }

        public Dictionary<string, PlayerJoinAck> JoinAcknowledgements { get; set; }

        public LotmNetworkManagerServer(string listenBindAddress)
            : base(UdpSocket.CreateServer(IPEndPoint.Parse(listenBindAddress)), new LotmNetworkPacketSerializationProvider()) //Start listen on bind ip and port
        {
            ConnectedPlayers = new List<IPEndPoint>();

            JoinAcknowledgements = new Dictionary<string, PlayerJoinAck>();
        }

        public void OnPlayerJoin(PlayerJoin playerJoin, Func<PlayerJoin, PlayerJoinAck> playerCreationCallback)
        {
            var lookupKey = playerJoin.Sender.ToString();

            //Only execute if the player was not previously known
            if (JoinAcknowledgements.ContainsKey(lookupKey))
            {
                //Fetch the previous ack package and resend it
                SendPacket(JoinAcknowledgements[lookupKey], playerJoin.Sender);

                //Console.WriteLine($"Resent PlayerJoinAck for {lookupKey}");
            }
            else
            {
                //Send new ack back to the player
                var ack = playerCreationCallback(playerJoin);

                JoinAcknowledgements[lookupKey] = ack;
                ConnectedPlayers.Add(playerJoin.Sender);

                SendPacket(ack, playerJoin.Sender);

                //Console.WriteLine($"Sent inital PlayerJoinAck for {lookupKey}");
            }
        }

        public void Broadcast(NetworkPacket packet)
        {
            foreach (var player in ConnectedPlayers)
            {
                SendPacket(packet, player);
            }
        }
    }
}

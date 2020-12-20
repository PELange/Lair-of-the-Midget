using LOTM.Server.Game.Network;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;
using static LOTM.Shared.Engine.Objects.GameObject;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        protected LotmNetworkManagerServer NetworkServer { get; set; }

        public int NextFreeEntityId { get; set; }

        protected Dictionary<string, PlayerObject> Players { get; set; }

        public LotmServer(string listenAddress)
            : base(new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
            Players = new Dictionary<string, PlayerObject>();
        }

        protected override void OnInit()
        {
            //World.Seed = new System.Random().Next(0, 100000);
            World.Seed = 130;
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
            //Process all incoming packets
            if (NetworkManager.TryGetPacket(out var inbound))
            {
                switch (inbound)
                {
                    //A player wants to join / and or has no recived our join ack yet
                    case PlayerJoin playerJoin:
                    {
                        NetworkServer.OnPlayerJoin(playerJoin, CreatePlayerJoinAck);
                        break;
                    }

                    //A player sends input for the character he controls
                    case PlayerInput playerInput:
                    {
                        if (Players.TryGetValue(playerInput.Sender.ToString(), out var playerObject))
                        {
                            //Console.WriteLine($"{DateTime.Now} Recieved input {playerInput.Inputs}");
                            playerObject.PacketsInbound.Enqueue(playerInput);
                        }

                        break;
                    }
                }
            }

            //Process broadcast all outbound packets
            foreach (var gameObject in World.Objects)
            {
                while (gameObject.PacketsOutbound.TryDequeue(out var outbound))
                {
                    NetworkServer.Broadcast(outbound);
                }
            }
        }

        public PlayerJoinAck CreatePlayerJoinAck(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            var playerObject = new PlayerObject(
                new Vector2(6 * 16, 6 * 16),
                scale: new Vector2(16, 16 * 2),
                instanceType: NetworkInstanceType.Server)
            {
                NetworkId = NextFreeEntityId++
            };

            Players[playerJoin.Sender.ToString()] = playerObject;

            World.Objects.Add(playerObject);

            return new PlayerJoinAck
            {
                PlayerGameObjectId = playerObject.NetworkId,
                WorldSeed = World.Seed
            };
        }
    }
}

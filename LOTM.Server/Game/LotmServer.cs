using LOTM.Server.Game.Network;
using LOTM.Server.Game.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        protected LotmNetworkManagerServer NetworkServer { get; set; }

        public int NextFreeEntityId { get; set; }

        protected Dictionary<string, PlayerBaseServer> Players { get; set; }

        public int WorldSeed { get; set; }

        public LotmServer(string listenAddress)
            : base(new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
            Players = new Dictionary<string, PlayerBaseServer>();
        }

        protected override void OnInit()
        {
            //WorldSeed = new System.Random().Next(0, 100000);
            WorldSeed = 130;
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
            //Process all incoming packets
            while (NetworkManager.TryGetPacket(out var inbound))
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
                            playerObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(playerInput);
                        }

                        break;
                    }
                }
            }

            //Run fixed simulation on all relevant world objects
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnFixedUpdate(FixedUpdateDeltaTime);
            }

            //Process broadcast all outbound packets
            foreach (var gameObject in World.GetAllObjects())
            {
                if (gameObject.GetComponent<NetworkSynchronization>() is NetworkSynchronization networkSynchronization)
                {
                    while (networkSynchronization.PacketsOutbound.TryDequeue(out var outbound))
                    {
                        NetworkServer.Broadcast(outbound);
                    }
                }
            }
        }

        protected override void OnUpdate(double deltaTime)
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnUpdate(deltaTime);
            }
        }

        public PlayerJoinAck CreatePlayerJoinAck(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            var spawnType = MovingHealthObjectType.PLAYER_WIZARD;
            var spawnPos = new Vector2(6 * 16, 6 * 16);
            var spawnScale = new Vector2(16, 16 * 2);
            var spawnHp = 100;

            var playerObject = new PlayerBaseServer(spawnType, spawnPos, spawnScale, spawnHp);

            //Set network id for newly spawned player
            var netId = NextFreeEntityId++;
            var netSync = playerObject.GetComponent<NetworkSynchronization>();
            netSync.NetworkId = netId;

            //Send inital create packet for the player
            netSync.PacketsOutbound.Enqueue(new MovingHealthObjectUpdate
            {
                Type = spawnType,
                PositionX = spawnPos.X,
                PositionY = spawnPos.Y,
                ScaleX = spawnScale.X,
                ScaleY = spawnScale.Y,
                Health = spawnHp,
            });

            //Store player object
            Players[playerJoin.Sender.ToString()] = playerObject;

            //Add to world
            World.AddObject(playerObject);

            //Respond to client
            return new PlayerJoinAck
            {
                PlayerObjectNetworkId = netId,
                WorldSeed = WorldSeed
            };
        }
    }
}

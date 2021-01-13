using LOTM.Server.Game.Network;
using LOTM.Server.Game.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        protected LotmNetworkManagerServer NetworkServer { get; set; }

        protected int NextFreeEntityId { get; set; }

        protected Dictionary<string, PlayerBaseServer> Players { get; set; }

        protected int WorldSeed { get; set; }

        protected bool GameActive { get; set; }

        protected uint LobbySize { get; }

        public LotmServer(string listenAddress, uint lobbySize)
            : base(1.0 / 60, new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
            Players = new Dictionary<string, PlayerBaseServer>();
            LobbySize = lobbySize;
        }

        protected override void OnInit()
        {
            //WorldSeed = new System.Random().Next(0, 100000);
            WorldSeed = 130;

            GenerateTestWorld();
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

                    case GameStateRequest gameStateRequest:
                    {
                        if (Players.TryGetValue(gameStateRequest.Sender.ToString(), out var playerObject))
                        {
                            //Respond back to client
                            NetworkServer.SendPacketTo(new GameStateUpdate { Active = GameActive }, gameStateRequest.Sender);
                        }

                        break;
                    }

                    //A player sends input for the character he controls
                    case PlayerInput playerInput:
                    {
                        if (Players.TryGetValue(playerInput.Sender.ToString(), out var playerObject))
                        {
                            playerObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(playerInput);

                            //Ack back to the player
                            NetworkServer.SendPacketTo(new PlayerInputAck { AckPacketId = playerInput.Id }, playerInput.Sender);
                        }

                        break;
                    }
                }
            }

            if (GameActive)
            {
                //Run fixed simulation on all relevant world objects
                foreach (var worldObject in World.GetAllObjects())
                {
                    worldObject.OnFixedUpdate(deltaTime, World);
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
            else if (!GameActive)
            {
                if (Players.Count >= LobbySize)
                {
                    GameActive = true;
                    NetworkServer.Broadcast(new GameStateUpdate { Active = true });
                }
            }
        }

        protected override void OnUpdate(double deltaTime)
        {
            if (GameActive)
            {
                foreach (var worldObject in World.GetAllObjects())
                {
                    worldObject.OnUpdate(deltaTime);
                }
            }
        }

        public PlayerJoinAck CreatePlayerJoinAck(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            if (playerJoin.PlayerType < ObjectType.Player_Wizard_Male || playerJoin.PlayerType > ObjectType.Player_Wizard_Male)
            {
                playerJoin.PlayerType = ObjectType.Player_Wizard_Male;
            }

            var spawnPos = new Vector2(-26, 97);
            //var spawnPos = new Vector2(-8, 12 * 16);
            var spawnHp = 100;

            var netId = NextFreeEntityId++;
            var playerObject = new PlayerBaseServer(netId, playerJoin.PlayerName, playerJoin.PlayerType, spawnPos, spawnHp);

            //Send inital create packet for the player
            var netSync = playerObject.GetComponent<NetworkSynchronization>();
            var transform = playerObject.GetComponent<Transformation2D>();
            var health = playerObject.GetComponent<Health>();
            var playerInfo = playerObject.GetComponent<PlayerInfo>();

            netSync.PacketsOutbound.Enqueue(new PlayerCreation
            {
                ObjectId = netSync.NetworkId,
                Type = playerObject.Type,
                PositionX = transform.Position.X,
                PositionY = transform.Position.Y,
                ScaleX = transform.Scale.X,
                ScaleY = transform.Scale.Y,
                Health = health.Value,
                Name = playerInfo.Name,
            });

            //Add to world
            World.AddObject(playerObject);

            //Store player object
            Players[playerJoin.Sender.ToString()] = playerObject;

            //Respond to client
            return new PlayerJoinAck
            {
                PlayerObjectNetworkId = netId,
                WorldSeed = WorldSeed
            };
        }

        protected void GenerateTestWorld()
        {
            var result = LevelGenerator.PreGenerate(4, WorldSeed);

            foreach (var obj in result)
            {
                if (obj.GetComponent<Collider>() != null)
                {
                    World.AddObject(obj);
                }
            }
        }
    }
}

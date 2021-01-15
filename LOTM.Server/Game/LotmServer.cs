using LOTM.Server.Game.Network;
using LOTM.Server.Game.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Collections.Generic;
using System.Linq;

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

        protected List<DungeonRoom> DungeonRooms { get; }

        public LotmServer(string listenAddress, uint lobbySize)
            : base(1.0 / 60, new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
            Players = new Dictionary<string, PlayerBaseServer>();
            LobbySize = lobbySize;
            DungeonRooms = new List<DungeonRoom>();
        }

        protected override void OnInit()
        {
            //WorldSeed = new System.Random().Next(0, 100000);
            WorldSeed = 120;

            PreGenerateWorld();
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

            MaintainDungeonRooomBuffer();

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

            var spawnPos = new Vector2(-8, -116);
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
                Health = health.CurrentHealth,
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
                WorldSeed = WorldSeed,
                LobbySize = (int)LobbySize
            };
        }

        protected void PreGenerateWorld()
        {
            const int PREGENERATE_COUNT = 1;

            var rooms = new List<DungeonRoom>
            {
                LevelGenerator.AddSpawn(Vector2.ZERO)
            };

            for (int preGenerate = 0; preGenerate < PREGENERATE_COUNT; preGenerate++)
            {
                rooms.Add(LevelGenerator.AppendRoom(rooms.Last(), (int)LobbySize, WorldSeed));
            }

            rooms.ForEach(x => AddDungeonRoom(x));
        }

        protected void MaintainDungeonRooomBuffer()
        {
            if (Players.Values.Count < 1) return; //No players yet

            //Find out how far the players did advance
            DungeonRoom highestActiveRoom = default;

            var lowestY = Players.Values.Min(x => x.GetComponent<Transformation2D>().Position.Y);

            foreach (var room in DungeonRooms)
            {
                if (lowestY < room.Position.Y && lowestY >= room.Position.Y - room.Size.Y)
                {
                    highestActiveRoom = room;
                    break;
                }
            }

            if (highestActiveRoom != default)
            {
                const int ROOM_BUFFER_COUNT = 2; //Clientside has 1 ... so we make it 2 in order to have all data prepared for the client as soon as he could ask for it

                //Re-addd rooms above if needed
                for (int nAbove = 1; nAbove <= ROOM_BUFFER_COUNT; nAbove++)
                {
                    if (!DungeonRooms.Any(x => x.RoomNumber == highestActiveRoom.RoomNumber + nAbove))
                    {
                        AddDungeonRoom(LevelGenerator.AppendRoom(DungeonRooms.First(x => x.RoomNumber == highestActiveRoom.RoomNumber + nAbove - 1), (int)LobbySize, WorldSeed));
                    }
                }
            }
        }

        protected void AddDungeonRoom(DungeonRoom dungeonRoom)
        {
            //System.Console.WriteLine($"Added room no. {dungeonRoom.RoomNumber} at <{dungeonRoom.Position.X};{dungeonRoom.Position.Y}>");

            //Only remember objects that have a collider or that are moveable
            dungeonRoom.Objects.RemoveAll(obj => !(obj.Item2.GetComponent<Collider>() != null || obj.Item2 is IMoveable));

            //Add the relevant objects to the world
            dungeonRoom.Objects.ForEach(obj => World.AddObject(obj.Item2));

            DungeonRooms.Add(dungeonRoom);
        }
    }
}

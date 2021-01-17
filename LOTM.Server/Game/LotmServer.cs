using LOTM.Server.Game.Network;
using LOTM.Server.Game.Objects;
using LOTM.Server.Game.Objects.Environment;
using LOTM.Server.Game.Objects.Interactable;
using LOTM.Server.Game.Objects.Living;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using LOTM.Shared.Game.Objects.Environment;
using LOTM.Shared.Game.Objects.Interactable;
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
            DungeonRooms = new List<DungeonRoom>();
            NextFreeEntityId = -1;
            LobbySize = lobbySize;
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
                        OnPlayerJoin(playerJoin);
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

                    //A player wants to know the most recent data about a dungeom room he loads
                    case DungeonRoomSyncRequest syncRequest:
                    {
                        SyncDungeonRoom(syncRequest);
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
                    NetworkServer.Broadcast(new GameStateUpdate { RequiresAck = true, Active = true });
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

        public void OnPlayerJoin(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            if (playerJoin.PlayerType < ObjectType.Player_Elf_Female || playerJoin.PlayerType > ObjectType.Player_Wizard_Male)
            {
                playerJoin.PlayerType = ObjectType.Player_Knight_Male;
            }

            var spawnPos = new Vector2(-8, -116);
            var spawnHp = 100;

            var objectId = NextFreeEntityId--;
            var playerObject = new PlayerBaseServer(objectId, playerJoin.PlayerName, playerJoin.PlayerType, spawnPos, spawnHp);

            if (Players.Count > 0) //Check if there are any other players besides the one that now joined
            {
                //Send inital create packet for the player to clients that were already connected.
                //Because this is the connect callback the new client is not yet part of the broadcast list
                NetworkServer.Broadcast(new PlayerCreation
                {
                    RequiresAck = true, //Guaranteed delivery!
                    ObjectId = playerObject.ObjectId,
                    Type = playerObject.Type,
                    PositionX = spawnPos.X,
                    PositionY = spawnPos.Y,
                    ScaleX = playerObject.GetComponent<Transformation2D>().Scale.X,
                    ScaleY = playerObject.GetComponent<Transformation2D>().Scale.Y,
                    Health = spawnHp,
                    Name = playerJoin.PlayerName,
                });
            }

            //Store player object
            Players[playerJoin.Sender.ToString()] = playerObject;

            //Add to world
            World.AddObject(playerObject);

            //Send info about all player objects including his own player to the connecting client
            foreach (var existingPlayer in Players.Values)
            {
                var playerObjTransform = existingPlayer.GetComponent<Transformation2D>();
                var playerObjHealth = existingPlayer.GetComponent<Health>();
                var playerObjInfo = existingPlayer.GetComponent<PlayerInfo>();

                NetworkServer.SendPacketTo(new PlayerCreation
                {
                    RequiresAck = true, //Guaranteed delivery!
                    ObjectId = playerObject.ObjectId,
                    Type = playerObject.Type,
                    PositionX = playerObjTransform.Position.X,
                    PositionY = playerObjTransform.Position.Y,
                    ScaleX = playerObjTransform.Scale.X,
                    ScaleY = playerObjTransform.Scale.Y,
                    Health = playerObjHealth.CurrentHealth,
                    Name = playerObjInfo.Name,
                }, playerJoin.Sender);
            }

            NetworkServer.AddConnectedPlayer(playerJoin.Sender);

            //Respond to client
            NetworkServer.SendPacketTo(new PlayerJoinAck
            {
                RequiresAck = true, //Guarantee delivery
                GameActive = GameActive,
                PlayerObjectId = objectId,
                WorldSeed = WorldSeed,
                LobbySize = (int)LobbySize
            }, playerJoin.Sender);
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

        protected void AddDungeonRoom(DungeonRoom dungeonRoom)
        {
            //System.Console.WriteLine($"Added room no. {dungeonRoom.RoomNumber} at <{dungeonRoom.Position.X};{dungeonRoom.Position.Y}>");

            //Only remember objects that have a collider or that are moveable
            dungeonRoom.Objects.RemoveAll(obj => !(obj.GetComponent<Collider>() != null || obj is IMoveable));

            for (int nObject = 0; nObject < dungeonRoom.Objects.Count; nObject++)
            {
                var obj = dungeonRoom.Objects[nObject];

                if (obj is DungeonDoor dungeonDoor)
                {
                    //Replace pickups with upgraded instance
                    dungeonRoom.Objects[nObject] = new DungeonDoorServer(dungeonDoor.ObjectId, dungeonDoor.Type, dungeonDoor.GetComponent<Transformation2D>().Position, dungeonDoor.Open, dungeonRoom);
                }
                else if (obj is Pickup pickup)
                {
                    //Replace pickups with upgraded instance
                    dungeonRoom.Objects[nObject] = new PickupServer(pickup.ObjectId, pickup.Type, pickup.GetComponent<Transformation2D>().Position);
                }
                else if (obj is LivingObject livingObject)
                {
                    var transform = livingObject.GetComponent<Transformation2D>();
                    var collider = livingObject.GetComponent<Collider>().Rects;
                    var health = livingObject.GetComponent<Health>();

                    //Replace object with upgraded instance
                    dungeonRoom.Objects[nObject] = new EnemyBaseServer(livingObject.ObjectId, livingObject.Type, transform.Position, transform.Scale, collider.FirstOrDefault(), health.CurrentHealth);

                    continue;
                }
            }

            //Add the relevant objects to the world
            dungeonRoom.Objects.ForEach(obj => World.AddObject(obj));

            DungeonRooms.Add(dungeonRoom);
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

        protected void SyncDungeonRoom(DungeonRoomSyncRequest syncRequest)
        {
            var dungeomRoom = DungeonRooms.Where(x => x.RoomNumber == syncRequest.RoomNumber).FirstOrDefault();

            if (dungeomRoom == null) return; //Client asked for a room he should not even know about yet.

            foreach (var enemy in dungeomRoom.Objects.Where(x => x is EnemyBaseServer).Select(x => x as EnemyBaseServer))
            {
                //Snyc position
                var transform = enemy.GetComponent<Transformation2D>();
                NetworkServer.SendPacketTo(new ObjectPositionUpdate
                {
                    RequiresAck = true, //Ensure client gets this packet
                    ObjectId = enemy.ObjectId,
                    PositionX = transform.Position.X,
                    PositionY = transform.Position.Y,
                }, syncRequest.Sender);

                //Sync health
                NetworkServer.SendPacketTo(new ObjectHealthUpdate
                {
                    ObjectId = enemy.ObjectId,
                    Health = enemy.GetComponent<Health>().CurrentHealth
                }, syncRequest.Sender);
            }

            foreach (var pickup in dungeomRoom.Objects.Where(x => x is PickupServer).Select(x => x as PickupServer))
            {
                NetworkServer.SendPacketTo(new PickupStateUpdate
                {
                    RequiresAck = true, //Ensure client gets this packet
                    ObjectId = pickup.ObjectId,
                    Active = pickup.Active,
                }, syncRequest.Sender);
            }

            foreach (var door in dungeomRoom.Objects.Where(x => x is DungeonDoorServer).Select(x => x as DungeonDoorServer))
            {
                if (!door.Open) continue; //No need to sync closed doors, as that is assumed the default state

                NetworkServer.SendPacketTo(new DoorStateUpdate
                {
                    RequiresAck = true, //Ensure client gets this packet
                    ObjectId = door.ObjectId,
                    Open = door.Open,
                }, syncRequest.Sender);
            }
        }
    }
}

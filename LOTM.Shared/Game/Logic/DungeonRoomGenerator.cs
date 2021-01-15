using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Environment;
using LOTM.Shared.Game.Objects.Interactable;
using System;
using System.Collections.Generic;

namespace LOTM.Shared.Game.Logic
{
    public class DungeonRoomGenerator
    {
        public List<GameObject> DungeonObjectList = new List<GameObject>();

        public List<Vector2> ObjectCoordList = new List<Vector2>();

        public Vector2 DefaultScaleVector => new Vector2(16, 16);
        public Vector2 StartCoords { get; set; }
        public Vector2 RoomStartCoords { get; set; }
        public int XDoorLeft { get; set; }
        public int XDoorRight { get; set; }
        public int Width { get; set; }
        public int WidthHalf { get; set; }
        public int Height { get; set; }
        public int HeightHalf { get; set; }
        public int TunnelLength { get; set; }
        public int PlayerCount { get; set; }
        public bool HasTunnel { get; set; }
        public Random Random { get; set; }

        protected int RoomObjectCounter { get; set; }
        protected int RoomNumber { get; }

        public DungeonRoomGenerator(int roomNumber, Vector2 startCoords, int width, int height, int tunnelLength, int playerCount, int seed, bool hasTunnel)
        {
            RoomObjectCounter = 100;
            RoomNumber = roomNumber;
            StartCoords = startCoords;
            RoomStartCoords = new Vector2(startCoords.X, startCoords.Y - tunnelLength * 16);
            Width = width;
            WidthHalf = width / 2;
            Height = height;
            HeightHalf = height / 2;
            TunnelLength = tunnelLength;
            HasTunnel = hasTunnel;
            PlayerCount = playerCount;
            XDoorLeft = -1;
            XDoorRight = XDoorLeft + 1;

            //Combine global seed and room start coords to get local room seed
            seed = (seed + (int)startCoords.X + (int)startCoords.Y) % int.MaxValue;

            Random = new Random(seed);
        }

        /// <summary>
        /// Creates the dungeon room
        /// </summary>
        public void CreateRoom()
        {
            CreateRoomStructure();
            CreatePillars();

            if (HasTunnel)
            {
                CreateRoomConnection();
            }

            CreatePickups();

            CreateEnemies();
        }

        int GetNextObjectId()
        {
            return RoomNumber * 10000 + (++RoomObjectCounter);
        }

        public void CreateRoomStructure(bool spawnRoom = false)
        {
            // Create ground tiles
            for (int i = -1; i > -Height - 1; i--)
            {
                for (int j = -WidthHalf; j < WidthHalf; j++)
                {
                    DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), (ObjectType)Random.Next((int)ObjectType.Tile_Ground_0, (int)ObjectType.Tile_Ground_3 + 1), new Vector2(RoomStartCoords.X + (j * 16), RoomStartCoords.Y + (i * 16)), DefaultScaleVector));
                }
            }

            //Create top corners
            DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_TopLeftCorner, new Vector2(RoomStartCoords.X - WidthHalf * 16, RoomStartCoords.Y - (Height + 2) * 16), new Vector2(16, 32)));
            DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_TopRightCorner, new Vector2(RoomStartCoords.X + (WidthHalf - 1) * 16, RoomStartCoords.Y - (Height + 2) * 16), new Vector2(16, 32)));

            //Create top walls
            for (int i = -WidthHalf + 1; i < WidthHalf - 1; i++)
            {
                if (i >= XDoorLeft - 1 && i <= XDoorRight + 1) continue; // Skip walls that would be at upper door position
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_TopWall, new Vector2(RoomStartCoords.X + (i * 16), RoomStartCoords.Y - (Height + 2) * 16), new Vector2(16, 32)));
            }

            // Create left and right walls
            for (int i = -2; i > -Height - 1; i--)
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_LeftWall, new Vector2(RoomStartCoords.X - WidthHalf * 16, RoomStartCoords.Y + (i * 16)), DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_RightWall, new Vector2(RoomStartCoords.X + (WidthHalf - 1) * 16, RoomStartCoords.Y + (i * 16)), DefaultScaleVector));
            }

            // Create Bottom corners
            DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_BottomLeftCorner, new Vector2(RoomStartCoords.X - WidthHalf * 16, RoomStartCoords.Y - 32), new Vector2(16, 32)));
            DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_BottomRightCorner, new Vector2(RoomStartCoords.X + (WidthHalf - 1) * 16, RoomStartCoords.Y - 32), new Vector2(16, 32)));

            // Create bottom walls
            for (int i = -WidthHalf + 1; i < WidthHalf - 1; i++)
            {
                if (i >= XDoorLeft - 1 && i <= XDoorRight + 1 && !spawnRoom) continue; // Skip walls that would be at lower door position if it isn't the spawnRoom
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_BottomWall, new Vector2(RoomStartCoords.X + (i * 16), RoomStartCoords.Y - 32), new Vector2(16, 32)));
            }

            // TODO make doors changeable from closed to opened
            CreateDoor(true, true); // Top door
            if (!spawnRoom)
            {
                CreateDoor(false, true); // Bottom door
            }
        }

        /// <summary>
        /// Creates an open/closed door at the top or bottom of a room
        /// </summary>
        /// <param name="top"></param>
        /// <param name="open"></param>
        public void CreateDoor(bool top, bool open)
        {
            int xCoord = (int)RoomStartCoords.X + (16 * XDoorLeft);
            int yCoord = top ? (int)RoomStartCoords.Y - (Height + 2) * 16 : (int)(RoomStartCoords.Y - 32);

            // Create door frame
            if (top)
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_DoorFrameTop, new Vector2(xCoord - 16, yCoord - 16), new Vector2(64, 64)));
            }
            else
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_DoorFrameBottom, new Vector2(xCoord - 16, yCoord - 16), new Vector2(64, 64)));
            }

            // Create door
            if (open)
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_DoorOpened, new Vector2(xCoord, yCoord), new Vector2(32, 32)));
            }
            else
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_DoorClosed, new Vector2(xCoord, yCoord), new Vector2(32, 32)));
            }

        }

        /// <summary>
        /// Creates a tunnel between two rooms
        /// </summary>
        public void CreateRoomConnection()
        {
            for (int i = 1; i <= TunnelLength; i++)
            {
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_Ground_0, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y - i * 16), DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_Ground_0, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y - i * 16), DefaultScaleVector));

                if (i < 1 || i > 2)
                {
                    DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_LeftWall, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y - i * 16), DefaultScaleVector));
                    DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_RightWall, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y - i * 16), DefaultScaleVector));
                }


            }
        }

        /// <summary>
        /// Create a random amount of pickups on random positions, based on seeds
        /// </summary>
        public void CreatePickups()
        {
            int pickupCount = Random.Next(PlayerCount / 2, PlayerCount + 1);
            while (pickupCount > 0)
            {
                Vector2 pickupCoords = GetFreeObjectCoords();
                ObjectCoordList.Add(pickupCoords);

                DungeonObjectList.Add(new Pickup(GetNextObjectId(), (ObjectType)Random.Next((int)ObjectType.Pickup_Health_Minor, (int)ObjectType.Pickup_Health_Major + 1), pickupCoords));
                pickupCount--;
            }
        }

        public void CreatePillars()
        {
            int maxPillarCount = (Width * Height) / 20;
            int pillarCount = Random.Next(maxPillarCount / 2, maxPillarCount + 1);

            while (pillarCount > 0)
            {
                Vector2 pillarCoords = GetFreeObjectCoords("pillar");
                ObjectCoordList.Add(pillarCoords);
                ObjectCoordList.Add(new Vector2(pillarCoords.X, pillarCoords.Y + 16));

                DungeonObjectList.Add(new DungeonTile(GetNextObjectId(), ObjectType.Tile_Pillar, pillarCoords, new Vector2(16, 32)));
                pillarCount--;
            }
        }

        /// <summary>
        /// Create random amount of enemys and place them on free tiles
        /// </summary>
        public void CreateEnemies()
        {
            int maxEnemyCount = PlayerCount * 2;
            int enemyCount = Random.Next(maxEnemyCount / 2, maxEnemyCount + 1);

            while (enemyCount > 0)
            {
                Vector2 enemyCoords = GetFreeObjectCoords();
                ObjectCoordList.Add(enemyCoords);

                DungeonObjectList.Add(GetRandomEnemy(enemyCoords));
                enemyCount--;
            }
        }

        /// <summary>
        /// Get a random enemy from enemy collection
        /// </summary>
        /// <param name="enemyCoords"></param>
        /// <returns></returns>
        public GameObject GetRandomEnemy(Vector2 enemyCoords)
        {
            switch ((ObjectType)Random.Next((int)ObjectType.Enemy_Ogre_Small, (int)ObjectType.Enemy_Blob_Small + 1))
            {
                case ObjectType.Enemy_Ogre_Small:
                {
                    return new LivingObject(GetNextObjectId(), ObjectType.Enemy_Ogre_Small, enemyCoords, new Vector2(16, 32), new Rectangle(0.25, 0.5, 0.7, 0.5), 100);
                }
                case ObjectType.Enemy_Skeleton_Small:
                {
                    return new LivingObject(GetNextObjectId(), ObjectType.Enemy_Skeleton_Small, enemyCoords, new Vector2(16, 32), new Rectangle(0.2, 0.55, 0.6, 0.45), 100);
                }
                case ObjectType.Enemy_Blob_Small:
                {
                    return new LivingObject(GetNextObjectId(), ObjectType.Enemy_Blob_Small, enemyCoords, new Vector2(16, 32), new Rectangle(0.1, 0.5, 0.8, 0.5), 100);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a free cell in the room where a object can be placed
        /// </summary>
        /// <returns></returns>
        public Vector2 GetFreeObjectCoords(string objectType = null)
        {
            for (int nIteration = 0; nIteration < 100000; nIteration++)
            {
                var objectX = Random.Next(-WidthHalf, WidthHalf);
                var objectY = Random.Next(-Height, -1);

                var objectCoords = new Vector2(RoomStartCoords.X + objectX * 16, RoomStartCoords.Y + objectY * 16);

                //Field already contains something
                if (ListContainsVector(objectCoords)) continue;

                //Do not place in front of door
                if ((objectX >= XDoorLeft - 1 && objectX <= XDoorRight + 1)
                    && ((objectY <= -Height + 1 && objectY >= -Height) || (objectY <= -1 && objectY >= -3))) continue;

                if (objectType == "pillar")
                {
                    if (ListContainsVector(new Vector2(objectCoords.X + 16, objectCoords.Y))) continue;
                    if (ListContainsVector(new Vector2(objectCoords.X - 16, objectCoords.Y))) continue;
                    if (ListContainsVector(new Vector2(objectCoords.X, objectCoords.Y + 16))) continue;
                    if (ListContainsVector(new Vector2(objectCoords.X, objectCoords.Y - 16))) continue;
                }

                return objectCoords;
            }

            return null;
        }

        /// <summary>
        /// Check if the objectCoordsList contains a vector with the same coordinates of the given vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public bool ListContainsVector(Vector2 vector)
        {
            foreach (Vector2 vec in ObjectCoordList)
            {
                if (vec.X == vector.X && vec.Y == vector.Y) return true;
            }
            return false;
        }
    }
}
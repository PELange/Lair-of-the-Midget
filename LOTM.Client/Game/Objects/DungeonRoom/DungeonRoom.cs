using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LOTM.Client.Game.Objects.DungeonRoom
{
    public class DungeonRoom
    {
        public enum TileType
        {
            Ground,
            Hole,
            StandardWall,
            LeftWall,
            RightWall,
            TopLeftCorner,
            TopRightCorner,
            BottomLeftCorner,
            BottomRightCorner,
            DoorClosed,
            DoorOpenedBottom,
            DoorOpenedTop,
            DoorArch,
            Pillar
        };

        public enum EnemyType
        {
            SkeletonSmall,
            OgreSmall,
            BlobGreen
        }

        public List<GameObject> DungeonObjectList = new List<GameObject>();

        public List<Vector2> ObjectCoordList = new List<Vector2>();

        public Vector2 DefaultScaleVector = new Vector2(16, 16);
        public Vector2 StartCoords { get; set; }
        public int XDoorLeft { get; set; }
        public int XDoorRight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TunnelLength { get; set; }
        public int PlayerCount { get; set; }
        public Random Random { get; set; }

        public DungeonRoom(Vector2 startCoords, int width, int height, int tunnelLength, int playerCount, int seed)
        {
            this.StartCoords = startCoords;
            this.Width = width;
            this.Height = height;
            this.TunnelLength = tunnelLength;
            this.PlayerCount = playerCount;
            this.XDoorLeft = (int)Width / 2 - 1;
            this.XDoorRight = XDoorLeft + 1;

            seed = (seed + (int)(startCoords.X) + (int)(startCoords.Y)) % int.MaxValue;

            Random = new Random(seed);

            this.CreateRoom();
        }

        /// <summary>
        /// Creates the dungeon room
        /// </summary>
        public void CreateRoom()
        {
            // Create ground tiles
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (j * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                }
            }

            CreateHoles();

            // Create ground tiles under bottom doors
            DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (XDoorLeft * 16), StartCoords.Y + (Height * 16)), 0, DefaultScaleVector));
            DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (XDoorRight * 16), StartCoords.Y + (Height * 16)), 0, DefaultScaleVector));

            this.CreatePickups();

            // Create top corners
            DungeonObjectList.Add(new DungeonTile(TileType.TopLeftCorner, Random, new Vector2(StartCoords.X, StartCoords.Y - 16), 0, new Vector2(16, 32)));
            DungeonObjectList.Add(new DungeonTile(TileType.TopRightCorner, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y - 16), 0, new Vector2(16, 32)));

            // Create top walls
            for (int i = 1; i < Width - 1; i++)
            {
                if (i == XDoorLeft || i == XDoorRight) continue; // Skip walls that would be at upper door position
                DungeonObjectList.Add(new DungeonTile(TileType.StandardWall, Random, new Vector2(StartCoords.X + (i * 16), StartCoords.Y - 16), 0, new Vector2(16, 32)));
            }

            // Create left and right walls
            for (int i = 1; i < Height - 1; i++)
            {
                DungeonObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X, StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
            }

            // Create Bottom corners
            DungeonObjectList.Add(new DungeonTile(TileType.BottomLeftCorner, Random, new Vector2(StartCoords.X, StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));
            DungeonObjectList.Add(new DungeonTile(TileType.BottomRightCorner, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));

            // Create bottom walls
            for (int i = 1; i < Width - 1; i++)
            {
                if (i == XDoorLeft || i == XDoorRight) continue; // Skip walls that would be at lower door position
                DungeonObjectList.Add(new DungeonTile(TileType.StandardWall, Random, new Vector2(StartCoords.X + (i * 16), StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));
            }

            this.CreateRoomConnection();

            this.CreateDoor(true, true);
            this.CreateDoor(false, true);

            this.CreateEnemies();
        }

        /// <summary>
        /// Creates an open/closed door at the top or bottom of a room
        /// </summary>
        /// <param name="top"></param>
        /// <param name="open"></param>
        public void CreateDoor(bool top, bool open)
        {
            int xCoord = (int)StartCoords.X + (16 * XDoorLeft);
            int yCoord = top ? (int)StartCoords.Y : (int)StartCoords.Y + Height * 16;

            if (open)
            {
                DungeonObjectList.Add(new DungeonTile(TileType.DoorOpenedBottom, Random, new Vector2(xCoord, yCoord), 0, new Vector2(32, 16)));
                DungeonObjectList.Add(new DungeonTile(TileType.DoorOpenedTop, Random, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 16)));
                DungeonObjectList.Add(new DungeonTile(TileType.DoorArch, Random, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }
            else
            {
                DungeonObjectList.Add(new DungeonTile(TileType.DoorClosed, Random, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 32)));
                DungeonObjectList.Add(new DungeonTile(TileType.DoorArch, Random, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }

        }

        /// <summary>
        /// Creates a tunnel between two rooms
        /// </summary>
        public void CreateRoomConnection()
        {
            for (int i = 1; i < TunnelLength; i++)
            {
                DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y - i * 16), 0, DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y - i * 16), 0, DefaultScaleVector));

            }

            for (int j = 0; j < TunnelLength; j++)
            {
                DungeonObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y - j * 16), 0, DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y - j * 16), 0, DefaultScaleVector));
            }
        }

        /// <summary>
        /// Only for the first room
        /// Creates a long tunnel that leads to the first dungeon room, so that the player cant see the beginning of the tunnel
        /// </summary>
        public void CreateDungeonEntrance()
        {
            for (int i = Height + 1; i < 20 + Height; i++)
            {
                DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));

                DungeonObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X + XDoorLeft * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
                DungeonObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + XDoorRight * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
            }
        }

        /// <summary>
        /// Create a random amount of pickups on random positions, based on seed
        /// </summary>
        public void CreatePickups()
        {
            int pickupCount = Random.Next(PlayerCount / 2, PlayerCount + 1);
            while (pickupCount > 0)
            {
                Vector2 pickupCoords = GetFreeObjectCoords();
                ObjectCoordList.Add(pickupCoords);

                DungeonObjectList.Add(new Pickup(Random, pickupCoords, 0, DefaultScaleVector));
                pickupCount--;
            }

        }

        /// <summary>
        /// Create pickups on every free tile except in front of doors
        /// </summary>
        public void CreatePickupsEverywhere()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (!((j == XDoorLeft || j == XDoorRight) && (i == Height - 1 || i == 1)))
                    {
                        DungeonObjectList.Add(new Pickup(Random, new Vector2(StartCoords.X + (j * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                    }
                }
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

                DungeonObjectList.Add(new DungeonTile(TileType.Pillar, Random, pillarCoords, 0, new Vector2(16, 48)));
                pillarCount--;
            }
        }

        /// <summary>
        /// Create non passable holes and delete the ground tiles at those positions
        /// </summary>
        public void CreateHoles()
        {
            int maxHoleCount = (Width * Height) / 10;
            int holeCount = Random.Next(maxHoleCount / 2, maxHoleCount + 1);

            while (holeCount > 0)
            {
                Vector2 holeCoords = GetFreeObjectCoords();
                ObjectCoordList.Add(holeCoords);

                RemoveObjectAtPosition(holeCoords);

                DungeonObjectList.Add(new DungeonTile(TileType.Hole, Random, holeCoords, 0, DefaultScaleVector));
                holeCount--;
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
            Array enemyTypes = Enum.GetValues(typeof(EnemyType));
            EnemyType enemyType = (EnemyType)enemyTypes.GetValue(Random.Next(enemyTypes.Length));
            switch (enemyType)
            {
                case EnemyType.SkeletonSmall:
                    return new SkeletonSmall(enemyCoords, 0, DefaultScaleVector);
                case EnemyType.OgreSmall:
                    return new OgreSmall(enemyCoords, 0, DefaultScaleVector);
                case EnemyType.BlobGreen:
                    return new BlobGreen(enemyCoords, 0, DefaultScaleVector);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets a free cell in the room where a object can be placed
        /// </summary>
        /// <returns></returns>
        public Vector2 GetFreeObjectCoords(String objectType = null)
        {
            int objectX = Random.Next(0, Width - 1);
            int objectY = Random.Next(1, Height - 1);
            Vector2 objectCoords = new Vector2(StartCoords.X + objectX * 16, StartCoords.Y + objectY * 16);
            bool coordsOccupied = ListContainsVector(objectCoords);

            // Create new coords for pickup until they are not in front of a door and there is a free cell
            while (coordsOccupied || ((objectX == XDoorLeft || objectX == XDoorRight) && (objectY == Height - 1 || objectY == 1)))
            {
                objectX = Random.Next(0, Width - 1);
                objectY = Random.Next(0, Height - 1);
                objectCoords = new Vector2(StartCoords.X + objectX * 16, StartCoords.Y + objectY * 16);
                coordsOccupied = ListContainsVector(objectCoords);
            }

            return objectCoords;
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

        /// <summary>
        /// Removes an object from the DungeonObjectList by given position
        /// </summary>
        /// <param name="vector"></param>
        public void RemoveObjectAtPosition(Vector2 vector)
        {
            foreach (GameObject gameObj in DungeonObjectList)
            {
                if (vector.X == gameObj.position.X && vector.Y == gameObj.position.Y)
                {
                    DungeonObjectList.Remove(gameObj);
                    break;
                }
            }
        }
    }
}
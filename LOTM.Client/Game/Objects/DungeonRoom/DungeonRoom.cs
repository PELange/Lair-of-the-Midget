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
            DoorArch
        };

        public List<GameObject> ObjectList = new List<GameObject>();

        public Vector2 DefaultScaleVector = new Vector2(16, 16);
        public Vector2 StartCoords { get; set; }
        public int xDoorLeft { get; set; }
        public int xDoorRight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TunnelLength { get; set; }
        public Random Random { get; set; }

        public DungeonRoom(Vector2 startCoords, int width, int height, int tunnelLength, int seed)
        {
            this.StartCoords = startCoords;
            this.Width = width;
            this.Height = height;
            this.TunnelLength = tunnelLength;
            this.xDoorLeft = (int)Width / 2 - 1;
            this.xDoorRight = xDoorLeft + 1;

            seed = (seed + (int)(startCoords.X) + (int)(startCoords.Y)) % int.MaxValue;

            Random = new Random(seed);

            this.CreateRoom();
        }

        public void CreateRoom()
        {
            // Create ground tiles
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (j * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                    
                }
            }

            // Create ground tiles under doors
            // Top door
            //ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (xDoorLeft * 16), StartCoords.Y), 0, DefaultScaleVector));
            //ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (xDoorRight * 16), StartCoords.Y), 0, DefaultScaleVector));
            //// Bottom door
            ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (xDoorLeft * 16), StartCoords.Y + (Height * 16)), 0, DefaultScaleVector));
            ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + (xDoorRight * 16), StartCoords.Y + (Height * 16)), 0, DefaultScaleVector));

            this.CreatePickups(5, 11);
            //this.CreatePickupsEverywhere();

            // Create top corners
            ObjectList.Add(new DungeonTile(TileType.TopLeftCorner, Random, new Vector2(StartCoords.X, StartCoords.Y - 16), 0, new Vector2(16, 32)));
            ObjectList.Add(new DungeonTile(TileType.TopRightCorner, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y - 16), 0, new Vector2(16, 32)));

            // Create top walls
            for (int i = 1; i < Width - 1; i++)
            {
                if (i == xDoorLeft || i == xDoorRight) continue; // Skip walls that would be at upper door position
                ObjectList.Add(new DungeonTile(TileType.StandardWall, Random, new Vector2(StartCoords.X + (i * 16), StartCoords.Y - 16), 0, new Vector2(16, 32)));
            }

            // Create left and right walls
            for (int i = 1; i < Height - 1; i++)
            {
                ObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X, StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                ObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
            }

            // Create Bottom corners
            ObjectList.Add(new DungeonTile(TileType.BottomLeftCorner, Random, new Vector2(StartCoords.X, StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));
            ObjectList.Add(new DungeonTile(TileType.BottomRightCorner, Random, new Vector2(StartCoords.X + ((Width - 1) * 16), StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));

            // Create bottom walls
            for (int i = 1; i < Width - 1; i++)
            {
                if (i == xDoorLeft || i == xDoorRight) continue; // Skip walls that would be at lower door position
                ObjectList.Add(new DungeonTile(TileType.StandardWall, Random, new Vector2(StartCoords.X + (i * 16), StartCoords.Y + ((Height - 1) * 16)), 0, new Vector2(16, 32)));
            }

            this.CreateRoomConnection();

            this.CreateDoor(true, true);
            this.CreateDoor(false, true);
        }

        public void CreateDoor(bool top, bool open)
        {
            int xCoord = (int)StartCoords.X + (16 * xDoorLeft);
            int yCoord = top ? (int)StartCoords.Y : (int)StartCoords.Y + Height * 16;

            if (open)
            {
                ObjectList.Add(new DungeonTile(TileType.DoorOpenedBottom, Random, new Vector2(xCoord, yCoord), 0, new Vector2(32, 16)));
                ObjectList.Add(new DungeonTile(TileType.DoorOpenedTop, Random, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 16)));
                ObjectList.Add(new DungeonTile(TileType.DoorArch, Random, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }
            else
            {
                ObjectList.Add(new DungeonTile(TileType.DoorClosed, Random, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 32)));
                ObjectList.Add(new DungeonTile(TileType.DoorArch, Random, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }

        }

        public void CreateRoomConnection()
        {
            for (int i = 1; i < TunnelLength; i++)
            {
                ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + xDoorLeft * 16, StartCoords.Y - i * 16), 0, DefaultScaleVector));
                ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + xDoorRight * 16, StartCoords.Y - i * 16), 0, DefaultScaleVector));
                
            }

            for (int j = 0; j < TunnelLength; j++)
            {
                ObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X + xDoorLeft * 16, StartCoords.Y - j * 16), 0, DefaultScaleVector));
                ObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + xDoorRight * 16, StartCoords.Y - j * 16), 0, DefaultScaleVector));
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
                ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + xDoorLeft * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
                ObjectList.Add(new DungeonTile(TileType.Ground, Random, new Vector2(StartCoords.X + xDoorRight * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));

                ObjectList.Add(new DungeonTile(TileType.LeftWall, Random, new Vector2(StartCoords.X + xDoorLeft * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
                ObjectList.Add(new DungeonTile(TileType.RightWall, Random, new Vector2(StartCoords.X + xDoorRight * 16, StartCoords.Y + i * 16), 0, DefaultScaleVector));
            }
        }

        /// <summary>
        /// Create a random amount of pickups on random positions, based on seed
        /// </summary>
        /// <param name="minPickups"></param>
        /// <param name="maxPickups"></param>
        public void CreatePickups(int minPickups, int maxPickups)
        {
            int pickupCount = Random.Next(minPickups, maxPickups);
            List<Vector2> pickupCoordList = new List<Vector2>();
            while(pickupCount > -1)
            {
                int pickupX = Random.Next(0, Width  - 1);
                int pickupY = Random.Next(1, Height - 1);
                Vector2 pickupCoords = new Vector2(StartCoords.X + pickupX * 16, StartCoords.Y + pickupY * 16);
                bool coordsOccupied = ListContainsVector(pickupCoordList, pickupCoords);
                
                // Create new coords for pickup until they are not in front of a door and there is a free cell
                while (coordsOccupied || ((pickupX == xDoorLeft || pickupX == xDoorRight) && (pickupY == Height - 1 || pickupY == 1))) {
                    pickupX = Random.Next(0, Width - 1);
                    pickupY = Random.Next(0, Height - 1);
                    pickupCoords = new Vector2(StartCoords.X + pickupX * 16, StartCoords.Y + pickupY * 16);
                    coordsOccupied = ListContainsVector(pickupCoordList, pickupCoords);
                }
                pickupCoordList.Add(pickupCoords);

                ObjectList.Add(new Pickup(Random, pickupCoords, 0, DefaultScaleVector));
                pickupCount--;
            }

        }

        /// <summary>
        /// Create pickups on every free tile except
        /// </summary>
        public void CreatePickupsEverywhere()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (!((j == xDoorLeft || j == xDoorRight) && (i == Height - 1 || i == 1)))
                    {
                        ObjectList.Add(new Pickup(Random, new Vector2(StartCoords.X + (j * 16), StartCoords.Y + (i * 16)), 0, DefaultScaleVector));
                    }
                }
            }
        }

        /// <summary>
        /// Check if a list of vectors contains a vector with the same coordinates of the given vector
        /// </summary>
        /// <param name="vectorList"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public bool ListContainsVector(List<Vector2> vectorList, Vector2 vector)
        {
            foreach (Vector2 vec in vectorList)
            {
                if (vec.X == vector.X && vec.Y == vector.Y) return true;
            }
            return false;
        }
    }
}
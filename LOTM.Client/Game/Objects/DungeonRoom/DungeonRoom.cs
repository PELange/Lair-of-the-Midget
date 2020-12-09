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

        public List<GameObject> tileList = new List<GameObject>();
        Vector2 startCoords;
        int width;
        int height;

        public Random Random { get; set; }

        public DungeonRoom(Vector2 startCoords, int width, int height, int seed)
        {
            this.startCoords = startCoords;
            this.width = width;
            this.height = height;

            seed = (seed + (int)(startCoords.X) + (int)(startCoords.Y)) % int.MaxValue;

            Random = new Random(seed);

            this.createRoom();
        }

        public void createRoom()
        {
            // Create ground tiles
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tileList.Add(new DungeonTile(TileType.Ground, new Vector2(startCoords.X + (j * 16), startCoords.Y + (i * 16)), 0, new Vector2(16, 16)));
                }
            }
            // Create ground tiles under doors
            // Top door
            tileList.Add(new DungeonTile(TileType.Ground, new Vector2(startCoords.X + ((int)(width / 2 - 1) * 16), startCoords.Y), 0, new Vector2(16, 16)));
            tileList.Add(new DungeonTile(TileType.Ground, new Vector2(startCoords.X + ((int)(width / 2) * 16), startCoords.Y), 0, new Vector2(16, 16)));
            // Bottom door
            tileList.Add(new DungeonTile(TileType.Ground, new Vector2(startCoords.X + ((int)(width / 2 - 1) * 16), startCoords.Y + (height * 16)), 0, new Vector2(16, 16)));
            tileList.Add(new DungeonTile(TileType.Ground, new Vector2(startCoords.X + ((int)(width / 2) * 16), startCoords.Y + (height * 16)), 0, new Vector2(16, 16)));

            // Create top corners
            tileList.Add(new DungeonTile(TileType.TopLeftCorner, new Vector2(startCoords.X, startCoords.Y - 16), 0, new Vector2(16, 32)));
            tileList.Add(new DungeonTile(TileType.TopRightCorner, new Vector2(startCoords.X + ((width - 1) * 16), startCoords.Y - 16), 0, new Vector2(16, 32)));

            // Create top walls
            for (int i = 1; i < width - 1; i++)
            {
                if (i == (int)width / 2 - 1 || i == (int)width / 2) continue; // Skip walls that would be at upper door position
                tileList.Add(new DungeonTile(TileType.StandardWall, new Vector2(startCoords.X + (i * 16), startCoords.Y - 16), 0, new Vector2(16, 32)));
            }

            // Create left and right walls
            for (int i = 0; i < height - 1; i++)
            {
                tileList.Add(new DungeonTile(TileType.LeftWall, new Vector2(startCoords.X, startCoords.Y + (i * 16)), 0, new Vector2(16, 48)));
                tileList.Add(new DungeonTile(TileType.RightWall, new Vector2(startCoords.X + ((width - 1) * 16), startCoords.Y + (i * 16)), 0, new Vector2(16, 48)));
            }

            // Create Bottom corners
            tileList.Add(new DungeonTile(TileType.BottomLeftCorner, new Vector2(startCoords.X, startCoords.Y + ((height - 1) * 16)), 0, new Vector2(16, 32)));
            tileList.Add(new DungeonTile(TileType.BottomRightCorner, new Vector2(startCoords.X + ((width - 1) * 16), startCoords.Y + ((height - 1) * 16)), 0, new Vector2(16, 32)));

            // Create bottom walls
            for (int i = 1; i < width - 1; i++)
            {
                if (i == (int)width / 2 - 1 || i == (int)width / 2) continue; // Skip walls that would be at lower door position
                tileList.Add(new DungeonTile(TileType.StandardWall, new Vector2(startCoords.X + (i * 16), startCoords.Y + ((height - 1) * 16)), 0, new Vector2(16, 32)));
            }

            this.createDoor(true, false);
            this.createDoor(false, true);


            for (int i = 0; i < 5; i++)
            {
                tileList.Add(new DemonBoss(new Vector2(Random.Next((int)startCoords.X, (int)(startCoords.X + (width * 16))), Random.Next((int)startCoords.Y, (int)(startCoords.Y + (height * 16)))), 0, new Vector2(32, 32)));
            }
        }

        public void createDoor(bool top, bool open)
        {
            int xCoord = (int)startCoords.X + (16 * (width / 2 - 1));
            int yCoord = top ? (int)startCoords.Y : (int)startCoords.Y + height * 16;

            if (open)
            {
                tileList.Add(new DungeonTile(TileType.DoorOpenedBottom, new Vector2(xCoord, yCoord), 0, new Vector2(32, 16)));
                tileList.Add(new DungeonTile(TileType.DoorOpenedTop, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 16)));
                tileList.Add(new DungeonTile(TileType.DoorArch, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }
            else
            {
                tileList.Add(new DungeonTile(TileType.DoorClosed, new Vector2(xCoord, yCoord - 16), 0, new Vector2(32, 32)));
                tileList.Add(new DungeonTile(TileType.DoorArch, new Vector2(xCoord, yCoord - 32), 0, new Vector2(32, 16)));
            }

        }
    }
}
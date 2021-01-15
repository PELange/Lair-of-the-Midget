using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System.Collections.Generic;

namespace LOTM.Shared.Game.Logic
{
    public class LevelGenerator
    {
        public static List<GameObject> PreGenerate(int playerCount, int seed)
        {
            var result = new List<GameObject>();

            int roomCount = 3;
            int roomWidth = 9 + playerCount;
            int roomHeight = 9 + playerCount;
            if (roomWidth % 2 == 1) roomWidth += 1;
            if (roomHeight % 2 == 1) roomHeight += 1;
            int tunnelLength = 5;

            // Create rooms
            for (int nRoom = -1; nRoom < roomCount; nRoom++)
            {
                if (nRoom == -1)
                {
                    var roomCoords = new Vector2(0, 0);
                    var dungeonRoomGenerator = new DungeonRoomGenerator(roomCoords, 10, 10, 0, playerCount, seed, false);
                    dungeonRoomGenerator.CreateRoomStructure(true);
                    result.AddRange(dungeonRoomGenerator.DungeonObjectList);
                }
                else
                {
                    var roomCoords = new Vector2(0, -nRoom * (roomHeight + tunnelLength) * 16 - 160); // -160 is the offset for the spawnroom
                    var dungeonRoomGenerator = new DungeonRoomGenerator(roomCoords, roomWidth, roomHeight, tunnelLength, playerCount, seed, true);
                    dungeonRoomGenerator.CreateRoom();
                    result.AddRange(dungeonRoomGenerator.DungeonObjectList);
                }
            }

            return result;
        }

        public static DungeonRoom AddSpawn(Vector2 position)
        {
            var dungeonRoomGenerator = new DungeonRoomGenerator(position, 10, 10, 0, 0, 0, false);
            dungeonRoomGenerator.CreateRoomStructure(true);
            return new DungeonRoom(0, position, new Vector2(dungeonRoomGenerator.Width * 16, (dungeonRoomGenerator.Height + dungeonRoomGenerator.TunnelLength) * 16), dungeonRoomGenerator.DungeonObjectList);
        }

        public static DungeonRoom AddRoom(int roomNumber, Vector2 position, int playerCount, int seed)
        {
            int roomWidth = 9 + playerCount;
            int roomHeight = 9 + playerCount;

            //Make room dimensions even
            if (roomWidth % 2 == 1) roomWidth += 1;
            if (roomHeight % 2 == 1) roomHeight += 1;

            int tunnelLength = 5;

            var dungeonRoomGenerator = new DungeonRoomGenerator(position, roomWidth, roomHeight, tunnelLength, playerCount, seed, true);
            dungeonRoomGenerator.CreateRoom();
            return new DungeonRoom(roomNumber, position, new Vector2(dungeonRoomGenerator.Width * 16, (dungeonRoomGenerator.Height + dungeonRoomGenerator.TunnelLength) * 16), dungeonRoomGenerator.DungeonObjectList);
        }

        public static DungeonRoom AppendRoom(DungeonRoom lastRoom, int playerCount, int seed)
        {
            return AddRoom(lastRoom.RoomNumber + 1, new Vector2(lastRoom.Position.X, lastRoom.Position.Y - lastRoom.Size.Y), playerCount, seed);
        }

        public static DungeonRoom PrependRoom(DungeonRoom firstRoom, int playerCount, int seed)
        {
            var position = new Vector2(firstRoom.Position.X, firstRoom.Position.Y + firstRoom.Size.Y);

            if (firstRoom.RoomNumber <= 1)
            {
                position.Y = firstRoom.Position.Y + 160;
                return AddSpawn(position);
            }

            return AddRoom(firstRoom.RoomNumber - 1, position, playerCount, seed);
        }
    }
}

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
                    var dungeonRoom = new DungeonRoom(roomCoords, 10, 10, 0, playerCount, seed, false);
                    dungeonRoom.CreateRoomStructure(true);
                    result.AddRange(dungeonRoom.DungeonObjectList);
                } else
                {
                    var roomCoords = new Vector2(0, -nRoom * (roomHeight + tunnelLength) * 16 - 160); // -160 is the offset for the spawnroom
                    var dungeonRoom = new DungeonRoom(roomCoords, roomWidth, roomHeight, tunnelLength, playerCount, seed, true);
                    dungeonRoom.CreateRoom();
                    result.AddRange(dungeonRoom.DungeonObjectList);
                }


            }

            return result;
        }
    }
}

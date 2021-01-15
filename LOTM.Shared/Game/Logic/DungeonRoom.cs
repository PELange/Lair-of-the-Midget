using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System.Collections.Generic;

namespace LOTM.Shared.Game.Logic
{
    public class DungeonRoom
    {
        public int RoomNumber { get; set; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public List<GameObject> Objects { get; }

        public DungeonRoom(int roomNumber, Vector2 position, Vector2 size, List<GameObject> objects)
        {
            RoomNumber = roomNumber;
            Position = position;
            Size = size;
            Objects = objects;
        }
    }
}

using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Environment
{
    public class DungeonDoor : DungeonTile
    {
        public bool Open { get; set; }

        public DungeonDoor(int id, ObjectType type, Vector2 position, bool open)
            : base(id, type, position, new Vector2(32, 32))
        {
            Open = open;

            AddComponent(new NetworkSynchronization());

            AddComponent(new Collider(this, new Rectangle(0, 0, 1, 1), !open));
        }
    }
}

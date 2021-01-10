using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Environment
{
    public class DungeonTile : TypedObject
    {
        public DungeonTile(ObjectType type, Vector2 position, Vector2 scale)
            : base(type, position, 0, scale)
        {
            switch (Type)
            {
                case ObjectType.Tile_Pillar:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.5, 1, 0.5)));
                    break;
                }

                //todo add walls and doors ...
            }
        }
    }
}

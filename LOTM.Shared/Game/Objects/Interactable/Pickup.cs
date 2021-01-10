using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Interactable
{
    public class Pickup : TypedObject
    {
        public Pickup(ObjectType type, Vector2 position)
            : base(type, position, 0, new Vector2(16, 16))
        {
            AddComponent(new Collider(this, new BoundingBox(0, 0, 1, 1)));
        }
    }
}

using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Interactable
{
    public class Pickup : TypedObject
    {
        public Pickup(int id, ObjectType type, Vector2 position)
            : base(id, type, position, 0, new Vector2(16, 16))
        {
            AddComponent(new Collider(this, new Rectangle(0.25, 0.35, 0.5, 0.5)));
        }
    }
}

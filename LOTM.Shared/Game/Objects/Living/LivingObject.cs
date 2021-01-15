using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Shared.Game.Objects
{
    public abstract class LivingObject : TypedObject, IMoveable
    {
        public LivingObject(int id, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(id, type, position, 0, scale)
        {
            Components.Add(new Collider(this, colliderInfo));

            Components.Add(new NetworkSynchronization());

            Components.Add(new Health(100, health));
        }
    }
}

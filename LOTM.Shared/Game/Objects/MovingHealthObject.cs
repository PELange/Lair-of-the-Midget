using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Shared.Game.Objects
{
    public abstract class MovingHealthObject : GameObject, IMoveable
    {
        public MovingHealthObjectType Type { get; set; }

        public MovingHealthObject(MovingHealthObjectType type, Vector2 position = default, Vector2 scale = default, double health = default)
            : base(position, 0, scale)
        {
            Type = type;

            Components.Add(new Health(health));
            Components.Add(new NetworkSynchronization());
        }
    }
}

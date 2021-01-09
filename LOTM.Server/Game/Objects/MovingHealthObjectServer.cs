using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;

namespace LOTM.Server.Game.Objects
{
    public class MovingHealthObjectServer : LivingObject
    {
        public MovingHealthObjectServer(ObjectType type, Vector2 position, Vector2 scale, BoundingBox colliderInfo, double health)
            : base(type, position, scale, colliderInfo, health)
        {
        }
    }
}

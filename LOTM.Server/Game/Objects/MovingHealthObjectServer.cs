using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;

namespace LOTM.Server.Game.Objects
{
    public class MovingHealthObjectServer : MovingHealthObject
    {
        public MovingHealthObjectServer(MovingHealthObjectType type, Vector2 position, Vector2 scale, double health)
            : base(type, position, scale, health)
        {
        }
    }
}

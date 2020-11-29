using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;

namespace LOTM.Client.Game.Objects
{
    public class DemonBoss : GameObject
    {
        public DemonBoss(Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Components.Add(new SpriteRenderer(AssetManager.GetSprite("demonboss_idle_0")));
        }

        public override void OnFixedUpdate(double deltaTime)
        {
        }

        public override void OnUpdate(double deltaTime)
        {
            var transform = GetComonent<Transformation2D>();

            transform.Rotation += 45 * deltaTime;
        }
    }
}

using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects
{
    public class DemonBoss : GameObject
    {
        public DemonBoss(Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                new SpriteRenderer.Segment(AssetManager.GetSprite($"demonboss_idle_{0}_1"), new Vector2(0.5, 0.5)),
                new SpriteRenderer.Segment(AssetManager.GetSprite($"demonboss_idle_{0}_2"), new Vector2(0.5, 0.5), new Vector2(0.5, 0)),
                new SpriteRenderer.Segment(AssetManager.GetSprite($"demonboss_idle_{0}_3"), new Vector2(0.5, 0.5), new Vector2(0, 0.5), layer: 1001),
                new SpriteRenderer.Segment(AssetManager.GetSprite($"demonboss_idle_{0}_4"), new Vector2(0.5, 0.5), new Vector2(0.5, 0.5), layer: 1001),
            }));
        }

        public override void OnUpdate(double deltaTime)
        {
            var transform = GetComponent<Transformation2D>();

            transform.Rotation += 45 * deltaTime;
        }
    }
}

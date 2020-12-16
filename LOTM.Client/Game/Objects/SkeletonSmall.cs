using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static LOTM.Client.Engine.Controls.InputManager;

namespace LOTM.Client.Game.Objects
{
    class SkeletonSmall : GameObject
    {
        protected int CurrentAnimationPhase { get; set; }
        protected double AnimationTimer { get; set; }

        public SkeletonSmall(Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                new SpriteRenderer.Segment(AssetManager.GetSprite($"skeleton_small_m_idle_anim_f{0}"))
            }));
        }

        public override void OnFixedUpdate(double deltaTime)
        {
        }

        public override void OnUpdate(double deltaTime)
        {
            //animation prototype
            AnimationTimer += deltaTime;

            if (AnimationTimer >= 0.16)
            {
                AnimationTimer -= 0.16;

                CurrentAnimationPhase = (CurrentAnimationPhase + 1) % 3;
            }

            if (GetComonent<SpriteRenderer>() is SpriteRenderer spriteRenderer)
            {

                spriteRenderer.Segments[0].Sprite = AssetManager.GetSprite($"skeleton_small_m_idle_anim_f{CurrentAnimationPhase}");
            }
        }
    }
}

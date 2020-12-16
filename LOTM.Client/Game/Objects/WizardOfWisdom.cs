using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System.Collections.Generic;
using static LOTM.Client.Engine.Controls.InputManager;

namespace LOTM.Client.Game.Objects
{
    class WizardOfWisdom : GameObject
    {
        protected int CurrentAnimationPhase { get; set; }
        protected double AnimationTimer { get; set; }

        public WizardOfWisdom(Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                new SpriteRenderer.Segment(AssetManager.GetSprite($"wizzard_m_idle_anim_f{0}"))
            }));
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            //walking prototype
            var walkSpeed = 100;
            if (GetComonent<Transformation2D>() is Transformation2D transformation)
            {
                if (IsControlPressed(ControlType.WALK_LEFT))
                {
                    transformation.Position.X -= walkSpeed * deltaTime;
                }
                else if (IsControlPressed(ControlType.WALK_RIGHT))
                {
                    transformation.Position.X += walkSpeed * deltaTime;
                }

                if (IsControlPressed(ControlType.WALK_UP))
                {
                    transformation.Position.Y -= walkSpeed * deltaTime;
                }
                else if (IsControlPressed(ControlType.WALK_DOWN))
                {
                    transformation.Position.Y += walkSpeed * deltaTime;
                }
            }
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
                spriteRenderer.Segments[0].Color = new Vector4(1, 0, 0, 1);
                spriteRenderer.Segments[0].Sprite = AssetManager.GetSprite($"wizzard_m_idle_anim_f{CurrentAnimationPhase}");
            }
        }
    }
}

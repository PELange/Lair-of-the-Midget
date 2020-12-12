using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LOTM.Client.Game.Objects
{
    class OgreSmall : GameObject
    {
        protected int CurrentAnimationPhase { get; set; }
        protected double AnimationTimer { get; set; }

        public OgreSmall(Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Components.Add(new SpriteRenderer());
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

                spriteRenderer.Sprite = AssetManager.GetSprite($"ogre_small_m_idle_anim_f{CurrentAnimationPhase}");
            }
        }
    }
}

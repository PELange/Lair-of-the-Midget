﻿using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects.Player
{
    public class PlayerBaseClient : MovingHealthObjectClient
    {
        protected int CurrentAnimationPhase { get; set; }
        protected double AnimationTimer { get; set; }

        public PlayerBaseClient(int networkId, MovingHealthObjectType type, Vector2 position, Vector2 scale, double health)
            : base(networkId, type, position, scale, health)
        {
            if (Type == MovingHealthObjectType.PLAYER_WIZARD)
            {
                Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
                {
                    new SpriteRenderer.Segment(AssetManager.GetSprite($"wizzard_m_idle_anim_f{0}"))
                }));
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

            if (GetComponent<SpriteRenderer>() is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.Segments[0].Color = new Vector4(1, 0, 0, 1);

                if (Type == MovingHealthObjectType.PLAYER_WIZARD)
                {
                    spriteRenderer.Segments[0].Sprite = AssetManager.GetSprite($"wizzard_m_idle_anim_f{CurrentAnimationPhase}");
                }
            }
        }
    }
}
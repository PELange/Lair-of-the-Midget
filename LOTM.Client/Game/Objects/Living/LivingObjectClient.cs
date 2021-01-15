using LOTM.Client.Engine;
using LOTM.Client.Engine.Graphics;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects
{
    public class LivingObjectClient : LivingObject
    {
        private enum AnimationState
        {
            Idle,
            Walk
        }

        private int CurrentAnimationPhase { get; set; }
        private AnimationState CurrentAnimationState { get; set; }
        private double AnimationTimer { get; set; }
        private string AnimationSpriteSet { get; set; }

        private Vector2 LastLocalPosition { get; }

        public LivingObjectClient(int objectId, ObjectType type, Vector2 position, Vector2 scale, Rectangle colliderInfo, double health)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
            LastLocalPosition = new Vector2(position.X, position.Y); //Copy not reference!!!

            CurrentAnimationPhase = 0;
            CurrentAnimationState = AnimationState.Idle;

            AnimationSpriteSet = Type switch
            {
                ObjectType.Player_Wizard_Male => $"wizzard_m",
                ObjectType.Enemy_Ogre_Small => $"ogre_small_m",
                ObjectType.Enemy_Skeleton_Small => $"skeleton_small_m",
                ObjectType.Enemy_Blob_Small => $"blob_green_m",
                _ => ""
            };

            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                //primary body sprite
                new SpriteRenderer.Segment(GetCurrentBodySprite() ?? AssetManager.GetSprite($"solid_white")),

                //Base health bar
                new SpriteRenderer.Segment(AssetManager.GetSprite($"solid_white"), new Vector2(1, 0.1), new Vector2(0, 0.25), new Vector4(0.929, 0.172, 0.219, 1.0)),

                //Green hp bar overlaying the red bar
                new SpriteRenderer.Segment(AssetManager.GetSprite($"solid_white"), new Vector2(1, 0.1), new Vector2(0, 0.25), new Vector4(0.074, 0.705, 0.094, 1.0)),
            }));

        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //Process inbound packets

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectPositionUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectPositionUpdate objectPositionUpdate)
            {
                var transform = GetComponent<Transformation2D>();

                transform.Position.X = objectPositionUpdate.PositionX;
                transform.Position.Y = objectPositionUpdate.PositionY;
            }

            //2. Check for health updates and only apply the lastest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectHealthUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectHealthUpdate objectHealthUpdate)
            {
                var health = GetComponent<Health>();

                health.CurrentHealth = objectHealthUpdate.Health;
            }

            networkSynchronization.PacketsInbound.Clear();
        }

        public override void OnUpdate(double deltaTime)
        {
            //1. Advance current animation phase
            AnimationTimer += deltaTime;

            if (AnimationTimer >= 0.16)
            {
                AnimationTimer -= 0.16;

                CurrentAnimationPhase = (CurrentAnimationPhase + 1) % 3;
            }

            //2. Update animation states

            ////2.1 Detect position changes -> aka walking
            //var transform = GetComponent<Transformation2D>();
            //if (LastLocalPosition.X != transform.Position.X || LastLocalPosition.Y != transform.Position.Y)
            //{
            //    LastLocalPosition.X = transform.Position.X;
            //    LastLocalPosition.Y = transform.Position.Y;

            //    CurrentAnimationState = AnimationState.Walk;
            //}
            //else
            //{
            //    //CurrentAnimationState = AnimationState.Idle;
            //}

            var spriteRenderer = GetComponent<SpriteRenderer>();

            //2.2 Update primary body sprite
            spriteRenderer.Segments[0].Sprite = GetCurrentBodySprite();

            //3. Update health bar
            var health = GetComponent<Health>();
            spriteRenderer.Segments[2].Size.X = health.CurrentHealth / health.MaxHealth;
        }

        private Sprite GetCurrentBodySprite()
        {
            return AssetManager.GetSprite($"{AnimationSpriteSet}_{((CurrentAnimationState == AnimationState.Idle) ? "idle" : "walk")}_anim_f{CurrentAnimationPhase}");
        }
    }
}

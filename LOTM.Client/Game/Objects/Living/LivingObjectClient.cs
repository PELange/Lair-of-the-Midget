using LOTM.Client.Engine;
using LOTM.Client.Engine.Graphics;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects
{
    public class LivingObjectClient : LivingObject
    {
        public int LastPostionUpdatePacketId { get; set; }
        public int LastHealthUpdatePacketId { get; set; }

        private enum AnimationState
        {
            Idle,
            Walk
        }

        private int CurrentAnimationPhase { get; set; }
        private AnimationState CurrentAnimationState { get; set; }
        private double AnimationTimer { get; set; }
        private string AnimationSpriteSet { get; set; }

        protected bool IsLeft { get; set; }
        protected bool IsPlayer { get; set; }
        private Vector2 LatestServerPosition { get; set; }

        public LivingObjectClient(int objectId, ObjectType type, Vector2 position, Vector2 scale, Rectangle colliderInfo, double health)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
            IsLeft = true;

            IsPlayer = Type switch
            {
                ObjectType.Player_Elf_Female => true,
                ObjectType.Player_Knight_Male => true,
                ObjectType.Player_Wizard_Male => true,
                _ => false
            };


            CurrentAnimationPhase = 0;
            CurrentAnimationState = AnimationState.Idle;

            AnimationSpriteSet = Type switch
            {
                ObjectType.Player_Elf_Female => $"elf_f",
                ObjectType.Player_Knight_Male => $"knight_m",
                ObjectType.Player_Wizard_Male => $"wizzard_m",
                ObjectType.Enemy_Ogre_Small => $"ogre_small_m",
                ObjectType.Enemy_Skeleton_Small => $"skeleton_small_m",
                ObjectType.Enemy_Blob_Small => $"blob_green_m",
                _ => ""
            };

            var baseRenderLayer = Type switch
            {
                ObjectType.Enemy_Ogre_Small => 1001,
                ObjectType.Enemy_Skeleton_Small => 1002,
                ObjectType.Enemy_Blob_Small => 1003,
                _ => 1000
            };

            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                //primary body sprite
                new SpriteRenderer.Segment(GetCurrentBodySprite() ?? AssetManager.GetSprite($"solid_white"), layer: baseRenderLayer),

                //Base health bar
                new SpriteRenderer.Segment(AssetManager.GetSprite($"solid_white"), new Vector2(1, 0.1), new Vector2(0, 0.25), new Vector4(0.929, 0.172, 0.219, 1.0), layer: baseRenderLayer + 2000),

                //Green hp bar overlaying the red bar
                new SpriteRenderer.Segment(AssetManager.GetSprite($"solid_white"), new Vector2(1, 0.1), new Vector2(0, 0.25), new Vector4(0.074, 0.705, 0.094, 1.0), layer: baseRenderLayer + 2000),

                //Skull when dead
                new SpriteRenderer.Segment(AssetManager.GetSprite($"skull"), new Vector2(1, 0.5), new Vector2(0, 0.5), layer: baseRenderLayer - 100, active: false),
            }));

        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //Process inbound packets

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectPositionUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectPositionUpdate objectPositionUpdate)
            {
                //Only accept the position update, if the packet id is larger than the last known update about it. This avoids retransmission issues.
                if (objectPositionUpdate.Id > LastPostionUpdatePacketId)
                {
                    LastPostionUpdatePacketId = objectPositionUpdate.Id;

                    LatestServerPosition = new Vector2(objectPositionUpdate.PositionX, objectPositionUpdate.PositionY);

                    //var transform = GetComponent<Transformation2D>();
                    //transform.Position.X = objectPositionUpdate.PositionX;
                    //transform.Position.Y = objectPositionUpdate.PositionY;
                }
            }

            //2. Check for health updates and only apply the lastest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectHealthUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectHealthUpdate objectHealthUpdate)
            {
                //Only accept the health update, if the packet id is larger than the last known update about it. This avoids retransmission issues.
                if (objectHealthUpdate.Id > LastHealthUpdatePacketId)
                {
                    LastHealthUpdatePacketId = objectHealthUpdate.Id;

                    GetComponent<Health>().CurrentHealth = objectHealthUpdate.Health;
                }
            }

            networkSynchronization.PacketsInbound.Clear();
        }

        public override void OnUpdate(double deltaTime)
        {
            var transform = GetComponent<Transformation2D>();

            //1. Advance current animation phase
            AnimationTimer += deltaTime;

            if (AnimationTimer >= 0.16)
            {
                AnimationTimer -= 0.16;

                CurrentAnimationPhase = (CurrentAnimationPhase + 1) % 3;
            }

            //2. Interpolate positions
            if (LatestServerPosition != null)
            {
                const int maxLerpDistance = 16 * 6;

                //Target position is too far away for lerping. Make visual cut and teleport there
                if (Math.Abs(LatestServerPosition.X - transform.Position.X) > maxLerpDistance ||
                    Math.Abs(LatestServerPosition.Y - transform.Position.Y) > maxLerpDistance)
                {
                    transform.Position.X = LatestServerPosition.X;
                    transform.Position.Y = LatestServerPosition.Y;
                }
                else if (transform.Position.X != LatestServerPosition.X || transform.Position.Y != LatestServerPosition.Y) //We are not yet where the server told us to be
                {
                    var newPosition = Vector2.Lerp(transform.Position, LatestServerPosition, deltaTime * (IsPlayer ? LotmGameConfig.PlayerMovementSpeed : LotmGameConfig.EnemyMovementSpeed));

                    //Avoid X-axis extrapolation
                    if (transform.Position.X < LatestServerPosition.X && newPosition.X > LatestServerPosition.X ||
                        transform.Position.X > LatestServerPosition.X && newPosition.X < LatestServerPosition.X ||
                        transform.Position.X == LatestServerPosition.X)
                    {
                        newPosition.X = LatestServerPosition.X;
                    }

                    //Avoid Y-axis extrapolation
                    if (transform.Position.Y < LatestServerPosition.Y && newPosition.Y > LatestServerPosition.Y ||
                        transform.Position.Y > LatestServerPosition.Y && newPosition.Y < LatestServerPosition.Y ||
                        transform.Position.Y == LatestServerPosition.Y)
                    {
                        newPosition.Y = LatestServerPosition.Y;
                    }

                    transform.Position.X = newPosition.X;
                    transform.Position.Y = newPosition.Y;
                }
            }

            //3. Update animation states

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

            //3.2 Update primary body sprite
            spriteRenderer.Segments[0].Sprite = GetCurrentBodySprite();
            spriteRenderer.Segments[0].VerticalFlip = IsLeft;

            //4. Update health related visuals
            var health = GetComponent<Health>();

            spriteRenderer.Segments[2].Size.X = health.CurrentHealth / health.MaxHealth;

            if (!health.IsDead())
            {
                //Enable hp bar and main body sprite
                spriteRenderer.Segments[0].Active = true;
                spriteRenderer.Segments[1].Active = true;
                spriteRenderer.Segments[2].Active = true;

                //Hide skull
                spriteRenderer.Segments[3].Active = false;
            }
            else
            {
                //Disable hp bar and main body
                spriteRenderer.Segments[0].Active = false;
                spriteRenderer.Segments[1].Active = false;
                spriteRenderer.Segments[2].Active = false;

                //Show skull
                spriteRenderer.Segments[3].Active = true;
            }

            //Disable collider visuals when dead
            GetComponent<Collider>().Active = !health.IsDead();
        }

        private Sprite GetCurrentBodySprite()
        {
            return AssetManager.GetSprite($"{AnimationSpriteSet}_{((CurrentAnimationState == AnimationState.Idle) ? "idle" : "walk")}_anim_f{CurrentAnimationPhase}");
        }
    }
}

using LOTM.Client.Engine;
using LOTM.Client.Engine.Graphics;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects.Player
{
    public class PlayerBaseClient : LivingObjectClient
    {
        public int LastAttackStateUpdatePacketId { get; set; }

        protected DateTime LastAttackTime { get; set; }
        protected double AttackAnimationProgress { get; set; }
        protected SpriteRenderer.Segment WeaponSegment { get; }
        protected Vector2 WeaponOffset { get; }
        protected Vector2 WeaponRotationOffset { get; }

        public PlayerBaseClient(int networkId, string name, ObjectType type, Vector2 position, Vector2 scale, double health)
            : base(networkId, type, position, scale, new Rectangle(0.2, 0.75, 0.7, 0.25), health)
        {
            AddComponent(new PlayerInfo(name));

            AddComponent(new TextRenderer(new List<TextRenderer.Segment>
            {
                new TextRenderer.Segment(name, "showcard_gothic", 5, new Vector2(0.5, 0.2))
            }));

            //Add Weapon
            string weaponSprite = "";
            Vector2 weaponDimensions = default;

            switch (Type)
            {
                case ObjectType.Player_Elf_Female:
                {
                    weaponSprite = "golden_sword";
                    weaponDimensions = new Vector2(1, 1);
                    WeaponOffset = new Vector2(-0.25, 0.1);
                    WeaponRotationOffset = new Vector2(-0.25, 0.4);
                    break;
                }

                case ObjectType.Player_Knight_Male:
                {
                    weaponSprite = "sword";
                    weaponDimensions = new Vector2(1, 1);
                    WeaponOffset = new Vector2(-0.25, 0);
                    WeaponRotationOffset = new Vector2(-0.25, 0.35);
                    break;
                }

                case ObjectType.Player_Wizard_Male:
                {
                    weaponSprite = "staff";
                    weaponDimensions = new Vector2(1, 1);
                    WeaponOffset = new Vector2(-0.25, 0.1);
                    WeaponRotationOffset = new Vector2(-0.25, 0.4);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(weaponSprite))
            {
                WeaponSegment = new SpriteRenderer.Segment(AssetManager.GetSprite(weaponSprite), weaponDimensions, new Vector2(WeaponOffset.X, WeaponOffset.Y))
                {
                    RotationCenterOffset = new Vector2(WeaponRotationOffset.X, WeaponRotationOffset.Y)
                };

                GetComponent<SpriteRenderer>().Segments.Add(WeaponSegment);
            }
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is AttackStateUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is AttackStateUpdate attackStateUpdate)
            {
                //Only accept the position update, if the packet id is larger than the last known update about it. This avoids retransmission issues.
                if (attackStateUpdate.Id > LastAttackStateUpdatePacketId)
                {
                    LastAttackStateUpdatePacketId = attackStateUpdate.Id;

                    if (attackStateUpdate.Attacking) TriggerAttackAnimation();
                }
            }

            base.OnFixedUpdate(deltaTime, world);
        }

        public override void OnUpdate(double deltaTime)
        {
            base.OnUpdate(deltaTime);

            var health = GetComponent<Health>();
            var spriteRenderer = GetComponent<SpriteRenderer>();

            var deltaSinceAttackStart = (DateTime.Now - LastAttackTime).TotalMilliseconds;

            double attackAnimationTime = 250;
            double attackAnimationTotalSwingDegrees = 220 * (IsLeft ? -1 : 1);
            double attackAnimationSwingRotationOffser = -110 * (IsLeft ? -1 : 1);

            var oldProgress = AttackAnimationProgress;
            AttackAnimationProgress = Math.Max(0, Math.Min(1.0, deltaSinceAttackStart / attackAnimationTime));

            if (AttackAnimationProgress >= 0 && AttackAnimationProgress <= 1.0 && oldProgress != 1.0)
            {
                //From half of the animation start reversing it
                var rotation = attackAnimationTotalSwingDegrees * 2 * (AttackAnimationProgress < 0.5 ? AttackAnimationProgress : 1 - AttackAnimationProgress) + attackAnimationSwingRotationOffser;

                WeaponSegment.Rotation = rotation;
                WeaponSegment.Rotation %= 360;

                //WeaponSegment.Active = true;
            }
            else
            {
                //WeaponSegment.Active = false;
            }

            WeaponSegment.RenderLayer = GetComponent<SpriteRenderer>().Segments[0].RenderLayer + (IsLeft ? -1 : 1);
            WeaponSegment.Offset.X = WeaponOffset.X * (IsLeft ? -1 : 1);
            WeaponSegment.RotationCenterOffset.X = WeaponRotationOffset.X * (IsLeft ? -1 : 1);
            WeaponSegment.VerticalFlip = IsLeft;

            //Override dead appearance from base living object
            WeaponSegment.Active = !health.IsDead();

            //Show body
            spriteRenderer.Segments[0].Active = true;
            spriteRenderer.Segments[0].Color = health.IsDead() ? new Vector4(1, 1, 1, 0.3) : Vector4.ONE;

            //Hide skull
            spriteRenderer.Segments[3].Active = false;

            //Hide name if dead
            GetComponent<TextRenderer>().Segments[0].Active = !health.IsDead();
        }

        void TriggerAttackAnimation()
        {
            LastAttackTime = DateTime.Now;
        }
    }
}

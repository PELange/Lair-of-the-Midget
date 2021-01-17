using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects.Player
{
    public class PlayerBaseClient : LivingObjectClient
    {
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

        public override void OnUpdate(double deltaTime)
        {
            base.OnUpdate(deltaTime);

            var deltaSinceAttackStart = (DateTime.Now - LastAttackTime).TotalMilliseconds;

            double attackAnimationTime = 500;
            double attackAnimationTotalSwingDegrees = 220 * (IsLeft ? -1 : 1);
            double attackAnimationSwingRotationOffser = -110 * (IsLeft ? -1 : 1);

            AttackAnimationProgress = Math.Max(0, Math.Min(1.0, deltaSinceAttackStart / attackAnimationTime));

            if (AttackAnimationProgress >= 0 && AttackAnimationProgress <= 1.0)
            {
                //From half of the animation start reversing it
                var rotation = attackAnimationTotalSwingDegrees * 2 * (AttackAnimationProgress < 0.5 ? AttackAnimationProgress : 1 - AttackAnimationProgress) + attackAnimationSwingRotationOffser;

                WeaponSegment.Rotation = rotation;
                WeaponSegment.Rotation %= 360;
            }

            WeaponSegment.RenderLayer = GetComponent<SpriteRenderer>().Segments[0].RenderLayer + (IsLeft ? -1 : 1);
            WeaponSegment.Offset.X = WeaponOffset.X * (IsLeft ? -1 : 1);
            WeaponSegment.RotationCenterOffset.X = WeaponRotationOffset.X * (IsLeft ? -1 : 1);
            WeaponSegment.VerticalFlip = IsLeft;
        }

        public void TriggerAttack()
        {
            LastAttackTime = DateTime.Now;
        }
    }
}

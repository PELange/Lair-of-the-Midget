using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects.Player
{
    public class PlayerBaseClient : LivingObjectClient
    {
        protected int CurrentAnimationPhase { get; set; }
        protected double AnimationTimer { get; set; }

        public PlayerBaseClient(int networkId, string name, ObjectType type, Vector2 position, Vector2 scale, double health)
            : base(networkId, type, position, scale, new BoundingBox(0, 0.5, 1, 0.5), health)
        {
            AddComponent(new PlayerInfo(name));

            if (Type == ObjectType.Player_Wizard_Male)
            {
                Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
                {
                    new SpriteRenderer.Segment(AssetManager.GetSprite($"wizzard_m_idle_anim_f{0}"))
                }));
            }
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld word)
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

                health.Value = objectHealthUpdate.Health;
            }

            networkSynchronization.PacketsInbound.Clear();
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

                if (Type == ObjectType.Player_Wizard_Male)
                {
                    spriteRenderer.Segments[0].Sprite = AssetManager.GetSprite($"wizzard_m_idle_anim_f{CurrentAnimationPhase}");
                }
            }
        }
    }
}

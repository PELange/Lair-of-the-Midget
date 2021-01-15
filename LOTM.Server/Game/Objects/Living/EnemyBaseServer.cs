using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Server.Game.Objects.Living
{
    public class EnemyBaseServer : LivingObjectServer
    {
        public EnemyBaseServer(int objectId, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            var health = GetComponent<Health>();

            health.DeplateHealthAbsolute(5 * deltaTime);

            GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectHealthUpdate
            {
                ObjectId = ObjectId,
                Health = health.CurrentHealth,
            });
        }
    }
}

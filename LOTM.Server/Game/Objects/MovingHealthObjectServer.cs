using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Server.Game.Objects
{
    public class MovingHealthObjectServer : MovingHealthObject
    {
        public MovingHealthObjectServer(MovingHealthObjectType type, Vector2 position, Vector2 scale, double health)
            : base(type, position, scale, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //Process inbound packets
            networkSynchronization.PacketsInbound.Clear();

            //Process outbound packets
            if (networkSynchronization.StateSyncFlags.Contains("position"))
            {
                var transformation = GetComponent<Transformation2D>();

                networkSynchronization.PacketsOutbound.Enqueue(new ObjectPositionUpdate
                {
                    PositionX = transformation.Position.X,
                    PositionY = transformation.Position.Y,
                });
            }

            if (networkSynchronization.StateSyncFlags.Contains("health"))
            {
                var health = GetComponent<Health>();

                networkSynchronization.PacketsOutbound.Enqueue(new ObjectHealthUpdate
                {
                    Health = health.Value
                });
            }

            networkSynchronization.StateSyncFlags.Clear();

            //var transform = GetComponent<Transformation2D>();

            //networkSynchronization.PacketsOutbound.Enqueue(new MovingHealthObjectUpdate
            //{
            //    Type = Type,
            //    PositionX = transform.Position.X,
            //    PositionY = transform.Position.Y,
            //    ScaleX = transform.Scale.X,
            //    ScaleY = transform.Scale.Y,
            //    Health = GetComponent<Health>().Value,
            //});
        }
    }
}

using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Linq;

namespace LOTM.Server.Game.Objects
{
    public class PlayerBaseServer : MovingHealthObjectServer
    {
        public PlayerBaseServer(MovingHealthObjectType type, Vector2 position, Vector2 scale, double health)
            : base(type, position, scale, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //System.Console.WriteLine($"Processing {networkSynchronization.PacketsInbound.Count} packets.");

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is PlayerInput).OrderByDescending(x => x.Id).FirstOrDefault() is PlayerInput playerInput)
            {
                ApplyPlayerinput(playerInput, deltaTime);
            }

            networkSynchronization.PacketsInbound.Clear();
        }

        protected void ApplyPlayerinput(PlayerInput playerInput, double deltaTime)
        {
            var walkSpeed = 100;
            var walkDirection = Vector2.ZERO;

            if ((playerInput.Inputs & InputType.WALK_UP) != 0)
            {
                walkDirection.Y -= 1;
            }
            else if ((playerInput.Inputs & InputType.WALK_DOWN) != 0)
            {
                walkDirection.Y += 1;
            }

            if ((playerInput.Inputs & InputType.WALK_LEFT) != 0)
            {
                walkDirection.X -= 1;
            }
            else if ((playerInput.Inputs & InputType.WALK_RIGHT) != 0)
            {
                walkDirection.X += 1;
            }

            if (walkDirection.X != 0 || walkDirection.Y != 0)
            {
                //Normalize direction vector
                var magnitude = System.Math.Sqrt(walkDirection.X * walkDirection.X + walkDirection.Y * walkDirection.Y);

                walkDirection.X /= magnitude;
                walkDirection.Y /= magnitude;

                var transformation = GetComponent<Transformation2D>();
                transformation.Position.X += walkDirection.X * walkSpeed * deltaTime;
                transformation.Position.Y += walkDirection.Y * walkSpeed * deltaTime;

                GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectPositionUpdate
                {
                    PositionX = transformation.Position.X,
                    PositionY = transformation.Position.Y,
                });
            }
        }
    }
}

using LOTM.Server.Game.Objects.Living;
using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Linq;

namespace LOTM.Server.Game.Objects
{
    public class PlayerBaseServer : LivingObjectServer
    {
        public PlayerInput LastKownInput { get; set; }

        public PlayerBaseServer(int networkId, string name, ObjectType type, Vector2 position, double health)
            : base(networkId, type, position, new Vector2(16, 32), new Rectangle(0, 0.75, 1, 0.25), health)
        {
            AddComponent(new PlayerInfo(name));
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is PlayerInput).OrderByDescending(x => x.Id).FirstOrDefault() is PlayerInput playerInput)
            {
                LastKownInput = playerInput;
            }

            networkSynchronization.PacketsInbound.Clear();

            if (LastKownInput != null)
            {
                ApplyPlayerinput(LastKownInput, deltaTime, world);
            }
        }

        protected void ApplyPlayerinput(PlayerInput playerInput, double deltaTime, GameWorld world)
        {
            var walkSpeed = 50;
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
                walkDirection.Normalize();

                var transformation = GetComponent<Transformation2D>();
                var desiredPosition = new Vector2(transformation.Position.X + walkDirection.X * walkSpeed * deltaTime, transformation.Position.Y + walkDirection.Y * walkSpeed * deltaTime);

                if (TryMovePosition(desiredPosition, world))
                {
                    var networkSynchronization = GetComponent<NetworkSynchronization>();

                    networkSynchronization.PacketsOutbound.Enqueue(new ObjectPositionUpdate
                    {
                        ObjectId = ObjectId,
                        PositionX = transformation.Position.X,
                        PositionY = transformation.Position.Y,
                    });
                }
            }
        }
    }
}

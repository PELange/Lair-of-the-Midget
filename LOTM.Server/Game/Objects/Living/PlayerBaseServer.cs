using LOTM.Server.Game.Objects.Living;
using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System;
using System.Linq;

namespace LOTM.Server.Game.Objects
{
    public class PlayerBaseServer : LivingObjectServer
    {
        public PlayerInput LastKownInput { get; set; }

        public DateTime LastAttackTime { get; set; }
        protected const double AttackCooldown = 500; //in ms

        public PlayerBaseServer(int networkId, string name, ObjectType type, Vector2 position, double health)
            : base(networkId, type, position, new Vector2(16, 32), new Rectangle(0.2, 0.75, 0.7, 0.25), health)
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

            if ((playerInput.Inputs & InputType.ATTACK) != 0)
            {
                if (LastAttackTime == null || (DateTime.Now - LastAttackTime).TotalMilliseconds > AttackCooldown)
                {
                    LastAttackTime = DateTime.Now;

                    Attack(world);
                }
            }
        }

        protected void Attack(GameWorld world)
        {
            double attackRadius = 32;
            var attackRadiusHalf = attackRadius / 2.0;

            var playerColliderBox = GetComponent<Collider>().AsBoundingBoxes().First();
            var playerCenter = new Vector2(playerColliderBox.X + playerColliderBox.Width / 2.0, playerColliderBox.Y + playerColliderBox.Height / 2.0);

            var worldObjects = world.GetDynamicObjectsInArea(new Rectangle(playerColliderBox.X - attackRadiusHalf, playerColliderBox.Y - attackRadiusHalf, playerColliderBox.Width + attackRadius, playerColliderBox.Height + attackRadius));

            foreach (var enemy in worldObjects.Where(x => x is EnemyBaseServer).Select(x => x as EnemyBaseServer))
            {
                //Check if enemy is affected by attack
                var enemyPosRect = enemy.GetComponent<Collider>().AsBoundingBoxes().First();
                var enemyCenter = new Vector2(enemyPosRect.X + enemyPosRect.Width / 2.0, enemyPosRect.Y + enemyPosRect.Height / 2.0);

                //Cut off bottom part of the attakck range circle to meet visual representation
                if (enemyCenter.Y > playerCenter.Y && (Math.Abs(enemyCenter.X - playerCenter.X) < 16)) continue; //anything below and close on X axis can not be hit
                if (enemyCenter.Y > playerCenter.Y + 16) continue; //anything that is 16 units under the player can also not be hit

                if (DistanceMetrics.Euclidean(playerCenter, enemyCenter) < attackRadius)
                {
                    var enemyHealth = enemy.GetComponent<Health>();

                    if (enemyHealth.DepleteHealthPercentage(0.1))
                    {
                        enemy.GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectHealthUpdate { ObjectId = enemy.ObjectId, Health = enemyHealth.CurrentHealth });
                    }
                }
            }

            GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new AttackStateUpdate { ObjectId = ObjectId, Attacking = true });
        }
    }
}

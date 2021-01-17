using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Server.Game.Objects.Living
{
    public class EnemyBaseServer : LivingObjectServer
    {
        protected PlayerBaseServer AggroTarget { get; set; }
        protected double WalkSpeed { get; set; }
        protected double AggroRadius { get; set; }
        protected double Damage { get; set; }

        public EnemyBaseServer(int objectId, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
            WalkSpeed = 16;
            AggroRadius = 16 * 3;
            Damage = 0.1; //10% per second
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            HandleEnemyAi(deltaTime, world);
        }

        void HandleEnemyAi(double deltaTime, GameWorld world)
        {
            if (GetComponent<Health>().IsDead())
            {
                AggroTarget = null; //Enemy died
            }
            else if (AggroTarget == null) //No target, find one
            {
                AggroTarget = FindTarget(world);
            }
            else if (AggroTarget.GetComponent<Health>().IsDead()) //Enemy has target but it died, find new one
            {
                AggroTarget = null;
            }
            else
            {
                //Try to hit the target from where enemy is right now
                if (!TryHitTarget(AggroTarget, deltaTime))
                {
                    //If it was not possible we must get closer to the target
                    TryReachTarget(AggroTarget, deltaTime, world);
                }
            }
        }

        PlayerBaseServer FindTarget(GameWorld world)
        {
            var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
            var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);
            var visibleRect = new Rectangle(enemyCenter.X - AggroRadius / 2.0, enemyCenter.Y - AggroRadius / 2, AggroRadius, AggroRadius);

            var visibleValidTargets = new List<PlayerBaseServer>();
            foreach (var worldObject in world.GetDynamicObjectsInArea(visibleRect))
            {
                if (worldObject is PlayerBaseServer playerBaseServer && !playerBaseServer.GetComponent<Health>().IsDead())
                {
                    visibleValidTargets.Add(playerBaseServer);
                }
            }

            return visibleValidTargets.OrderBy(player =>
            {
                var playerCollider = player.GetComponent<Collider>().AsBoundingBoxes().First();

                var playerCenter = new Vector2(playerCollider.X + playerCollider.Width / 2.0, playerCollider.Y + playerCollider.Height / 2.0);

                return DistanceMetrics.EuclideanSquared(enemyCenter, playerCenter);
            }).FirstOrDefault();
        }

        bool TryReachTarget(PlayerBaseServer target, double deltaTime, GameWorld world)
        {
            var transformation = GetComponent<Transformation2D>();
            var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
            var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

            var playerCollider = target.GetComponent<Collider>().AsBoundingBoxes().First();
            var targetCenter = new Vector2(playerCollider.X + playerCollider.Width / 2.0, playerCollider.Y + playerCollider.Height / 2.0);

            var playerTransform = target.GetComponent<Transformation2D>();
            var walkDirection = new Vector2(targetCenter.X - enemyCenter.X, targetCenter.Y - enemyCenter.Y);

            if (walkDirection.X == 0 && walkDirection.Y == 0)
            {
                //Already reached the target position
                return true;
            }

            //Normalize to be multiplyable by walkspeed and avoid faster movement diagonally
            walkDirection.Normalize();

            var nextPosition = new Vector2(transformation.Position.X + walkDirection.X * WalkSpeed * deltaTime, transformation.Position.Y + walkDirection.Y * WalkSpeed * deltaTime);

            if (walkDirection.X > 0)
            {
                nextPosition.X = System.Math.Min(playerTransform.Position.X, nextPosition.X);
            }
            else if (walkDirection.X < 0)
            {
                nextPosition.X = System.Math.Max(playerTransform.Position.X, nextPosition.X);
            }

            if (walkDirection.Y > 0)
            {
                nextPosition.Y = System.Math.Min(playerTransform.Position.Y, nextPosition.Y);
            }
            else if (walkDirection.Y < 0)
            {
                nextPosition.Y = System.Math.Max(playerTransform.Position.Y, nextPosition.Y);
            }

            if (!TryMovePosition(nextPosition, world, false))
            {
                //Get updated values after enemy movement
                posRect = GetComponent<Collider>().AsBoundingBoxes().First();
                enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

                var missingStep = new Vector2(nextPosition.X - transformation.Position.X, nextPosition.Y - transformation.Position.Y);
                var remaininDirection = new Vector2(targetCenter.X - enemyCenter.X, targetCenter.Y - enemyCenter.Y);

                //System.Console.WriteLine($"{System.DateTime.Now} <{missingStep.X};{missingStep.Y}>");

                if (missingStep.Y != 0)
                {
                    nextPosition.X += 1 * WalkSpeed * deltaTime;
                    nextPosition.Y = transformation.Position.Y;
                }
                else if (missingStep.X != 0)
                {
                    nextPosition.Y += 1 * WalkSpeed * deltaTime;
                    nextPosition.X = transformation.Position.X;
                }

                TryMovePosition(nextPosition, world);
            }

            var networkSynchronization = GetComponent<NetworkSynchronization>();

            networkSynchronization.PacketsOutbound.Enqueue(new ObjectPositionUpdate
            {
                ObjectId = ObjectId,
                PositionX = transformation.Position.X,
                PositionY = transformation.Position.Y,
            });

            return true;
        }

        bool TryHitTarget(PlayerBaseServer target, double deltaTime)
        {
            foreach (var enemyBbox in GetComponent<Collider>().AsBoundingBoxes())
            {
                foreach (var targetBbox in target.GetComponent<Collider>().AsBoundingBoxes())
                {
                    if (enemyBbox.IntersectsWith(targetBbox))
                    {
                        //Enemy is in contact with the target and can now damage him

                        //Deplate target health 
                        var targetHealth = target.GetComponent<Health>();
                        if (targetHealth.DepleteHealthPercentage(Damage * deltaTime))
                        {
                            target.GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectHealthUpdate { ObjectId = target.ObjectId, Health = targetHealth.CurrentHealth });
                        }

                        //TEST TODO REMOVE Deplate own health
                        var ownHealth = GetComponent<Health>();
                        if (ownHealth.DepleteHealthPercentage(0.5 * deltaTime))
                        {
                            GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectHealthUpdate { ObjectId = ObjectId, Health = ownHealth.CurrentHealth });
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}

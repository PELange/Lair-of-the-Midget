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

        public Vector2 AxisMovementForce { get; set; }
        public Vector2 AxisMovementUnlockCondition { get; set; }

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
                    var targetTransformation = AggroTarget.GetComponent<Transformation2D>();
                    var playerCollider = AggroTarget.GetComponent<Collider>().AsBoundingBoxes().First();
                    var targetCenter = new Vector2(playerCollider.X + playerCollider.Width / 2.0, playerCollider.Y + playerCollider.Height / 2.0);
                    var targetPosition = new Vector2(targetTransformation.Position.X, targetTransformation.Position.Y);

                    var transformation = GetComponent<Transformation2D>();

                    if (AxisMovementForce != null)
                    {
                        var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
                        var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

                        if (AxisMovementForce.X != 0)
                        {
                            targetCenter.X = enemyCenter.X + AxisMovementForce.X;
                            targetPosition.X = transformation.Position.X + AxisMovementForce.X;

                            targetCenter.Y = enemyCenter.Y;
                            targetPosition.Y = transformation.Position.Y;
                        }
                        else if (AxisMovementForce.Y != 0)
                        {
                            targetCenter.X = enemyCenter.X;
                            targetPosition.X = transformation.Position.X;

                            targetCenter.Y = enemyCenter.Y + AxisMovementForce.Y;
                            targetPosition.Y = transformation.Position.Y + AxisMovementForce.Y;
                        }
                    }

                    var posBefore = new Vector2(transformation.Position.X, transformation.Position.Y);

                    if (!TryReachPosition(targetPosition, targetCenter, deltaTime, world, out var missingMovement))
                    {
                        if (AxisMovementForce == null)
                        {
                            var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
                            var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

                            if (missingMovement.X != 0 && System.Math.Abs(missingMovement.X) > System.Math.Abs(missingMovement.Y))
                            {
                                AxisMovementForce = new Vector2(0, targetCenter.Y > enemyCenter.Y ? 1 : -1);
                                AxisMovementUnlockCondition = new Vector2(missingMovement.X > 0 ? 1 : -1, 0);
                            }
                            else if (missingMovement.Y != 0 && System.Math.Abs(missingMovement.Y) > System.Math.Abs(missingMovement.X))
                            {
                                AxisMovementForce = new Vector2(targetCenter.X > enemyCenter.X ? 1 : -1, 0);
                                AxisMovementUnlockCondition = new Vector2(0, missingMovement.Y > 0 ? 1 : -1);
                            }
                        }
                    }

                    if (transformation.Position.X == posBefore.X && transformation.Position.Y == posBefore.Y)
                    {
                        //System.Console.WriteLine($"{System.DateTime.Now} Stuck!");

                        //Target got stuck. Without proper pathfinding it will be hard to get unstuck in a meaningful way.
                        //We rather just wait and sit here and wait for the player to kill the enemey.
                        //If he does not kill the enemy, he can not progress any further :)
                        AggroTarget = null;
                        AxisMovementForce = null;
                        AxisMovementUnlockCondition = null;
                    }

                    if (AxisMovementForce != null)
                    {
                        var testUnlock = new Vector2(transformation.Position.X, transformation.Position.Y);

                        testUnlock.X += AxisMovementUnlockCondition.X;
                        testUnlock.Y += AxisMovementUnlockCondition.Y;

                        //We were able to successfully move into the direction that is considered the unlock condition
                        if (TryMovePosition(testUnlock, world, false))
                        {
                            AxisMovementForce = null;
                            AxisMovementUnlockCondition = null;
                        }
                    }
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

        bool TryReachPosition(Vector2 targetPosition, Vector2 targetCenter, double deltaTime, GameWorld world, out Vector2 missingMovement)
        {
            missingMovement = Vector2.ZERO;

            var transformation = GetComponent<Transformation2D>();
            var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
            var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

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
                nextPosition.X = System.Math.Min(targetPosition.X, nextPosition.X);
            }
            else if (walkDirection.X < 0)
            {
                nextPosition.X = System.Math.Max(targetPosition.X, nextPosition.X);
            }

            if (walkDirection.Y > 0)
            {
                nextPosition.Y = System.Math.Min(targetPosition.Y, nextPosition.Y);
            }
            else if (walkDirection.Y < 0)
            {
                nextPosition.Y = System.Math.Max(targetPosition.Y, nextPosition.Y);
            }

            var success = true;

            if (!TryMovePosition(nextPosition, world, false))
            {
                success = false;

                missingMovement = new Vector2(nextPosition.X - transformation.Position.X, nextPosition.Y - transformation.Position.Y);
            }

            //Sync the position change, be it partial success or full movement
            GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new ObjectPositionUpdate
            {
                ObjectId = ObjectId,
                PositionX = transformation.Position.X,
                PositionY = transformation.Position.Y,
            });

            return success;
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

                        return true;
                    }
                }
            }

            return false;
        }
    }
}

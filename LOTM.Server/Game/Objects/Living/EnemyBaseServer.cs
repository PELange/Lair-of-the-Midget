using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Server.Game.Objects.Living
{
    public class EnemyBaseServer : LivingObjectServer
    {
        protected PlayerBaseServer AggroTarget { get; set; }

        public EnemyBaseServer(int objectId, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            var transformation = GetComponent<Transformation2D>();
            var posRect = GetComponent<Collider>().AsBoundingBoxes().First();
            var enemyCenter = new Vector2(posRect.X + posRect.Width / 2.0, posRect.Y + posRect.Height / 2.0);

            if (AggroTarget == null)
            {
                //Find new target
                var viewRange = 16 * 3;
                var visibleRect = new Rectangle(enemyCenter.X - viewRange / 2.0, enemyCenter.Y - viewRange / 2, viewRange, viewRange);

                var visiblePlayers = new List<PlayerBaseServer>();

                foreach (var worldObject in world.GetDynamicObjectsInArea(visibleRect))
                {
                    if (worldObject is PlayerBaseServer playerBaseServer)
                    {
                        visiblePlayers.Add(playerBaseServer);
                    }
                }

                if (visiblePlayers.Count > 0)
                {
                    var nearestPlayer = visiblePlayers.OrderBy(player =>
                    {
                        var playerCollider = player.GetComponent<Collider>().AsBoundingBoxes().First();

                        var playerCenter = new Vector2(playerCollider.X + playerCollider.Width / 2.0, playerCollider.Y + playerCollider.Height / 2.0);

                        return DistanceMetrics.EuclideanSquared(enemyCenter, playerCenter);
                    }).FirstOrDefault();

                    AggroTarget = nearestPlayer;
                }
            }
            else
            {
                const int walkSpeed = 16;

                var playerCollider = AggroTarget.GetComponent<Collider>().AsBoundingBoxes().First();
                var targetCenter = new Vector2(playerCollider.X + playerCollider.Width / 2.0, playerCollider.Y + playerCollider.Height / 2.0);

                var playerTransform = AggroTarget.GetComponent<Transformation2D>();
                var walkDirection = new Vector2(targetCenter.X - enemyCenter.X, targetCenter.Y - enemyCenter.Y);

                if (walkDirection.X == 0 && walkDirection.Y == 0)
                {
                    //Already reached the target position
                    return;
                }

                //Normalize to be multiplyable by walkspeed and avoid faster movement diagonally
                walkDirection.Normalize();

                var nextPosition = new Vector2(transformation.Position.X + walkDirection.X * walkSpeed * deltaTime, transformation.Position.Y + walkDirection.Y * walkSpeed * deltaTime);

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

                    System.Console.WriteLine($"{System.DateTime.Now} <{missingStep.X};{missingStep.Y}>");

                    if (missingStep.Y != 0)
                    {
                        nextPosition.X += 1 * walkSpeed * deltaTime;
                        nextPosition.Y = transformation.Position.Y;
                    }
                    else if (missingStep.X != 0)
                    {
                        nextPosition.Y += 1 * walkSpeed * deltaTime;
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
            }
        }
    }
}

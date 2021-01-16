using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LOTM.Server.Game.Objects.Living
{
    public class EnemyBaseServer : LivingObjectServer
    {
        protected GameObject AggroTarget { get; set; }

        public EnemyBaseServer(int objectId, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            var transformation = GetComponent<Transformation2D>();

            if (AggroTarget == null)
            {
                //Find new target
                var viewRange = 16 * 3;
                var center = new Vector2(transformation.Position.X + transformation.Scale.X / 2.0, transformation.Position.Y + transformation.Scale.Y / 2.0);
                var visibleRect = new Rectangle(center.X - viewRange / 2.0, center.Y - viewRange / 2, viewRange, viewRange);

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
                        var playerTransform = player.GetComponent<Transformation2D>();

                        var playerCenter = new Vector2(playerTransform.Position.X + playerTransform.Scale.X / 2.0, playerTransform.Position.Y + playerTransform.Scale.Y / 2.0);

                        return DistanceMetrics.EuclideanSquared(center, playerCenter);
                    }).FirstOrDefault();

                    AggroTarget = nearestPlayer;
                }
            }
            else
            {
                const int walkSpeed = 16;

                var aggroTransform = AggroTarget.GetComponent<Transformation2D>();
                var walkDirection = new Vector2(aggroTransform.Position.X - transformation.Position.X, aggroTransform.Position.Y - transformation.Position.Y);

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
                    nextPosition.X = System.Math.Min(aggroTransform.Position.X, nextPosition.X);
                }
                else if (walkDirection.X < 0)
                {
                    nextPosition.X = System.Math.Max(aggroTransform.Position.X, nextPosition.X);
                }

                if (walkDirection.Y > 0)
                {
                    nextPosition.Y = System.Math.Min(aggroTransform.Position.Y, nextPosition.Y);
                }
                else if (walkDirection.Y < 0)
                {
                    nextPosition.Y = System.Math.Max(aggroTransform.Position.Y, nextPosition.Y);
                }

                if (!TryMovePosition(nextPosition, world, false))
                {
                    var missingStep = new Vector2(nextPosition.X - transformation.Position.X, nextPosition.Y - transformation.Position.Y);
                    var remaininDirection = new Vector2(aggroTransform.Position.X - transformation.Position.X, aggroTransform.Position.Y - transformation.Position.Y);

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

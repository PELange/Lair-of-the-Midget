using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Objects;

namespace LOTM.Server.Game.Objects.Living
{
    public class LivingObjectServer : LivingObject
    {
        public LivingObjectServer(int networkId, ObjectType type, Vector2 position = default, Vector2 scale = default, BoundingBox colliderInfo = default, double health = default)
            : base(networkId, type, position, scale, colliderInfo, health)
        {
        }

        protected bool TryMovePosition(Vector2 desiredPosition, GameWorld world, bool allowPartialMovement = true)
        {
            var transformation = GetComponent<Transformation2D>();
            var collider = GetComponent<Collider>();

            var desiredDelta = new Vector2(desiredPosition.X - transformation.Position.X, desiredPosition.Y - transformation.Position.Y);

            if (desiredDelta.X == 0 && desiredDelta.Y == 0) return true; //No movement at all

            var possibleDelta = new Vector2(desiredDelta.X, desiredDelta.Y);

            var objectBounds = transformation.GetBoundingBox();

            foreach (var worldObject in world.GetObjectsInArea(new BoundingBox(objectBounds.X + desiredDelta.X, objectBounds.Y + desiredDelta.Y, objectBounds.Width, objectBounds.Height)))
            {
                var objectCollider = worldObject.GetComponent<Collider>();

                if (objectCollider != null)
                {
                    if (collider.CollidesAfterOffsetWith(desiredDelta, objectCollider, out var collisionResult))
                    {
                        if (collisionResult.Overlap.Width <= collisionResult.Overlap.Height) //X axis overlap is smaller, so we need to bounce off -X to be collision free
                        {
                            if (desiredDelta.X > 0) //Delta was desired to be positive change on X axis. Overlap reduces change down to zero
                            {
                                possibleDelta.X = System.Math.Min(possibleDelta.X, possibleDelta.X - collisionResult.Overlap.Width);
                            }
                            else //Delta was desired to be negative change on X axis. Overlap reduces change back up to 0
                            {
                                possibleDelta.X = System.Math.Max(possibleDelta.X, possibleDelta.X + collisionResult.Overlap.Width);
                            }
                        }

                        if (collisionResult.Overlap.Height <= collisionResult.Overlap.Width)//Y axis overlap is smaller, so we need to bounce off -Y to be collision free
                        {
                            if (desiredDelta.Y > 0) //Delta was desired to be positive change on Y axis. Overlap reduces change down to zero
                            {
                                possibleDelta.Y = System.Math.Min(possibleDelta.Y, possibleDelta.Y - collisionResult.Overlap.Height);
                            }
                            else //Delta was desired to be negative change on Y axis. Overlap reduces change back up to 0
                            {
                                possibleDelta.Y = System.Math.Max(possibleDelta.Y, possibleDelta.Y + collisionResult.Overlap.Height);
                            }
                        }

                        //In case that the overlap is identical on both acis, we bounce back equally on box axis as perfect corner collision
                    }
                }
            }

            //Some collision makes it impossible to achieve the desired position channge. If only full collision free movement was allowed abort here
            if ((possibleDelta.X != desiredDelta.X || possibleDelta.Y != desiredDelta.Y) && !allowPartialMovement) return false;

            //No collision was detected and hence the desiredDelta == possibleDelta, or we allow partial movement to as much delta as possible

            transformation.Position.X = transformation.Position.X + possibleDelta.X;
            transformation.Position.Y = transformation.Position.Y + possibleDelta.Y;

            return true;
        }
    }
}

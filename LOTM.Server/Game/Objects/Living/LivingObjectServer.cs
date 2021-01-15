using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Interactable;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Server.Game.Objects.Living
{
    public class LivingObjectServer : LivingObject
    {
        public LivingObjectServer(int objectId, ObjectType type, Vector2 position = default, Vector2 scale = default, Rectangle colliderInfo = default, double health = default)
            : base(objectId, type, position, scale, colliderInfo, health)
        {
        }

        protected bool TryMovePosition(Vector2 desiredPosition, GameWorld world, bool allowPartialMovement = true)
        {
            var transformation = GetComponent<Transformation2D>();
            var collider = GetComponent<Collider>();
            var objectBounds = collider.AsBoundingBoxes().First(); //Optimzation based on the assumtion that living objects only have one convex collider bbox
            var objectBoundsCenter = new Vector2(objectBounds.X + objectBounds.Width / 2, objectBounds.Y + objectBounds.Height / 2);

            var desiredDelta = new Vector2(desiredPosition.X - transformation.Position.X, desiredPosition.Y - transformation.Position.Y);
            var possibleDelta = new Vector2(desiredDelta.X, desiredDelta.Y);

            //Rect from from current topleft to desired bottomright
            var collisionDetectionBounds = new Rectangle(
                System.Math.Min(objectBounds.X, objectBounds.X + desiredDelta.X),
                System.Math.Min(objectBounds.Y, objectBounds.Y + desiredDelta.Y),
                System.Math.Max(objectBounds.Width, objectBounds.Width + desiredDelta.X),
                System.Math.Max(objectBounds.Height, objectBounds.Height + desiredDelta.Y));

            var collisions = new List<(Rectangle, double)>();

            //Get all colliders that are in the 
            foreach (var worldObject in world.GetObjectsInArea(collisionDetectionBounds))
            {
                if (worldObject == this) continue; //Avoid self collision

                if (worldObject is EnemyBaseServer || worldObject is PlayerBaseServer || worldObject is Pickup) continue; //No movement collision between enemies <-> enemies, player <-> enemy, player <-> player, player <-> pickup

                var objectCollider = worldObject.GetComponent<Collider>();

                if (objectCollider != null)
                {
                    //Foreach found collider box
                    foreach (var rect in objectCollider.AsBoundingBoxes())
                    {
                        //Enlarge boxes by half the dimensions of the object that wants to move to catch tunneling and find the correct position to slide along walls
                        rect.X -= objectBounds.Width * 0.5;
                        rect.Width += objectBounds.Width;

                        rect.Y -= objectBounds.Height * 0.5;
                        rect.Height += objectBounds.Height;

                        //var collisionRay = new Ray(objectBounds.X + objectBounds.Width / 2, objectBounds.Y + objectBounds.Height / 2, possibleDelta.X, possibleDelta.Y);

                        //if (rect.IntersectsWith(collisionRay, out var contactPoint, out var contactNormal, out var contactTime))
                        //{
                        //    //Store collision rects to be able to later sort them by time of contact
                        //    collisions.Add((rect, contactTime));
                        //}

                        //Optimzation, instead of performing the collision to accurately know which collider we hit first, we approximate based on distance between both centers
                        //Distance metric: Squared Euclidian
                        var rectCenterX = rect.X + rect.Width * 0.5;
                        var rectCenterY = rect.Y + rect.Height * 0.5;

                        collisions.Add((rect, (objectBoundsCenter.X - rectCenterX) * (objectBoundsCenter.X - rectCenterX) + (objectBoundsCenter.Y - rectCenterY) * (objectBoundsCenter.Y - rectCenterY)));
                    }
                }
            }

            foreach (var collision in collisions.OrderBy(x => x.Item2))
            {
                var collisionRay = new Ray(objectBounds.X + objectBounds.Width * 0.5, objectBounds.Y + objectBounds.Height * 0.5, possibleDelta.X, possibleDelta.Y);

                if (collision.Item1.IntersectsWith(collisionRay, out var contactPoint, out var contactNormal, out var contactTime))
                {
                    if (contactNormal.X != 0) possibleDelta.X = 0;
                    if (contactNormal.Y != 0) possibleDelta.Y = 0;

                    //possibleDelta.X += contactNormal.X * System.Math.Abs(possibleDelta.X) * (1 - contactTime);
                    //possibleDelta.Y += contactNormal.Y * System.Math.Abs(possibleDelta.Y) * (1 - contactTime);
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

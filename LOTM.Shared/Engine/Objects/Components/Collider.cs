using LOTM.Shared.Engine.Math;

namespace LOTM.Shared.Engine.Objects.Components
{
    public class Collider : IComponent
    {
        public GameObject Parent { get; }
        public BoundingBox Offset { get; }

        public bool Active { get; set; }

        public BoundingBox AsBoundingBox()
        {
            var transform = Parent.GetComponent<Transformation2D>();

            return new BoundingBox(
                transform.Position.X + (transform.Scale.X * Offset.X),
                transform.Position.Y + (transform.Scale.Y * Offset.Y),
                transform.Scale.X * Offset.Width,
                transform.Scale.Y * Offset.Height);
        }

        public Collider(GameObject parent, BoundingBox boundingInfo)
        {
            Parent = parent;
            Offset = boundingInfo;
            Active = true;
        }

        public bool CollidesWith(Collider other, out CollisionResult collisionResult)
        {
            collisionResult = default;

            if (this == other) return false; //Avoid self collision

            if (!Active || !other.Active) return false; //Avoid inactive collider evaluation

            var thisBox = AsBoundingBox();
            var otherBox = other.AsBoundingBox();

            var intersection = thisBox.GetInsectionArea(otherBox);

            if (intersection != null)
            {
                collisionResult = new CollisionResult(intersection);
                return true;
            }

            return false;
        }

        public bool CollidesAfterOffsetWith(Vector2 offset, Collider other, out CollisionResult collisionResult)
        {
            collisionResult = default;

            if (this == other) return false; //Avoid self collision

            if (!Active || !other.Active) return false; //Avoid inactive collider evaluation

            var thisBox = AsBoundingBox();
            var otherBox = other.AsBoundingBox();

            var intersection = new BoundingBox(thisBox.X + offset.X, thisBox.Y + offset.Y, thisBox.Width, thisBox.Height).GetInsectionArea(otherBox);

            if (intersection != null)
            {
                collisionResult = new CollisionResult(intersection);
                return true;
            }

            return false;
        }

        public class CollisionResult
        {
            public BoundingBox Overlap { get; }

            public CollisionResult(BoundingBox overlap)
            {
                Overlap = overlap;
            }
        }
    }
}

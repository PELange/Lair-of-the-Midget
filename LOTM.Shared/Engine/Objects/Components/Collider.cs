using LOTM.Shared.Engine.Math;

namespace LOTM.Shared.Engine.Objects.Components
{
    public class Collider : IComponent
    {
        public GameObject Parent { get; }
        public BoundingBox Offset { get; }

        public Collider(GameObject parent, BoundingBox boundingInfo)
        {
            Parent = parent;
            Offset = boundingInfo;
        }

        public bool CollidesWith(Collider other, out CollisionResult collisionResult)
        {
            collisionResult = default;

            var transform = Parent.GetComponent<Transformation2D>();
            var otherTransform = other.Parent.GetComponent<Transformation2D>();

            var thisBox = new BoundingBox(
                transform.Position.X + (transform.Scale.X * Offset.X),
                transform.Position.Y + (transform.Scale.Y * Offset.Y),
                transform.Scale.X * Offset.Width,
                transform.Scale.Y * Offset.Height);

            var otherBox = new BoundingBox(
                otherTransform.Position.X + (otherTransform.Scale.X * other.Offset.X),
                otherTransform.Position.Y + (otherTransform.Scale.Y * other.Offset.Y),
                otherTransform.Scale.X * other.Offset.Width,
                otherTransform.Scale.Y * other.Offset.Height);

            var maxLeft = System.Math.Max(thisBox.X, otherBox.X);
            var minRight = System.Math.Min(thisBox.X + thisBox.Width, otherBox.X + otherBox.Width);
            var maxTop = System.Math.Max(thisBox.Y, otherBox.Y);
            var minBottom = System.Math.Min(thisBox.Y + thisBox.Height, otherBox.Y + otherBox.Height);

            if (maxLeft < minRight && maxTop < minBottom)
            {
                collisionResult = new CollisionResult(new BoundingBox(maxLeft, maxTop, minRight - maxLeft, minBottom - maxTop));
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

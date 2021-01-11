using LOTM.Shared.Engine.Math;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Shared.Engine.Objects.Components
{
    public class Collider : IComponent
    {
        protected GameObject Parent { get; }
        protected List<BoundingBox> Meshes { get; }

        public bool Active { get; set; }

        public List<BoundingBox> AsBoundingBoxes()
        {
            var transform = Parent.GetComponent<Transformation2D>();

            return Meshes.Select(mesh => new BoundingBox(
                transform.Position.X + (transform.Scale.X * mesh.X),
                transform.Position.Y + (transform.Scale.Y * mesh.Y),
                transform.Scale.X * mesh.Width,
                transform.Scale.Y * mesh.Height)).ToList();
        }

        public Collider(GameObject parent, BoundingBox boundingInfo)
            : this(parent, new List<BoundingBox> { boundingInfo })
        {
        }

        public Collider(GameObject parent, List<BoundingBox> colliderBoxes)
        {
            Parent = parent;
            Meshes = colliderBoxes;
            Active = true;
        }

        public bool CollidesWith(Collider other, out CollisionResult collisionResult)
        {
            return CollidesAfterOffsetWith(Vector2.ZERO, other, out collisionResult);
        }

        public bool CollidesAfterOffsetWith(Vector2 offset, Collider other, out CollisionResult collisionResult)
        {
            collisionResult = default;

            if (other == null) return false; //Collsion with non existent collider

            if (this == other) return false; //Avoid self collision

            if (!Active || !other.Active) return false; //Avoid inactive collider evaluation

            foreach (var thisBox in AsBoundingBoxes())
            {
                foreach (var otherBox in other.AsBoundingBoxes())
                {
                    var intersection = new BoundingBox(thisBox.X + offset.X, thisBox.Y + offset.Y, thisBox.Width, thisBox.Height).GetIntersectionArea(otherBox);

                    if (intersection != null)
                    {
                        collisionResult = new CollisionResult(intersection);
                        return true;
                    }
                }
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

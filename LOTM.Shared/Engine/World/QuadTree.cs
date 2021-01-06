using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Shared.Engine.World
{
    public class QuadTree
    {
        protected List<GameObject> Objects { get; set; } // The list holding all objects in this QuadTree
        protected BoundingBox BoundingRect { get; set; } // The area this QuadTree represents

        protected QuadTree ChildTL { get; set; } // Top Left Child
        protected QuadTree ChildTR { get; set; } // Top Right Child
        protected QuadTree ChildBL { get; set; } // Bottom Left Child
        protected QuadTree ChildBR { get; set; } // Bottom Right Child

        public QuadTree(BoundingBox rect)
        {
            BoundingRect = rect;
            Objects = new List<GameObject>();
        }

        protected int Count
        {
            get
            {
                int count = 0;

                // Add the objects at this level
                if (Objects != null)
                {
                    count += Objects.Count;
                }

                // Add the objects that are contained in the children
                if (ChildTL != null)
                {
                    count += ChildTL.Count;
                    count += ChildTR.Count;
                    count += ChildBL.Count;
                    count += ChildBR.Count;
                }

                return count;
            }
        }

        private void Split()
        {
            // When max capacity is reached, split the tree
            var size = new Vector2(BoundingRect.Width / 2, BoundingRect.Height / 2);
            var mid = new Vector2(BoundingRect.X + size.X, BoundingRect.Y + size.Y);

            ChildTL = new QuadTree(new BoundingBox(BoundingRect.X, BoundingRect.Y, size.X, size.Y));
            ChildTR = new QuadTree(new BoundingBox(mid.X, BoundingRect.Y, size.X, size.Y));
            ChildBL = new QuadTree(new BoundingBox(BoundingRect.X, mid.Y, size.X, size.Y));
            ChildBR = new QuadTree(new BoundingBox(mid.X, mid.Y, size.X, size.Y));

            // If they're completely contained by the quad, bump objects down
            for (int i = 0; i < Objects.Count; i++)
            {
                var destTree = GetDestinationTree(Objects[i]);

                if (destTree != this)
                {
                    // Insert to the appropriate tree, remove the object, and back up one in the loop
                    destTree.Add(Objects[i]);
                    Objects.Remove(Objects[i]);
                    i--;
                }
            }
        }

        private QuadTree GetDestinationTree(GameObject item)
        {
            // If a child can't contain an object, it will live in this Quad
            var destTree = this;

            var boundingBox = item.GetComponent<Transformation2D>().GetBoundingBox();

            if (ChildTL.BoundingRect.Contains(boundingBox))
            {
                destTree = ChildTL;
            }
            else if (ChildTR.BoundingRect.Contains(boundingBox))
            {
                destTree = ChildTR;
            }
            else if (ChildBL.BoundingRect.Contains(boundingBox))
            {
                destTree = ChildBL;
            }
            else if (ChildBR.BoundingRect.Contains(boundingBox))
            {
                destTree = ChildBR;
            }

            return destTree;
        }

        public void Clear()
        {
            // Clear out the children, if we have any
            if (ChildTL != null)
            {
                ChildTL.Clear();
                ChildTR.Clear();
                ChildBL.Clear();
                ChildBR.Clear();
            }

            // Clear any objects at this level
            if (Objects != null)
            {
                Objects.Clear();
                Objects = null;
            }

            // Set the children to null
            ChildTL = null;
            ChildTR = null;
            ChildBL = null;
            ChildBR = null;
        }

        public void Delete(GameObject item)
        {
            // If this level contains the object, remove it
            bool objectRemoved = false;

            if (Objects != null && Objects.Contains(item))
            {
                Objects.Remove(item);
                objectRemoved = true;
            }

            // If we didn't find the object in this tree, try to delete from its children
            if (ChildTL != null && !objectRemoved)
            {
                ChildTL.Delete(item);
                ChildTR.Delete(item);
                ChildBL.Delete(item);
                ChildBR.Delete(item);
            }

            if (ChildTL != null)
            {
                // If all the children are empty, delete all the children
                if (ChildTL.Count == 0 &&
                    ChildTR.Count == 0 &&
                    ChildBL.Count == 0 &&
                    ChildBR.Count == 0)
                {
                    ChildTL = null;
                    ChildTR = null;
                    ChildBL = null;
                    ChildBR = null;
                }
            }
        }

        public void Add(GameObject item)
        {
            // If this quad doesn't intersect the items bounds, do nothing
            if (!BoundingRect.IntersectsWith(item.GetComponent<Transformation2D>().GetBoundingBox()))
            {
                return; //Todo instead of skip, resize quadtree and rebuild it?
            }

            if (Objects == null || (ChildTL == null && Objects.Count + 1 < 4)) //As soon as we hit 3 items we subdevide
            {
                // If there's room to add the object, just add it
                Objects.Add(item);
            }
            else
            {
                // No quads, create them and bump objects down where appropriate
                if (ChildTL == null)
                {
                    Split();
                }

                // Find out which tree this object should go in and add it there
                var destTree = GetDestinationTree(item);
                if (destTree == this)
                {
                    Objects.Add(item);
                }
                else
                {
                    destTree.Add(item);
                }
            }
        }

        public List<GameObject> GetObjects(BoundingBox rect)
        {
            var results = new List<GameObject>();

            // If the search area completely contains this quad, just get every object this quad and all it's children have
            if (rect.Contains(BoundingRect))
            {
                results.AddRange(GetAllObjects());
            }
            // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search RectangleF
            else if (rect.IntersectsWith(BoundingRect))
            {
                if (Objects != null)
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        if (rect.IntersectsWith(Objects[i].GetComponent<Transformation2D>().GetBoundingBox()))
                        {
                            results.Add(Objects[i]);
                        }
                    }
                }

                if (ChildTL != null)
                {
                    results.AddRange(ChildTL.GetObjects(rect));
                    results.AddRange(ChildTR.GetObjects(rect));
                    results.AddRange(ChildBL.GetObjects(rect));
                    results.AddRange(ChildBR.GetObjects(rect));
                }
            }

            return results;

        }

        public List<GameObject> GetAllObjects()
        {
            var results = new List<GameObject>();

            if (Objects != null)
                results.AddRange(Objects);

            if (ChildTL != null)
            {
                results.AddRange(ChildTL.GetAllObjects());
                results.AddRange(ChildTR.GetAllObjects());
                results.AddRange(ChildBL.GetAllObjects());
                results.AddRange(ChildBR.GetAllObjects());
            }

            return results;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;

namespace LOTM.Shared.Engine.World
{
    // <summary>
    /// Interface to define Rect, so that QuadTree knows how to store the object.
    /// </summary>
    public interface IHasRectangle
    {
        /// <summary>
        /// The rectangle that defines the object's boundaries.
        /// </summary>
        Rectangle Rect { get; }
    }

    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of objects in a world space.
    /// </summary>
    /// <typeparam name="T">Any object iheriting from IHasRect.</typeparam>
    public class QuadTree<T> where T : IHasRectangle
    {
        // How many objects can exist in a QuadTree before it sub divides itself
        private const int MAX_OBJECTS_PER_NODE = 2;
        public List<T> Objects { get; set; } // The list holding all objects in this QuadTree
        public Rectangle BoundingRect { get; set; } // The area this QuadTree represents

        public QuadTree<T> ChildTL { get; set; } // Top Left Child
        public QuadTree<T> ChildTR { get; set; } // Top Right Child
        public QuadTree<T> ChildBL { get; set; } // Bottom Left Child
        public QuadTree<T> ChildBR { get; set; } // Bottom Right Child
        public int Count { get { return this.ObjectCount(); } } // How many total objects are contained within this QuadTree (includes children)

        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        public QuadTree(Rectangle rect)
        {
            this.BoundingRect = rect;
        }

        /// <summary>
        /// Add an item to the object list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        private void AddItem(T item)
        {
            if (Objects == null)
                Objects = new List<T>();

            Objects.Add(item);
        }

        /// <summary>
        /// Remove an item from the object list.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        private void Remove(T item)
        {
            if (Objects != null && Objects.Contains(item))
                Objects.Remove(item);
        }

        /// <summary>
        /// Get the total for all objects in this QuadTree, including children.
        /// </summary>
        private int ObjectCount()
        {
            int count = 0;

            // Add the objects at this level
            if (Objects != null) count += Objects.Count;

            // Add the objects that are contained in the children
            if (ChildTL != null)
            {
                count += ChildTL.ObjectCount();
                count += ChildTR.ObjectCount();
                count += ChildBL.ObjectCount();
                count += ChildBR.ObjectCount();
            }

            return count;
        }

        /// <summary>
        /// Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
        /// </summary>
        private void Split()
        {
            // When max capacity is reached, split the tree
            Point size = new Point(BoundingRect.Width / 2, BoundingRect.Height / 2);
            Point mid = new Point(BoundingRect.X + size.X, BoundingRect.Y + size.Y);

            ChildTL = new QuadTree<T>(new Rectangle(BoundingRect.Left, BoundingRect.Top, size.X, size.Y));
            ChildTR = new QuadTree<T>(new Rectangle(mid.X, BoundingRect.Top, size.X, size.Y));
            ChildBL = new QuadTree<T>(new Rectangle(BoundingRect.Left, mid.Y, size.X, size.Y));
            ChildBR = new QuadTree<T>(new Rectangle(mid.X, mid.Y, size.X, size.Y));

            // If they're completely contained by the quad, bump objects down
            for (int i = 0; i < Objects.Count; i++)
            {
                QuadTree<T> destTree = GetDestinationTree(Objects[i]);

                if (destTree != this)
                {
                    // Insert to the appropriate tree, remove the object, and back up one in the loop
                    destTree.Add(Objects[i]);
                    Remove(Objects[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Get the child Quad that would contain an object.
        /// </summary>
        /// <param name="item">The object to get a child for.</param>
        private QuadTree<T> GetDestinationTree(T item)
        {
            // If a child can't contain an object, it will live in this Quad
            QuadTree<T> destTree = this;

            if (ChildTL.BoundingRect.Contains(item.Rect))
            {
                destTree = ChildTL;
            }
            else if (ChildTR.BoundingRect.Contains(item.Rect))
            {
                destTree = ChildTR;
            }
            else if (ChildBL.BoundingRect.Contains(item.Rect))
            {
                destTree = ChildBL;
            }
            else if (ChildBR.BoundingRect.Contains(item.Rect))
            {
                destTree = ChildBR;
            }

            return destTree;
        }

        /// <summary>
        /// Clears the QuadTree of all objects, including any objects living in its children.
        /// </summary>
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

        /// <summary>
        /// Deletes an item from this QuadTree. If the object is removed causes this Quad to have no objects in its children, it's children will be removed as well.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Delete(T item)
        {
            // If this level contains the object, remove it
            bool objectRemoved = false;
            if (Objects != null && Objects.Contains(item))
            {
                Remove(item);
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

        /// <summary>
        /// Adds an item into this QuadTree.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        public void Add(T item)
        {
            // If this quad doesn't intersect the items rectangle, do nothing
            if (!BoundingRect.IntersectsWith(item.Rect))
                return;

            if (Objects == null || (ChildTL == null && Objects.Count + 1 <= MAX_OBJECTS_PER_NODE))
            {
                // If there's room to add the object, just add it
                AddItem(item);
            }
            else
            {
                // No quads, create them and bump objects down where appropriate
                if (ChildTL == null)
                {
                    Split();
                }

                // Find out which tree this object should go in and add it there
                QuadTree<T> destTree = GetDestinationTree(item);
                if (destTree == this)
                {
                    AddItem(item);
                }
                else
                {
                    destTree.Add(item);
                }
            }
        }

        /// <summary>
        /// Get the objects in this tree that intersect with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to find objects in.</param>
        public List<T> GetObjects(Rectangle rect)
        {
            List<T> results = new List<T>();

            // If the search area completely contains this quad, just get every object this quad and all it's children have
            if (rect.Contains(this.BoundingRect))
            {
                results.AddRange(GetAllObjects());
            }
            // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
            else if (rect.IntersectsWith(this.BoundingRect))
            {
                if (Objects != null)
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        if (rect.IntersectsWith(Objects[i].Rect))
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

        /// <summary>
        /// Get all objects in this Quad, and it's children.
        /// </summary>
        public List<T> GetAllObjects()
        {
            List<T> results = new List<T>();
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
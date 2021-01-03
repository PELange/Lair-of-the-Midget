using System;
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

        public List<T> objects = null;       // The list holding all objects in this QuadTree
        public Rectangle rect;               // The area this QuadTree represents

        public QuadTree<T> childTL = null;   // Top Left Child
        public QuadTree<T> childTR = null;   // Top Right Child
        public QuadTree<T> childBL = null;   // Bottom Left Child
        public QuadTree<T> childBR = null;   // Bottom Right Child

        /// <summary>
        /// How many total objects are contained within this QuadTree (ie, includes children)
        /// </summary>
        public int Count { get { return this.ObjectCount(); } }

        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        public QuadTree(Rectangle rect)
        {
            this.rect = rect;
        }
        /// <summary>
        /// Add an item to the object list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        private void AddItem(T item)
        {
            if (objects == null)
                objects = new List<T>();

            objects.Add(item);
        }

        /// <summary>
        /// Remove an item from the object list.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        private void Remove(T item)
        {
            if (objects != null && objects.Contains(item))
                objects.Remove(item);
        }

        /// <summary>
        /// Get the total for all objects in this QuadTree, including children.
        /// </summary>
        /// <returns>The number of objects contained within this QuadTree and its children.</returns>
        private int ObjectCount()
        {
            int count = 0;

            // Add the objects at this level
            if (objects != null) count += objects.Count;

            // Add the objects that are contained in the children
            if (childTL != null)
            {
                count += childTL.ObjectCount();
                count += childTR.ObjectCount();
                count += childBL.ObjectCount();
                count += childBR.ObjectCount();
            }

            return count;
        }

        /// <summary>
        /// Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
        /// </summary>
        private void Subdivide()
        {
            // We've reached capacity, subdivide...
            Point size = new Point(rect.Width / 2, rect.Height / 2);
            Point mid = new Point(rect.X + size.X, rect.Y + size.Y);

            childTL = new QuadTree<T>(new Rectangle(rect.Left, rect.Top, size.X, size.Y));
            childTR = new QuadTree<T>(new Rectangle(mid.X, rect.Top, size.X, size.Y));
            childBL = new QuadTree<T>(new Rectangle(rect.Left, mid.Y, size.X, size.Y));
            childBR = new QuadTree<T>(new Rectangle(mid.X, mid.Y, size.X, size.Y));

            // If they're completely contained by the quad, bump objects down
            for (int i = 0; i < objects.Count; i++)
            {
                QuadTree<T> destTree = GetDestinationTree(objects[i]);

                if (destTree != this)
                {
                    // Insert to the appropriate tree, remove the object, and back up one in the loop
                    destTree.Add(objects[i]);
                    Remove(objects[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Get the child Quad that would contain an object.
        /// </summary>
        /// <param name="item">The object to get a child for.</param>
        /// <returns></returns>
        private QuadTree<T> GetDestinationTree(T item)
        {
            // If a child can't contain an object, it will live in this Quad
            QuadTree<T> destTree = this;

            if (childTL.rect.Contains(item.Rect))
            {
                destTree = childTL;
            }
            else if (childTR.rect.Contains(item.Rect))
            {
                destTree = childTR;
            }
            else if (childBL.rect.Contains(item.Rect))
            {
                destTree = childBL;
            }
            else if (childBR.rect.Contains(item.Rect))
            {
                destTree = childBR;
            }

            return destTree;
        }

        /// <summary>
        /// Clears the QuadTree of all objects, including any objects living in its children.
        /// </summary>
        public void Clear()
        {
            // Clear out the children, if we have any
            if (childTL != null)
            {
                childTL.Clear();
                childTR.Clear();
                childBL.Clear();
                childBR.Clear();
            }

            // Clear any objects at this level
            if (objects != null)
            {
                objects.Clear();
                objects = null;
            }

            // Set the children to null
            childTL = null;
            childTR = null;
            childBL = null;
            childBR = null;
        }

        /// <summary>
        /// Deletes an item from this QuadTree. If the object is removed causes this Quad to have no objects in its children, it's children will be removed as well.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Delete(T item)
        {
            // If this level contains the object, remove it
            bool objectRemoved = false;
            if (objects != null && objects.Contains(item))
            {
                Remove(item);
                objectRemoved = true;
            }

            // If we didn't find the object in this tree, try to delete from its children
            if (childTL != null && !objectRemoved)
            {
                childTL.Delete(item);
                childTR.Delete(item);
                childBL.Delete(item);
                childBR.Delete(item);
            }

            if (childTL != null)
            {
                // If all the children are empty, delete all the children
                if (childTL.Count == 0 &&
                    childTR.Count == 0 &&
                    childBL.Count == 0 &&
                    childBR.Count == 0)
                {
                    childTL = null;
                    childTR = null;
                    childBL = null;
                    childBR = null;
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
            if (!rect.IntersectsWith(item.Rect))
                return;

            if (objects == null ||
                (childTL == null && objects.Count + 1 <= MAX_OBJECTS_PER_NODE))
            {
                // If there's room to add the object, just add it
                AddItem(item);
            }
            else
            {
                // No quads, create them and bump objects down where appropriate
                if (childTL == null)
                {
                    Subdivide();
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
            if (rect.Contains(this.rect))
            {
                results.AddRange(GetAllObjects());
            }
            // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
            else if (rect.IntersectsWith(this.rect))
            {
                if (objects != null)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (rect.IntersectsWith(objects[i].Rect))
                        {
                            results.Add(objects[i]);
                        }
                    }
                }

                if (childTL != null)
                {
                    results.AddRange(childTL.GetObjects(rect));
                    results.AddRange(childTR.GetObjects(rect));
                    results.AddRange(childBL.GetObjects(rect));
                    results.AddRange(childBR.GetObjects(rect));
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
            if (objects != null)
                results.AddRange(objects);

            if (childTL != null)
            {
                results.AddRange(childTL.GetAllObjects());
                results.AddRange(childTR.GetAllObjects());
                results.AddRange(childBL.GetAllObjects());
                results.AddRange(childBR.GetAllObjects());
            }
            return results;

        }
    }

    #region OLD
    //// Delegate to perform an action on a node
    //public delegate void QTAction(QuadTreeNode<T> node);

    //// Root node of this tree
    //public QuadTreeNode<T> rootNode;

    //// Bounds of this tree
    //public RectangleF Bounds;

    //// Maximum object capacity of nodes
    //public int NodeCapacity { get; set; }

    //// Minimum geographical size
    //public int MinNodeSize { get; set; }

    //// Get bounds of an object
    //public Func<T, RectangleF> GetBounds { get; set; }

    //// Get count of the content of rootNode and all subnodes
    ////public int Count { get { return rootNode.Count; } }

    //public QuadTree(RectangleF bounds)
    //{
    //    Bounds = bounds;
    //    rootNode = new QuadTreeNode<T>(bounds, this);
    //}

    //// Get all nodes starting at rootNode
    ////public IEnumerable<QuadTreeNode<T>> SubNodes
    ////{
    ////    get
    ////    {
    ////        yield return rootNode;
    ////        foreach (var node in rootNode.SubTreeNodes)
    ////            yield return node;
    ////    }
    ////}

    //public IEnumerable<QuadTreeNode<T>> SubNodes
    //{
    //    get
    //    {
    //        var list = new List<QuadTreeNode<T>>();
    //        list.Add(rootNode);
    //        foreach (var node in rootNode.SubTreeNodes())
    //            list.Add(node);
    //        return list;
    //    }
    //}

    //// Do given action on root node
    //public void DoAction(QTAction action)
    //{
    //    rootNode.DoAction(action);
    //}

    //// Insert new object into tree
    //public void Add(T obj)
    //{
    //    rootNode.InsertObject(obj, GetBounds(obj));
    //}

    //// Remove object from tree
    //public void Remove(T obj)
    //{
    //    DoAction(node =>
    //    {
    //        if (node.contents.Contains(obj))
    //            node.contents.Remove(obj);
    //    });
    //}

    //// Get objects in the given area of the tree
    //public IEnumerable<T> GetObjectsInArea(RectangleF area)
    //{
    //    return rootNode.GetObjectsInArea(area);
    //}
    #endregion OLD
}
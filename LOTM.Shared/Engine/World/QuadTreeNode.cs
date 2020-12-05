using System;
using System.Collections.Generic;
using System.Drawing;

namespace LOTM.Shared.Engine.World
{
    public class QuadTreeNode<T>
    {
        // Tree where this node belongs to
        public QuadTree<T> Tree { get; set; }

        // Bounds of this node
        public RectangleF Bounds;

        // Content of this node, filled with gameObjects
        public List<T> contents = new List<T>();

        // Childnodes of this partial tree
        public List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>(4);

        public QuadTreeNode(RectangleF bounds, QuadTree<T> tree)
        {
            Tree = tree;
            Bounds = bounds;
        }

        // Checks if this node has no content
        public bool IsEmpty { get { return contents.Count == 0 && (Bounds.IsEmpty || nodes.Count == 0); } }

        // Get total number of content objects in this node and it's childnodes
        public int Count
        {
            get
            {
                int count = 0;

                foreach (QuadTreeNode<T> node in nodes)
                {
                    count += node.Count;
                }

                count += contents.Count;

                return count;
            }
        }

        // Get all the content from this node and it's childnodes
        public IEnumerable<T> SubTreeContents
        {
            get
            {
                foreach (QuadTreeNode<T> node in nodes)
                {
                    foreach (T content in node.SubTreeContents)
                    {
                        yield return content;
                    }
                }

                foreach (T content in contents)
                {
                    yield return content;
                }
            }
        }

        // Get all childnodes and their childnodes recursively
        public IEnumerable<QuadTreeNode<T>> SubTreeNodes
        {
            get
            {
                foreach (var node in nodes)
                {
                    yield return node;
                    foreach (var subNode in node.nodes)
                        yield return subNode;
                }
            }
        }

        // Execute given action for this node and it's childnodes
        public void DoAction(QuadTree<T>.QTAction action)
        {
            action(this);

            // draw the child quads
            foreach (QuadTreeNode<T> node in nodes)
                node.DoAction(action);
        }

        // Get all objects of given area
        public IEnumerable<T> GetObjectsInArea(RectangleF area)
        {
            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad 
            // to see if they intersect.
            foreach (T obj in contents)
            {
                if (area.IntersectsWith(Tree.GetBounds(obj)))
                    yield return obj;
            }

            foreach (QuadTreeNode<T> node in nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Bounds.Contains(area))
                {
                    foreach (T obj in node.GetObjectsInArea(area))
                        yield return obj;
                    break;
                }

                // Case 2: Sub-quad completely contained by search area 
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children 
                // to the result set. You need to continue the loop to test 
                // the other quads
                if (area.Contains(node.Bounds))
                {
                    foreach (T obj in node.SubTreeContents)
                        yield return obj;
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Bounds.IntersectsWith(area))
                {
                    foreach (T obj in node.GetObjectsInArea(area))
                        yield return obj;
                }
            }
        }

        // Move the contents of this node to it's subnodes if object is in bounds of this subnode
        void MoveContentsToSubNodes()
        {
            contents.RemoveAll(obj =>
            {
                foreach (var node in nodes)
                {
                    var bounds = Tree.GetBounds(obj);
                    if (node.Bounds.Contains(bounds))
                    {
                        node.InsertObject(obj, bounds);
                        return true;
                    }
                }
                return false;
            });
        }

        // Create subnodes for this node
        private void CreateSubNodes()
        {
            // If the nodes size is already minimum, don't create subnodes
            if ((Bounds.Height * Bounds.Width) <= Tree.MinNodeSize)
                return;

            float halfWidth = (Bounds.Width / 2f);
            float halfHeight = (Bounds.Height / 2f);

            nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(Bounds.Left, Bounds.Top), new SizeF(halfWidth, halfHeight)), Tree));
            nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(Bounds.Left, Bounds.Top + halfHeight), new SizeF(halfWidth, halfHeight)), Tree));
            nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(Bounds.Left + halfWidth, Bounds.Top), new SizeF(halfWidth, halfHeight)), Tree));
            nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(Bounds.Left + halfWidth, Bounds.Top + halfHeight), new SizeF(halfWidth, halfHeight)), Tree));
        }

        // Insert new object into node
        public void InsertObject(T obj, RectangleF bounds)
        {
            // if the item is not contained in this quad, there's a problem
            if (!Bounds.Contains(bounds))
                throw new ArgumentException("object is out of the bounds of this quadtree node");

            // If this nodes capacity is already at it's limit, create subnodes and move content to those
            if (nodes.Count == 0 && contents.Count >= Tree.NodeCapacity)
            {
                CreateSubNodes();
                MoveContentsToSubNodes();
            }

            if (contents.Count > Tree.NodeCapacity)
            {
                //this node is full, let's try and store T in a subnode, if it's small enough.

                // for each subnode:
                // if the node contains the item, add the item to that node and return
                // this recurses into the node that is just large enough to fit this item
                foreach (QuadTreeNode<T> node in nodes)
                {
                    if (node.Bounds.Contains(bounds))
                    {
                        node.InsertObject(obj, bounds);
                        return;
                    }
                }
            }

            //add, even if we are over capacity.
            contents.Add(obj);
        }
    }
}
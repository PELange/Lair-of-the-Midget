using System;
using System.Collections.Generic;
using System.Drawing;

namespace LOTM.Shared.Engine.World
{
    public class QuadTree<T>
    {
        // Delegate to perform an action on a node
        public delegate void QTAction(QuadTreeNode<T> node);

        // Root node of this tree
        public QuadTreeNode<T> rootNode;

        // Bounds of this tree
        public RectangleF Bounds;

        // Maximum object capacity of nodes
        public int NodeCapacity { get; set; }

        // Minimum geographical size
        public int MinNodeSize { get; set; }

        // Get bounds of an object
        public Func<T, RectangleF> GetBounds { get; set; }

        // Get count of the content of rootNode and all subnodes
        //public int Count { get { return rootNode.Count; } }

        public QuadTree(RectangleF bounds)
        {
            Bounds = bounds;
            rootNode = new QuadTreeNode<T>(bounds, this);
        }

        // Get all nodes starting at rootNode
        //public IEnumerable<QuadTreeNode<T>> SubNodes
        //{
        //    get
        //    {
        //        yield return rootNode;
        //        foreach (var node in rootNode.SubTreeNodes)
        //            yield return node;
        //    }
        //}

        public IEnumerable<QuadTreeNode<T>> SubNodes
        {
            get
            {
                var list = new List<QuadTreeNode<T>>();
                list.Add(rootNode);
                foreach (var node in rootNode.SubTreeNodes())
                    list.Add(node);
                return list;
            }
        }

        // Do given action on root node
        public void DoAction(QTAction action)
        {
            rootNode.DoAction(action);
        }

        // Insert new object into tree
        public void Add(T obj)
        {
            rootNode.InsertObject(obj, GetBounds(obj));
        }

        // Remove object from tree
        public void Remove(T obj)
        {
            DoAction(node =>
            {
                if (node.contents.Contains(obj))
                    node.contents.Remove(obj);
            });
        }

        // Get objects in the given area of the tree
        public IEnumerable<T> GetObjectsInArea(RectangleF area)
        {
            return rootNode.GetObjectsInArea(area);
        }
    }
}
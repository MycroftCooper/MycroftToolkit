using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode.Structure {
    public enum QuadTreeNeighborDir { Up, Down, Left, Right }
    public enum QuadTreeChildDir { RightUp, RightDown, RightLeft, LeftDown, LeftUp }

    public class QuadTreeNode<T> {
        public Rect Bounds { get; }
        public bool IsLeaf => _children == null || _children.Length == 0;
        private QuadTreeNode<T>[] _children;
        
        public List<T> Objects { get; protected set; }

        public QuadTreeNode(Rect bounds) {
            Bounds = bounds;
            Objects = new List<T>();
            _children = null;
        }
    }
    
    public class QuadTree<T> {
        private readonly QuadTreeNode<T> _root;
        public Rect Bounds => _root.Bounds;
        public readonly int MaxDepth;
        public readonly int MinChunkSize;

        public QuadTree(Rect bounds, int maxDepth) {
            maxDepth = maxDepth;
            _root = new QuadTreeNode<T>(bounds);
        }

        public QuadTreeNode<T> GetNode(Vector2 pos, int depth) {
            return null;
        }

        public bool AddObject(T obj, Vector2 pos, int depth) {
            return false;
        }

        public bool RemoveObject(T obj, Vector2 pos, int depth) {
            return false;
        }
        
        

        // 插入对象
        public bool Insert(QuadTreeNode<T> obj) {
            // 如果对象不在当前区域内，返回false
            if (!bounds.Contains(obj.Position))
                return false;

            // 如果当前节点未满，并且没有子节点，直接添加对象
            if (objects.Count < capacity) {
                objects.Add(obj);
                return true;
            }

            // 如果已经存在子节点，插入到合适的子节点中
            if (topLeft == null)
                Subdivide();

            if (topLeft.Insert(obj) || topRight.Insert(obj) || bottomLeft.Insert(obj) || bottomRight.Insert(obj))
                return true;

            return false;
        }

        // 查询范围内的对象
        public List<T> Query(Rect range) {
            List<T> result = new List<T>();

            // 如果当前区域与查询范围没有交集，返回空
            if (!bounds.Overlaps(range))
                return result;

            // 检查当前节点中的所有对象
            foreach (var obj in objects) {
                if (range.Contains(obj.Position))
                    result.Add(obj.Data);
            }

            // 如果存在子节点，查询子节点
            if (topLeft != null) {
                result.AddRange(topLeft.Query(range));
                result.AddRange(topRight.Query(range));
                result.AddRange(bottomLeft.Query(range));
                result.AddRange(bottomRight.Query(range));
            }

            return result;
        }

        // 子节点划分
        private void Subdivide() {
            float halfWidth = bounds.width / 2;
            float halfHeight = bounds.height / 2;
            Vector2 center = bounds.center;

            topLeft = new QuadTree<T>(new Rect(center.x - halfWidth, center.y + halfHeight, halfWidth, halfHeight),
                capacity);
            topRight = new QuadTree<T>(new Rect(center.x, center.y + halfHeight, halfWidth, halfHeight), capacity);
            bottomLeft = new QuadTree<T>(new Rect(center.x - halfWidth, center.y - halfHeight, halfWidth, halfHeight),
                capacity);
            bottomRight = new QuadTree<T>(new Rect(center.x, center.y - halfHeight, halfWidth, halfHeight), capacity);
        }
    }
}
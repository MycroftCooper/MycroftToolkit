using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MycroftToolkit.DiscreteGridToolkit {
    public enum EPointSetType { Any, Line, Rect, Radius }
    
    
    public class PointComparer : IComparer<Vector2Int> {
        public int Compare(Vector2Int x, Vector2Int y) {
            if (x == y) return 0;
            if (x.y > y.y) return 1;
            else if (x.x < y.x) return -1;
            else {
                if (x.x > y.x) return 1;
                if (x.x < y.x) return -1;
                return 0;
            }
        }
    }
    
    
    public class PointSet {
        private static readonly PointComparer Comparer = new PointComparer();
        internal SortedSet<Vector2Int> Points;
        public EPointSetType Type;
        public int Count => Points.Count;

        public Vector2Int Min => Points.Min();
        public Vector2Int Max => Points.Max();

        // ReSharper disable once MemberCanBeProtected.Global
        public PointSet(EPointSetType type = EPointSetType.Any) {
            Type = type;
            Points = new SortedSet<Vector2Int>(Comparer);
        }
        public PointSet(List<Vector2Int> points, EPointSetType type = EPointSetType.Any) {
            Type = type;
            Points = new SortedSet<Vector2Int>(points,Comparer);
        }
        public PointSet(SortedSet<Vector2Int> points, EPointSetType type = EPointSetType.Any) {
            Type = type;
            Points = new SortedSet<Vector2Int>(points, Comparer);
        }
        public PointSet(Vector2Int centerPos, List<Vector2Int> template) {
            Type = EPointSetType.Any;
            Points = new SortedSet<Vector2Int>();
            foreach (Vector2Int pos in template) {
                Vector2Int p = centerPos + pos;
                Points.Add(p);
            }
        }

        protected virtual void UpdatePointSet() { }

        public bool HasPoint(Vector2Int point)
            => Points.Contains(point);
        public void ForEach(Action<Vector2Int> action) {
            foreach (var point in Points) {
                action?.Invoke(point);
            }
        }
        public PointSet Copy() => new PointSet(Points, Type);
        public bool AddPoint(Vector2Int point)
            => !Points.Add(point);
        public bool AddPoints(List<Vector2Int> points) {
            bool hasPoint = false;
            points.ForEach(p => {
                if (!Points.Add(p))
                    hasPoint = true;
            });
            return hasPoint;
        }
        public bool RemovePoint(Vector2Int point)
            => Points.Remove(point);
        public void RemovePoints(List<Vector2Int> point)
            => point.ForEach(x => Points.Remove(x));
        public void Clear() => Points.Clear();


        #region 集合运算
        /// <summary>
        /// 取两点集交集
        /// </summary>
        public PointSet Intersect(PointSet pointSet) {
            IEnumerable<Vector2Int> target = Points.Intersect(pointSet.Points);
            SortedSet<Vector2Int> newPoints = new SortedSet<Vector2Int>(target,Comparer);
            return new PointSet(newPoints);
        }
        
        /// <summary>
        /// 两点集是否有交集
        /// </summary>
        public bool IsIntersect(PointSet pointSet) {
            return pointSet.Points.Any(point => Points.Contains(point));
        }
        /// <summary>
        /// 取两点集并集
        /// </summary>
        public static PointSet operator +(PointSet pointSet1, PointSet pointSet2) {
            IEnumerable<Vector2Int> target = pointSet1.Points.Union(pointSet2.Points);
            SortedSet<Vector2Int> newPoints = new SortedSet<Vector2Int>(target,Comparer);
            return new PointSet(newPoints);
        }

        /// <summary>
        /// 取两点集差集
        /// </summary>
        public static PointSet operator -(PointSet pointSet1, PointSet pointSet2) {
            IEnumerable<Vector2Int> target = pointSet1.Points.Except(pointSet2.Points);
            SortedSet<Vector2Int> newPoints = new SortedSet<Vector2Int>(target,Comparer);
            return new PointSet(newPoints);
        }
        /// <summary>
        /// 是否为指定集合的真子集
        /// </summary>
        public bool IsProperSubsetOf(PointSet pointSet)
           => Points.IsProperSubsetOf(pointSet.Points);
        /// <summary>
        /// 是否为指定集合的真超集
        /// </summary>
        public bool IsProperSupersetOf(PointSet pointSet)
            => Points.IsProperSupersetOf(pointSet.Points);
        /// <summary>
        /// 是否为指定集合的子集
        /// </summary>
        public bool IsSubsetOf(PointSet pointSet)
            => Points.IsSubsetOf(pointSet.Points);
        /// <summary>
        /// 是否为指定集合的超集
        /// </summary>
        public bool IsSupersetOf(PointSet pointSet)
             => Points.IsSupersetOf(pointSet.Points);
        #endregion

        public void Move(Vector2Int offset) {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>(Comparer);
            foreach (var i in Points) {
                newPS.Add(i + offset);
            }
            Points.Clear();
            Points = newPS;
        }
        public void Flip_X() {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>(Comparer);
            foreach (var i in Points) {
                newPS.Add(new Vector2Int(i.x, -i.y));
            }
            Points.Clear();
            Points = newPS;
        }
        public void Flip_Y() {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>(Comparer);
            foreach (var i in Points) {
                newPS.Add(new Vector2Int(-i.x, i.y));
            }
            Points.Clear();
            Points = newPS;
        }
    }
}

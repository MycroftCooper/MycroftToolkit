using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace MycroftToolkit.DiscreteGridToolkit {
    public enum EPointSetType { any, line, rect, radius }
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
        internal SortedSet<Vector2Int> _points;
        public EPointSetType type;
        public int Count => _points.Count;

        public Vector2Int min => _points.Min();
        public Vector2Int max => _points.Max();

        public PointSet(EPointSetType type = EPointSetType.any) {
            this.type = type;
            this._points = new SortedSet<Vector2Int>(new PointComparer());
        }
        public PointSet(List<Vector2Int> points, EPointSetType type = EPointSetType.any) {
            this.type = type;
            this._points = new SortedSet<Vector2Int>(new PointComparer());
            foreach (var point in points) {
                this._points.Add(point);
            }
        }
        public PointSet(SortedSet<Vector2Int> points, EPointSetType type = EPointSetType.any) {
            this.type = type;
            this._points = new SortedSet<Vector2Int>(points, new PointComparer());
        }
        public PointSet(Vector2Int centerPos, List<Vector2Int> template) {
            type = EPointSetType.any;
            _points = new SortedSet<Vector2Int>();
            foreach (Vector2Int pos in template) {
                Vector2Int p = centerPos + pos;
                _points.Add(p);
            }
        }

        protected virtual void updatePointSet() { }

        public void HasPoint(Vector2Int point)
            => _points.Contains(point);
        public void ForEach(Action<Vector2Int> action) {
            foreach (var point in _points) {
                action?.Invoke(point);
            }
        }
        public PointSet Copy() => new PointSet(_points, type);
        public bool AddPoint(Vector2Int point)
            => !_points.Add(point);
        public bool AddPoints(List<Vector2Int> points) {
            bool hasPoint = false;
            points.ForEach(p => {
                if (!_points.Add(p))
                    hasPoint = true;
            });
            return hasPoint;
        }
        public bool RemovePoint(Vector2Int point)
            => _points.Remove(point);
        public void RemovePoints(List<Vector2Int> point)
            => point.ForEach(x => _points.Remove(x));
        public void Clear() => _points.Clear();


        #region 集合运算
        /// <summary>
        /// 取两点集交集
        /// </summary>
        public PointSet Intersect(PointSet pointSet) {
            SortedSet<Vector2Int> newPoints = (SortedSet<Vector2Int>)_points.Intersect<Vector2Int>(pointSet._points);
            return new PointSet(newPoints);
        }
        /// <summary>
        /// 两点集是否有交集
        /// </summary>
        public bool IsIntersect(PointSet pointSet) {
            foreach (var point in pointSet._points) {
                if (_points.Contains(point)) return true;
            }
            return false;
        }
        /// <summary>
        /// 取两点集并集
        /// </summary>
        public static PointSet operator +(PointSet pointSet1, PointSet pointSet2) {
            SortedSet<Vector2Int> newPoints = (SortedSet<Vector2Int>)pointSet1._points.Union<Vector2Int>(pointSet2._points);
            return new PointSet(newPoints);
        }

        /// <summary>
        /// 取两点集差集
        /// </summary>
        public static PointSet operator -(PointSet pointSet1, PointSet pointSet2) {
            SortedSet<Vector2Int> newPoints = (SortedSet<Vector2Int>)pointSet1._points.Except<Vector2Int>(pointSet2._points);
            return new PointSet(newPoints);
        }
        /// <summary>
        /// 是否为指定集合的真子集
        /// </summary>
        public bool IsProperSubsetOf(PointSet pointSet)
           => _points.IsProperSubsetOf(pointSet._points);
        /// <summary>
        /// 是否为指定集合的真超集
        /// </summary>
        public bool IsProperSupersetOf(PointSet pointSet)
            => _points.IsProperSupersetOf(pointSet._points);
        /// <summary>
        /// 是否为指定集合的子集
        /// </summary>
        public bool IsSubsetOf(PointSet pointSet)
            => _points.IsSubsetOf(pointSet._points);
        /// <summary>
        /// 是否为指定集合的超集
        /// </summary>
        public bool IsSupersetOf(PointSet pointSet)
             => _points.IsSupersetOf(pointSet._points);
        #endregion

        public void Move(Vector2Int offset) {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>();
            foreach (var i in _points) {
                newPS.Add(i + offset);
            }
            _points.Clear();
            _points = newPS;
        }
        public void Flip_X() {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>();
            foreach (var i in _points) {
                newPS.Add(new Vector2Int(i.x, -i.y));
            }
            _points.Clear();
            _points = newPS;
        }
        public void Flip_Y() {
            SortedSet<Vector2Int> newPS = new SortedSet<Vector2Int>();
            foreach (var i in _points) {
                newPS.Add(new Vector2Int(-i.x, i.y));
            }
            _points.Clear();
            _points = newPS;
        }
    }
}

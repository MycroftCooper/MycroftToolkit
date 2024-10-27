using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem.PathFinding {
    public class PathFindingRequest {
        public readonly Vector2Int StartPos;
        public readonly Vector2Int EndPos;

        public readonly bool NeedHandleImmediately;
        public readonly bool CanUseCache;
        public List<Vector2Int> ResultPath;
        public readonly Action<PathFindingRequest> PathFoundHandler;

        public PathFindingRequest(Vector2Int startPos, Vector2Int endPos, bool canUseCache = false, bool needHandleImmediately = false, Action<PathFindingRequest> pathFoundHandler = null) {
            StartPos = startPos;
            EndPos = endPos;
            CanUseCache = canUseCache;
            NeedHandleImmediately = needHandleImmediately;
            PathFoundHandler = pathFoundHandler;
        }

        public void SetResultPath(List<Vector2Int> resultPath) {
            ResultPath = resultPath;
            PathFoundHandler?.Invoke(this);
        }
    }
    
    public class GridPoint {
        public readonly int X, Y;
        public bool IsPassable;
        public GridPoint P;
        public int G; // 从起点到当前节点的代价 
        public int H; // 从当前节点到终点的预估代价
        public int F; // G + H;

        public Vector2Int[] JumpPoints; // 跳点信息存储结构：每个节点在8个方向的跳点
        public GridPoint(int x, int y, bool isPassable) {
            X = x;
            Y = y;
            IsPassable = isPassable;
            Reset();
            JumpPoints = new Vector2Int[8];
            for (int i = 0; i < 8; i++) {
                JumpPoints[i] = new Vector2Int(-1, -1); // 初始化为无效位置
            }
        }

        public void Reset() {
            G = H = 0;
            F = -1;
            P = null;
        }
        
        public void SetData(int g, int h, GridPoint p) {
            G = g;
            H = h;
            F = G + H;
            P = p;
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y);
        }

        public override string ToString() {
            return $"(point -> x: {X}, y: {Y}, f: {F})";
        }
        
        public static readonly Vector2Int[] Directions = { // 定义方向数组: 上、下、左、右、左上、右上、左下、右下
            new Vector2Int(0, 1),   // 上
            new Vector2Int(0, -1),  // 下
            new Vector2Int(-1, 0),  // 左
            new Vector2Int(1, 0),   // 右
            new Vector2Int(-1, 1),  // 左上
            new Vector2Int(1, 1),   // 右上
            new Vector2Int(-1, -1), // 左下
            new Vector2Int(1, -1)   // 右下
        };
    }
    
    public class PointLibrary : ICollection<GridPoint>,
        IReadOnlyCollection<GridPoint> {
        private readonly int _maxLength;
        private readonly HashSet<GridPoint>[] _data;
        private int _minPointer;

        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public PointLibrary(int maxLength) {
            _maxLength = maxLength;
            _data = new HashSet<GridPoint>[_maxLength];
            for (int i = 0; i < _maxLength; i++) {
                _data[i] = new HashSet<GridPoint>();
            }
            _minPointer = _maxLength;
            Count = 0;
        }

        public bool TryAdd(GridPoint point) {
            int index = point.F / 10;
            if (index < 0) {
                Debug.LogError($"{point}尚未被初始化！");
                return false;
            }

            if (index >= _maxLength) {
                return false;
            }

            if (!_data[index].Add(point)) {
                return false;
            }

            Count++;
                
            if (_minPointer > index) {
                _minPointer = index;
            }

            return true;
        }

        public void Add(GridPoint point) {
            TryAdd(point);
        }

        public GridPoint PopMin() {
            var hashSet = _data[_minPointer];
            var value = hashSet.ElementAt(0);
            hashSet.Remove(value);
            Count--;

            if (Count == 0) {
                _minPointer = _maxLength;
            }
            else if (hashSet.Count == 0) {
                do {
                    _minPointer++;
                } while (_data[_minPointer].Count == 0);
            }

            return value;
        }

        public bool Contains(GridPoint point) {
            if (point == null || point.F < 0) return false;
            int index = point.F / 10;
            return index < _maxLength && _data[index].Contains(point);
        }

        public void CopyTo(GridPoint[] array, int arrayIndex) {
            Debug.LogError("PointLibrary不支持拷贝到列表");
        }

        public bool Remove(GridPoint item) {
            Debug.LogError("PointLibrary不支持单点移除");
            return false;
        }
        
        public void Clear() {
            for (int i = 0; i < _maxLength; i++) {
                _data[i].Clear();
            }
            _minPointer = _maxLength;
            Count = 0;
        }

        public IEnumerator<GridPoint> GetEnumerator() {
            List<GridPoint> points = new List<GridPoint>(Count);
            for (int index = _minPointer; index < _maxLength; index++) {
                points.AddRange(_data[index]);
            }

            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
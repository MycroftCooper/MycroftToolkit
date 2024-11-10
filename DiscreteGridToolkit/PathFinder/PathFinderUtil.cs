using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding {
    public class SourceMap {
        public readonly bool[,] PassableMap;
        public readonly int Width;
        public readonly int Height;

        public SourceMap(bool[,] passableMap) {
            PassableMap = passableMap;
            Width = PassableMap.GetLength(0);
            Height = PassableMap.GetLength(1);
        }
        
        public bool IsPassable(int x, int y, bool checkEdge = true) {
            if (checkEdge) {
                return x >= 0 && x < Width && y >= 0 && y < Height && PassableMap[x, y];
            }
            return x < 0 || x >= Width || y < 0 || y >= Height || PassableMap[x, y];
        }

        public void UpdateMap(RectInt bounds, bool passable) {
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    PassableMap[x, y] = passable;
                }
            }
        }
        
        public enum Directions { Up, Down, Left, Right, LeftUp, RightUp, LeftDown, RightDown, Length }
        public static readonly Dictionary<Directions, Vector2Int> Direction2VectorDict = new() {
            { Directions.Up, Vector2Int.up },
            { Directions.Down, Vector2Int.down },
            { Directions.Left, Vector2Int.left },
            { Directions.Right, Vector2Int.right },
            { Directions.LeftUp, new Vector2Int(-1, 1) },
            { Directions.RightUp, new Vector2Int(1, 1) },
            { Directions.LeftDown, new Vector2Int(-1, -1) },
            { Directions.RightDown, new Vector2Int(1, -1) }
        };
        public static readonly Dictionary<Vector2Int, Directions> Vector2DirectionDict = new() {
            { Vector2Int.up, Directions.Up },
            { Vector2Int.down, Directions.Down },
            { Vector2Int.left, Directions.Left },
            { Vector2Int.right, Directions.Right },
            { new Vector2Int(-1, 1), Directions.LeftUp },
            { new Vector2Int(1, 1), Directions.RightUp },
            { new Vector2Int(-1, -1), Directions.LeftDown },
            { new Vector2Int(1, -1), Directions.RightDown }
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
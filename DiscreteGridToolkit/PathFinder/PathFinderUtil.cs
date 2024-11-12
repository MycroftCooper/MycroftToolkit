using System;
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
        
        // Theta算法
        public bool LineOfSight(JPSPlusPoint parentJpsPlusPoint, JPSPlusPoint currentJpsPlusPoint) {
            int x0 = parentJpsPlusPoint.X;
            int y0 = parentJpsPlusPoint.Y;
            int x1 = currentJpsPlusPoint.X;
            int y1 = currentJpsPlusPoint.Y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true) {
                // 如果当前格子不可通过，返回 false
                if (!IsPassable(x0, y0)) return false;

                // 检查对角线穿越障碍的情况
                if (dx != 0 && dy != 0) { // 如果沿对角线移动
                    if (!IsPassable(x0 - sx, y0, false) && !IsPassable(x0, y0 - sy, false)) {
                        // 如果水平和垂直方向都被阻挡，则阻止对角线穿越
                        return false;
                    }
                }

                // 如果到达目标节点，返回 true
                if (x0 == x1 && y0 == y1) return true;

                int e2 = 2 * err;
                if (e2 > -dy) {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    y0 += sy;
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

    public static class HeuristicFunctions {
        public class HeuristicFunction : IHeuristicFunction {
            public HeuristicType Heuristic { get; set; }
            
            public HeuristicFunction(HeuristicType type) {
                Heuristic = type;
            }
            public int CalculateHeuristic(Vector2Int a, Vector2Int b) {
                return HeuristicFunctions.CalculateHeuristic(Heuristic, a, b);
            }

            public int CalculateMaxFCost(Vector2Int mapSize) {
                return HeuristicFunctions.CalculateMaxFCost(Heuristic, mapSize);
            }
        }

        public const int DiagonalCost = 2;
        public const int StraightCost = 1;
        public static int CalculateHeuristic(HeuristicType type, Vector2Int a, Vector2Int b) {
            switch (type) {
                case HeuristicType.Manhattan:
                    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 曼哈顿距离
                case HeuristicType.Euclidean:
                    return (int)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));// 欧式
                case HeuristicType.SquaredEuclidean:
                    return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);// 欧式平方
                case HeuristicType.Diagonal:
                    return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
                case HeuristicType.WeightedDiagonal:
                    int deltaX = Math.Abs(a.x - b.x);
                    int deltaY = Math.Abs(a.y - b.y);
                    return DiagonalCost * Math.Min(deltaX, deltaY) + StraightCost * Math.Abs(deltaX - deltaY);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static int CalculateMaxFCost(HeuristicType type, Vector2Int mapSize) {
            int width = mapSize.x;
            int height = mapSize.y;
            int maxG = 2 * (width + height);// 通用的 G 值最大计算：沿地图边界绕一圈的代价
            int maxF = CalculateHeuristic(type, Vector2Int.zero, mapSize - Vector2Int.one);
            return maxG + maxF;
        }
    }
    
    public class PointLibrary : ICollection<AStartPoint>,
        IReadOnlyCollection<AStartPoint> {
        private readonly int _maxLength;
        private readonly HashSet<AStartPoint>[] _data;
        private int _minPointer;

        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public PointLibrary(int maxLength) {
            _maxLength = maxLength;
            _data = new HashSet<AStartPoint>[_maxLength];
            for (int i = 0; i < _maxLength; i++) {
                _data[i] = new HashSet<AStartPoint>();
            }
            _minPointer = _maxLength;
            Count = 0;
        }

        public bool TryAdd(AStartPoint point) {
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

        public void Add(AStartPoint point) {
            TryAdd(point);
        }

        public AStartPoint PopMin() {
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

        public bool Contains(AStartPoint point) {
            if (point == null || point.F < 0) return false;
            int index = point.F / 10;
            return index < _maxLength && _data[index].Contains(point);
        }

        public void CopyTo(AStartPoint[] array, int arrayIndex) {
            Debug.LogError("PointLibrary不支持拷贝到列表");
        }

        public bool Remove(AStartPoint item) {
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

        public IEnumerator<AStartPoint> GetEnumerator() {
            List<AStartPoint> points = new List<AStartPoint>(Count);
            for (int index = _minPointer; index < _maxLength; index++) {
                points.AddRange(_data[index]);
            }

            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
        
    public class BucketPriorityQueue<T> : ICollection<T> where T : BucketPriorityQueue<T>.IBucketPriorityQueueItem {
        public interface IBucketPriorityQueueItem {
            int PriorityValue { get; }
        }

        public bool NeedBestSolution = true;
        private readonly int _bucketSize;// 每个桶包含的优先级范围
        private readonly List<HashSet<T>> _buckets;// 桶集合
        private int _minIndex;// 当前优先级最小的非空桶索引
        private int _maxIndex; // 当前优先级最大的非空桶索引
        public int Count { get; private set; } // 总元素数
        public bool IsReadOnly => false; // 可变集合

        public BucketPriorityQueue(int maxPriorityValue, int bucketSize = 10) {
            _bucketSize = bucketSize;
            int bucketCount = maxPriorityValue / bucketSize + 1;
            _buckets = new List<HashSet<T>>(bucketCount);
            for (int i = 0; i < bucketCount; i++) {
                _buckets.Add(new HashSet<T>());
            }
            _minIndex = -1;
            _maxIndex = -1;
            Count = 0;
        }
        
        public void Add(T item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            
            int index = item.PriorityValue / _bucketSize;
            if (index < 0 || index >= _buckets.Count) {
                Debug.LogError($"PriorityValue {item.PriorityValue} is out of range[0,{_buckets.Count * _bucketSize}].");
                return;
            }
            if (!_buckets[index].Add(item)) {
                Debug.LogError($"PriorityValue {item.PriorityValue} is already added!");
                return;
            }
            Count++;
            if (_minIndex == -1 || index < _minIndex) {
                _minIndex = index; // 更新最小索引
            }
            if (_maxIndex == -1 || index > _maxIndex) {
                _maxIndex = index; // 更新最大索引
            }
        }

        public bool TryAdd(T item) {
            if (item == null) {
                return false;
            }
            
            int index = item.PriorityValue / _bucketSize;
            if (index < 0 || index >= _buckets.Count) {
                return false;
            }

            if (!_buckets[index].Add(item)) return false;
            Count++;
            if (_minIndex == -1 || index < _minIndex) {
                _minIndex = index; // 更新最小索引
            }
            if (_maxIndex == -1 || index > _maxIndex) {
                _maxIndex = index; // 更新最大索引
            }
            return true;
        }
        
        public bool Remove(T item) {
            if (item == null) {
                return false;
            }
            int index = item.PriorityValue / _bucketSize;
            if (index < 0 || index >= _buckets.Count) {
                Debug.LogError($"PriorityValue {item.PriorityValue} is out of range[0,{_buckets.Count * _bucketSize}].");
                return false;
            }
            if (!_buckets[index].Remove(item)) return false;
            
            Count--;
            if (_buckets[index].Count == 0 && _minIndex == index) {
                _minIndex = _buckets.FindIndex(b => b.Count > 0); // 更新最小索引
                _maxIndex = _buckets.FindLastIndex(b => b.Count > 0);
            }
            return true;
        }
        
        public T DequeueMin() {
            if (_minIndex == -1 || Count == 0) {
                throw new InvalidOperationException("The queue is empty.");
            }

            var bucket = _buckets[_minIndex];
            T item = default;
            if (NeedBestSolution) {
                foreach (var x in bucket) {
                    if (item == null || x.PriorityValue < item.PriorityValue) {
                        item = x;
                    }
                }
            } else {
                item = bucket.First();
            }
            bucket.Remove(item);
            Count--;

            if (bucket.Count == 0) {
                _minIndex = _buckets.FindIndex(b => b.Count > 0); // 更新最小索引
            }

            return item;
        }
        
        public T DequeueMax() {
            if (_maxIndex == -1 || Count == 0) {
                throw new InvalidOperationException("The queue is empty.");
            }

            var bucket = _buckets[_maxIndex];
            T item = default;
            if (NeedBestSolution) {
                foreach (var x in bucket) {
                    if (item == null || x.PriorityValue > item.PriorityValue) {
                        item = x;
                    }
                }
            } else {
                item = bucket.First();
            }
            bucket.Remove(item);
            Count--;

            if (bucket.Count == 0) {
                _maxIndex = _buckets.FindLastIndex(b => b.Count > 0); // 更新最小索引
            }

            return item;
        }
        
        public bool Contains(T item) {
            if (item == null) {
                return false;
            }
            int index = item.PriorityValue / _bucketSize;
            if (index < 0 || index >= _buckets.Count) {
                return false;
            }
            return _buckets[index].Contains(item);
        }
        
        public void Clear() {
            foreach (var bucket in _buckets) {
                bucket.Clear();
            }
            _minIndex = -1;
            _maxIndex = -1;
            Count = 0;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() {
            foreach (var bucket in _buckets) {
                foreach (var item in bucket) {
                    yield return item;
                }
            }
        }
        
        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count) throw new ArgumentException("The target array is too small.");

            foreach (var bucket in _buckets) {
                foreach (var item in bucket) {
                    array[arrayIndex++] = item;
                }
            }
        }
    }
}
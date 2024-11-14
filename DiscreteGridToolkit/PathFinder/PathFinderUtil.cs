using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding {
    public class SourceMap {
        public readonly bool[,] PassableMap;
        public bool CanDiagonallyPassByObstacle;
        public readonly int Width;
        public readonly int Height;

        public SourceMap(bool[,] passableMap, bool canDiagonallyPassByObstacle = false) {
            PassableMap = passableMap;
            CanDiagonallyPassByObstacle = canDiagonallyPassByObstacle;
            Width = PassableMap.GetLength(0);
            Height = PassableMap.GetLength(1);
        }
        
        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        
        public bool IsPassable(int x, int y, bool checkEdge = true) {
            if (checkEdge) {
                return x >= 0 && x < Width && y >= 0 && y < Height && PassableMap[x, y];
            }
            return x < 0 || x >= Width || y < 0 || y >= Height || PassableMap[x, y];
        }

        public bool CanMoveTo(int x, int y, Vector2Int dir) {
            int npx = x + dir.x;
            int npy = y + dir.y;
            if(!IsPassable(npx, npy)) return false;
            if (dir.x == 0 || dir.y == 0) {
                return true;
            }
            
            bool hPassable = IsPassable(npx, y);
            bool vPassable = IsPassable(x, npy);
            if(!hPassable && !vPassable) return false;
            if (!CanDiagonallyPassByObstacle && (!hPassable || !vPassable)) {
                return false;
            }
            return true;
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
        public class CommonHeuristicFunction : HeuristicFunctionBase {
            public CommonHeuristicFunction(HeuristicTypes types) {
                HeuristicType = types;
            }
            public override int CalculateHeuristic(int aX, int aY, int bX, int bY) {
                return HeuristicFunctions.CalculateHeuristic(HeuristicType, aX, aY, bX, bY);
            }
        }

        public const int DiagonalCost = 2;
        public const int StraightCost = 1;
        public static int CalculateHeuristic(HeuristicTypes types, int aX, int aY, int bX, int bY) {
            switch (types) {
                case HeuristicTypes.Manhattan:
                    return Mathf.Abs(aX - bX) + Mathf.Abs(aY - bY); // 曼哈顿距离
                case HeuristicTypes.Euclidean:
                    return (int)Math.Sqrt(Math.Pow(aX - bX, 2) + Math.Pow(aY - bY, 2));// 欧式
                case HeuristicTypes.SquaredEuclidean:
                    return (aX - bX) * (aX - bX) + (aY - bY) * (aY - bY);// 欧式平方
                case HeuristicTypes.Diagonal:
                    return Math.Max(Math.Abs(aX - bX), Math.Abs(aY - bY));
                case HeuristicTypes.WeightedDiagonal:
                    int deltaX = Math.Abs(aX - bX);
                    int deltaY = Math.Abs(aY - bY);
                    return DiagonalCost * Math.Min(deltaX, deltaY) + StraightCost * Math.Abs(deltaX - deltaY);
                default:
                    throw new ArgumentOutOfRangeException(nameof(types), types, null);
            }
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
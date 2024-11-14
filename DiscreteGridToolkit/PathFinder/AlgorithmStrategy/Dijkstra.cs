using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class Dijkstra : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.Dijkstra;
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        private SourceMap _map;
        
        private DijkstraPoint[,] _dijkstraMap;
        private BucketPriorityQueue<DijkstraPoint> _openList;
        private HashSet<DijkstraPoint> _closedList;
        
        public void InitMap(SourceMap map) {
            _map = map;
            _dijkstraMap = new DijkstraPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _dijkstraMap[x, y] = new DijkstraPoint(x, y);
                }
            }
            int maxG = _map.Width * _map.Height;
            int bucketCount = Mathf.CeilToInt(Mathf.Sqrt(maxG));
            int bucketSize = maxG / bucketCount;
            _openList = new BucketPriorityQueue<DijkstraPoint>(maxG, bucketSize);
            _closedList = new HashSet<DijkstraPoint>();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            _openList.Clear();
            _closedList.Clear();
            _openList.NeedBestSolution = NeedBestSolution;
            
            DijkstraPoint startPoint = _dijkstraMap[start.x, start.y];
            DijkstraPoint targetPoint = _dijkstraMap[target.x, target.y];
            
            startPoint.SetData(0, null); // 设置起点数据
            _openList.Add(startPoint);

            while (_openList.Count > 0) {
                DijkstraPoint current = _openList.DequeueMin();
                _closedList.Add(current);
                
                if (current == targetPoint) {
                    return ReconstructPath(current);
                }

                foreach (var direction in SourceMap.Vector2DirectionDict.Keys) {
                    Vector2Int nP = current.Pos + direction;

                    if (!_map.IsPassable(nP.x, nP.y) ||
                        (direction.x != 0 && direction.y != 0 && // 对角线障碍判断
                         (!_map.IsPassable(nP.x, current.Y) || 
                          !_map.IsPassable(current.X, nP.y)))) 
                        continue;

                    DijkstraPoint neighbor = _dijkstraMap[nP.x, nP.y];
                    // 如果相邻节点已经在封闭列表中，跳过
                    if (_closedList.Contains(neighbor)) continue;
                    
                    int tentativeG = current.G + 1; // Assuming uniform cost
                    if (!_openList.Contains(neighbor)) {
                        neighbor.SetData(tentativeG, current);
                        _openList.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.G)  {
                        _openList.Remove(neighbor);
                        neighbor.SetData(tentativeG, current);// 如果新的路径更短，更新
                        _openList.Add(neighbor);
                    }
                }
            }
            return null;
        }
        
        private List<Vector2Int> ReconstructPath(DijkstraPoint current) {
            List<Vector2Int> path = new List<Vector2Int>();
            while (current != null) {
                path.Insert(0, current.Pos);
                current = current.P;
            }
            return path;
        }
        
        public void UpdateMap(RectInt bounds, bool passable) { }
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }

    public class DijkstraPoint : BucketPriorityQueue<DijkstraPoint>.IBucketPriorityQueueItem {
        public readonly int X, Y;
        public readonly Vector2Int Pos;
        public DijkstraPoint P;
        
        public int G; // 从起点到当前节点的代价
        public int PriorityValue => G;
        
        public DijkstraPoint(int x, int y) {
            X = x;
            Y = y;
            Pos = new Vector2Int(X, Y);
            Reset();
        }
        
        public void SetData(int g, DijkstraPoint p) {
            G = g;
            P = p;
        }

        public void Reset() {
            G = 0;
            P = null;
        }
        
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"point({X},{Y})[g:{G};]";
    }
}
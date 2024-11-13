using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class JPS : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.JPS;
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        private SourceMap _map;
        private AStartPoint[,] _aStartMap;
        private BucketPriorityQueue<AStartPoint> _openList;
        private HashSet<AStartPoint> _closedList;
        
        public void InitMap(SourceMap map) {
            _map = map;
            _aStartMap = new AStartPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _aStartMap[x, y] = new AStartPoint(x, y);
                }
            }
            int maxF = HeuristicFunction.CalculateMaxFCost(new Vector2Int(_map.Width, _map.Height));
            int bucketCount = Mathf.CeilToInt(Mathf.Sqrt(_map.Width * _map.Height));
            int bucketSize = maxF / bucketCount;
            _openList = new BucketPriorityQueue<AStartPoint>(maxF, bucketSize);
            _closedList = new HashSet<AStartPoint>();
        }
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            _openList.Clear();
            _closedList.Clear();
            _openList.NeedBestSolution = NeedBestSolution;
            
            AStartPoint startPoint = _aStartMap[start.x, start.y];
            AStartPoint targetPoint = _aStartMap[target.x, target.y];
            
            startPoint.SetData(0, HeuristicFunction.CalculateHeuristic(start, target), null); // 设置起点数据
            _openList.Add(startPoint);
            
            while (_openList.Count > 0) {
                AStartPoint current = _openList.DequeueMin();
                _closedList.Add(current);
                
                if (current == targetPoint) {
                    return ReconstructPath(current);
                }
                
                // 3. 跳点处理
                foreach (var direction in SourceMap.Direction2VectorDict.Values) {
                    AStartPoint jumpPoint = FindJumpPoint(current, direction, targetPoint);
                    if (jumpPoint == null || _closedList.Contains(jumpPoint)) continue;
                    int newG = current.G + HeuristicFunction.CalculateHeuristic(current.Pos, jumpPoint.Pos);
                    if (!_openList.Contains(jumpPoint)) {
                        jumpPoint.SetData(newG, HeuristicFunction.CalculateHeuristic(
                            new Vector2Int(jumpPoint.X, jumpPoint.Y),
                            target
                        ), current);
                        _openList.Add(jumpPoint);
                    } else if (newG < jumpPoint.G) {
                        _openList.Remove(jumpPoint);
                        jumpPoint.SetData(newG, current);
                        _openList.Add(jumpPoint);
                    }
                }
            }
            return null;// 如果没有找到路径，返回空
        }

        private List<Vector2Int> ReconstructPath(AStartPoint current) {
            List<Vector2Int> path = new List<Vector2Int>();
            while (current.P != null) {
                path.Insert(0, new Vector2Int(current.X, current.Y));
                current = current.P;
            }
            return path;
        }

        private AStartPoint FindJumpPoint(AStartPoint current, Vector2Int direction, AStartPoint target) {
            int newX = current.X + direction.x;
            int newY = current.Y + direction.y;

            if (!_map.IsPassable(newX, newY)) {
                return null;
            }
            // if (direction.x != 0 && direction.y != 0 && // 对角线障碍判断
            //     (!_map.IsPassable(newX, current.Y) || !_map.IsPassable(current.X, newY))) {
            //     return null;
            // }

            AStartPoint newPoint = _aStartMap[newX, newY];
            if (newPoint == target) {
                return newPoint;
            }

            // 检查强迫邻居（Forced Neighbors）
            if (HasForcedNeighbors(newPoint, direction)) {
                return newPoint;
            }

            // 对角线移动需要处理两个方向的跳点
            if (direction.x != 0 && direction.y != 0) {
                if (FindJumpPoint(newPoint, new Vector2Int(direction.x, 0), target) != null ||
                    FindJumpPoint(newPoint, new Vector2Int(0, direction.y), target) != null) {
                    return newPoint;
                }
            }

            // 继续递归
            return FindJumpPoint(newPoint, direction, target);
        }

        private bool HasForcedNeighbors(AStartPoint point, Vector2Int direction) {
            int x = point.X;
            int y = point.Y;

            // 只检查与方向垂直的邻居
            if (direction.x != 0 && direction.y != 0) {
                return (!_map.IsPassable(x - direction.x, y) && _map.IsPassable(x - direction.x, y + direction.y)) ||
                       (!_map.IsPassable(x, y - direction.y) && _map.IsPassable(x + direction.x, y - direction.y));
            }
            if (direction.x != 0) {
                return (!_map.IsPassable(x, y + 1) && _map.IsPassable(x + direction.x, y + 1)) ||
                       (!_map.IsPassable(x, y - 1) && _map.IsPassable(x + direction.x, y - 1));
            }
            return (!_map.IsPassable(x + 1, y) && _map.IsPassable(x + 1, y + direction.y)) ||
                   (!_map.IsPassable(x - 1, y) && _map.IsPassable(x - 1, y + direction.y));
        }

        public void UpdateMap(RectInt bounds, bool passable) { }
        
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }
}
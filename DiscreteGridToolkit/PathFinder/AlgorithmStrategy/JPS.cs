using System;
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
                
                // 跳点处理
                List<AStartPoint> successors = GetSuccessors(current, targetPoint);
                foreach (var neighbor in successors) {
                    if (neighbor == null || _closedList.Contains(neighbor)) {
                        continue; // 没有跳点或跳点已在关闭列表
                    }
                    int newG = current.G + HeuristicFunction.CalculateHeuristic(current.Pos, neighbor.Pos);
                    if (!_openList.Contains(neighbor)) {
                        neighbor.SetData(newG, HeuristicFunction.CalculateHeuristic(
                            new Vector2Int(neighbor.X, neighbor.Y), target), current);
                        _openList.Add(neighbor);
                    } else if (newG < neighbor.G) {
                        _openList.Remove(neighbor);
                        neighbor.SetData(newG, current);
                        _openList.Add(neighbor);
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

            // 检查是否可通行
            if (!_map.IsPassable(newX, newY)) {
                return null;
            }

            // 对角线障碍判断
            if (direction.x != 0 && direction.y != 0 && 
                (!_map.IsPassable(newX, current.Y) || !_map.IsPassable(current.X, newY))) {
                // 如果对角线方向存在障碍物，先沿着水平方向和垂直方向前进一格
                if (_map.IsPassable(newX, current.Y)) {
                    return _aStartMap[newX, current.Y];
                }
                if (_map.IsPassable(current.X, newY)) {
                    return _aStartMap[current.X, newY];
                }
                // 如果没有合适的跳点，返回null
                return null;
            }

            AStartPoint newPoint = _aStartMap[newX, newY];
            // 如果到达目标点，直接返回
            if (newPoint == target) {
                return newPoint;
            }

            // 检查强迫邻居（Forced Neighbors）
            if (direction.x == 0 || direction.y == 0) {
                if (HasForcedNeighbors(newPoint, direction)) {
                    return newPoint;
                }
            } else { // 对角线移动需要处理两个方向的跳点
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
            if (direction.x != 0 && direction.y != 0) {// 防止穿越阻挡，对角线不认为是强迫邻居
                return (!_map.IsPassable(x - direction.x, y) && _map.IsPassable(x - direction.x, y + direction.y)) ||
                       (!_map.IsPassable(x, y - direction.y) && _map.IsPassable(x + direction.x, y - direction.y));
            }
            if (direction.x != 0) { // 水平方向
                return (!_map.IsPassable(x, y + 1) && _map.IsPassable(x + direction.x, y + 1)) ||
                       (!_map.IsPassable(x, y - 1) && _map.IsPassable(x + direction.x, y - 1));
            }
            // 垂直方向
            return (!_map.IsPassable(x + 1, y) && _map.IsPassable(x + 1, y + direction.y)) ||
                   (!_map.IsPassable(x - 1, y) && _map.IsPassable(x - 1, y + direction.y));
        }
        
         public List<AStartPoint> GetSuccessors(AStartPoint point, AStartPoint end) {
            List<AStartPoint> successors = new List<AStartPoint>();
            List<Vector2Int> directions;

            if (point.P != null) { // 有父节点，进行方向剪枝
                int dx = point.X - point.P.X;
                int dy = point.Y - point.P.Y;
                Vector2Int parentDir = new Vector2Int(Math.Sign(dx), Math.Sign(dy));
                directions = PruneDirections(point, parentDir);
            } else { // 起点，所有方向都考虑
                directions = new List<Vector2Int>(SourceMap.Vector2DirectionDict.Keys);
            }

            foreach (var dir in directions) {
                AStartPoint jumpPoint = FindJumpPoint(point, dir, end);
                if (jumpPoint != null) {
                    successors.Add(jumpPoint);
                }
            }

            return successors;
        }

        public List<Vector2Int> PruneDirections(AStartPoint point, Vector2Int parentDir) {
            List<Vector2Int> pruned = new List<Vector2Int>();
            int px = point.X;
            int py = point.Y;
            int dx = parentDir.x;
            int dy = parentDir.y;

            // 对角线方向
            if (dx != 0 && dy != 0) {
                bool dPassable = _map.IsPassable(px + dx, py + dy);
                bool hPassable = _map.IsPassable(px + dx, py);
                bool vPassable = _map.IsPassable(px, py + dy);
                // 保留前进方向
                if (dPassable || (hPassable && vPassable)) pruned.Add(new Vector2Int(dx, dy));

                // 水平和垂直方向
                if (hPassable) pruned.Add(new Vector2Int(dx, 0));
                if (vPassable) pruned.Add(new Vector2Int(0, dy));

                // 处理强迫邻居
                if (!_map.IsPassable(px - dx, py) && _map.IsPassable(px - dx, py + dy))
                    pruned.Add(new Vector2Int(-dx, dy));

                if (!_map.IsPassable(px, py - dy) && _map.IsPassable(px + dx, py - dy))
                    pruned.Add(new Vector2Int(dx, -dy));
            } else {
                // 水平或垂直方向
                if (dx != 0) {
                    if (_map.IsPassable(px + dx, py)) pruned.Add(new Vector2Int(dx, 0));

                    // 处理强迫邻居
                    if (!_map.IsPassable(px, py + 1) && _map.IsPassable(px + dx, py + 1))
                        pruned.Add(new Vector2Int(dx, 1));

                    if (!_map.IsPassable(px, py - 1) && _map.IsPassable(px + dx, py - 1))
                        pruned.Add(new Vector2Int(dx, -1));
                } else if (dy != 0) {
                    if (_map.IsPassable(px, py + dy)) pruned.Add(new Vector2Int(0, dy));

                    // 处理强迫邻居
                    if (!_map.IsPassable(px + 1, py) && _map.IsPassable(px + 1, py + dy))
                        pruned.Add(new Vector2Int(1, dy));

                    if (!_map.IsPassable(px - 1, py) && _map.IsPassable(px - 1, py + dy))
                        pruned.Add(new Vector2Int(-1, dy));
                }

                foreach (var VARIABLE in SourceMap.Vector2DirectionDict.Keys) {
                    
                }
            }

            return pruned;
        }

        public void UpdateMap(RectInt bounds, bool passable) { }
        
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }
}
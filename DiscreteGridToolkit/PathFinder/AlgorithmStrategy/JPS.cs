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
            
            // 设置起点
            startPoint.SetData(0, HeuristicFunction.CalculateHeuristic(start.x, start.y, target.x, target.y), null);
            _openList.Add(startPoint);
            
            while (_openList.Count > 0) {
                AStartPoint current = _openList.DequeueMin();
                _closedList.Add(current);
                
                if (current == targetPoint) {
                    return ReconstructPath(current);
                }
                
                // 跳点处理
                GetSuccessors(current, targetPoint);
                foreach (var neighbor in successors) {
                    if (neighbor == null || _closedList.Contains(neighbor)) {
                        continue; // 没有跳点或跳点已在关闭列表
                    }
                    int newG = current.G + HeuristicFunction.CalculateHeuristic(current.Pos.x, current.Pos.y,neighbor.Pos.x, neighbor.Pos.y);
                    if (!_openList.Contains(neighbor)) {
                        neighbor.SetData(newG, HeuristicFunction.CalculateHeuristic(
                            neighbor.X, neighbor.Y, target.x, target.y), current);
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

        private AStartPoint FindJumpPoint(AStartPoint current, int dx, int dy, AStartPoint target) {
            int newX = current.X + dx;
            int newY = current.Y + dy;
            
            if (!_map.IsPassable(newX, newY)) {
                return null;
            }

            bool isDirDiagonal = dx != 0 && dy != 0;
            // 对角线障碍判断
            if (isDirDiagonal &&
                (!_map.IsPassable(newX, current.Y) || !_map.IsPassable(current.X, newY))) {
                // 如果对角线方向存在障碍物，先沿着水平方向或垂直方向前进一格
                if (_map.IsPassable(newX, current.Y)) {
                    return _aStartMap[newX, current.Y];
                }
                if (_map.IsPassable(current.X, newY)) {
                    return _aStartMap[current.X, newY];
                }

                return null; // 如果没有合适的跳点，返回null
            }

            AStartPoint newPoint = _aStartMap[newX, newY];
            if (newPoint == target) {
                return newPoint;// 如果到达目标点，直接返回
            }

            // 检查强迫邻居
            if (isDirDiagonal) {// 对角线移动需要处理两个方向的跳点
                if (FindJumpPoint(newPoint, dx, 0, target) != null ||
                    FindJumpPoint(newPoint, 0, dy, target) != null) {
                    return newPoint;
                }
            } else {
                if (HasForcedNeighbors(newPoint, dx, dy)) {
                    return newPoint;
                }
            }

            // 继续递归
            return FindJumpPoint(newPoint, dx, dy, target);
        }

        private readonly List<AStartPoint> successors = new List<AStartPoint>();
        private readonly List<Vector2Int> directions = new List<Vector2Int>();
         public void GetSuccessors(AStartPoint point, AStartPoint end) {
             successors.Clear();
             directions.Clear();
             
            if (point.P != null) { // 有父节点，进行方向剪枝
                int dx = point.X - point.P.X;
                int dy = point.Y - point.P.Y;
                Vector2Int parentDir = new Vector2Int(Math.Sign(dx), Math.Sign(dy));
                PruneDirections(point, parentDir);
            } else { // 起点，所有方向都考虑
                directions.AddRange(SourceMap.Vector2DirectionDict.Keys);
            }

            foreach (var dir in directions) {
                AStartPoint jumpPoint = FindJumpPoint(point, dir.x, dir.y, end);
                if (jumpPoint != null) {
                    successors.Add(jumpPoint);
                }
            }
        }

        public void PruneDirections(AStartPoint point, Vector2Int parentDir) {
            int x = point.X;
            int y = point.Y;
            int dx = parentDir.x;
            int dy = parentDir.y;

            if (dx != 0 && dy != 0) { // 对角线方向
                bool dPassable = _map.IsPassable(x + dx, y + dy);
                bool hPassable = _map.IsPassable(x + dx, y);
                bool vPassable = _map.IsPassable(x, y + dy);
                // 保留前进方向
                if (dPassable || (hPassable && vPassable)) directions.Add(new Vector2Int(dx, dy));

                // 水平和垂直方向
                if (hPassable) directions.Add(new Vector2Int(dx, 0));
                if (vPassable) directions.Add(new Vector2Int(0, dy));

                // 处理强迫邻居
                if (!_map.IsPassable(x - dx, y) && _map.IsPassable(x - dx, y + dy))
                    directions.Add(new Vector2Int(-dx, dy));
                if (!_map.IsPassable(x, y - dy) && _map.IsPassable(x + dx, y - dy))
                    directions.Add(new Vector2Int(dx, -dy));
                return;
            }

            if (dx != 0) { // 水平方向
                // 处理对角线穿透后的搜索
                if ((!_map.IsPassable(x - dx, y + 1, false) && _map.IsPassable(x, y + 1, false)) || 
                    (!_map.IsPassable(x - dx, y - 1, false) && _map.IsPassable(x, y - 1, false))) {
                    foreach (var dir in SourceMap.Vector2DirectionDict.Keys) {
                        if (dir == -parentDir || !_map.IsPassable(x + dir.x, y + dir.y)) {
                            continue;
                        }
                        directions.Add(dir);
                    }
                    return;
                }
                
                // 保留前进方向
                if (_map.IsPassable(x + dx, y)) directions.Add(new Vector2Int(dx, 0));

                // 处理强迫邻居
                if (!_map.IsPassable(x, y + 1) && _map.IsPassable(x + dx, y + 1))
                    directions.Add(new Vector2Int(dx, 1));
                if (!_map.IsPassable(x, y - 1) && _map.IsPassable(x + dx, y - 1))
                    directions.Add(new Vector2Int(dx, -1));
                return;
            }

            // 垂直方向
            // 处理对角线穿透后的搜索
            if ((!_map.IsPassable(x + 1, y - dy, false) && _map.IsPassable(x + 1, y, false)) ||
                (!_map.IsPassable(x - 1, y - dy, false) && _map.IsPassable(x - 1, y, false))) {
                foreach (var dir in SourceMap.Vector2DirectionDict.Keys) {
                    if (dir == -parentDir || !_map.IsPassable(x + dir.x, y + dir.y)) {
                        continue;
                    }
                    directions.Add(dir);
                }
                return;
            }

            // 保留前进方向
            if (_map.IsPassable(x, y + dy)) directions.Add(new Vector2Int(0, dy));

            // 处理强迫邻居
            if (!_map.IsPassable(x + 1, y) && _map.IsPassable(x + 1, y + dy))
                directions.Add(new Vector2Int(1, dy));
            if (!_map.IsPassable(x - 1, y) && _map.IsPassable(x - 1, y + dy))
                directions.Add(new Vector2Int(-1, dy));
        }
        
        private bool HasForcedNeighbors(AStartPoint point, int dx, int dy) {
            int x = point.X;
            int y = point.Y;

            // 只检查与方向垂直的邻居
            if (dx != 0 && dy != 0) {// 防止穿越阻挡，对角线不认为是强迫邻居
                return (!_map.IsPassable(x - dx, y) && _map.IsPassable(x - dx, y + dy)) ||
                       (!_map.IsPassable(x, y - dy) && _map.IsPassable(x + dx, y - dy));
            }
            if (dx != 0) { // 水平方向
                return (!_map.IsPassable(x, y + 1) && _map.IsPassable(x + dx, y + 1)) ||
                       (!_map.IsPassable(x, y - 1) && _map.IsPassable(x + dx, y - 1));
            }
            // 垂直方向
            return (!_map.IsPassable(x + 1, y) && _map.IsPassable(x + 1, y + dy)) ||
                   (!_map.IsPassable(x - 1, y) && _map.IsPassable(x - 1, y + dy));
        }
        
        private List<Vector2Int> ReconstructPath(AStartPoint current) {
            List<Vector2Int> path = new List<Vector2Int>();
            while (current.P != null) {
                path.Insert(0, new Vector2Int(current.X, current.Y));
                current = current.P;
            }
            return path;
        }

        public void UpdateMap(RectInt bounds, bool passable) { }
        
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }
}
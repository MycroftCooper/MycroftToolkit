using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MapSystem.PathFinding {
    public class PathFinder : MonoBehaviour {
        #region 地图处理相关
        private GridPoint[,] _pointsMap;
        private int _mapWidth;
        private int _mapHeight;
        
        private bool IsPassable(int x, int y, bool checkEdge = true) {
            if (checkEdge) {
                return x >= 0 && x < _mapWidth &&
                       y >= 0 && y < _mapHeight &&
                       _pointsMap[x, y].IsPassable;
            }
            return x < 0 || x >= _mapWidth ||
                   y < 0 || y >= _mapHeight ||
                   _pointsMap[x, y].IsPassable;
        }
        
        [Button]
        public void SetPassableMap(bool[,] map) {
            if (isDebug) {
                map = TestMap;
            }
            
            _mapWidth= map.GetLength(0);
            _mapHeight= map.GetLength(1);
            _pointsMap = new GridPoint[_mapWidth, _mapHeight];
            for (int x = 0; x < _mapWidth; x++) {
                for (int y = 0; y < _mapHeight; y++) {
                    _pointsMap[x, y] = new GridPoint(x, y, map[x, y]);
                }
            }
            PreprocessJumpPoints();
            
            _openList = new PointLibrary(_maxSteps);
            _closeList = new PointLibrary(_maxSteps);
        }

        [Button]
        public void UpdatePassableMap(RectInt bounds, bool passable) {
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    _pointsMap[x, y].IsPassable = passable;
                }
            }
            // 更新地图时重新处理跳点
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    UpdateJumpPoints(new Vector2Int(x, y));
                }
            }
        }

        private void ResetPointsMap() {
            for (int x = 0; x < _mapWidth; x++) {
                for (int y = 0; y < _mapHeight; y++) {
                    _pointsMap[x, y].Reset();
                }
            }
            
            _openList.Clear();
            _closeList.Clear();
        }
        
        #endregion
        
        public void FindPath(PathFindingRequest request) {
            if (request == null) {
                Debug.LogError("PathFinder: request is null");
                return;
            }
            if (!IsPassable(request.StartPos.x, request.StartPos.y) || !IsPassable(request.EndPos.x, request.EndPos.y)) {
                Debug.LogError($"PathFinder: StartPos{request.StartPos} or EndPos{request.EndPos.x} is not passable or out of range");
                return;
            }

            if (!request.NeedHandleImmediately) {
                _requestQueue.Enqueue(request);
                return;
            }

            if (!request.CanUseCache) {
                var resultPath = FindPath(request.StartPos, request.EndPos);
                request.SetResultPath(resultPath);
            }
        }

        #region 寻路算法相关
        private Queue<PathFindingRequest> _requestQueue = new Queue<PathFindingRequest>();
        private Queue<PathFindingRequest> _pathCache;

        private int Heuristic(GridPoint a, GridPoint b) {
            int dx = Mathf.Abs(a.X - b.X);
            int dy = Mathf.Abs(a.Y - b.Y);
            return (int)Mathf.Sqrt(dx * dx + dy * dy);
        }

        #region Theta算法相关
        private bool LineOfSight(GridPoint parentGridPoint, GridPoint currentGridPoint) {
            int x0 = parentGridPoint.X;
            int y0 = parentGridPoint.Y;
            int x1 = currentGridPoint.X;
            int y1 = currentGridPoint.Y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
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
        #endregion

        #region 跳点相关

        private void PreprocessJumpPoints() {
            for (int x = 0; x < _mapWidth; x++) {
                for (int y = 0; y < _mapHeight; y++) {
                    if (!_pointsMap[x, y].IsPassable) {
                        continue;
                    }

                    for (int dir = 0; dir < GridPoint.Directions.Length; dir++) {
                        Vector2Int direction = GridPoint.Directions[dir];
                        Vector2Int currentJumpPoint = _pointsMap[x, y].JumpPoints[dir];

                        // 如果当前跳点未计算过，或者发现了一个新的跳点
                        if (currentJumpPoint == new Vector2Int(-1, -1)) {
                            Vector2Int jumpPoint = FindJumpPoint(_pointsMap[x, y], direction);
                            if (jumpPoint != new Vector2Int(-1, -1)) {
                                _pointsMap[x, y].JumpPoints[dir] = jumpPoint;
                            }
                        }
                    }
                }
            }
        }

        private Vector2Int FindJumpPoint(GridPoint startPoint, Vector2Int direction) {
            int x = startPoint.X;
            int y = startPoint.Y;
            int dx = direction.x;
            int dy = direction.y;

            while (true) {
                x += dx;
                y += dy;

                if (!IsPassable(x, y))
                    return new Vector2Int(-1, -1); // 无效跳点

                if (HasForcedNeighbor(_pointsMap[x, y], direction))
                    return new Vector2Int(x, y); // 找到跳点

                if (dx != 0 && dy != 0) {
                    Vector2Int jumpPoint;
                    if (IsPassable(x + dx, y)) {
                        jumpPoint = FindJumpPoint(_pointsMap[x + dx, y], direction);
                        if (jumpPoint != new Vector2Int(-1, -1)) {
                            return jumpPoint;
                        }
                    }

                    if (IsPassable(x, y + dy)) {
                        jumpPoint = FindJumpPoint(_pointsMap[x, y + dy], direction);
                        if (jumpPoint != new Vector2Int(-1, -1)) {
                            return jumpPoint;
                        }
                    }
                }
            }
        }

        public bool HasForcedNeighbor(GridPoint point, Vector2Int direction) {
            int x = point.X;
            int y = point.Y;
            int dx = direction.x;
            int dy = direction.y;

            // 对角线方向
            if (dx != 0 && dy != 0) {
                // 检查是否存在强迫邻居
                if ((IsPassable(x - dx, y + dy) && !IsPassable(x - dx, y)) ||
                    (IsPassable(x + dx, y - dy) && !IsPassable(x, y - dy)))
                    return true;
            }else if (dx != 0) { // 水平方向
                if ((IsPassable(x + dx, y + 1) && !IsPassable(x, y + 1)) ||
                    (IsPassable(x + dx, y - 1) && !IsPassable(x, y - 1)))
                    return true;
            }else if (dy != 0) { // 垂直方向
                if ((IsPassable(x + 1, y + dy) && !IsPassable(x + 1, y)) ||
                    (IsPassable(x - 1, y + dy) && !IsPassable(x - 1, y)))
                    return true;
            }
            return false;
        }

        public void UpdateJumpPoints(Vector2Int point) {

        }

        private GridPoint Jump(GridPoint current, Vector2Int direction, GridPoint end) {
            // 获取当前点在该方向上的预处理跳点
            Vector2Int jumpPoint = current.JumpPoints[GridPoint.GetDirectionIndex(direction)];
            if (jumpPoint != new Vector2Int(-1, -1)) return _pointsMap[jumpPoint.x, jumpPoint.y];
            
            int x = current.X;
            int y = current.Y;
            int dx = direction.x;
            int dy = direction.y;
            
            while (true) {
                x += dx;
                y += dy;
                
                if (!IsPassable(x, y)) return null;
                GridPoint nextPoint = _pointsMap[x, y];
                
                // 如果找到终点，直接返回该点
                if (nextPoint == end) return nextPoint;
                
                jumpPoint = nextPoint.JumpPoints[GridPoint.GetDirectionIndex(direction)];
                if (jumpPoint != new Vector2Int(-1, -1)) return _pointsMap[jumpPoint.x, jumpPoint.y];
                
                if (HasForcedNeighbor(_pointsMap[x, y], direction)) return nextPoint; // 找到跳点
                
                if (dx != 0 && dy != 0) {
                    GridPoint jp;
                    if (IsPassable(x + dx, y)) {
                        jp = Jump(_pointsMap[x + dx, y], direction, end);
                        if (jp != null) {
                            return jp;
                        }
                    }
                    if (IsPassable(x, y + dy)) {
                        jp = Jump(_pointsMap[x, y + dy], direction, end);
                        if (jp != null) {
                            return jp;
                        }
                    }
                }
            }
        }

        private readonly List<GridPoint> _successors = new List<GridPoint>();
        public List<GridPoint> GetSuccessors(GridPoint point, GridPoint end) {
            _successors.Clear();
            List<Vector2Int> directions;

            if (point.P != null) {
                // 有父节点，进行方向剪枝
                int dx = point.X - point.P.X;
                int dy = point.Y - point.P.Y;
                Vector2Int parentDir = new Vector2Int(Math.Sign(dx), Math.Sign(dy));
                directions = PruneDirections(point, parentDir);
            } else {
                // 起点，所有方向都考虑
                directions = new List<Vector2Int>(GridPoint.Directions);
            }

            foreach (var dir in directions) {
                GridPoint jumpPoint = Jump(point, dir, end);
                if (jumpPoint != null) {
                    _successors.Add(jumpPoint);
                }
            }

            return _successors;
        }

        private readonly List<Vector2Int> _pruned = new List<Vector2Int>();
        public List<Vector2Int> PruneDirections(GridPoint point, Vector2Int parentDir) {
            _pruned.Clear();
            int x = point.X;
            int y = point.Y;
            int dx = parentDir.x;
            int dy = parentDir.y;

            // 普通方向处理
            if (dx != 0 && dy != 0){// 对角线方向
                // 检查是否可以沿对角线方向移动
                bool canDiagonal = IsPassable(x + dx, y + dy) && IsPassable(x + dx, y) && IsPassable(x, y + dy);
                if (canDiagonal) {// 对角线方向
                    _pruned.Add(new Vector2Int(dx, dy));
                }
                if (IsPassable(x + dx, y)) {// 水平方向
                    _pruned.Add(new Vector2Int(dx, 0));
                }
                if (IsPassable(x, y + dy)) {// 垂直方向
                    _pruned.Add(new Vector2Int(0, dy));
                }
            }else if (dx != 0 && IsPassable(x + dx, y)) {// 水平方向
                _pruned.Add(new Vector2Int(dx, 0));
            }else if (dy != 0 && IsPassable(x, y + dy)){ // 垂直方向
                _pruned.Add(new Vector2Int(0, dy));
            }
            
            // 强迫邻居处理
            if (dx != 0 && dy != 0) { // 对角线方向
                // 左侧强迫邻居
                if (!IsPassable(x - dx, y) && IsPassable(x - dx, y + dy)) {
                    _pruned.Add(new Vector2Int(-dx, dy));
                }

                // 右侧强迫邻居
                if (!IsPassable(x, y - dy) && IsPassable(x + dx, y - dy)) {
                    _pruned.Add(new Vector2Int(dx, -dy));
                }
            }else if (dx != 0) { // 水平方向
                // 上方强迫邻居
                if (!IsPassable(x, y + 1) && IsPassable(x + dx, y + 1)) {
                    _pruned.Add(new Vector2Int(dx, 1));
                }

                // 下方强迫邻居
                if (!IsPassable(x, y - 1) && IsPassable(x + dx, y - 1)) {
                    _pruned.Add(new Vector2Int(dx, -1));
                }
            }
            else if (dy != 0) { // 垂直方向
                // 右侧强迫邻居
                if (!IsPassable(x + 1, y) && IsPassable(x + 1, y + dy)) {
                    _pruned.Add(new Vector2Int(1, dy));
                }

                // 左侧强迫邻居
                if (!IsPassable(x - 1, y) && IsPassable(x - 1, y + dy)) {
                    _pruned.Add(new Vector2Int(-1, dy));
                }
            }

            return _pruned;
        }

        #endregion

        private int _maxSteps = 100;
        private PointLibrary _openList;
        private PointLibrary _closeList;
        private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            ResetPointsMap();
            
            GridPoint startPoint = _pointsMap[start.x, start.y];
            GridPoint targetPoint = _pointsMap[target.x, target.y];
            
            // 初次使用 Theta* 直线检测，判断起点到终点是否连通
            if (LineOfSight(startPoint, targetPoint)) {
                return new List<Vector2Int> { target };
            }
            
            startPoint.SetData(0, Heuristic(startPoint, targetPoint), null);
            _openList.TryAdd(startPoint);

            while (_openList.Count > 0) {
                var currentPoint = _openList.PopMin();
                _closeList.TryAdd(currentPoint);

                // 找到目标点
                if (currentPoint == targetPoint) {
                    List<Vector2Int> path = RetracePath(startPoint, currentPoint);
                    if (doSmooth) {
                        SmoothPath(path);
                    }
                    return path;
                }

                List<GridPoint> successors = GetSuccessors(currentPoint, targetPoint);
                foreach (var neighbor in successors) {
                    if (neighbor == null || _closeList.Contains(neighbor)) {
                        continue; // 没有跳点或跳点已在关闭列表
                    }
                    
                    int newG = currentPoint.G + Heuristic(currentPoint, neighbor);

                    if (!_openList.Contains(neighbor) || newG < neighbor.G) {
                        neighbor.P = currentPoint;
                        neighbor.G = newG;
                        neighbor.H = Heuristic(neighbor, targetPoint);
                        neighbor.F = neighbor.G + neighbor.H;

                        if (_openList.Contains(neighbor)) {
                            _openList.Remove(neighbor); // 更新跳点位置
                        }
                        _openList.TryAdd(neighbor);
                    }
                }
            }

            return null; // 无法找到路径
        }
        
        private List<Vector2Int> RetracePath(GridPoint startGridPoint, GridPoint endGridPoint) {
            List<Vector2Int> path = new List<Vector2Int>();
            GridPoint currentGridPoint = endGridPoint;

            // 起点和终点必然是关键点
            path.Add(new Vector2Int(currentGridPoint.X, currentGridPoint.Y));

            // 开始路径回溯
            while (!Equals(currentGridPoint, startGridPoint)) {
                GridPoint parentGridPoint = currentGridPoint.P;

                if (parentGridPoint == null) {
                    Debug.LogError("Parent node is null during path retrace.");
                    break;  // 防止空引用异常
                }

                // 添加当前节点为关键点
                path.Add(new Vector2Int(parentGridPoint.X, parentGridPoint.Y));
                currentGridPoint = parentGridPoint;
            }
            
            // 最后加入起点为关键点
            path.Add(new Vector2Int(startGridPoint.X, startGridPoint.Y));

            // 反转路径，使其从起点到终点
            path.Reverse();
            return path;
        }
        #endregion

        #region 路线平滑后处理
        public bool doSmooth = true;
        public void SmoothPath(List<Vector2Int> path) {
            if (path.Count < 3) return;

            List<Vector2Int> smoothedPath = new List<Vector2Int> { path[0] }; // 起点

            for (int i = 1; i < path.Count - 1; i++) {
                // 检查是否可以从当前节点跳过中间节点，直接连接到下一个节点
                if (!HasObstacleBetweenTwoPoints(smoothedPath[^1], path[i + 1])) {
                    continue; // 如果没有障碍物，跳过当前节点
                }
                smoothedPath.Add(path[i]); // 加入当前节点作为路径中的关键点
            }

            smoothedPath.Add(path[^1]); // 终点
            path.Clear();
            path.AddRange(smoothedPath); // 用平滑后的路径替换原路径
        }

       private bool HasObstacleBetweenTwoPoints(Vector2Int start, Vector2Int end) {
           int x0 = start.x, y0 = start.y;
           int x1 = end.x, y1 = end.y;
            int xMin = Math.Min(x0, x1);
            int xMax = Math.Max(x0, x1);
            int yMin = Math.Min(y0, y1);
            int yMax = Math.Max(y0, y1);
            for (int x = xMin; x <= xMax; x++) {
                for (int y = yMin; y <= yMax; y++) {
                    if (!IsPassable(x, y) && LineInSquare((x0, y0), (x1, y1), (x, y))) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool LineInSquare((int x, int y) lineStart, (int x, int y) lineEnd, (int x, int y) squareCenter) {
            const float offset = 1f;
            var up = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y + offset), (squareCenter.x + offset, squareCenter.y + offset));
            var down = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y - offset), (squareCenter.x + offset, squareCenter.y - offset));
            var left = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y - offset), (squareCenter.x - offset, squareCenter.y + offset));
            var right = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x + offset, squareCenter.y - offset), (squareCenter.x + offset, squareCenter.y + offset));
            return up || down || left || right;
        }

        private static bool IsLineSegmentsIntersecting((int x, int y) startA, (int x, int y) endA, (float x, float y) startB, (float x, float y) endB) {
            float denominator = (endA.x - startA.x) * (endB.y - startB.y) - (endA.y - startA.y) * (endB.x - startB.x);

            // 两条线段平行
            if (denominator == 0) {
                return false;
            }

            float numerator1 = (startA.y - startB.y) * (endB.x - startB.x) - (startA.x - startB.x) * (endB.y - startB.y);
            float numerator2 = (startA.y - startB.y) * (endA.x - startA.x) - (startA.x - startB.x) * (endA.y - startA.y);

            // 两条线段重叠
            if (numerator1 == 0 && numerator2 == 0) {
                return true;
            }

            float r = numerator1 / denominator;
            float s = ((startA.y - startB.y) * (endA.x - startA.x) - (startA.x - startB.x) * (endA.y - startA.y)) / denominator;

            // 相交点不在两条线段上
            return !(r < 0) && !(r > 1) && !(s < 0) && !(s > 1);
        }

        #endregion

        #region Debug相关
        [ShowInInspector]
        public bool[,] TestMap = {
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true, false, false, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  }
        };
        public bool isDebug;
        public bool useOldPathFinder;
        private PathFindingRequest _debugRequest;
        private Stopwatch _stopwatch;

        [Button]
        private void DebugFindPath(Vector2Int start, Vector2Int end) {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            PathFindingRequest request = new PathFindingRequest(start, end, false, true);
            if (useOldPathFinder) {
                
            } else {
                FindPath(request);
            }
            _debugRequest = request;
            _stopwatch.Stop();
            Debug.Log($"Pathfinding completed in {_stopwatch.ElapsedMilliseconds} ms.");
        }
        
        void OnDrawGizmos() {
            if (!isDebug || _pointsMap == null) {
                return;
            }

            Gizmos.color = Color.gray; // 默认颜色设置为灰色
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);

            // 遍历 passableMap 并绘制格子
            for (int x = 0; x < _mapWidth; x++) {
                for (int y = 0; y < _mapHeight; y++) {
                    Vector3 position = oPos + new Vector3(x, y, 0); // 将网格位置映射到世界空间
                    Gizmos.color = _pointsMap[x, y].IsPassable  ? Color.gray : Color.red;
                    Gizmos.DrawCube(position, new Vector3(1, 1, 0.1f));
                    Gizmos.color = Color.black; // 边框颜色设置为黑色
                    Gizmos.DrawWireCube(position, Vector3.one);
                    if (_pointsMap[x, y].JumpPoints != null && _pointsMap[x, y].JumpPoints.Any(p => p != -Vector2Int.one)) {
                        Gizmos.color = Color.yellow; // 边框颜色设置为黑色
                        Gizmos.DrawCube(position, new Vector3(0.8f, 0.8f, 0.1f));
                    }
                }
            }

            DrawPath(_debugRequest);
        }
        
        private void DrawPath(PathFindingRequest path) {
            if (path == null || path.ResultPath == null || path.ResultPath.Count == 0) {
                return;
            }
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);
            Debug.DrawLine(path.StartPos.ToVec3() + oPos, path.ResultPath[0].ToVec3() + oPos, Color.blue);
            for (int i = 0; i < path.ResultPath.Count - 1; i++) {
                Vector3 startPos = oPos + new Vector3(path.ResultPath[i].x, path.ResultPath[i].y, 0.2f);
                Vector3 endPos = oPos + new Vector3(path.ResultPath[i + 1].x, path.ResultPath[i + 1].y, 0.2f);
                Debug.DrawLine(startPos, endPos, Color.blue);
            }
        }
        #endregion
    }
}
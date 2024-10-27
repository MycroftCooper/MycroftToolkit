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

        [Button]
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
                if (isDebug) {
                    _stopwatch = new Stopwatch();
                    _stopwatch.Start();
                }
                var resultPath = FindPath(request.StartPos, request.EndPos);
                request.SetResultPath(resultPath);
                if (isDebug) {
                    _stopwatch.Stop();
                    Debug.Log($"Pathfinding completed in {_stopwatch.ElapsedMilliseconds} ms.");
                    _debugRequest = request;
                }
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
        
        private readonly List<GridPoint> _neighbours = new List<GridPoint>(8); // 复用邻居列表
        private List<GridPoint> GetNeighbours(GridPoint currentGridPoint) {
            _neighbours.Clear();
            // 检查当前节点的四周八方向
            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    if (dx == 0 && dy == 0) continue;

                    int newX = currentGridPoint.X + dx;
                    int newY = currentGridPoint.Y + dy;

                    if (!IsPassable(newX, newY)) {
                        continue;
                    }
                    
                    // 如果是对角线方向，则需要检查水平和垂直方向的合法性
                    if (dx != 0 && dy != 0) {
                        if (!IsPassable(currentGridPoint.X + dx, currentGridPoint.Y, false) ||
                            !IsPassable(currentGridPoint.X, currentGridPoint.Y + dy, false)) {
                            // 如果水平或垂直方向阻挡，则不允许沿对角线前进
                            continue;
                        }
                    }

                    GridPoint neighbour = _pointsMap[newX, newY];
                    _neighbours.Add(neighbour);
                }
            }

            return _neighbours;
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
            for (int y = 0; y < _mapHeight; y++) {
                for (int x = 0; x < _mapWidth; x++) {
                    GridPoint point = _pointsMap[x, y];
                    if (!point.IsPassable)
                        continue;

                    for (int dirIndex = 0; dirIndex < GridPoint.Directions.Length; dirIndex++) {
                        Vector2Int direction = GridPoint.Directions[dirIndex];
                        Vector2Int jumpPoint = FindJumpPoint(point, direction);
                        point.JumpPoints[dirIndex] = jumpPoint;
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

                if (HasForcedNeighbor(startPoint, direction))
                    return new Vector2Int(x, y); // 找到跳点

                // 对于对角线方向，需要检查水平和垂直方向的跳点
                if (dx != 0 && dy != 0) {
                    if (IsPassable(x - dx, y + dy) && !IsPassable(x - dx, y))
                        return new Vector2Int(x, y);
                    if (IsPassable(x + dx, y - dy) && !IsPassable(x, y - dy))
                        return new Vector2Int(x, y);
                }
            }
        }
        
        public bool HasForcedNeighbor(GridPoint point, Vector2Int direction) {
            int x = point.X;
            int y = point.Y;

            if (direction.x != 0 && direction.y != 0) { // 对角线方向
                if ((IsPassable(x - direction.x, y + direction.y) && !IsPassable(x - direction.x, y)) ||
                    (IsPassable(x + direction.x, y - direction.y) && !IsPassable(x, y - direction.y)))
                    return true;
            } else {
                if (direction.x != 0) { // 垂直方向
                    if ((IsPassable(x + direction.x, y + 1) && !IsPassable(x, y + 1)) ||
                        (IsPassable(x + direction.x, y - 1) && !IsPassable(x, y - 1)))
                        return true;
                } else { // 水平方向
                    if ((IsPassable(x + 1, y + direction.y) && !IsPassable(x + 1, y)) ||
                        (IsPassable(x - 1, y + direction.y) && !IsPassable(x - 1, y)))
                        return true;
                }
            }
            return false;
        }
        
        public void UpdateJumpPoints(Vector2Int point) {
            // int x = point.x;
            // int y = point.y;
            //
            // // 更新自身的跳点数据
            // for (int d = 0; d < Directions.Length; d++) {
            //     _jumpPoints[x, y][d] = CalculateJumpPoint(x, y, Directions[d].x, Directions[d].y);
            // }
            //
            // // 更新相邻点的跳点数据
            // for (int i = -1; i <= 1; i++) {
            //     for (int j = -1; j <= 1; j++) {
            //         if (i == 0 && j == 0) continue; // 跳过自身
            //
            //         int nx = x + i;
            //         int ny = y + j;
            //
            //         // 确保相邻点在地图范围内
            //         if (nx >= 0 && nx < _mapWidth && ny >= 0 && ny < _mapHeight) {
            //             // 重新计算指向 P 点方向的跳点
            //             for (int d = 0; d < Directions.Length; d++) {
            //                 _jumpPoints[nx, ny][d] = CalculateJumpPoint(nx, ny, Directions[d].x, Directions[d].y);
            //             }
            //         }
            //     }
            // }
        }

        private GridPoint Jump(GridPoint current, Vector2Int direction, GridPoint end) {
            int x = current.X + direction.x;
            int y = current.Y + direction.y;

            if (!IsPassable(x, y)) return null;

            GridPoint nextPoint = _pointsMap[x, y];

            if (nextPoint.Equals(end)) return nextPoint;

            // 检查是否存在强迫邻居
            if (HasForcedNeighbor(nextPoint, direction)) return nextPoint;

            // 对于对角线方向，需要检查水平和垂直方向
            if (direction.x != 0 && direction.y != 0) {
                GridPoint horizontalJump = Jump(nextPoint, new Vector2Int(direction.x, 0), end);
                GridPoint verticalJump = Jump(nextPoint, new Vector2Int(0, direction.y), end);
                if (horizontalJump != null || verticalJump != null) return nextPoint;
            }

            // 递归调用
            return Jump(nextPoint, direction, end);
        }

        public List<GridPoint> GetSuccessors(GridPoint point, GridPoint end) {
            List<GridPoint> successors = new List<GridPoint>();
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
                    successors.Add(jumpPoint);
                }
            }

            return successors;
        }

        public List<Vector2Int> PruneDirections(GridPoint point, Vector2Int parentDir) {
            List<Vector2Int> pruned = new List<Vector2Int>();
            int dx = parentDir.x;
            int dy = parentDir.y;

            // 对角线方向
            if (dx != 0 && dy != 0) {
                // 保留前进方向
                if (IsPassable(point.X + dx, point.Y + dy)) pruned.Add(new Vector2Int(dx, dy));

                // 水平和垂直方向
                if (IsPassable(point.X + dx, point.Y)) pruned.Add(new Vector2Int(dx, 0));

                if (IsPassable(point.X, point.Y + dy)) pruned.Add(new Vector2Int(0, dy));

                // 处理强迫邻居
                if (!IsPassable(point.X - dx, point.Y) && IsPassable(point.X - dx, point.Y + dy))
                    pruned.Add(new Vector2Int(-dx, dy));

                if (!IsPassable(point.X, point.Y - dy) && IsPassable(point.X + dx, point.Y - dy))
                    pruned.Add(new Vector2Int(dx, -dy));
            } else {
                // 水平或垂直方向
                if (dx != 0) {
                    if (IsPassable(point.X + dx, point.Y)) pruned.Add(new Vector2Int(dx, 0));

                    // 处理强迫邻居
                    if (!IsPassable(point.X, point.Y + 1) && IsPassable(point.X + dx, point.Y + 1))
                        pruned.Add(new Vector2Int(dx, 1));

                    if (!IsPassable(point.X, point.Y - 1) && IsPassable(point.X + dx, point.Y - 1))
                        pruned.Add(new Vector2Int(dx, -1));
                } else if (dy != 0) {
                    if (IsPassable(point.X, point.Y + dy)) pruned.Add(new Vector2Int(0, dy));

                    // 处理强迫邻居
                    if (!IsPassable(point.X + 1, point.Y) && IsPassable(point.X + 1, point.Y + dy))
                        pruned.Add(new Vector2Int(1, dy));

                    if (!IsPassable(point.X - 1, point.Y) && IsPassable(point.X - 1, point.Y + dy))
                        pruned.Add(new Vector2Int(-1, dy));
                }
            }

            return pruned;
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
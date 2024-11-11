using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding {
    public class JPSPlus : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.JPSPlus;
        private SourceMap _map;
        private JPSPlusPoint[,] _pointsMap;
        
        public void InitMap(SourceMap map) {
            _map = map;
            _pointsMap = new JPSPlusPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _pointsMap[x, y] = new JPSPlusPoint(x, y);
                }
            }
            PreprocessJumpPoints();
            
            _openList = new PointLibrary(_map.Width * _map.Height);
            _closeList = new PointLibrary(_map.Width * _map.Height);
        }

        public void UpdateMap(RectInt bounds, bool passable) {
            _map.UpdateMap(bounds, passable);
            // 更新地图时重新处理跳点
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    UpdateJumpPoints(new Vector2Int(x, y));
                }
            }
        }
        
        private int Heuristic(JPSPlusPoint a, JPSPlusPoint b) {
            int dx = Mathf.Abs(a.X - b.X);
            int dy = Mathf.Abs(a.Y - b.Y);
            return (int)Mathf.Sqrt(dx * dx + dy * dy);
        }
        
        #region Theta算法相关
        private bool LineOfSight(JPSPlusPoint parentJpsPlusPoint, JPSPlusPoint currentJpsPlusPoint) {
            int x0 = parentJpsPlusPoint.X;
            int y0 = parentJpsPlusPoint.Y;
            int x1 = currentJpsPlusPoint.X;
            int y1 = currentJpsPlusPoint.Y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                // 如果当前格子不可通过，返回 false
                if (!_map.IsPassable(x0, y0)) return false;

                // 检查对角线穿越障碍的情况
                if (dx != 0 && dy != 0) { // 如果沿对角线移动
                    if (!_map.IsPassable(x0 - sx, y0, false) && !_map.IsPassable(x0, y0 - sy, false)) {
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
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    if (!_map.IsPassable(x, y)) {
                        continue;
                    }

                    for (int dir = 0; dir < (int)SourceMap.Directions.Length; dir++) {
                        Vector2Int direction = SourceMap.Direction2VectorDict[(SourceMap.Directions)dir];
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

        private Vector2Int FindJumpPoint(JPSPlusPoint startPoint, Vector2Int direction) {
            int x = startPoint.X;
            int y = startPoint.Y;
            int dx = direction.x;
            int dy = direction.y;

            while (true) {
                x += dx;
                y += dy;

                if (!_map.IsPassable(x, y))
                    return new Vector2Int(-1, -1); // 无效跳点

                if (HasForcedNeighbor(_pointsMap[x, y], direction))
                    return new Vector2Int(x, y); // 找到跳点

                if (dx != 0 && dy != 0) {
                    Vector2Int jumpPoint;
                    if (_map.IsPassable(x + dx, y)) {
                        jumpPoint = FindJumpPoint(_pointsMap[x + dx, y], direction);
                        if (jumpPoint != new Vector2Int(-1, -1)) {
                            return jumpPoint;
                        }
                    }

                    if (_map.IsPassable(x, y + dy)) {
                        jumpPoint = FindJumpPoint(_pointsMap[x, y + dy], direction);
                        if (jumpPoint != new Vector2Int(-1, -1)) {
                            return jumpPoint;
                        }
                    }
                }
            }
        }

        public bool HasForcedNeighbor(JPSPlusPoint point, Vector2Int direction) {
            int x = point.X;
            int y = point.Y;
            int dx = direction.x;
            int dy = direction.y;

            // 对角线方向
            if (dx != 0 && dy != 0) {
                // 检查是否存在强迫邻居
                if ((_map.IsPassable(x - dx, y + dy) && !_map.IsPassable(x - dx, y)) ||
                    (_map.IsPassable(x + dx, y - dy) && !_map.IsPassable(x, y - dy)))
                    return true;
            }else if (dx != 0) { // 水平方向
                if ((_map.IsPassable(x + dx, y + 1) && !_map.IsPassable(x, y + 1)) ||
                    (_map.IsPassable(x + dx, y - 1) && !_map.IsPassable(x, y - 1)))
                    return true;
            }else if (dy != 0) { // 垂直方向
                if ((_map.IsPassable(x + 1, y + dy) && !_map.IsPassable(x + 1, y)) ||
                    (_map.IsPassable(x - 1, y + dy) && !_map.IsPassable(x - 1, y)))
                    return true;
            }
            return false;
        }

        public void UpdateJumpPoints(Vector2Int point) {

        }

        #endregion
        
        private void ResetPointsMap() {
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _pointsMap[x, y].Reset();
                }
            }
            
            _openList.Clear();
            _closeList.Clear();
        }
        
        private PointLibrary _openList;
        private PointLibrary _closeList;
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            ResetPointsMap();
            
            JPSPlusPoint startPoint = _pointsMap[start.x, start.y];
            JPSPlusPoint targetPoint = _pointsMap[target.x, target.y];
            
            // 初次使用 Theta* 直线检测，判断起点到终点是否连通
            // if (LineOfSight(startPoint, targetPoint)) {
            //     return new List<Vector2Int> { target };
            // }
            
            startPoint.SetData(0, Heuristic(startPoint, targetPoint), null);
            _openList.TryAdd(startPoint);

            while (_openList.Count > 0) {
                var currentPoint = (JPSPlusPoint)_openList.PopMin();
                _closeList.TryAdd(currentPoint);

                // 找到目标点
                if (currentPoint == targetPoint) {
                    List<Vector2Int> path = RetracePath(startPoint, currentPoint);
                    return path;
                }

                // 根据是否有跳点数据，选择相应的处理方法
                if (currentPoint.JumpPoints.All(jp => jp == new Vector2Int(-1, -1))) {
                    HandleAdjacentPoints(currentPoint, targetPoint);
                } else {
                    HandleJumpPoints(currentPoint, targetPoint);
                }
            }

            return new List<Vector2Int>(); // 如果没有找到路径，返回空列表
        }

        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) {
            int x = targetPos.x;
            int y = targetPos.y;
            if (_pointsMap[x, y].JumpPoints != null && _pointsMap[x, y].JumpPoints.Any(p => p != -Vector2Int.one)) {
                Vector3 position = originPos + new Vector3(x, y, 0); // 将网格位置映射到世界空间
                Gizmos.color = Color.yellow; // 边框颜色设置为黑色
                Gizmos.DrawCube(position, new Vector3(0.8f, 0.8f, 0.1f));
            }
        }

        // 处理没有跳点数据的邻接节点的函数
        private void HandleAdjacentPoints(JPSPlusPoint currentPoint, JPSPlusPoint targetPoint) {
            foreach (Vector2Int direction in SourceMap.Direction2VectorDict.Values) {
                int adjX = currentPoint.X + direction.x;
                int adjY = currentPoint.Y + direction.y;

                if (!_map.IsPassable(adjX, adjY)) continue; // 邻接点不可通行，跳过

                JPSPlusPoint adjPoint = _pointsMap[adjX, adjY];

                if (_closeList.Contains(adjPoint)) continue; // 邻接点已在闭合列表中，跳过

                // 计算G和H值
                int gCost = currentPoint.G + Heuristic(currentPoint, adjPoint);
                int hCost = Heuristic(adjPoint, targetPoint);
                adjPoint.SetData(gCost, hCost, currentPoint);

                // 如果邻接点不在开放列表中，将其添加
                if (!_openList.Contains(adjPoint)) {
                    _openList.TryAdd(adjPoint);
                }
            }
        }

        // 处理有跳点数据的节点
        private void HandleJumpPoints(JPSPlusPoint currentPoint, JPSPlusPoint targetPoint) {
            foreach (Vector2Int jump in currentPoint.JumpPoints) {
                int jumpX = jump.x;
                int jumpY = jump.y;

                if (jumpX == -1 && jumpY == -1) continue; // 无效跳点，跳过

                JPSPlusPoint jumpPoint = _pointsMap[jumpX, jumpY];

                if (!_map.IsPassable(jumpX, jumpY) || _closeList.Contains(jumpPoint)) continue; // 跳点不可通行或已在闭合列表中，跳过

                // 计算G和H值
                int gCost = currentPoint.G + Heuristic(currentPoint, jumpPoint);
                int hCost = Heuristic(jumpPoint, targetPoint);
                jumpPoint.SetData(gCost, hCost, currentPoint);

                // 如果跳点不在开放列表中，添加它
                _openList.TryAdd(jumpPoint);
            }
        }
        
        private List<Vector2Int> RetracePath(JPSPlusPoint startJpsPlusPoint, JPSPlusPoint endJpsPlusPoint) {
            List<Vector2Int> path = new List<Vector2Int>();
            JPSPlusPoint currentJpsPlusPoint = endJpsPlusPoint;

            // 起点和终点必然是关键点
            path.Add(new Vector2Int(currentJpsPlusPoint.X, currentJpsPlusPoint.Y));

            // 开始路径回溯
            while (!Equals(currentJpsPlusPoint, startJpsPlusPoint)) {
                JPSPlusPoint parentJpsPlusPoint = (JPSPlusPoint)currentJpsPlusPoint.P;

                if (parentJpsPlusPoint == null) {
                    Debug.LogError("Parent node is null during path retrace.");
                    break;  // 防止空引用异常
                }

                // 添加当前节点为关键点
                path.Add(new Vector2Int(parentJpsPlusPoint.X, parentJpsPlusPoint.Y));
                currentJpsPlusPoint = parentJpsPlusPoint;
            }
            
            // 最后加入起点为关键点
            path.Add(new Vector2Int(startJpsPlusPoint.X, startJpsPlusPoint.Y));

            // 反转路径，使其从起点到终点
            path.Reverse();
            return path;
        }
    }
    
    public class JPSPlusPoint : AStartPoint {
        public Vector2Int[] JumpPoints; // 跳点信息存储结构：每个节点在8个方向的跳点
        public JPSPlusPoint(int x, int y): base(x,y) {
            JumpPoints = new Vector2Int[8];
            for (int i = 0; i < 8; i++) {
                JumpPoints[i] = new Vector2Int(-1, -1); // 初始化为无效位置
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PathFinding {
    public class JPSPlus : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.JPSPlus;
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        private SourceMap _map;
        private JPSPlusPoint[,] _pointsMap;
        
        public void InitMap(SourceMap map) {
            _map = map;
            PreprocessJumpMap();
            
            int maxF = HeuristicFunction.CalculateMaxFCost(new Vector2Int(_map.Width, _map.Height));
            int bucketCount = Mathf.CeilToInt(Mathf.Sqrt(_map.Width * _map.Height));
            int bucketSize = maxF / bucketCount;
            _openList = new BucketPriorityQueue<JPSPlusPoint>(maxF, bucketSize);
            _closedList = new HashSet<JPSPlusPoint>();
        }

        public void UpdateMap(RectInt bounds, bool passable) {
            // 队列存储待更新的节点，HashSet 防止重复加入
            Queue<Vector2Int> updateQueue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            // 将 bounds 范围内的所有点加入队列
            for (int x = bounds.xMin; x <= bounds.xMax; x++) {
                for (int y = bounds.yMin; y <= bounds.yMax; y++) {
                    Vector2Int point = new Vector2Int(x, y);
                    updateQueue.Enqueue(point);
                    visited.Add(point);
                }
            }

            // 处理更新队列
            while (updateQueue.Count > 0) {
                Vector2Int current = updateQueue.Dequeue();
                int x = current.x;
                int y = current.y;

                // 重新计算当前点的 JumpStep 值
                var point = _pointsMap[x, y];
                if (!_map.IsPassable(x, y)) {
                    for (int i = 0; i < 8; i++) {
                        point.JumpStep[i] = 0; // 如果点不可通行，所有方向的步数置为 0
                    }
                } else {
                    for (int dirIndex = 0; dirIndex < 8; dirIndex++) {
                        Vector2Int dir = SourceMap.Direction2VectorDict[(SourceMap.Directions)dirIndex];
                        int oldStep = point.JumpStep[dirIndex];
                        int newStep = CalculateStep(x, y, dir);

                        
                        if (oldStep == newStep) continue;
                        // 如果步数发生变化，更新并将受影响的点加入队列
                        point.JumpStep[dirIndex] = newStep;

                        // 沿当前方向扩展影响范围
                        for (int step = 1; step <= oldStep; step++) {
                            int affectedX = x + dir.x * step;
                            int affectedY = y + dir.y * step;

                            if (!_map.IsInBounds(affectedX, affectedY)) break;

                            Vector2Int affectedPoint = new Vector2Int(affectedX, affectedY);
                            if (!visited.Contains(affectedPoint)) {
                                updateQueue.Enqueue(affectedPoint);
                                visited.Add(affectedPoint);
                            }
                        }
                    }
                }
            }
        }

        #region 预处理相关
        private void PreprocessJumpMap() {
            _pointsMap = new JPSPlusPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _pointsMap[x, y] = new JPSPlusPoint(x, y);
                }
            }
            
            // 遍历地图，计算每个点的JumpStep
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    var point = _pointsMap[x, y];
                    if (!_map.IsPassable(x, y)) {
                        // 如果当前节点不可通行，跳点信息全部为0
                        for (int i = 0; i < 8; i++) {
                            point.JumpStep[i] = 0;
                        }
                        continue;
                    }

                    // 计算八个方向的JumpStep
                    for (int dirIndex = 0; dirIndex < 8; dirIndex++) {
                        Vector2Int dir = SourceMap.Direction2VectorDict[(SourceMap.Directions)dirIndex];
                        point.JumpStep[dirIndex] = CalculateStep(x, y, dir);
                    }
                }
            }
        }

        private int CalculateStep(int startX, int startY, Vector2Int dir) {
            int step = 0;
            int x = startX;
            int y = startY;
            int dx = dir.x;
            int dy = dir.y;

            while (true) {
                x += dir.x;
                y += dir.y;
                step++;
                if (!_map.IsPassable(x, y)) {// 障碍或越界
                    return step - 1;
                }
                if (!_map.CanMoveTo(x - dx, y - dy, dx, dy)) {
                    return step - 1;
                }
                if (HasForcedNeighbors(_pointsMap[x, y], dir.x, dir.y)) {
                    return step;// 遇到跳点
                }

                if (_map.CanDiagonallyPassByObstacle || (dx != 0 && dy != 0)) continue;
                // 处理对角线穿透后的搜索
                if (dy == 0) {
                    // 水平方向
                    if ((!_map.IsPassable(x - dx, y + 1, false) && _map.IsPassable(x, y + 1, false)) ||
                        (!_map.IsPassable(x - dx, y - 1, false) && _map.IsPassable(x, y - 1, false))) {
                        return step;
                    }
                } else {
                    if ((!_map.IsPassable(x + 1, y - dy, false) && _map.IsPassable(x + 1, y, false)) ||
                        (!_map.IsPassable(x - 1, y - dy, false) && _map.IsPassable(x - 1, y, false))) {
                        return step;
                    }
                }
            }
        }
        
        private bool HasForcedNeighbors(JPSPlusPoint point, int dx, int dy) {
            int x = point.X;
            int y = point.Y;

            // 只检查与方向垂直的邻居
            if (dx != 0 && dy != 0) {
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
        #endregion
        
        private BucketPriorityQueue<JPSPlusPoint> _openList;
        private HashSet<JPSPlusPoint> _closedList;
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            _openList.Clear();
            _closedList.Clear();
            _openList.NeedBestSolution = NeedBestSolution;
            
            var startPoint = _pointsMap[start.x, start.y];
            var targetPoint = _pointsMap[target.x, target.y];
            int tx = target.x;
            int ty = target.y;
            
            startPoint.SetData(0, HeuristicFunction.CalculateHeuristic(start.x, start.y, target.x, target.y), null); // 设置起点数据
            _openList.Add(startPoint);
            
            // 主循环
            while (_openList.Count > 0) {
                // 从 openList 中取出 F 值最小的节点
                var current = _openList.DequeueMin();
                _closedList.Add(current);

                // 如果到达目标节点，重建路径
                if (current == targetPoint) {
                    return ReconstructPath(targetPoint);
                }
                
                int cx = current.X;
                int cy = current.Y;
                
                GetSuccessors(current, targetPoint);
                foreach (var neighbor in _successors) {
                    if (_closedList.Contains(neighbor)) {
                        continue; // 没有跳点或跳点已在关闭列表
                    }
                    int nx = neighbor.X;
                    int ny = neighbor.Y;
                    int tentativeG = current.G + HeuristicFunction.CalculateHeuristic(cx, cy,nx, ny);
                    if (!_openList.Contains(neighbor)) {
                        // 如果是新节点，加入 openList
                        neighbor.SetData(tentativeG, HeuristicFunction.CalculateHeuristic(nx, ny, tx, ty), current);
                        _openList.Add(neighbor);
                    } else if (tentativeG < neighbor.G) {
                        _openList.Remove(neighbor);
                        neighbor.SetData(tentativeG, current);// 如果新的路径更短，更新
                        _openList.Add(neighbor);
                    }
                }
            }

            // 如果未找到路径，返回 null
            return null;
        }
        
        private readonly List<JPSPlusPoint> _successors = new List<JPSPlusPoint>();
        private void GetSuccessors(JPSPlusPoint current, JPSPlusPoint target) {
            _successors.Clear();
            Vector2Int targetV = target.Pos - current.Pos;
            int targetDirX = Math.Sign(targetV.x);
            int targetDirY = Math.Sign(targetV.y);
            
            Vector2Int parentDir = Vector2Int.zero;
            if (current.P != null) {
                int dx = current.X - current.P.X;
                int dy = current.Y - current.P.Y;
                parentDir = new Vector2Int(Math.Sign(dx), Math.Sign(dy));
            } 

            for (int dirIndex = 0; dirIndex < 8; dirIndex++) {
                Vector2Int dir = SourceMap.Direction2VectorDict[(SourceMap.Directions)dirIndex];
                if(dir == -parentDir) continue;
                
                int step = current.JumpStep[dirIndex];
                if (step == 0) continue; // 如果方向上没有可扩展步数，跳过
                
                Vector2Int tp;
                if ((targetDirX == 0 || dir.x == targetDirX) &&
                    (targetDirY == 0 || dir.y == targetDirY)) {
                    int actualStep = dir.x != 0 && dir.y != 0 ? 
                        Math.Min(step, Math.Min(targetV.x, targetV.y)) : Math.Min(step, targetV.x + targetV.y);
                    tp = current.Pos + dir * actualStep;
                    _successors.Add(_pointsMap[tp.x, tp.y]);
                    continue;
                }
                
                tp = current.Pos + dir * step;
                var dp = tp + dir;// 通过再移一格是否出界来判断是跳点还是边界
                if (_map.IsInBounds(dp.x, dp.y)) {// 如果是跳点，直接加入
                    _successors.Add(_pointsMap[tp.x, tp.y]);
                }
            }
        }

        private List<Vector2Int> ReconstructPath(JPSPlusPoint current) {
            List<Vector2Int> path = new List<Vector2Int>();
            while (current.P != null) {
                path.Insert(0, current.Pos);
                current = current.P;
            }
            path.Insert(0, current.Pos);
            return path;
        }

        private readonly GUIStyle _style = new GUIStyle {
            fontSize = 20, // 设置字体大小
            normal = {
                textColor = Color.yellow // 设置字体颜色
            },
        };
        
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) {
            int x = targetPos.x;
            int y = targetPos.y;

            Vector3 position = originPos + new Vector3(x, y, -2); // 将网格位置映射到世界空间

            JPSPlusPoint point = _pointsMap[targetPos.x, targetPos.y];
            for (int dirIndex = 0; dirIndex < 8; dirIndex++) {
                Vector2Int dir = SourceMap.Direction2VectorDict[(SourceMap.Directions)dirIndex];
                Vector3 p = position + new Vector3(dir.x, dir.y, 0) * 0.2f;
                Handles.Label(p, point.JumpStep[dirIndex].ToString(), _style);
            }
        }
    }
    
    public class JPSPlusPoint : BucketPriorityQueue<JPSPlusPoint>.IBucketPriorityQueueItem {
        public readonly int X, Y;
        public readonly Vector2Int Pos;
        public JPSPlusPoint P;
        public int G; // 从起点到当前节点的代价 
        public int H; // 从当前节点到终点的预估代价
        public int F; // G + H;
        public int PriorityValue => F;
        
        public int[] JumpStep; // 跳点信息存储结构：每个节点在8个方向的跳点
        public JPSPlusPoint(int x, int y) {
            X = x;
            Y = y;
            Pos = new Vector2Int(X, Y);
            JumpStep = new int[8];
            Reset();
        }
        
        public void SetData(int g, int h, JPSPlusPoint p) {
            G = g;
            H = h;
            F = G + H;
            P = p;
        }
        
        public void SetData(int g, JPSPlusPoint p) {
            G = g;
            F = G + H;
            P = p;
        }

        public void Reset() {
            G = H = 0;
            F = -1;
            P = null;
            for (int i = 0; i < JumpStep.Length; i++) {
                JumpStep[i] = 0;
            }
        }
        
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"point({X},{Y})[g:{G}; h:{H} f:{F}]";
    }
}
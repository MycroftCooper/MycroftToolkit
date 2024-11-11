using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class AStart : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.AStar;
        private SourceMap _map;
        private AStartPoint[,] _aStartMap;
        
        public void InitMap(SourceMap map) {
            _map = map;
            _aStartMap = new AStartPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _aStartMap[x, y] = new AStartPoint(x, y);
                }
            }
        }

        public void UpdateMap(RectInt bounds, bool passable) {
            _map.UpdateMap(bounds, passable);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            var openList = new List<AStartPoint>(); // 节点开放列表
            var closedList = new HashSet<AStartPoint>(); // 节点封闭列表

            AStartPoint startPoint = _aStartMap[start.x, start.y];
            AStartPoint targetPoint = _aStartMap[target.x, target.y];

            startPoint.SetData(0, CalculateHeuristic(start, target), null); // 设置起点数据
            openList.Add(startPoint);

            while (openList.Count > 0) {
                // 从开放列表中取出F值最小的节点
                openList.Sort((a, b) => a.F.CompareTo(b.F));
                AStartPoint current = openList[0];
                openList.RemoveAt(0);
                closedList.Add(current);

                // 如果到达目标节点，回溯路径
                if (current == targetPoint) {
                    return ReconstructPath(current);
                }

                // 遍历当前节点的相邻节点
                foreach (var direction in SourceMap.Direction2VectorDict.Values) {
                    Vector2Int neighborPosition = new Vector2Int(current.X + direction.x, current.Y + direction.y);
                    
                    if (!_map.IsPassable(neighborPosition.x, neighborPosition.y) ||
                        (direction.x != 0 && direction.y != 0 && // 对角线障碍判断
                         (!_map.IsPassable(neighborPosition.x, current.Y) || 
                          !_map.IsPassable(current.X, neighborPosition.y)))) 
                        continue;
                    
                    AStartPoint neighbor = _aStartMap[neighborPosition.x, neighborPosition.y];

                    // 如果相邻节点已经在封闭列表中，跳过
                    if (closedList.Contains(neighbor)) continue;

                    // 计算G值、H值和F值
                    int tentativeG = current.G + 1; // 设定步长为1
                    if (!openList.Contains(neighbor)) {
                        neighbor.SetData(tentativeG, CalculateHeuristic(neighborPosition, target), current);
                        openList.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.G) {
                        // 如果新的路径更短，更新
                        neighbor.SetData(tentativeG, current);
                    }
                }
            }
            return null; // 如果没有找到路径，返回空
        }

        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }

        private int CalculateHeuristic(Vector2Int a, Vector2Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 曼哈顿距离
        }
        
        // 回溯路径
        private List<Vector2Int> ReconstructPath(AStartPoint current) {
            List<Vector2Int> path = new List<Vector2Int>();
            while (current.P != null) {
                path.Insert(0, new Vector2Int(current.X, current.Y));
                current = current.P;
            }
            return path;
        }
    }
    
    public class AStartPoint {
        public readonly int X, Y;
        public AStartPoint P;
        public int G; // 从起点到当前节点的代价 
        public int H; // 从当前节点到终点的预估代价
        public int F; // G + H;
        
        public AStartPoint(int x, int y) {
            X = x;
            Y = y;
            Reset();
        }
        
        public void SetData(int g, int h, AStartPoint p) {
            G = g;
            H = h;
            F = G + H;
            P = p;
        }
        
        public void SetData(int g, AStartPoint p) {
            G = g;
            F = G + H;
            P = p;
        }

        public void Reset() {
            G = H = 0;
            F = -1;
            P = null;
        }
        
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"point({X},{Y})[g:{G}; h:{H} f:{F}]";
    }
}
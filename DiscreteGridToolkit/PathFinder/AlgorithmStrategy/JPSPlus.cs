using System.Collections.Generic;
using System.Linq;
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
            _pointsMap = new JPSPlusPoint[_map.Width, _map.Height];
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    _pointsMap[x, y] = new JPSPlusPoint(x, y);
                }
            }
            PreprocessJumpPoints();
            
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

        }
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            return null;
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
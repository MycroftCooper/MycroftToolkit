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
            PreprocessJumpMap();
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
        
        public void UpdateJumpPoints(Vector2Int point) {

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

            while (true) {
                x += dir.x;
                y += dir.y;
                step++;
                
                if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height) {
                    return step - 1;// 超出地图边界
                }
                if (!_map.IsPassable(x, y)) {
                    return step - 1;// 遇到阻挡
                }
                if (HasForcedNeighbors(_pointsMap[x, y], dir.x, dir.y)) {
                    return step;// 遇到跳点
                }
            }
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
        #endregion
        
        private void ResetPointsMap() {

        }
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            return null;
        }

        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) {
            // int x = targetPos.x;
            // int y = targetPos.y;
            //
            //     Vector3 position = originPos + new Vector3(x, y, 0); // 将网格位置映射到世界空间
            //     Gizmos.color = Color.yellow; // 边框颜色设置为黑色
            //     Gizmos.DrawCube(position, new Vector3(0.8f, 0.8f, 0.1f));
        }
    }
    
    public class JPSPlusPoint : AStartPoint {
        public int[] JumpStep; // 跳点信息存储结构：每个节点在8个方向的跳点
        public JPSPlusPoint(int x, int y): base(x,y) {
            JumpStep = new int[8];
        }
    }
}
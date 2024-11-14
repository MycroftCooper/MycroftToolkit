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
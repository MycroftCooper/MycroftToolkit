using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public enum PathFinderAlgorithms {AStar, JPS, JPSPlus, BFS, DFS, Dijkstra}

    public interface IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm { get; }
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        public void InitMap(SourceMap map);
        public void UpdateMap(RectInt bounds, bool passable);
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target);
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos);
    }

    public enum HeuristicTypes {Manhattan, Euclidean, SquaredEuclidean, Diagonal, WeightedDiagonal}

    public abstract class HeuristicFunctionBase {
        public HeuristicTypes HeuristicType { get; set; }
        public abstract int CalculateHeuristic(int aX, int aY, int bX, int bY);
        public int CalculateMaxFCost(Vector2Int mapSize) {
            int maxG = mapSize.x * mapSize.y;// 通用的 G 值最大计算：沿地图边界绕一圈的代价
            int maxF = CalculateHeuristic(0, 0, mapSize.x - 1, mapSize.y -1);
            return maxG + maxF;
        }
    }
    
    public enum PathReprocesses {None, Default  }
    public interface IPathReprocess {
        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map);
        public PathReprocesses PathReprocess { get; }
    }
}
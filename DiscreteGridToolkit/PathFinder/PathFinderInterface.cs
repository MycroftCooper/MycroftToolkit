using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public enum PathFinderAlgorithms {AStar, JPS, JPSPlus, BFS, DFS, Dijkstra, FlowField}
    /*
        BFS	        O(m * n)	                    无权图，寻找最短路径或判断可达性。
        DFS	        O(m * n)	                    搜索所有可能路径（非最短路径）或检查连通性。
        A*	        O(b^d)（最好）/ O(m * n)（最坏）	启发式路径规划，依赖 h(n) 的质量，适合寻找最优路径。
        JPS	        O(k * \log(k))	                优化的 A*，适合规则网格地图中快速找到最优路径。
        Dijkstra	O(m * n * \log(m * n))	        加权图的最短路径，适合处理网格中不同权重的场景。
     */

    public interface IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm { get; }
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        public void InitMap(SourceMap map);
        public void UpdateMap(RectInt bounds, bool passable);
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target);
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos);
    }

    public enum HeuristicTypes {None, Manhattan, Euclidean, SquaredEuclidean, Diagonal, WeightedDiagonal}

    public abstract class HeuristicFunctionBase {
        public HeuristicTypes HeuristicType { get; set; }
        public abstract int CalculateHeuristic(int aX, int aY, int bX, int bY);
        public int CalculateMaxFCost(Vector2Int mapSize) {
            int maxG = mapSize.x * mapSize.y;// 通用的 G 值最大计算：沿地图边界绕一圈的代价
            int maxH = CalculateHeuristic(0, 0, mapSize.x - 1, mapSize.y -1);
            return maxG + maxH;
        }
    }
    
    public enum PathReprocesses {None, Default, Theta  }
    public interface IPathReprocess {
        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map);
        public PathReprocesses PathReprocess { get; }
    }
}
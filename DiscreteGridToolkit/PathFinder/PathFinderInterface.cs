using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public enum PathFinderAlgorithms {AStar, JPS, JPSPlus}

    public interface IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm { get; }
        public IHeuristicFunction HeuristicFunction { get; set; }
        public void InitMap(SourceMap map);
        public void UpdateMap(RectInt bounds, bool passable);
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target);
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos);
    }

    public enum HeuristicType {Manhattan, Euclidean, SquaredEuclidean, Diagonal, WeightedDiagonal}

    public interface IHeuristicFunction {
        public HeuristicType Heuristic { get; }
        public int CalculateMaxFCost(Vector2Int mapSize);
        public int CalculateHeuristic(Vector2Int a, Vector2Int b);
    }
    
    public enum PathReprocesses {None, Default  }
    public interface IPathReprocess {
        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map);
        public PathReprocesses PathReprocess { get; }
    }
}
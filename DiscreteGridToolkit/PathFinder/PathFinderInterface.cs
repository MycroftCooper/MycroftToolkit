using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public enum PathFinderAlgorithms {AStar, JPS, JPSPlus}

    public interface IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm { get; }
        public void InitMap(SourceMap map);
        public void UpdateMap(RectInt bounds, bool passable);
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target);
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos);
    }
    
    public enum PathReprocesses {None, Default  }
    public interface IPathReprocess {
        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map);
        public PathReprocesses PathReprocess { get; }
    }
}
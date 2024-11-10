using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class JPS : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.JPS;
        
        public void InitMap(SourceMap map) {
            throw new System.NotImplementedException();
        }

        public void UpdateMap(RectInt bounds, bool passable) {
            throw new System.NotImplementedException();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            throw new System.NotImplementedException();
        }

        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) {
            throw new System.NotImplementedException();
        }
    }
}
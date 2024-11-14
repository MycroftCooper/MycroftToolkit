using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class DFS : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.DFS;
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        private SourceMap _map;
        public void InitMap(SourceMap map) {
            _map = map;
        }
        
        private readonly HashSet<Vector2Int> _visited = new HashSet<Vector2Int>();
        private readonly Stack<(Vector2Int position, List<Vector2Int> path)> _stack = new Stack<(Vector2Int position, List<Vector2Int> path)>();
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            _stack.Clear();
            _visited.Clear();
            
            _stack.Push((start, new List<Vector2Int> { start }));
            _visited.Add(start);

            // DFS 循环
            while (_stack.Count > 0) {
                var (currentPosition, currentPath) = _stack.Pop();

                // 探索所有方向
                foreach (var direction in SourceMap.Direction2VectorDict.Values) {
                    Vector2Int neighbor = currentPosition + direction;

                    if (!_map.CanMoveTo(currentPosition.x, currentPosition.y, direction)) {
                        continue;
                    }
                    
                    // 如果邻居是目标点，直接返回路径
                    if (neighbor == target) {
                        currentPath.Add(neighbor);
                        return currentPath;
                    }

                    // 如果邻居在地图内，并且是可通行的且未被访问过，加入栈
                    if (_visited.Add(neighbor)) {
                        var newPath = new List<Vector2Int>(currentPath) { neighbor };
                        _stack.Push((neighbor, newPath));
                    }
                }
            }

            // 如果栈为空，说明没有找到路径
            return null;
        }
        
        public void UpdateMap(RectInt bounds, bool passable) { }
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }
}
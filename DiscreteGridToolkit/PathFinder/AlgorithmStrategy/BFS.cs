using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class BFS : IPathFinderAlgorithm {
        public PathFinderAlgorithms Algorithm => PathFinderAlgorithms.BFS;
        public bool NeedBestSolution { get; set; }
        public HeuristicFunctionBase HeuristicFunction { get; set; }
        private SourceMap _map;

        public void InitMap(SourceMap map) {
            _map = map;
        }

        private readonly HashSet<Vector2Int> _visited = new HashSet<Vector2Int>();
        private readonly Queue<(Vector2Int position, List<Vector2Int> path)> _queue = new Queue<(Vector2Int position, List<Vector2Int> path)>();
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target) {
            _queue.Clear();
            _visited.Clear();
            
            _queue.Enqueue((start, new List<Vector2Int> { start }));
            _visited.Add(start);

            // BFS 循环
            while (_queue.Count > 0) {
                var (currentPosition, currentPath) = _queue.Dequeue();

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

                    // 如果邻居在地图内，并且是可通行的且未被访问过，加入队列
                    if (_visited.Add(neighbor)) {
                        var newPath = new List<Vector2Int>(currentPath) { neighbor };
                        _queue.Enqueue((neighbor, newPath));
                    }
                }
            }

            // 如果队列为空，说明没有找到路径
            return null;
        }

        public void UpdateMap(RectInt bounds, bool passable) { }
        public void OnDebugDrawGizmos(Vector3 originPos, Vector2Int targetPos) { }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class Theta : IPathReprocess {
        // 时间复杂度：O(n * d)
        public PathReprocesses PathReprocess => PathReprocesses.Theta;

        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map) {
            if (path == null || path.Count < 2) return path;

            List<Vector2Int> optimizedPath = new List<Vector2Int>();
            optimizedPath.Add(path[0]); // 起点

            int currentIndex = 0;

            for (int i = 1; i < path.Count; i++) {
                if (!map.IsLineOfSight(path[currentIndex], path[i])) {
                    // 如果无法直线通行，将前一个点作为关键点
                    optimizedPath.Add(path[i - 1]);
                    currentIndex = i - 1;
                }
            }

            optimizedPath.Add(path[^1]); // 添加终点
            return optimizedPath;
        }
    }
}
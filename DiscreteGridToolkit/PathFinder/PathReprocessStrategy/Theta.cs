using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class Theta : IPathReprocess {
        public PathReprocesses PathReprocess => PathReprocesses.Theta;

        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map) {
            List<Vector2Int> output = new List<Vector2Int>();
            if (path is not { Count: > 2 }) return path;

            output.Add(path[0]); // 起点必定是关键点
            int p = 0;
            int q = 1;
            while (q < path.Count) {
                if (map.LineOfSight(path[p], path[q])) {
                    q++;
                    continue;
                }
                output.Add(path[q-1]);
                output.Add(path[q]);
                p = q;
                q = p + 2;
            }

            if (path[^1] != output[^1]) {
                output.Add(path[^1]);// 最后一个点必定是关键点
            }

            return output;
        }
    }
}
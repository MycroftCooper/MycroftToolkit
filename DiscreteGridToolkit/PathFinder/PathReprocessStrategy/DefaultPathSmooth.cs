using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class DefaultPathSmooth : IPathReprocess {
        public PathReprocesses PathReprocess => PathReprocesses.Default;
        private SourceMap _map;
        
        public List<Vector2Int> ReprocessPath(List<Vector2Int> path, SourceMap map) {
            _map = map;
            var result = new List<Vector2Int>(path);
            SmoothPath(result);
            return result;
        }
        
        public void SmoothPath(List<Vector2Int> path) {
            if (path.Count < 3) return;

            List<Vector2Int> smoothedPath = new List<Vector2Int> { path[0] }; // 起点

            for (int i = 1; i < path.Count - 1; i++) {
                // 检查是否可以从当前节点跳过中间节点，直接连接到下一个节点
                if (!HasObstacleBetweenTwoPoints(smoothedPath[^1], path[i + 1])) {
                    continue; // 如果没有障碍物，跳过当前节点
                }
                smoothedPath.Add(path[i]); // 加入当前节点作为路径中的关键点
            }

            smoothedPath.Add(path[^1]); // 终点
            path.Clear();
            path.AddRange(smoothedPath); // 用平滑后的路径替换原路径
        }

       private bool HasObstacleBetweenTwoPoints(Vector2Int start, Vector2Int end) {
           int x0 = start.x, y0 = start.y;
           int x1 = end.x, y1 = end.y;
            int xMin = Math.Min(x0, x1);
            int xMax = Math.Max(x0, x1);
            int yMin = Math.Min(y0, y1);
            int yMax = Math.Max(y0, y1);
            for (int x = xMin; x <= xMax; x++) {
                for (int y = yMin; y <= yMax; y++) {
                    if (!_map.IsPassable(x, y) && LineInSquare((x0, y0), (x1, y1), (x, y))) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool LineInSquare((int x, int y) lineStart, (int x, int y) lineEnd, (int x, int y) squareCenter) {
            const float offset = 1f;
            var up = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y + offset), (squareCenter.x + offset, squareCenter.y + offset));
            var down = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y - offset), (squareCenter.x + offset, squareCenter.y - offset));
            var left = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x - offset, squareCenter.y - offset), (squareCenter.x - offset, squareCenter.y + offset));
            var right = IsLineSegmentsIntersecting(lineStart, lineEnd, (squareCenter.x + offset, squareCenter.y - offset), (squareCenter.x + offset, squareCenter.y + offset));
            return up || down || left || right;
        }

        private static bool IsLineSegmentsIntersecting((int x, int y) startA, (int x, int y) endA, (float x, float y) startB, (float x, float y) endB) {
            float denominator = (endA.x - startA.x) * (endB.y - startB.y) - (endA.y - startA.y) * (endB.x - startB.x);

            // 两条线段平行
            if (denominator == 0) {
                return false;
            }

            float numerator1 = (startA.y - startB.y) * (endB.x - startB.x) - (startA.x - startB.x) * (endB.y - startB.y);
            float numerator2 = (startA.y - startB.y) * (endA.x - startA.x) - (startA.x - startB.x) * (endA.y - startA.y);

            // 两条线段重叠
            if (numerator1 == 0 && numerator2 == 0) {
                return true;
            }

            float r = numerator1 / denominator;
            float s = ((startA.y - startB.y) * (endA.x - startA.x) - (startA.x - startB.x) * (endA.y - startA.y)) / denominator;

            // 相交点不在两条线段上
            return !(r < 0) && !(r > 1) && !(s < 0) && !(s > 1);
        }
    }
}
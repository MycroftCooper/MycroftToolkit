using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class Bezier {
        public static Vector3 Bezier2(Vector3 start, Vector3 control, Vector3 end, float t) {
            return (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end;
        }
        public static Vector3[] Bezier2Path(Vector3 start, Vector3 control, Vector3 end, int pointCount) {
            Vector3[] path = new Vector3[pointCount];
            for (int i = 1; i <= pointCount; i++) {
                float t = i * 1f / pointCount;
                path[i - 1] = Bezier2(start, control, end, t);
            }

            return path;
        }
        
        /// <summary>
        /// 曲线插值计算
        /// </summary>
        public static Vector3 Interp(Vector3[] pts, float t) {
            int numSections = pts.Length - 3;
            int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currPt;

            Vector3 a = pts[currPt];
            Vector3 b = pts[currPt + 1];
            Vector3 c = pts[currPt + 2];
            Vector3 d = pts[currPt + 3];

            return .5f * (
                (-a + 3f * b - 3f * c + d) * (u * u * u)
                + (2f * a - 5f * b + 4f * c - d) * (u * u)
                + (-a + c) * u
                + 2f * b
            );
        }

    }
}
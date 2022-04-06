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
    }
}
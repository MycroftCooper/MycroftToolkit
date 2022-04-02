using System;
using System.Linq;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class MathTool {
        /// <summary>
        /// 中式四舍五入(取整)
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <returns>取整结果</returns>
        public static int Round(float input) {
            int output = (int)input;
            int t = (int)Mathf.Abs(input * 10 % 10);
            if (t > 4) {
                if (input > 0) output++;
                if (input < 0) output--;
            }
            return output;
        }

        /// <summary>
        /// 中式四舍五入(精确)
        /// </summary>
        /// <param name="input">精确目标</param>
        /// <param name="dp">精确到几位小数</param>
        /// <returns>精确结果</returns>
        public static float Round(float input, int dp) {
            int p = (int)Math.Pow(10, (dp + 1));
            int output = (int)(input * (p / 10));
            int t = (int)Mathf.Abs(input * p % 10);
            if (t > 4) {
                if (input > 0) output++;
                if (input < 0) output--;
            }
            return (float)output / (p / 10);
        }

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

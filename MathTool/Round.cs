using System;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class Rounding {
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
    }
}

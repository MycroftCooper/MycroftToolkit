using System;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class MathTool {
        /// <summary>
        /// 简单随机数工具包
        /// @FAM 22.02.08
        /// </summary>
        public static class RandomEx {
            private static System.Random random = new System.Random();
            /// <summary>
            /// 获取随机Bool
            /// </summary>
            /// <param name="probability">为真的概率(默认0.5)</param>
            /// <returns>随机Bool</returns>
            public static bool GetBool(float probability = 0.5f) => random.NextDouble() < probability;
            public static int GetInt(int x, int y) => random.Next(x, y);
            public static float GetFloat(float x = 0, float y = 1) => (float)random.NextDouble() * (y - x) + x;
            public static double GetDouble(double x = 0, double y = 1) => random.NextDouble() * (y - x) + x;
        }
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

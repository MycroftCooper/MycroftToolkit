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

        /// <summary>
        /// 指定倍数向上取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static int RoundMultiple_Up(int input, int multiple)
            => input + multiple - input % multiple;
        /// <summary>
        /// 指定倍数向下取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static int RoundMultiple_Down(int input, int multiple)
            => input - input % multiple;
        /// <summary>
        /// 指定倍数接近取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static int RoundMultiple(int input, int multiple)
            => input % multiple >= (float)multiple / 2f ? input + multiple - input % multiple : input - input % multiple;
        /// <summary>
        /// 指定倍数向上取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static float RoundMultiple_Up(float input, float multiple)
            => input + multiple - input % multiple;
        /// <summary>
        /// 指定倍数向下取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static float RoundMultiple_Down(float input, float multiple)
            => input - input % multiple;
        /// <summary>
        /// 指定倍数接近取整
        /// </summary>
        /// <param name="input">取整目标</param>
        /// <param name="multiple">取整倍数</param>
        /// <returns>取整结果</returns>
        public static float RoundMultiple(float input, float multiple)
          => input % multiple >= (float)multiple / 2f ? input + multiple - input % multiple : input - input % multiple;
    }
}

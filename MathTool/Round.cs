using System;

namespace MycroftToolkit.MathTool {
    public static class Rounding {
        // todo: 两个 RoundByMultiple 方法对负数支持不是很好，主要是因为没有分清楚取整模式
        // 根据：https://docs.microsoft.com/zh-cn/dotnet/api/system.midpointrounding?view=net-6.0
        // 可知取整模式应该有至少 AwayFromZero, ToNegativeInfinity， ToPositiveInfinity， ToZero 四种
        // 后面再完善吧
        
        public enum RoundingMode {Up, Down, Close}
        public static int RoundByMultiple(this int target, int precision = 1, RoundingMode mode = RoundingMode.Close) {
            if (precision <= 0) throw new ArgumentOutOfRangeException(nameof(precision));
            var output = mode switch {
                RoundingMode.Up => target + (precision - target % precision),
                RoundingMode.Down => target - target % precision,
                RoundingMode.Close => target % precision >= precision / 2f
                    ? target + precision - target % precision
                    : target - target % precision,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            return output;
        }

        public static float RoundByMultiple(this float target, float precision, RoundingMode mode = RoundingMode.Close) {
            if (precision <= 0) throw new ArgumentOutOfRangeException(nameof(precision));
            var output = mode switch {
                RoundingMode.Up => target + (precision - target % precision),
                RoundingMode.Down => target - target % precision,
                RoundingMode.Close => target % precision >= precision / 2f
                    ? target + precision - target % precision
                    : target - target % precision,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            return output;
        }
        
        /// <summary>
        /// 中式四舍五入(取整)
        /// </summary>
        /// <returns>取整结果</returns>
        public static int RoundToInt(this float target) 
            =>(int) Math.Round(target, MidpointRounding.AwayFromZero);

        /// <summary>
        /// 中式四舍五入(精确)
        /// </summary>
        /// <param name="target">精确目标</param>
        /// <param name="dp">精确到几位小数</param>
        /// <returns>精确结果</returns>
        public static float Round(this float target, int dp) 
            =>(float) Math.Round(target, dp, MidpointRounding.AwayFromZero);
    }
}

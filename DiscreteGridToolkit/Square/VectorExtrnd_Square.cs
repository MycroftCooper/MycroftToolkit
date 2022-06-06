using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MycroftToolkit.DiscreteGridToolkit.Square {
    public static class PointsDistance {
        /// <summary>
        /// 求两点间距离
        /// </summary>
        /// <param name="pos1">点一坐标</param>
        /// <param name="pos2">点二坐标</param>
        /// <returns>欧拉距离</returns>
        public static float GetDistance_Euler(this Vector2Int pos1, Vector2Int pos2) {
            double dx = Math.Pow((pos1.x - pos2.x), 2);
            double dy = (int)Math.Pow((pos1.y - pos2.y), 2);
            float d = (float)Math.Sqrt(dx + dy);
            Console.WriteLine(d);
            return d;
        }


        /// <summary>
        /// 求两点间距离
        /// </summary>
        /// <param name="pos1">点一坐标</param>
        /// <param name="pos2">点二坐标</param>
        /// <returns>d4距离</returns>
        public static int GetDistance_D4(this Vector2Int pos1, Vector2Int pos2) {
            int dx = Math.Abs(pos1.x - pos2.x);
            int dy = Math.Abs(pos1.y - pos2.y);
            return dx + dy;
        }


        /// <summary>
        /// 求两点间距离
        /// </summary>
        /// <param name="pos1">点一坐标</param>
        /// <param name="pos2">点二坐标</param>
        /// <returns>d8距离</returns>
        public static int GetDistance_D8(this Vector2Int pos1, Vector2Int pos2) {
            int dx = Math.Abs(pos1.x - pos2.x);
            int dy = Math.Abs(pos1.y - pos2.y);
            return Math.Max(dx, dy);
        }
    }
}

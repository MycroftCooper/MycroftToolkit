using System;
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
        
          #region 邻接相关
        public static Vector2[] GetNeighborsD4(this Vector2 target) {
            Vector2[] output = new Vector2[4];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2StructExtension.Vec2NeighborsD4[i] + target;
            }
            return output;
        }
        
        public static Vector2[] GetNeighborsD8(this Vector2 target) {
            Vector2[] output = new Vector2[8];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2StructExtension.Vec2NeighborsD8[i] + target;
            }
            return output;
        }
        
        public static Vector2[] GetD4(this Vector2 target) {
            Vector2[] output = new Vector2[5];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2StructExtension.Vec2D4[i] + target;
            }
            return output;
        }
        
        public static Vector2[] GetD8(this Vector2 target) {
            Vector2[] output = new Vector2[9];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2StructExtension.Vec2D8[i] + target;
            }
            return output;
        }
        
        public static Vector2Int[] GetNeighborsD4(this Vector2Int target) {
            Vector2Int[] output = new Vector2Int[4];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2IntStructExtension.Vec2NeighborsD4[i] + target;
            }
            return output;
        }
        
        public static Vector2Int[] GetNeighborsD8(this Vector2Int target) {
            Vector2Int[] output = new Vector2Int[8];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2IntStructExtension.Vec2NeighborsD8[i] + target;
            }
            return output;
        }
        
        public static Vector2Int[] GetD4(this Vector2Int target) {
            Vector2Int[] output = new Vector2Int[5];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2IntStructExtension.Vec2D4[i] + target;
            }
            return output;
        }
        
        public static Vector2Int[] GetD8(this Vector2Int target) {
            Vector2Int[] output = new Vector2Int[9];
            for (int i = 0; i < output.Length; i++) {
                output[i] = Vector2IntStructExtension.Vec2D8[i] + target;
            }
            return output;
        }
        #endregion
    }
}

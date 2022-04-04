using MycroftToolkit.QuickCode;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.DiscreteGridToolkit.Hex {
    public static class Coordinate_Cube {
        #region 坐标转换
        public static Vector2Int ToAxial(Vector3Int point) {
            return new Vector2Int(point.x, point.y);
        }

        /// <summary>
        /// 立方体坐标转偏移坐标_平顶
        /// </summary>
        /// <param name="point">立方体坐标点</param>
        /// <param name="isEven">是否是偶数偏移</param>
        /// <returns>偏移坐标_平顶</returns>
        public static Vector2Int ToOffsetQ(Vector3Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y + (int)((point.x + offset * (point.x & 1)) / 2);
            return new Vector2Int(x, point.x);
        }

        public static Vector2Int ToOffsetR(Vector3Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.x + (int)((point.y + offset * (point.y & 1)) / 2);
            return new Vector2Int(x, point.y);
        }
        #endregion

        #region 邻接
        private static Vector3Int[] neighborsVectors = {
            new Vector3Int(+1, 0, -1), new Vector3Int(+1, -1, 0), new Vector3Int(0, -1, +1),
            new Vector3Int(-1, 0, +1), new Vector3Int(-1, +1, 0), new Vector3Int(0, +1, -1)
        };

        public static List<Vector3Int> GetNeighbors(Vector3Int point) {
            List<Vector3Int> output = new List<Vector3Int>(6);
            for (int i = 0; i < 6; i++) {
                output[i] = neighborsVectors[i] + point;
            }
            return output;
        }
        #endregion

        #region 对角线
        private static Vector3Int[] diagonalsVectors = {
            new Vector3Int(+2, -1, -1), new Vector3Int(+1, -2, +1), new Vector3Int(-1, -1, +2),
            new Vector3Int(-2, +1, +1), new Vector3Int(-1, +2, -1), new Vector3Int(+1, +1, -2)
        };

        public static List<Vector3Int> GetDiagonals(Vector3Int point) {
            List<Vector3Int> output = new List<Vector3Int>(6);
            for (int i = 0; i < 6; i++) {
                output[i] = diagonalsVectors[i] + point;
            }
            return output;
        }
        #endregion

        public static int GetDistance(Vector3Int pointA, Vector3Int pointB) {
            Vector3Int d = pointA - pointB;
            return Math.Max(Math.Max(Math.Abs(d.x), Math.Abs(d.y)), Math.Abs(d.z));
        }
        public static float GetDistance_Euler(Vector3Int pointA, Vector3Int pointB) {
            Vector3Int d = pointA - pointB;
            return Mathf.Sqrt(Mathf.Pow(d.x, 2) + Mathf.Pow(d.y, 2) + d.x * d.y);
        }

        public static List<Vector3Int> GetLine(Vector3Int pointA, Vector3Int pointB) {
            int d = GetDistance(pointA, pointB);
            List<Vector3Int> output = new List<Vector3Int>(d);
            for (int i = 0; i < d; i++) {
                output[i] = Round(Vector3.Lerp(pointA, pointB, i / d));
            }
            return output;
        }

        /// <summary>
        /// 将连续坐标转换为离散坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3Int Round(Vector3 pos) {
            int q = MathTool.Round(pos.x);
            int r = MathTool.Round(pos.y);
            int s = MathTool.Round(pos.z);

            float q_diff = Math.Abs(q - pos.x);
            float r_diff = Math.Abs(r - pos.y);
            float s_diff = Math.Abs(s - pos.z);

            if (q_diff > r_diff && q_diff > s_diff) q = -r * s;
            else if (r_diff > s_diff) r = -q * s;
            else s = -q * r;
            return new Vector3Int(q, r, s);
        }
    }

    public static class Coordinate_Axial {
        #region 坐标转换
        public static Vector3Int ToCube(Vector2Int point) {
            return new Vector3Int(point.x, point.y, -(point.x + point.y));
        }
        public static Vector2Int ToOffsetR(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.x + (int)((point.y + offset * (point.y & 1)) / 2);
            return new Vector2Int(x, point.y);
        }
        public static Vector2Int ToOffsetQ(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y + (int)((point.x + offset * (point.x & 1)) / 2);
            return new Vector2Int(x, point.x);
        }
        public static Vector2Int ToDoubleHeight(Vector2Int point) {
            return new Vector2Int(point.x, 2 * point.y + point.x);
        }
        public static Vector2Int ToDoubleWidth(Vector2Int point) {
            return new Vector2Int(2 * point.x + point.y, point.y);
        }
        #endregion

        #region 邻接
        private static Vector2Int[] neighborsVectors = {
            new Vector2Int(+1, 0), new Vector2Int(+1, -1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(-1, +1), new Vector2Int(0, +1)
        };

        public static List<Vector2Int> GetNeighbors(Vector2Int point) {
            List<Vector2Int> output = new List<Vector2Int>(6);
            for (int i = 0; i < 6; i++) {
                output[i] = neighborsVectors[i] + point;
            }
            return output;
        }
        #endregion

        #region 对角线
        private static Vector2Int[] diagonalsVectors = {
            new Vector2Int(+2, -1), new Vector2Int(+1, -2), new Vector2Int(-1, -1),
            new Vector2Int(-2, +1), new Vector2Int(-1, +2), new Vector2Int(+1, +1)
        };

        public static List<Vector2Int> GetDiagonals(Vector2Int point) {
            List<Vector2Int> output = new List<Vector2Int>(6);
            for (int i = 0; i < 6; i++) {
                output[i] = neighborsVectors[i] + point;
            }
            return output;
        }
        #endregion

        public static int GetDistance(Vector2Int pointA, Vector2Int pointB) {
            Vector3Int a = ToCube(pointA);
            Vector3Int b = ToCube(pointB);
            return Coordinate_Cube.GetDistance(a, b);
        }
        public static float GetDistance_Euler(Vector2Int pointA, Vector2Int pointB) {
            Vector3Int a = ToCube(pointA);
            Vector3Int b = ToCube(pointB);
            return Coordinate_Cube.GetDistance_Euler(a, b);
        }
        public static List<Vector3Int> GetLine(Vector2Int pointA, Vector2Int pointB) {
            Vector3Int a = ToCube(pointA);
            Vector3Int b = ToCube(pointB);
            return Coordinate_Cube.GetLine(a, b);
        }
        public static Vector2Int Round(Vector3 pos) {
            Vector3Int cubePos = Coordinate_Cube.Round(pos);
            return Coordinate_Cube.ToAxial(cubePos);
        }
    }

    public static class Coordinate_Offset {
        public static Vector3Int QToCube(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y;
            int y = point.x - (int)((point.y + offset * (point.y & 1)) / 2);
            int z = -x - y;
            return new Vector3Int(x, y, z);
        }
        public static Vector3Int RToCube(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y - (int)((point.x + offset * (point.x & 1)) / 2);
            int y = point.x;
            int z = -x - y;
            return new Vector3Int(x, y, z);
        }
        public static Vector2Int RToAxial(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y - (int)((point.x + offset * (point.x & 1)) / 2);
            int y = point.x;
            return new Vector2Int(x, y);
        }
        public static Vector2Int QToAxial(Vector2Int point, bool isEven) {
            int offset = isEven ? 1 : -1;
            int x = point.y;
            int y = point.x - (int)((point.y + offset * (point.y & 1)) / 2);
            return new Vector2Int(x, y);
        }

    }


    public static class Coordinate_Doubled {
        public static Vector2Int HeightToAxial(Vector2Int point) {
            var q = point.x;
            var r = (point.y - point.x) / 2;
            return new Vector2Int(q, r);
        }

        public static Vector2Int WidthToAxial(Vector2Int point) {
            var q = (point.x - point.y) / 2;
            var r = point.y;
            return new Vector2Int(q, r);
        }

        public static int GetDistance_Width(Vector2Int pointA, Vector2Int pointB) {
            Vector2Int d = pointA - pointB;
            return d.y + Math.Max(0, (d.x - d.y) / 2);
        }
        public static int GetDistance_Height(Vector2Int pointA, Vector2Int pointB) {
            Vector2Int d = pointA - pointB;
            return d.x + Math.Max(0, (d.y - d.x) / 2);
        }
    }
}


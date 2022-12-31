using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MycroftToolkit.DiscreteGridToolkit.Hex {
    // 理论参考：https://www.redblobgames.com/grids/hexagons/
    public static class HexGridTool {
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

            public static Vector3Int ContinuityToDiscrete(Vector2 point, float size, bool isFlat) {
                float x, y;
                if (isFlat) {
                    x = (2f / 3 * point.x) / size;
                    y = (-1f / 3 * point.x + Mathf.Sqrt(3) / 3 * point.y) / size;
                } else {
                    x = (Mathf.Sqrt(3) / 3 * point.x - 1f / 3 * point.y) / size;
                    y = (2f / 3 * point.y) / size;
                }
                Vector3 cube = new Vector3(x, y, -(point.x + point.y));
                return Round(cube);
            }

            public static Vector2 DiscreteToContinuity(Vector3Int hex, float size, bool isFlat) {
                float x, y;
                if (isFlat) {
                    x = size * (3f / 2 * hex.x);
                    y = size * (Mathf.Sqrt(3) / 2f * hex.x + Mathf.Sqrt(3) * hex.y);
                } else {
                    x = size * (Mathf.Sqrt(3) * hex.x + Mathf.Sqrt(3) / 2f * hex.y);
                    y = size * (3f / 2f * hex.y);
                }
                return new Vector2(x, y);
            }
            #endregion

            #region 邻接
            public static Vector3Int[] DirectionVectors = {
            new Vector3Int(+1, 0, -1), new Vector3Int(+1, -1, 0), new Vector3Int(0, -1, +1),
            new Vector3Int(-1, 0, +1), new Vector3Int(-1, +1, 0), new Vector3Int(0, +1, -1)
        };

            public static List<Vector3Int> GetNeighbors(Vector3Int point) {
                List<Vector3Int> output = new List<Vector3Int>(6);
                for (int i = 0; i < 6; i++) {
                    output[i] = DirectionVectors[i] + point;
                }
                return output;
            }
            public static Vector3Int GetNeighbor(Vector3Int point, Vector3Int direction) {
                return point + direction;
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
                int q = (int) Math.Round(pos.x, MidpointRounding.AwayFromZero);
                int r = (int) Math.Round(pos.y, MidpointRounding.AwayFromZero);
                int s = (int) Math.Round(pos.z, MidpointRounding.AwayFromZero);

                float q_diff = Math.Abs(q - pos.x);
                float r_diff = Math.Abs(r - pos.y);
                float s_diff = Math.Abs(s - pos.z);

                if (q_diff > r_diff && q_diff > s_diff) q = -r * s;
                else if (r_diff > s_diff) r = -q * s;
                else s = -q * r;
                return new Vector3Int(q, r, s);
            }

            public static List<Vector3Int> GetPointsInArea_Template(Vector3Int centerPos, List<Vector3Int> template) {
                List<Vector3Int> output = new List<Vector3Int>();
                foreach (Vector3Int t in template) {
                    output.Add(centerPos + t);
                }
                return output;
            }
            public static List<Vector3Int> GetPointsInArea_Rectangle(Vector3Int centerPos, Vector2Int size) {
                List<Vector3Int> output = new List<Vector3Int>();
                int px_1 = centerPos.x;
                int px_2 = px_1 + size.x;
                for (int y = 0; y < size.y; y++) {
                    for (int x = px_1; x < px_2; x++) {
                        output.Add(new Vector3Int(x, y, -(x + y)));
                    }
                    if (y % 2 != 0) {
                        px_1--;
                        px_2--;
                    }
                }
                return output;
            }
            public static List<Vector3Int> GetPointsInArea_Rhombus(Vector3Int centerPos, Vector2Int size) {
                List<Vector3Int> output = new List<Vector3Int>();
                for (int y = 0; y < size.y; y++) {
                    for (int x = 0; x < size.x; x++) {
                        output.Add(new Vector3Int(x, y, -(x + y)) + centerPos);
                    }
                }
                return output;
            }
            public static List<Vector3Int> GetPointsInHexagon(Vector3Int centerPos, int radius) {
                List<Vector3Int> output = new List<Vector3Int>();
                for (int x = -radius; x <= radius; x++) {
                    for (int y = Math.Max(-radius, -x - radius); y <= Math.Min(radius, -x + radius); y++) {
                        Vector3Int v = new Vector3Int(x, y, -x - y);
                        output.Add(v + centerPos);
                    }
                }
                return output;
            }

            public static List<Vector3Int> GetPointsInRing_Single(Vector3Int centerPos, int radius) {
                if (radius == 0) return null;
                List<Vector3Int> output = new List<Vector3Int>();
                Vector3Int hex = centerPos + DirectionVectors[4] * radius;
                for (int i = 0; i < 6; i++) {
                    for (int j = 0; j < radius; j++) {
                        output.Add(hex);
                        hex = GetNeighbor(hex, diagonalsVectors[i]);
                    }
                }
                return output;
            }
            public static List<Vector3Int> GetPointsInRing_Spiral(Vector3Int centerPos, int radius) {
                if (radius == 0) return null;
                List<Vector3Int> output = new List<Vector3Int>();
                for (int i = 1; i <= radius; i++) {
                    output = output.Union(GetPointsInRing_Single(centerPos, i)).ToList();
                }
                return output;
            }

            #region 集合运算
            /// <summary>
            /// 取两点集交集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>两点集交集</returns>
            public static List<Vector3Int> GetIntersect_Set(List<Vector3Int> pointSet1, List<Vector3Int> pointSet2)
            => pointSet1.Intersect(pointSet2).ToList();


            /// <summary>
            /// 两点集是否有交集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>是否相交</returns>
            public static bool IsIntersect_Set(List<Vector3Int> pointSet1, List<Vector3Int> pointSet2)
                => pointSet1.Intersect(pointSet2).ToList().Count > 0;


            /// <summary>
            /// 取两点集并集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>两点集并集</returns>
            public static List<Vector3Int> GetUnion_Set(List<Vector3Int> pointSet1, List<Vector3Int> pointSet2)
                => pointSet1.Union(pointSet2).ToList();


            /// <summary>
            /// 取两点集差集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集1</param>
            /// <returns>两点集差集</returns>
            public static List<Vector3Int> GetExcept_Set(List<Vector3Int> pointSet1, List<Vector3Int> pointSet2)
                => pointSet1.Except(pointSet2).ToList();

            #endregion

            #region 旋转
            public static Vector3Int Rotate60(Vector3Int centerPos, Vector3Int pos) {
                Vector3Int t1 = pos - centerPos;
                Vector3Int t2 = new Vector3Int(-t1.y, -t1.z, -t1.x);
                return t2 + centerPos;
            }
            public static Vector3Int RotateN60(Vector3Int centerPos, Vector3Int pos) {
                Vector3Int t1 = pos - centerPos;
                Vector3Int t2 = new Vector3Int(-t1.z, -t1.x, -t1.y);
                return t2 + centerPos;
            }
            public static Vector3Int Rotate(Vector3Int centerPos, Vector3Int pos, int time) {
                if (time == 0) return pos;
                time = time % 6;
                Vector3Int output = pos;
                for (int i = 0; i < Math.Abs(time); i++) {
                    if (time > 0)
                        output = Rotate60(output, centerPos);
                    else
                        output = RotateN60(output, centerPos);
                }
                return output;
            }
            #endregion
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

            public static Vector2Int ContinuityToDiscrete(Vector2 point, float size, bool isFlat) {
                float x, y;
                if (isFlat) {
                    x = (2f / 3 * point.x) / size;
                    y = (-1f / 3 * point.x + Mathf.Sqrt(3) / 3 * point.y) / size;
                } else {
                    x = (Mathf.Sqrt(3) / 3 * point.x - 1f / 3 * point.y) / size;
                    y = (2f / 3 * point.y) / size;
                }
                Vector3 cube = new Vector3(x, y, -(point.x + point.y));
                return Round(cube);
            }

            public static Vector2 DiscreteToContinuity(Vector2Int hex, float size, bool isFlat) {
                float x, y;
                if (isFlat) {
                    x = size * (3f / 2 * hex.x);
                    y = size * (Mathf.Sqrt(3) / 2f * hex.x + Mathf.Sqrt(3) * hex.y);
                } else {
                    x = size * (Mathf.Sqrt(3) * hex.x + Mathf.Sqrt(3) / 2f * hex.y);
                    y = size * (3f / 2f * hex.y);
                }
                return new Vector2(x, y);
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

            public static List<Vector2Int> GetPointsInArea_Template(Vector2Int centerPos, List<Vector2Int> template) {
                List<Vector2Int> output = new List<Vector2Int>();
                foreach (Vector2Int t in template) {
                    output.Add(centerPos + t);
                }
                return output;
            }
            public static List<Vector2Int> GetPointsInArea_Rectangle(Vector2Int centerPos, Vector2Int size) {
                List<Vector2Int> output = new List<Vector2Int>();
                int px_1 = centerPos.x;
                int px_2 = px_1 + size.x;
                for (int y = 0; y < size.y; y++) {
                    for (int x = px_1; x < px_2; x++) {
                        output.Add(new Vector2Int(x, y));
                    }
                    if (y % 2 != 0) {
                        px_1--;
                        px_2--;
                    }
                }
                return output;
            }
            public static List<Vector2Int> GetPointsInArea_Rhombus(Vector2Int centerPos, Vector2Int size) {
                List<Vector2Int> output = new List<Vector2Int>();
                for (int y = 0; y < size.y; y++) {
                    for (int x = 0; x < size.x; x++) {
                        output.Add(new Vector2Int(x, y) + centerPos);
                    }
                }
                return output;
            }
            public static List<Vector2Int> GetPointsInHexagon(Vector2Int centerPos, int radius) {
                List<Vector2Int> output = new List<Vector2Int>();
                for (int x = -radius; x <= radius; x++) {
                    for (int y = Math.Max(-radius, -x - radius); y <= Math.Min(radius, -x + radius); y++) {
                        Vector2Int v = new Vector2Int(x, y);
                        output.Add(v + centerPos);
                    }
                }
                return output;
            }


            #region 集合运算
            /// <summary>
            /// 取两点集交集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>两点集交集</returns>
            public static List<Vector2Int> GetIntersect_Set(List<Vector2Int> pointSet1, List<Vector2Int> pointSet2)
                => pointSet1.Intersect(pointSet2).ToList();


            /// <summary>
            /// 两点集是否有交集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>是否相交</returns>
            public static bool IsIntersect_Set(List<Vector2Int> pointSet1, List<Vector2Int> pointSet2)
                => pointSet1.Intersect(pointSet2).ToList().Count > 0;


            /// <summary>
            /// 取两点集并集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集2</param>
            /// <returns>两点集并集</returns>
            public static List<Vector2Int> GetUnion_Set(List<Vector2Int> pointSet1, List<Vector2Int> pointSet2)
                => pointSet1.Union(pointSet2).ToList();


            /// <summary>
            /// 取两点集差集
            /// </summary>
            /// <param name="pointSet1">点集1</param>
            /// <param name="pointSet2">点集1</param>
            /// <returns>两点集差集</returns>
            public static List<Vector2Int> GetExcept_Set(List<Vector2Int> pointSet1, List<Vector2Int> pointSet2)
                => pointSet1.Except(pointSet2).ToList();
            #endregion

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
}
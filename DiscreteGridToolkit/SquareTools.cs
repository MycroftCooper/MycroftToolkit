using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MycroftToolkit.DiscreteGridToolkit.Square {
    public static class SquareGridTool {
        public static class PointsSetOP {
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
        }


        public static class PointsSetMove {
            /// <summary>
            /// 移动点集
            /// </summary>
            /// <param name="points">点集</param>
            /// <param name="offset">偏移量</param>
            public static void MovePoints(List<Vector2Int> points, Vector2Int offset) {
                for (int i = 0; i < points.Count; i++) {
                    points[i] += offset;
                }
            }


            /// <summary>
            /// 翻转点集(X轴上下翻转)
            /// </summary>
            /// <param name="points">需要翻转的点集</param>
            public static void FlipPoints_X(List<Vector2Int> points) {
                for (int i = 0; i < points.Count; i++) {
                    points[i] = new Vector2Int(points[i].x, -points[i].y);
                }
            }


            /// <summary>
            /// 翻转点集(Y轴左右翻转)
            /// </summary>
            /// <param name="points">需要翻转的点集</param>
            public static void FlipPoints_Y(List<Vector2Int> points) {
                for (int i = 0; i < points.Count; i++) {
                    points[i] = new Vector2Int(-points[i].x, points[i].y);
                }
            }
        }


        public static class PointsDistance {
            /// <summary>
            /// 求两点间距离
            /// </summary>
            /// <param name="pos1">点一坐标</param>
            /// <param name="pos2">点二坐标</param>
            /// <returns>欧拉距离</returns>
            public static float GetDistance_Euler(Vector2Int pos1, Vector2Int pos2) {
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
            public static int GetDistance_D4(Vector2Int pos1, Vector2Int pos2) {
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
            public static int GetDistance_D8(Vector2Int pos1, Vector2Int pos2) {
                int dx = Math.Abs(pos1.x - pos2.x);
                int dy = Math.Abs(pos1.y - pos2.y);
                return Math.Max(dx, dy);
            }
        }


        public static class PointsArea {
            /// <summary>
            /// 使用模板获取区域内点集
            /// </summary>
            /// <param name="centerPos">锚点坐标</param>
            /// <param name="template">模板矩阵</param>
            /// <returns>区域内点集</returns>
            public static List<Vector2Int> GetPointsInArea_Template(Vector2Int centerPos, List<Vector2Int> template) {
                List<Vector2Int> output = new List<Vector2Int>();
                foreach (Vector2Int pos in template) {
                    Vector2Int p = centerPos + pos;
                    output.Add(p);
                }
                return output;
            }


            /// <summary>
            /// 获取两点方块区域内点集
            /// </summary>
            /// <param name="pos1">点一坐标</param>
            /// <param name="pos2">点二坐标</param>
            /// <returns>点集</returns>
            public static List<Vector2Int> GetPointsInArea_Block_Points(Vector2Int pos1, Vector2Int pos2) {
                List<Vector2Int> output = new List<Vector2Int>();
                int xStart = Math.Min(pos1.x, pos2.x);
                int xEnd = Math.Max(pos1.x, pos2.x);
                int yStart = Math.Min(pos1.y, pos2.y);
                int yEnd = Math.Max(pos1.y, pos2.y);
                for (int x = xStart; x <= xEnd; x++) {
                    for (int y = yStart; y <= yEnd; y++) {
                        Vector2Int p = new Vector2Int(x, y);
                        output.Add(p);
                    }
                }
                return output;
            }
            /// <summary>
            /// 获取方块区域内点集
            /// </summary>
            /// <param name="pos">起始坐标(左下角)</param>
            /// <param name="pos2">区域大小</param>
            /// <returns>点集</returns>
            public static List<Vector2Int> GetPointsInArea_Block_Size(Vector2Int pos, Vector2Int size) {
                List<Vector2Int> output = new List<Vector2Int>();
                int xStart = pos.x;
                int xEnd = pos.x + size.x;
                int yStart = pos.y;
                int yEnd = pos.y + size.y;
                for (int x = xStart; x < xEnd; x++) {
                    for (int y = yStart; y < yEnd; y++) {
                        Vector2Int p = new Vector2Int(x, y);
                        output.Add(p);
                    }
                }
                return output;
            }

            /// <summary>
            /// 获取中心点半径内点集(欧拉距离)
            /// </summary>
            /// <param name="centerPos">中心点坐标</param>
            /// <param name="radius">半径</param>
            /// <returns>半径内点集</returns>
            public static List<Vector2Int> GetPointsInRadius_Euler(Vector2Int centerPos, float radius) {
                if (radius < 0) {
                    Debug.LogError("离散网格工具>欧拉半径计算器>Error>半径不可为负");
                    return null;
                }
                List<Vector2Int> output = new List<Vector2Int>();
                if (radius == 0) {
                    output.Add(centerPos);
                    return output;
                }
                int r = (int)Math.Ceiling(radius);
                for (int x = -r; x <= radius; x++) {
                    for (int y = -r; y <= radius; y++) {
                        Vector2Int p = new Vector2Int(centerPos.x + x, centerPos.y + y);
                        if (PointsDistance.GetDistance_Euler(new Vector2Int(0, 0), new Vector2Int(x, y)) <= radius)
                            output.Add(p);
                    }
                }

                return output;
            }


            /// <summary>
            /// 获取中心点半径内点集(d4距离)
            /// </summary>
            /// <param name="centerPos">中心点坐标</param>
            /// <param name="radius">半径</param>
            /// <returns>半径内点集</returns>
            public static List<Vector2Int> GetPointsInRadius_D4(Vector2Int centerPos, int radius) {
                if (radius < 0) {
                    Debug.LogError("离散网格工具>D4半径计算器>Error>半径不可为负");
                    return null;
                }
                List<Vector2Int> output = new List<Vector2Int>();
                if (radius == 0) {
                    output.Add(centerPos);
                    return output;
                }

                for (int x = -radius; x <= radius; x++) {
                    for (int y = -radius; y <= radius; y++) {
                        Vector2Int p = new Vector2Int(centerPos.x + x, centerPos.y + y);
                        if (PointsDistance.GetDistance_D4(new Vector2Int(0, 0), new Vector2Int(x, y)) <= radius)
                            output.Add(p);
                    }
                }

                return output;
            }


            /// <summary>
            /// 获取中心点半径内点集(d8距离)
            /// </summary>
            /// <param name="centerPos">中心点坐标</param>
            /// <param name="radius">半径</param>
            /// <returns>半径内点集</returns>
            public static List<Vector2Int> GetPointsInRadius_D8(Vector2Int centerPos, int radius) {
                if (radius < 0) {
                    Debug.LogError("离散网格工具>D8半径计算器>Error>半径不可为负");
                    return null;
                }
                List<Vector2Int> output = new List<Vector2Int>();
                if (radius == 0) {
                    output.Add(centerPos);
                    return output;
                }
                for (int x = -radius; x <= radius; x++) {
                    for (int y = -radius; y <= radius; y++) {
                        Vector2Int p = new Vector2Int(centerPos.x + x, centerPos.y + y);
                        output.Add(p);
                    }
                }
                return output;
            }
        }


        public static class PointsLine {
            /// <summary>
            /// 两直线是否相交
            /// </summary>
            /// <param name="startPos1">直线1起点</param>
            /// <param name="endPos1">直线1终点</param>
            /// <param name="startPos2">直线2起点</param>
            /// <param name="endPos2">直线2终点</param>
            /// <returns>是否相交</returns>
            public static bool IsIntersect_Line(Vector2Int startPos1, Vector2Int endPos1, Vector2Int startPos2, Vector2Int endPos2) {
                List<Vector2Int> line1 = GetPointsInLine_Bresenham(startPos1, endPos1);
                List<Vector2Int> line2 = GetPointsInLine_Bresenham(startPos2, endPos2);
                return PointsSetOP.IsIntersect_Set(line1, line2);
            }
            /// <summary>
            /// 求两直线交点(请先判断是否相交)
            /// </summary>
            /// <param name="startPos1">直线1起点</param>
            /// <param name="endPos1">直线1终点</param>
            /// <param name="startPos2">直线2起点</param>
            /// <param name="endPos2">直线2终点</param>
            /// <returns>两直线交点</returns>
            public static Vector2Int GetIntersectPonit_Line(Vector2Int startPos1, Vector2Int endPos1, Vector2Int startPos2, Vector2Int endPos2) {
                List<Vector2Int> line1 = GetPointsInLine_Bresenham(startPos1, endPos1);
                List<Vector2Int> line2 = GetPointsInLine_Bresenham(startPos2, endPos2);
                try {
                    return PointsSetOP.GetIntersect_Set(line1, line2)[0];
                } catch (Exception e) {
                    Console.WriteLine("离散网格工具>获取直线交点>两直线不相交!直线1:{},{}|直线2:{},{}\n" + e.Message, startPos1, endPos1, startPos2, endPos2);
                    return new Vector2Int(0, 0);
                }
            }

            /// <summary>
            /// 绘制直线的光栅化算法（Bresenham算法，绘制离散像素点）
            /// </summary>
            /// <param name="startPos">起点</param>
            /// <param name="endPos">终点</param>
            /// <returns>直线上所有的点</returns>
            public static List<Vector2Int> GetPointsInLine_Bresenham(Vector2Int startPos, Vector2Int endPos) {
                List<Vector2Int> output = new List<Vector2Int>();
                if (startPos == endPos) {
                    output.Add(startPos);
                    return output;
                }
                int w = endPos.x - startPos.x;
                int h = endPos.y - startPos.y;
                int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
                if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
                if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
                if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
                int longest = Math.Abs(w);
                int shortest = Math.Abs(h);
                if (!(longest > shortest)) {
                    longest = Math.Abs(h);
                    shortest = Math.Abs(w);
                    if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                    dx2 = 0;
                }
                int numerator = longest >> 1;
                int x = startPos.x;
                int y = startPos.y;
                for (int i = 0; i <= longest; i++) {
                    output.Add(new Vector2Int(x, y));
                    numerator += shortest;
                    if (!(numerator < longest)) {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                    } else {
                        x += dx2;
                        y += dy2;
                    }
                }
                return output;
            }
        }
    }
}
using System;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class LerpExtensions {
        /// <summary>
        /// 圆柱螺旋曲线插值
        /// </summary>
        /// <param name="centerPos">中心点坐标</param>
        /// <param name="r">半径</param>
        /// <param name="h">高度(z轴)</param>
        /// <param name="startTheta">起始角度</param>
        /// <param name="endTheta">结束角度</param>
        /// <param name="t">插值</param>
        public static Vector3 CircularSpiralLerp(Vector3 centerPos, float r, float h, float startTheta, float endTheta,
            float t) {
            float angle = Mathf.Lerp(startTheta, endTheta, t);
            float z = Mathf.Lerp(0, h, t);
            Polar2 polar2 = new Polar2(r, angle);
            Vector3 vector3 = polar2.ToVector2().ToVec3().SetZ(z);
            Vector3 result = centerPos + vector3;
            return result;
        }

        /// <summary>
        /// 圆柱螺旋曲线插值
        /// </summary>
        /// <param name="r">半径</param>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="t">插值</param>
        public static Vector3 CircularSpiralLerp(float r, Vector3 startPos, Vector3 endPos, float t) {
            var h = endPos.z - startPos.z;
            Vector2 centerPos = GeometryTools.GetCircleCenter(startPos.ToVec2(), endPos.ToVec2(), r);
            Vector2 p1 = startPos.ToVec2() - centerPos;
            Vector2 p2 = endPos.ToVec2() - centerPos;
            float theta1 = p1.DirToAngle();
            float theta2 = p2.DirToAngle();

            Vector3 result = CircularSpiralLerp(Vector3.zero, r, h, theta1, theta2, t) + centerPos.ToVec3();
            return result;
        }

        /// <summary>
        /// 阿基米德螺旋曲线插值
        /// r = a + bθ
        /// 平面笛卡尔坐标方程式为: x = (a + bθ)cos(θ), y = (a + bθ)sin(θ)
        /// </summary>
        /// <param name="centerPos">中心点坐标</param>
        /// <param name="h">螺旋线高度</param>
        /// <param name="a">当θ=0°时的极径，将改变螺线形状</param>
        /// <param name="b">螺旋线系数,表示每旋转1度时极径的增加/减小量，控制螺线间距离</param>
        /// <param name="startTheta">极角,单位为度,表示阿基米德螺旋线开始转的度数</param>
        /// <param name="endTheta">极角,单位为度,表示阿基米德螺旋线转过的总度数</param>
        /// <param name="t">插值</param>
        public static Vector3 ArchimedeanSpiralLerp(Vector3 centerPos, float h, float a, float b, float startTheta,
            float endTheta, float t) {
            float angle = Mathf.Lerp(startTheta, endTheta, t);
            float rad = Mathf.Deg2Rad * angle;
            //公式计算坐标
            float x = (a + b * rad) * Mathf.Cos(rad);
            float y = (a + b * rad) * Mathf.Sin(rad);
            //Z值增量
            float z = Mathf.Lerp(0, h, t);
            Vector3 result = new Vector3(x, y, z);
            return centerPos + result;
        }

        /// <summary>
        /// 阿基米德螺旋曲线插值(最短路径)
        /// </summary>
        /// <param name="centerPos">中心点</param>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="t">插值</param>
        public static Vector3 ArchimedeanSpiralLerp(Vector3 centerPos, Vector3 startPos, Vector3 endPos, float t) {
            var h = endPos.z - startPos.z;
            Vector2 p1 = startPos - centerPos;
            Vector2 p2 = endPos - centerPos;

            var r1 = Vector2.Distance(Vector2.zero, p1);
            var r2 = Vector2.Distance(Vector2.zero, p2);
            var theta1 = p1.DirToAngle() * Mathf.Deg2Rad;
            var theta2 = p2.DirToAngle() * Mathf.Deg2Rad;
            if (Math.Abs(theta1 - theta2) < float.Epsilon) {
                theta2 += 360 * Mathf.Deg2Rad;
            }

            var b = (r1 - r2) / (theta1 - theta2);
            var a = r1 - theta1 * b;

            Vector3 output =
                ArchimedeanSpiralLerp(Vector3.zero, h, a, b, theta1 * Mathf.Rad2Deg, theta2 * Mathf.Rad2Deg, t) +
                centerPos;
            return output;
        }
        
        public static int MapRange(this float target, Vector2 originalRange, Vector2Int targetRange) {
            // 将目标数在原始区间内的位置计算为一个[0, 1]之间的比例
            float t = Mathf.InverseLerp(originalRange.x, originalRange.y, target);
            // 将比例映射到目标区间
            float mappedFloat = Mathf.Lerp(targetRange.x, targetRange.y, t);
            // 将结果转换为整数。你可以选择适合你需求的四舍五入方法。
            return Mathf.RoundToInt(mappedFloat);
        }

        public static float MapRange(this float target, Vector2 originalRange, Vector2 targetRange) {
            float t = Mathf.InverseLerp(originalRange.x, originalRange.y, target);
            float mappedFloat = Mathf.Lerp(targetRange.x, targetRange.y, t);
            return mappedFloat;
        }
    }
}
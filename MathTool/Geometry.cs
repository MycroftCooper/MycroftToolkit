using System;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class Geometry {
        public enum Relation { In, On, Out }
        public static Relation GetPointCircleRelation(Vector2 pos, Vector2 circleCenter, float radius) {
            float d = Vector2.Distance(pos, circleCenter);
            Relation output;
            if (d < radius) {
                output = Relation.In;
            } else if (Math.Abs(d - radius) < float.Epsilon) {
                output =  Relation.On;
            }else {
                output = Relation.Out;
            }
            return output;
        }

        public static Vector2[] GetTangentPoint(Vector2 pos, Vector2 circleCenter, float radius) {
            //圆外一点做圆切线的切点坐标
            Relation relation = GetPointCircleRelation(pos, circleCenter, radius);
            Vector2[] output;
            switch (relation) {
                case Relation.In:
                    return null;
                case Relation.On:
                    return new[] { pos };
                case Relation.Out:
                    output = new Vector2[2];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            float d = Vector2.Distance(pos, circleCenter);
            float l = Mathf.Sqrt(d * d - radius * radius); // 点p 到切点的距离
            Vector2 posToCenter = (circleCenter - pos) / d;// 点到圆心的单位向量
            float angle = Mathf.Asin(radius / d);// 计算切线与点心连线的夹角
            
            // 向正反两个方向旋转单位向量
            output[0] = new Vector2(
                posToCenter.x * Mathf.Cos(angle) - posToCenter.y * Mathf.Sin(angle),
                posToCenter.x * Mathf.Sin(angle) + posToCenter.y * Mathf.Cos(angle));
            output[1] = new Vector2(
                posToCenter.x * Mathf.Cos(-angle) - posToCenter.y * Mathf.Sin(-angle),
                posToCenter.x * Mathf.Sin(-angle) + posToCenter.y * Mathf.Cos(-angle));
            output[0] = (output[0] + pos) * l;
            output[1] = (output[1] + pos) * l;
            return output;
        }

        public static Vector2 GetCircleCenter(Vector2 p1, Vector2 p2, float r) {
            if (Vector2.Distance(p1, p2) > r * 2) {
                Debug.LogError($"MathTool>Geometry>Error> 点A{p1.ToString()}与点{p2.ToString()}构成的弦长大于直径：{r*2}");
                return default;
            }
            float c1 = (p2.x*p2.x - p1.x*p1.x + p2.y*p2.y - p1.y*p1.y) / (2 *(p2.x - p1.x));  
            float c2 = (p2.y - p1.y) / (p2.x - p1.x);  //斜率
            float a = (c2*c2 + 1);  
            float b = (2 * p1.x*c2 - 2 * c1*c2 - 2 * p1.y);  
            float c = p1.x*p1.x - 2 * p1.x*c1 + c1*c1 + p1.y*p1.y - r*r;
            float y = (-b + Mathf.Sqrt(b*b - 4 * a*c)) / (2 * a);
            float x = c1 - c2 * y;
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// 圆柱螺旋曲线插值
        /// </summary>
        /// <param name="centerPos">中心点坐标</param>
        /// <param name="r">半径</param>
        /// <param name="h">高度(z轴)</param>
        /// <param name="startTheta">起始角度</param>
        /// <param name="endTheta">结束角度</param>
        /// <param name="t">插值</param>
        public static Vector3 CircularSpiralLerp(Vector3 centerPos, float r, float h,float startTheta, float endTheta, float t) {
            float angle = Mathf.Lerp(startTheta, endTheta, t);
            float z = Mathf.Lerp(0,h,t); 
            Polar2 polar2 = new Polar2(r, angle);
            Vector3 vector3 = polar2.ToVector2().ToVec3().SetZ(z);
            Vector3 result =centerPos + vector3;
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
            Vector2 centerPos = GetCircleCenter(startPos.ToVec2(), endPos.ToVec2(), r);
            Vector2 p1 = startPos.ToVec2() - centerPos;
            Vector2 p2 = endPos.ToVec2() - centerPos;
            float theta1 = p1.DirToAngle();
            float theta2 = p2.DirToAngle();
            
            Vector3 result = CircularSpiralLerp(Vector3.zero, r,h,theta1,theta2,t)+centerPos.ToVec3();
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
        public static Vector3 ArchimedeanSpiralLerp(Vector3 centerPos,float h, float a, float b,float startTheta, float endTheta, float t) {
            float angle = Mathf.Lerp(startTheta, endTheta, t);
            float rad = Mathf.Deg2Rad*angle;
            //公式计算坐标
            float x = (a + b * rad) * Mathf.Cos(rad);
            float y = (a + b * rad) * Mathf.Sin(rad);
            //Z值增量
            float z = Mathf.Lerp(0,h,t); 
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
            var theta1 = p1.DirToAngle()*Mathf.Deg2Rad;
            var theta2 = p2.DirToAngle()*Mathf.Deg2Rad;
            if (Math.Abs(theta1 - theta2) < float.Epsilon) {
                theta2 += 360*Mathf.Deg2Rad;
            }
            
            var b = (r1 - r2) / (theta1 - theta2);
            var a = r1 - theta1 * b;

            Vector3 output = ArchimedeanSpiralLerp(Vector3.zero, h,a,b,theta1*Mathf.Rad2Deg,theta2*Mathf.Rad2Deg,t)+centerPos;
            return output;
        }
    }
}
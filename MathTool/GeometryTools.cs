using System;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class GeometryTools {
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
            output[0] = output[0] * l+ pos;
            output[1] = output[1] * l+ pos;
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
    }
}
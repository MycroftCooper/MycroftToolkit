using UnityEngine;
namespace MycroftToolkit.MathTool {
    public static class VectorExtension {
        public static Polar2 ToPolar2(this Vector2 v2) {
            float length = v2.magnitude;
            float azimuth = Vector2.SignedAngle(Vector2.right, v2);
            return new Polar2(length, azimuth);
        }
        public static Polar3 ToPolar3(this Vector3 v3) {
            float length = v3.magnitude;
            float x = Mathf.Acos(v3.z / length);
            float y = Mathf.Atan(v3.y / v3.x);
            return new Polar3(length, new Vector2(x, y) / Mathf.Deg2Rad);
        }

        public static Vector3Int ToVec3Int(this Vector3 v3) {
            return new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
        }
        public static Vector2 ToVec2(this Vector3 v3) {
            return new Vector2(v3.x, v3.y);
        }
        public static Vector2Int ToVec2Int(this Vector3 v3) {
            return new Vector2Int((int)v3.x, (int)v3.y);
        }

        public static Vector3 ToVec3(this Vector3Int v3) {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        public static Vector2 ToVec2(this Vector3Int v3) {
            return new Vector2(v3.x, v3.y);
        }
        public static Vector2Int ToVec2Int(this Vector3Int v3) {
            return new Vector2Int(v3.x, v3.y);
        }

        public static Vector3 ToVec3(this Vector2 v2) {
            return new Vector3(v2.x, v2.y, 0);
        }
        public static Vector3Int ToVec3Int(this Vector2 v2) {
            return new Vector3Int((int)v2.x, (int)v2.y, 0);
        }
        public static Vector2Int ToVec2Int(this Vector2 v2) {
            return new Vector2Int((int)v2.x, (int)v2.y);
        }

        public static Vector3 ToVec3(this Vector2Int v2) {
            return new Vector3(v2.x, v2.y, 0);
        }
        public static Vector3Int ToVec3Int(this Vector2Int v2) {
            return new Vector3Int(v2.x, v2.y, 0);
        }
        public static Vector2 ToVec2(this Vector2Int v2) {
            return new Vector2(v2.x, v2.y);
        }



        public static Vector3 SwapXY(this Vector3 v3) {
            return new Vector3(v3.y, v3.x, v3.z);
        }
        public static Vector3 SwapXZ(this Vector3 v3) {
            return new Vector3(v3.z, v3.y, v3.x);
        }
        public static Vector3 SwapYZ(this Vector3 v3) {
            return new Vector3(v3.x, v3.z, v3.y);
        }

        public static Vector3Int SwapXY(this Vector3Int v3) {
            return new Vector3Int(v3.y, v3.x, v3.z);
        }
        public static Vector3Int SwapXZ(this Vector3Int v3) {
            return new Vector3Int(v3.z, v3.y, v3.x);
        }
        public static Vector3Int SwapYZ(this Vector3Int v3) {
            return new Vector3Int(v3.x, v3.z, v3.y);
        }

        public static Vector2 SwapXY(this Vector2 v2) {
            return new Vector2(v2.y, v2.x);
        }
        public static Vector2Int SwapXY(this Vector2Int v2) {
            return new Vector2Int(v2.y, v2.x);
        }
    }
}

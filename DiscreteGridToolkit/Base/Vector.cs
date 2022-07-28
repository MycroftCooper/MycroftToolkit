using UnityEngine;
namespace MycroftToolkit.DiscreteGridToolkit {
    public struct Vector2StructExtension
    {
        public static readonly Vector2 UpLeft = new Vector2(-1,1);
        public static readonly Vector2 UpRight = new Vector2(1,1);
        public static readonly Vector2 DownRight = new Vector2(1,-1);
        public static readonly Vector2 DownLeft = new Vector2(-1,-1);
        public static readonly Vector2[] Vec2NeighborsD4 = 
            { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        public static readonly Vector2[] Vec2NeighborsD8 = 
            {UpLeft, Vector2.up, UpRight, Vector2.left, Vector2.right, DownLeft, Vector2.down, DownRight};
        public static readonly Vector2[] Vec2D4 = 
            { Vector2.up, Vector2.down, Vector2.left, Vector2.right ,Vector2.zero};
        public static readonly Vector2[] Vec2D8 = 
            {UpLeft, Vector2.up, UpRight, Vector2.left, Vector2.zero, Vector2.right, DownLeft, Vector2.down, DownRight};
    }
    
    public struct Vector2IntStructExtension {
        public static readonly Vector2Int UpLeft = new Vector2Int(-1,1);
        public static readonly Vector2Int UpRight = new Vector2Int(1,1);
        public static readonly Vector2Int DownRight = new Vector2Int(1,-1);
        public static readonly Vector2Int DownLeft = new Vector2Int(-1,-1);
        public static readonly Vector2Int[] Vec2NeighborsD4 = 
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        public static readonly Vector2Int[] Vec2NeighborsD8 = 
            {UpLeft, Vector2Int.up, UpRight, Vector2Int.left, Vector2Int.right, DownLeft, Vector2Int.down, DownRight};
        public static readonly Vector2Int[] Vec2D4 = 
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right ,Vector2Int.zero};
        public static readonly Vector2Int[] Vec2D8 = 
            {UpLeft, Vector2Int.up, UpRight, Vector2Int.left, Vector2Int.zero, Vector2Int.right, DownLeft, Vector2Int.down, DownRight};
    }
    
    public static class VectorExtension {
        #region 转换相关
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
        

        #endregion

        
        #region 交换相关
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
        

        #endregion
        

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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MycroftToolkit.QuickCode.Direction {
    public interface IDirection {
        int DirCode { get; set; }
        T GetDirEnum<T>() where T : Enum;
        void SetDirEnum<T>(T dirEnum) where T : Enum;
        Vector2 ToVector2();
        Vector2Int ToVector2Int();
        Vector3 ToVector3();
        Vector3Int ToVector3Int();
        IDirection GetOpposite();
    }
    
    public enum D4Dirs {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
    public struct D4Dir : IDirection {
        private D4Dirs _dir;

        public int DirCode {
            get => (int)_dir; 
            set => _dir = (D4Dirs)value;
        }

        public D4Dir(D4Dirs dir = default) {
            _dir = dir;
        }
        
        public T GetDirEnum<T>() where T : Enum {
            if (typeof(T) != typeof(D4Dirs))
                throw new InvalidOperationException("Invalid enum type for D4Dir.");
            return (T)(object)_dir;
        }

        public void SetDirEnum<T>(T dirEnum) where T : Enum {
            if (typeof(T) != typeof(D4Dirs))
                throw new InvalidOperationException("Invalid enum type for D4Dir.");
            _dir = (D4Dirs)(object)dirEnum;
        }
        
        public override string ToString() => _dir.ToString();
        public override int GetHashCode() => _dir.GetHashCode();
        public override bool Equals(object obj) => obj is IDirection other && DirCode == other.DirCode;

        public Vector2 ToVector2() =>
            DirectionsExtensions.DirToVectorDict.TryGetValue(DirCode, out var v) ? v : Vector2.zero;
        public Vector2Int ToVector2Int() => new Vector2Int((int)ToVector2().x, (int)ToVector2().y);

        public Vector3 ToVector3() => new Vector3(ToVector2().x, ToVector2().y, 0);
        public Vector3Int ToVector3Int() => new Vector3Int(ToVector2Int().x, ToVector2Int().y, 0);

        public IDirection GetOpposite() => new D4Dir(_dir switch {
            D4Dirs.Up => D4Dirs.Down,
            D4Dirs.Down => D4Dirs.Up,
            D4Dirs.Left => D4Dirs.Right,
            D4Dirs.Right => D4Dirs.Left,
            _ => D4Dirs.None,
        });
    }
    
    
    public enum D8Dirs {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        RightUp = Right | Up,
        RightDown = Right | Down,
        LeftDown = Left | Down,
        LeftUp = Left | Up
    }
    public struct D8Dir : IDirection {
        private D8Dirs _dir;
        public int DirCode {
            get => (int)_dir; 
            set => _dir = (D8Dirs)value;
        }
        
        public D8Dir(D8Dirs dir = default) {
            _dir = dir;
        }
        
        public T GetDirEnum<T>() where T : Enum {
            if (typeof(T) != typeof(D8Dirs))
                throw new InvalidOperationException("Invalid enum type for D8Dir.");
            return (T)(object)_dir;
        }

        public void SetDirEnum<T>(T dirEnum) where T : Enum {
            if (typeof(T) != typeof(D8Dirs))
                throw new InvalidOperationException("Invalid enum type for D8Dir.");
            _dir = (D8Dirs)(object)dirEnum;
        }
        
        public override string ToString() => _dir.ToString();
        public override int GetHashCode() => _dir.GetHashCode();
        public override bool Equals(object obj) => obj is IDirection other && DirCode == other.DirCode;

        public Vector2 ToVector2() =>
            DirectionsExtensions.DirToVectorDict.TryGetValue(DirCode, out var v) ? v : Vector2.zero;
        public Vector2Int ToVector2Int() => new Vector2Int((int)ToVector2().x, (int)ToVector2().y);

        public Vector3 ToVector3() => new Vector3(ToVector2().x, ToVector2().y, 0);
        public Vector3Int ToVector3Int() => new Vector3Int(ToVector2Int().x, ToVector2Int().y, 0);

        public IDirection GetOpposite() => new D8Dir(_dir switch {
            D8Dirs.Up => D8Dirs.Down,
            D8Dirs.Down => D8Dirs.Up,
            D8Dirs.Left => D8Dirs.Right,
            D8Dirs.Right => D8Dirs.Left,
            D8Dirs.RightUp => D8Dirs.LeftDown,
            D8Dirs.RightDown => D8Dirs.LeftUp,
            D8Dirs.LeftUp => D8Dirs.RightDown,
            D8Dirs.LeftDown => D8Dirs.RightUp,
            _ => D8Dirs.None,
        });
    }

    public static class DirectionsExtensions {
        public static readonly Dictionary<int, Vector2> DirToVectorDict = new Dictionary<int, Vector2> {
            { (int)D8Dirs.None , Vector2.zero},
            { (int)D8Dirs.Up, Vector2.up },
            { (int)D8Dirs.Down, Vector2.down },
            { (int)D8Dirs.Left, Vector2.left },
            { (int)D8Dirs.Right, Vector2.right },
            { (int)D8Dirs.RightUp, new Vector2(1, 1) },
            { (int)D8Dirs.RightDown, new Vector2(1, -1) },
            { (int)D8Dirs.LeftUp, new Vector2(-1, 1) },
            { (int)D8Dirs.LeftDown, new Vector2(-1, -1) }
        };
        
        public static T ToDir<T>(this Vector2 vector) where T : IDirection, new() {
            vector = vector.normalized;
            // 找到点积最大的方向
            int closest = 0;
            float maxDot = float.MinValue;

            foreach (var (dir, vec) in DirToVectorDict) {
                float dot = Vector2.Dot(vector, vec);
                if (dot > maxDot) {
                    maxDot = dot;
                    closest = dir;
                }
            }

            T result = new T {
                DirCode = closest
            };
            return result;
        }
    }
}
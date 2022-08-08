using UnityEngine;

// 理论来源：https://www.doc88.com/p-1542101922865.html
// https://en.wikipedia.org/wiki/Spherical_coordinate_system

namespace MycroftToolkit.DiscreteGridToolkit {
    public struct Polar2 {
        public float Length;
        public float Azimuth;

        public static readonly Polar2 ZeroPolar = new Polar2(0f, 0f);

        public static readonly Polar2 UpPolar = new Polar2(1f, 0f);

        public static readonly Polar2 DownPolar = new Polar2(1f, 180f);

        public static readonly Polar2 LeftPolar = new Polar2(1f, 270f);

        public static readonly Polar2 RightPolar = new Polar2(1f, 90f);

        public Polar2(float length, float azimuth) {
            this.Length = length;
            this.Azimuth = azimuth;
        }
        public Vector2 ToVector2() {
            float rad = Azimuth * Mathf.Deg2Rad;
            return new Vector2(Length * Mathf.Cos(rad), Length * Mathf.Sin(rad));
        }

        public override string ToString() {
            return $"{Length}∠{Azimuth}°";
        }
        public override bool Equals(object other) {
            if (!(other is Polar2 polar2)) return false;
            return this == polar2;
        }
        public override int GetHashCode() {
            return Length.GetHashCode() ^ (Azimuth % 360).GetHashCode();
        }
        public void Normalize() {
            if (Length < 0) {
                Azimuth += 180;
                Length = -Length;
            }
            Azimuth %= 360;
            if (Azimuth < 0) Azimuth += 360;
        }
        public static bool operator ==(Polar2 a, Polar2 b) {
            Polar2 aa = a;
            Polar2 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.Length == bb.Length && aa.Azimuth == bb.Azimuth) return true;
            return false;
        }
        public static bool operator !=(Polar2 a, Polar2 b) {
            Polar2 aa = a;
            Polar2 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.Length == bb.Length && aa.Azimuth == bb.Azimuth) return false;
            return true;
        }
        public static Polar2 operator +(Polar2 a, Polar2 b) => (a.ToVector2() + b.ToVector2()).ToPolar2();
        public static Polar2 operator -(Polar2 a, Polar2 b) => (a.ToVector2() - b.ToVector2()).ToPolar2();
        // 待确认正确性
        public static Polar2 operator *(Polar2 a, Polar2 b) => new Polar2(a.Length * b.Length, a.Azimuth + b.Azimuth);
        public static Polar2 operator *(float a, Polar2 b) => new Polar2(a * b.Length, b.Azimuth);
        public static Polar2 operator *(Polar2 a, float b) => new Polar2(a.Length * b, a.Azimuth);
        public static Polar2 operator /(Polar2 a, Polar2 b) => new Polar2(a.Length / b.Length, a.Azimuth - b.Azimuth);
        public static Polar2 operator /(Polar2 a, float b) => new Polar2(a.Length / b, a.Azimuth);
    }
    public struct Polar3 {
        public float Length;
        public Vector2 Azimuth;
        public Polar3(float length, Vector2 azimuth) {
            Length = length;
            Azimuth = azimuth;
        }
        public Vector3 ToVector3() {
            Vector2 rad = Azimuth * Mathf.Deg2Rad;
            float x = Length * Mathf.Sin(rad.x) * Mathf.Cos(rad.y);
            float y = Length * Mathf.Sin(rad.x) * Mathf.Sin(rad.y);
            float z = Length * Mathf.Cos(rad.x);
            return new Vector3(x, y, z);
        }
        public override string ToString() {
            return $"{Length}∠{Azimuth}°";
        }
        public override bool Equals(object other) {
            if (!(other is Polar3 polar3)) return false;
            return this == polar3;
        }
        public override int GetHashCode() {
            return Length.GetHashCode() ^ Azimuth.GetHashCode();
        }
        public void Normalize() {
            // 整不会了
        }
        public static bool operator ==(Polar3 a, Polar3 b) {
            Polar3 aa = a;
            Polar3 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.Length == bb.Length && aa.Azimuth == bb.Azimuth) return true;
            return false;
        }
        public static bool operator !=(Polar3 a, Polar3 b) {
            Polar3 aa = a;
            Polar3 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.Length == bb.Length && aa.Azimuth == bb.Azimuth) return false;
            return true;
        }
        public static Polar3 operator +(Polar3 a, Polar3 b) => (a.ToVector3() + b.ToVector3()).ToPolar3();
        public static Polar3 operator -(Polar3 a, Polar3 b) => (a.ToVector3() - b.ToVector3()).ToPolar3();
        // 待确认正确性
        public static Polar3 operator *(Polar3 a, Polar3 b) => new Polar3(a.Length * b.Length, a.Azimuth + b.Azimuth);
        public static Polar3 operator *(float a, Polar3 b) => new Polar3(a * b.Length, b.Azimuth);
        public static Polar3 operator *(Polar3 a, float b) => new Polar3(a.Length * b, a.Azimuth);
        public static Polar3 operator /(Polar3 a, Polar3 b) => new Polar3(a.Length / b.Length, a.Azimuth - b.Azimuth);
        public static Polar3 operator /(Polar3 a, float b) => new Polar3(a.Length / b, a.Azimuth);
    }
}

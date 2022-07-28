using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 理论来源：https://www.doc88.com/p-1542101922865.html
// https://en.wikipedia.org/wiki/Spherical_coordinate_system

namespace MycroftToolkit.DiscreteGridToolkit {
    public struct Polar2 {
        public float length;
        public float azimuth;

        public static readonly Polar2 zeroPolar = new Polar2(0f, 0f);

        public static readonly Polar2 upPolar = new Polar2(1f, 0f);

        public static readonly Polar2 downPolar = new Polar2(1f, 180f);

        public static readonly Polar2 leftPolar = new Polar2(1f, 270f);

        public static readonly Polar2 rightPolar = new Polar2(1f, 90f);

        public Polar2(float length, float azimuth) {
            this.length = length;
            this.azimuth = azimuth;
        }
        public Vector2 ToVector2() {
            float rad = azimuth * Mathf.Deg2Rad;
            return new Vector2(length * Mathf.Cos(rad), length * Mathf.Sin(rad));
        }

        public override string ToString() {
            return $"{length}∠{azimuth}°";
        }
        public override bool Equals(object other) {
            if (!(other is Polar2)) return false;
            return this == ((Polar2)other);
        }
        public override int GetHashCode() {
            return length.GetHashCode() ^ (azimuth % 360).GetHashCode();
        }
        public void Normalize() {
            if (length < 0) {
                azimuth += 180;
                length = -length;
            }
            azimuth %= 360;
            if (azimuth < 0) azimuth += 360;
        }
        public static bool operator ==(Polar2 a, Polar2 b) {
            Polar2 aa = a;
            Polar2 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.length == bb.length && aa.azimuth == bb.azimuth) return true;
            return false;
        }
        public static bool operator !=(Polar2 a, Polar2 b) {
            Polar2 aa = a;
            Polar2 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.length == bb.length && aa.azimuth == bb.azimuth) return false;
            return true;
        }
        public static Polar2 operator +(Polar2 a, Polar2 b) => (a.ToVector2() + b.ToVector2()).ToPolar2();
        public static Polar2 operator -(Polar2 a, Polar2 b) => (a.ToVector2() - b.ToVector2()).ToPolar2();
        // 待确认正确性
        public static Polar2 operator *(Polar2 a, Polar2 b) => new Polar2(a.length * b.length, a.azimuth + b.azimuth);
        public static Polar2 operator *(float a, Polar2 b) => new Polar2(a * b.length, b.azimuth);
        public static Polar2 operator *(Polar2 a, float b) => new Polar2(a.length * b, a.azimuth);
        public static Polar2 operator /(Polar2 a, Polar2 b) => new Polar2(a.length / b.length, a.azimuth - b.azimuth);
        public static Polar2 operator /(Polar2 a, float b) => new Polar2(a.length / b, a.azimuth);
    }
    public struct Polar3 {
        float length;
        Vector2 azimuth;
        public Polar3(float length, Vector2 azimuth) {
            this.length = length;
            this.azimuth = azimuth;
        }
        public Vector3 ToVector3() {
            Vector2 rad = azimuth * Mathf.Deg2Rad;
            float x = length * Mathf.Sin(rad.x) * Mathf.Cos(rad.y);
            float y = length * Mathf.Sin(rad.x) * Mathf.Sin(rad.y);
            float z = length * Mathf.Cos(rad.x);
            return new Vector3(x, y, z);
        }
        public override string ToString() {
            return $"{length}∠{azimuth}°";
        }
        public override bool Equals(object other) {
            if (!(other is Polar3)) return false;
            return this == ((Polar3)other);
        }
        public override int GetHashCode() {
            return length.GetHashCode() ^ azimuth.GetHashCode();
        }
        public void Normalize() {
            // 整不会了
        }
        public static bool operator ==(Polar3 a, Polar3 b) {
            Polar3 aa = a;
            Polar3 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.length == bb.length && aa.azimuth == bb.azimuth) return true;
            return false;
        }
        public static bool operator !=(Polar3 a, Polar3 b) {
            Polar3 aa = a;
            Polar3 bb = b;
            aa.Normalize();
            bb.Normalize();
            if (aa.length == bb.length && aa.azimuth == bb.azimuth) return false;
            return true;
        }
        public static Polar3 operator +(Polar3 a, Polar3 b) => (a.ToVector3() + b.ToVector3()).ToPolar3();
        public static Polar3 operator -(Polar3 a, Polar3 b) => (a.ToVector3() - b.ToVector3()).ToPolar3();
        // 待确认正确性
        public static Polar3 operator *(Polar3 a, Polar3 b) => new Polar3(a.length * b.length, a.azimuth + b.azimuth);
        public static Polar3 operator *(float a, Polar3 b) => new Polar3(a * b.length, b.azimuth);
        public static Polar3 operator *(Polar3 a, float b) => new Polar3(a.length * b, a.azimuth);
        public static Polar3 operator /(Polar3 a, Polar3 b) => new Polar3(a.length / b.length, a.azimuth - b.azimuth);
        public static Polar3 operator /(Polar3 a, float b) => new Polar3(a.length / b, a.azimuth);
    }
}

using System;

namespace RG.Basic.Math {
    [Serializable]
    public struct Vct2 {
        public float x;
        public float y;

        public Vct2(float v) { x = v; y = v; }
        public Vct2(float _x, float _y) { x = _x; y = _y; }
        public Vct2(Vct2 origin) { x = origin.x; y = origin.y; }

        public override bool Equals(object obj) { return base.Equals(obj); } 
        public override int GetHashCode() { return base.GetHashCode(); }

        public static bool operator ==(Vct2 v1, Vct2 v2) {
            if ((object)v1 == null || (object)v2 == null) return false;
            return v1.x == v2.x && v1.y == v2.y;
        }

        public static bool operator !=(Vct2 v1, Vct2 v2) {
            return !(v1 == v2);
        }

        #region Static Value

        public static readonly Vct2 Zero = new Vct2(0, 0);
        public static readonly Vct2 One = new Vct2(1, 1);
        public static readonly Vct2 UnitX = new Vct2(1, 0);
        public static readonly Vct2 UnitY = new Vct2(0, 1);

        
        public float this[int axis] {
            get { return axis == 0 ? x : axis == 1 ? y : -999999; }
            set { switch (axis) { case 0: x = value; break; case 1: y = value; break; } }
        }

        #endregion

        #region Arithmetic

        public float magnitude { get { return Mth.Sqrt(x * x + y * y); } }

        public Vct2 normal {
            get {
                float length = magnitude;
                if (length > Mth.EPS) return new Vct2(x / length, y / length);
                return Zero;
            }
        }

        public static Vct2 Add(Vct2 v1, Vct2 v2) { return new Vct2(v1.x + v2.x, v1.y + v2.y); }

        public static Vct2 operator +(Vct2 v1, Vct2 v2) { return Add(v1, v2); }

        public static Vct2 Subtract(Vct2 v1, Vct2 v2) { return new Vct2(v1.x - v2.x, v1.y - v2.y); }

        public static Vct2 operator -(Vct2 v1, Vct2 v2) { return Subtract(v1, v2); }

        public static Vct2 Negate(Vct2 v1) { return new Vct2(-v1.x, -v1.y); }

        public static Vct2 operator -(Vct2 v1) { return Negate(v1); }

        public static Vct2 Multiply(Vct2 v1, float factor) { return new Vct2(v1.x * factor, v1.y * factor); }

        public static Vct2 Multiply(Vct2 v1, Vct2 v2) { return new Vct2(v1.x * v2.x, v1.y * v2.y); }

        public static Vct2 operator *(Vct2 v1, float factor) { return Multiply(v1, factor); }

        public static Vct2 operator *(float factor, Vct2 v1) { return Multiply(v1, factor); }

        public static Vct2 operator *(Vct2 v1, Vct2 v2) { return Multiply(v1, v2); }

        public static Vct2 Divide(Vct2 v1, float divider) { if (Mth.Abs(divider) > Mth.EPS) return new Vct2(v1.x / divider, v1.y / divider); return v1; }

        public static Vct2 Divide(Vct2 v1, Vct2 v2) { return new Vct2(v2.x > Mth.EPS ? v1.x : (v1.x / v2.x), v2.y > Mth.EPS ? v1.y : (v1.y / v2.y)); }

        public static Vct2 operator /(Vct2 v1, float v) { return Divide(v1, v); }

        public static Vct2 operator /(Vct2 v1, Vct2 v2) { return Divide(v1, v2); }

        #endregion

        #region Lerp

        public static Vct2 Lerp(Vct2 v1, Vct2 v2, float lerp) { return v1 + (v2 - v1) * lerp;  }

        public static Vct2 SmoothStep(Vct2 v1, Vct2 v2, float lerp) { return Lerp(v1, v2, lerp.Pow2() * (3 - 2 * lerp)); }

        public static Vct2 CatmullRom(Vct2 v1, Vct2 v2, Vct2 v3, Vct2 v4, float lerp) {  
            float amountPow2 = lerp.Pow2();
            float amountPow3 = amountPow2 * lerp; 
            return new Vct2(
                    ((2f * v2.x + (-v1.x + v3.x) * lerp + (2f * v1.x - 5f * v2.x + 4f * v3.x - v4.x) * amountPow2 + (3f * v2.x - 3f * v3.x - v1.x + v4.x) * amountPow3) * 0.5f),
                    ((2f * v2.y + (-v1.y + v3.y) * lerp + (2f * v1.y - 5f * v2.y + 4f * v3.y - v4.y) * amountPow2 + (3f * v2.y - 3f * v3.y - v1.y + v4.y) * amountPow3) * 0.5f)
            );
        }

        public static Vct2 Hermite(Vct2 value1, Vct2 tangent1, Vct2 value2, Vct2 tangent2, float lerp) { 
            float lerpPow2 = lerp.Pow2();
            float lerpPow3 = lerpPow2 * lerp; 
            float h1 = 2 * lerpPow3 - 3 * lerpPow2 + 1;
            float h2 = -2 * lerpPow3 + 3 * lerpPow2;
            float h3 = lerpPow3 - 2 * lerpPow2 + lerp;
            float h4 = lerpPow3 - lerpPow2; 
            return new Vct2(
                    h1 * value1.x + h2 * value2.x + h3 * tangent1.x + h4 * tangent2.x,
                    h1 * value1.y + h2 * value2.y + h3 * tangent1.y + h4 * tangent2.y
            );
        }

        #endregion
         
        public static float Dot(Vct2 v1, Vct2 v2) { return v1.x * v2.x + v1.y * v2.y; }

        public static float Cross(Vct2 v1, Vct2 v2) { return v1.x * v2.y - v1.y * v2.x; }

        public static float Distance(Vct2 v1, Vct2 v2) { return Mth.Sqrt(Distance2(v1, v2)); }

        public static float Distance2(Vct2 v1, Vct2 v2) { return (v1.x - v2.x).Pow2() + (v1.y - v2.y).Pow2(); }

        /// <summary>
        /// 向量夹角
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>返回值范围：[0,360)</returns> 
        public static float Angle(Vct2 v1, Vct2 v2) {
            float v = Mth.Atan(v1.x, v1.y) - Mth.Atan(v2.x, v2.y);
            if (v < 0) v += 360;
            return v;
        }

        

        public override string ToString() { return "x:" + x + " y:" + y; }
    }


    public struct Vct3 {
        public float x;
        public float y;
        public float z;

        public readonly static Vct3 Zero = new Vct3(0, 0, 0);
        public readonly static Vct3 One = new Vct3(1, 1, 1);

        public Vct3(float _x, float _y, float _z) { x = _x; y = _y; z = _z; }
        public Vct3(Vct3 origin) { x = origin.x; y = origin.y; z = origin.z; }

        public float this[int i] {
            get { return i == 0 ? x : i == 1 ? y : i == 2 ? z : -999999; }
            set {
                switch (i) {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                }
            }
        }

        public float magnitude { get { return Mth.Sqrt(x * x + y * y + z * z); } }

        public Vct3 normal {
            get {
                float l = magnitude;
                if (l > Mth.EPS) return new Vct3(x / l, y / l, z / l);
                return Zero;
            }
        }

        public static Vct3 operator +(Vct3 v1, Vct3 v2) {
            return new Vct3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vct3 operator -(Vct3 v1, Vct3 v2) {
            return new Vct3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vct3 operator *(Vct3 v1, float v) {
            return new Vct3(v1.x * v, v1.y * v, v1.z * v);
        }

        public static Vct3 operator *(float v, Vct3 v1) {
            return new Vct3(v1.x * v, v1.y * v, v1.z * v);
        }

        public static Vct3 operator /(Vct3 v1, float v) {
            if (Mth.Abs(v) > Mth.EPS)
                return new Vct3(v1.x / v, v1.y / v, v1.z / v);
            return v1;
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }


        public static bool operator ==(Vct3 v1, Vct3 v2) {
            if ((object)v1 != null || (object)v2 != null) return false;
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public static bool operator !=(Vct3 v1, Vct3 v2) {
            return !(v1 == v2);
        }

        public static float Distance(Vct3 v1, Vct3 v2) {
            return Mth.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z));
        }

        public static float Dot(Vct3 v1, Vct3 v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
    }
}


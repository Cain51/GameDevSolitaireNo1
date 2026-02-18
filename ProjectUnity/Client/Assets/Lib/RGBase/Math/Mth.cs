using System.Collections;

namespace RG.Basic {
    public static class Mth {
        public const float EPS = 1e-5f;
        public const float E = (float)System.Math.E;

        public const float Log10E = 0.4342944819032f;
        public const float Log2E = 1.442695040888f;

        public const float Pi = (float)System.Math.PI; //3.1415926f;
        public const float PiDev2 = (float)(System.Math.PI / 2.0);
        public const float PiDev4 = (float)(System.Math.PI / 4.0);
        public const float PiMul2 = (float)(System.Math.PI * 2.0);
        public const float PiMul4 = (float)(System.Math.PI * 2.0);
         
        /// <summary>
        /// 角度制转弧度制
        /// </summary> 
        public static float AngleDegToRad(this float degree) {
            return degree * Pi / 180.0f;
        }

        /// <summary>
        /// 弧度制转角度制
        /// </summary> 
        public static float AngleRadToDeg(this float radian) {
            return radian * 180.0f / Pi;
        }

        public static float AngleRadWrap(float radian) { //-180 ~ 180
            radian = radian % PiMul2;
            if (radian > Pi)  return radian - PiMul2;
            if (radian < -Pi) return radian + PiMul2;
            return radian;
        }

        public static float AngleDegWrap(float radian) {
            radian = radian % 360;
            if (radian > 180) return radian - 360;
            if (radian < -180) return radian + 360;
            return radian;
        }

        public static float Abs(this float v) { return System.Math.Abs(v); }
        public static int Floor(this float v) { return (int)System.Math.Floor(v); }
        public static float Pow2(this float v) { return v * v; } 
        public static float Sqrt(this float v) { return (float)System.Math.Sqrt((double)v); }

        public static float Pow(float v, float pow) { return (float)System.Math.Pow(v, pow); }
        public static float Max(float value1, float value2) { return System.Math.Max(value1, value2); }
        public static int Max(int value1, int value2) { return System.Math.Max(value1, value2); }
        public static float Min(float value1, float value2) { return System.Math.Min(value1, value2); } 
        public static int Min(int value1, int value2) { return System.Math.Min(value1, value2); }

        public static float Lerp(float v1, float v2, float lerp) { return v1 + (v2 - v1) * lerp; }

        public static long Lerp(long v1, long v2, long lerp) { return v1 + (v2 - v1) * lerp; }

        public static float Distance(float v1, float v2) { return System.Math.Abs(v1 - v2); }


        /// <summary>
        /// 返回角度制0-360
        /// </summary> 
        public static float Atan(float x, float y) {
            float ax = Abs(x);
            float ay = Abs(y);
            if (ax < EPS) {
                if (ay < EPS) return 0;
                else return y > 0 ? 90 : 270;
            } else {
                if (ay < EPS) return x > 0 ? 0 : 180;
                else {
                    float v = AngleRadToDeg((float)System.Math.Atan(ay / ax));
                    if (x > 0) {
                        if (y > 0) return v;
                        else return 360 - v;
                    } else {
                        if (y > 0) return 180 - v;
                        else return 180 + v;
                    }
                }
            } 
        }

        public static float Sin(float degree) { return (float)System.Math.Sin(AngleDegToRad((float)degree)); }

        public static float Cos(float degree) { return (float)System.Math.Cos(AngleDegToRad((float)degree)); }

        public static float Clamp(float v, float minValue, float maxValue) { return v < minValue ? minValue : v > maxValue ? maxValue : v; }

        public static float Clamp01(float v) { return Clamp(v, 0, 1); }

        public static int Clamp(int v, int minValue, int maxValue) { return v < minValue ? minValue : v > maxValue ? maxValue : v; }

        public static int Clamp01(int v) { return Clamp(v, 0, 1); }

        public static int LowBit(int v) { return v & -v; }
    }
}


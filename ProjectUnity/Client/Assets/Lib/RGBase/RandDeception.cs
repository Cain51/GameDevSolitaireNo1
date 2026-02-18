using System.Collections;


/*
 * G:利用随机算法产生随机数
 * 借鉴网上一个随机数产生算法
 * 数字为x的对应值为pow(x,d)mod((p-1)*(q-1))
 * 这个值相对随机,在2~N-1范围内与传入的值对应
 * 此处d特意选择31，所以快速幂的时候不再判断0和1
 * e、d、p、q慎改,如果要造成其他随机性，可以添加随机种子
 * 随机值在2000多范围内滚动，概率精度为100：2000
 */
namespace RG.Basic
{
    public class RandDeception
    {

        //此处设置e=71
        static int d = 31;
        static int p = 23;
        static int q = 101;
        static int N = (p - 1) * (q - 1);

        public static int MaxVal { get { return N - 1; } }

        //public static int seed = 1154111317;
        public static int seed = 53158454;

        public static int Rand(int x)
        {
            x ^= seed;
            x %= N;
            x += 2;
            int tx = x;
            for (int i = 1; i <= 4; i++)
            {
                x *= x;
                tx = (tx + (x %= N)) % N;
            }
            return tx;
        }

        public static float RandRate(int x)
        {
            return (float)Rand(x) / MaxVal;
        }

        public static int RandWithRange(int x, int range)
        {
            return (int)(RandRate(x) * range);
        }

        public static int Rand2(int x)
        {
            uint y = (uint)x;
            y ^= y >> 11;
            y ^= (y << 7) & 0x9D2C5680;
            y ^= (y << 15) & 0xEFC6000;
            y ^= y >> 18;
            return (int)(y / 2);
        }
    }
}

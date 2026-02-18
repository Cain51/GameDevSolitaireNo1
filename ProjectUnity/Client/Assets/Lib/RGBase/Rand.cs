using System; 

namespace RG.Basic
{
    public class Random : System.Random
    {
        static System.Random r = new System.Random();
        public static int Range(int min, int max)
        {
            return r.Next(min, max);
        } 

        public static float Range(float min,float max)
        {
            return (float)r.NextDouble() * (max - min) + min;
        }

        public static int Binomial(int n, float p)
        {
            int x = 0;
            for (int i = 0; i < n; i++)
            {
                x+=r.NextDouble() < p ? 1 : 0;
            }
            return x;
        }
    }
}

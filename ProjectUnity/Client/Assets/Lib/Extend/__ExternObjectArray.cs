using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RG.Basic{

    public static class __ExternObjectArray { 

        public static bool GetBool(this object[] array, int index, bool defaultValue) {
            if (index >= 0 && index < array.Length) {
                return (bool)array[index];
            }
            return defaultValue;
        }
        public static double GetDouble(this object[] array, int index, double defaultValue)
        {
            if (index >= 0 && index < array.Length)
            {
                return (double)array[index];
            }
            return defaultValue;
        }
        public static int GetInt(this object[] array, int index, int defaultValue)
        {
            if (index >= 0 && index < array.Length)
            {
                return (int)((double)array[index]);
            }
            return defaultValue;
        }
    }
}

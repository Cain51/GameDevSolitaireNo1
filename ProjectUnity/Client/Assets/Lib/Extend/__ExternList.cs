using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RG.Basic{

    public static class __ExternList { 

        public static string ConcatToString<T>(this List<T> list, string split = ",") {
            if (list == null) return null;
            System.Text.StringBuilder stringbuilder = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++) { 
                    stringbuilder.Append(list[i] == null ? "null" : list[i].ToString()).Append(split); 
            }
            if (stringbuilder.Length > 0) stringbuilder.Remove(stringbuilder.Length - 1, 1);
            return stringbuilder.ToString();
        }
        public static List<T> SafeRemove<T>(this List<T> list,int[] index) {
            List<T> result = new List<T>();
            int srcCount = list.Count;
            for (int i = 0; i < srcCount; i++) {
                if (index.Contains<int>(i)) {
                    continue;
                }
                result.Add(list[i]);
            }
            return result;
        }
    }
}

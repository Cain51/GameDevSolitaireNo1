using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class __ExternTransform {

    public static void ClearChild(this Transform str)
    {
        for (int idx = 0; idx < str.childCount; idx++)
        {
            if (str.GetChild(idx).gameObject.activeSelf == true) {
                UnityEngine.Object.Destroy(str.GetChild(idx).gameObject);
            }
        }
    }

    public static void DestoryChild(this Transform str) {
        for (int idx = 0; idx < str.childCount; idx++) {
            UnityEngine.Object.Destroy(str.GetChild(idx).gameObject);
        }
    }
}

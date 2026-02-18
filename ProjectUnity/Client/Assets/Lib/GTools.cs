using UnityEngine;
using System.Collections;

public static class GTools {
    public static void NormalizeTransform(GameObject go)
    {
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = new Quaternion(0, 0, 0, 0);
        go.transform.localScale = Vector3.one;
    }

    public static void NormalizeTransform(Transform go)
    {
        go.localPosition = Vector3.zero;
        go.localRotation = new Quaternion(0, 0, 0, 0);
        go.localScale = Vector3.one;
    }

    public static float DistanceIgnoreY(Vector3 a,Vector3 b)
    {
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
    }

    public static Vector3 PosOnGround(Vector3 a, float groundHight = 0)
    {
        return new Vector3(a.x, groundHight, a.z);
    }
}

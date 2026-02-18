using RG.Basic;
using RG.Zeluda;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public struct MatDrop
{
	public int id;
	public int rate;
	public int max;
}
public struct MatSpDrop
{
	public int[] tag;
	public MatPair[] mats;
}
public static class DataUtil
{
	public static MatSpDrop StringToMatSpDrop(string val1, string val2)
	{
		MatSpDrop result = new MatSpDrop();
		if (val1 == string.Empty) { return result; }

		string[] str = val1.Split('|');
		int[] rel = new int[str.Length];
		for (int i = 0; i < str.Length; i++)
		{
			rel[i] = (int)Convert.ToSingle(str[i]);
		}
		result.tag = rel;

		string[] str2 = val2.Split('|');
		MatPair[] mats = new MatPair[str2.Length];
		for (int i = 0; i < str2.Length; i++)
		{
			string str2s = str2[i];
			MatPair mat = mats[i];
			string[] str3 = str2s.Split(',');

			mat.id = Convert.ToInt32(str3[0]);
			mat.cnt = Convert.ToInt32(str3[1]);
		}
		result.tag = rel;
		result.mats = mats;

		return result;
	}
	public static MatPair StringToMatPair(string val)
	{
		MatPair result = new MatPair();
		if (val == string.Empty) { return result; }

		string[] str = val.Split(':');
		result.id = Convert.ToInt32(str[0]);
		result.cnt = Convert.ToInt32(str[1]);

		return result;
	}
	public static int[][] StringToIntAryAry(string val)
	{

		if (val == string.Empty) { return new int[0][]; }

		string[] str = val.Split('|');
		int[][] result = new int[str.Length][];
		for (int i = 0; i < str.Length; i++)
		{
			string s = str[i];
			string[] sp = s.Split(',');
			int[] ids = new int[sp.Length];
			for (int j = 0; j < sp.Length; j++)
			{
				ids[j] = Convert.ToInt32(sp[j]);
			}
			result[i] = ids;
		}
		return result;
	}
	public static MatPair[] StringToMatPairAry(string val)
	{
		string[] str = val.Split('|');
		int len = str.Length;
		MatPair[] rel = new MatPair[len];
		for (int i = 0; i < len; i++)
		{
			string data = str[i];
			if (data == string.Empty || data.Contains(':') == false) { continue; }
			MatPair result = new MatPair();
			string[] str2 = data.Split(':');
			result.id = Convert.ToInt32(str2[0]);
			result.cnt = Convert.ToInt32(str2[1]);

			rel[i] = result;
		}
		return rel;
	}
	public static MatDrop StringToMatDrop(string val)
	{
		MatDrop result = new MatDrop();
		if (val == string.Empty) { return result; }

		string[] str = val.Split(',');
		result.id = Convert.ToInt32(str[0]);
		result.rate = Convert.ToInt32(str[1]);
		result.max = Convert.ToInt32(str[2]);

		return result;
	}

	public static string IntAry(int[] ary)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < ary.Length; i++)
		{
			if (i != 0)
			{
				sb.Append("|");
			}
			sb.Append(ary[i]);
		}
		return sb.ToString();
	}
	public static int[] StringToIntAry(string val)
	{
		if (val == string.Empty) { return new int[0]; }

		string[] str = val.Split('|');
		int[] rel = new int[str.Length];
		for (int i = 0; i < str.Length; i++)
		{
			rel[i] = (int)Convert.ToSingle(str[i]);
		}
		return rel;
	}
    public static string[] StringToStringAry(string val)
    {
        if (val == string.Empty) { return new string[0]; }

        string[] str = val.Split('|');
        return str;
    }
    public static Vector2Int[] StringToV2IAry(string val)
	{
		if (val == string.Empty) { return new Vector2Int[0]; }

		string[] str = val.Split('|');
		Vector2Int[] rel = new Vector2Int[str.Length];
		for (int i = 0; i < str.Length; i++)
		{
			string[] xy = str[i].Split(':');
			Vector2Int pos = new Vector2Int();
			pos.x = Convert.ToInt32(xy[0]);
			pos.y = Convert.ToInt32(xy[1]);
			rel[i] = pos;
		}
		return rel;
	}
	public static Pair<int, int>[] StringToIntPairAry(string val)
	{
		if (val == string.Empty) { return new Pair<int, int>[0]; }

		string[] str = val.Split('|');
		Pair<int, int>[] rel = new Pair<int, int>[str.Length];
		for (int i = 0; i < str.Length; i++)
		{
			string[] xy = str[i].Split(':');
			rel[i] = new Pair<int, int>(Convert.ToInt32(xy[0]), Convert.ToInt32(xy[1]));
		}
		return rel;
	}
	public static Pair3<int, int, int>[] StringToIntPair3Ary(string val)
	{
		if (val == string.Empty) { return new Pair3<int, int, int>[0]; }

		string[] str = val.Split('|');
		Pair3<int, int, int>[] rel = new Pair3<int, int, int>[str.Length];
		for (int i = 0; i < str.Length; i++)
		{
			string[] xy = str[i].Split(':');
			rel[i] = new Pair3<int, int, int>(Convert.ToInt32(xy[0]), Convert.ToInt32(xy[1]), Convert.ToInt32(xy[2]));
		}
		return rel;
	}
	public static Dictionary<int, int> StringToIntDic(string val)
	{
		Dictionary<int, int> rel = new Dictionary<int, int>();
		if (val == string.Empty) { return rel; }

		string[] str = val.Split('|');

		for (int i = 0; i < str.Length; i++)
		{
			string[] xy = str[i].Split(':');
			int id = Convert.ToInt32(xy[0]);
			if (rel.ContainsKey(id))
			{
				rel[id] += Convert.ToInt32(xy[1]);
			}
			else
			{
				rel.Add(id, Convert.ToInt32(xy[1]));
			}
		}
		return rel;
	}

	public static string DicAry(Dictionary<int, int> ary)
	{
		StringBuilder sb = new StringBuilder();
		bool sta = false;
		foreach (var item in ary)
		{
			if (sta == true)
			{
				sb.Append("|");
			}

			sb.Append(item.Key);
			sb.Append(":");
			sb.Append(item.Value);
			sta = true;
		}
		return sb.ToString();
	}
}

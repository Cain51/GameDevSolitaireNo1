using RG.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


public static partial class __ExternString {
    public static int CountSubString(this string str, string sub) {
        int i = str.IndexOf(sub, 0);
        int n = 0;
        while (i > -1) {
            n++;
            i = str.IndexOf(sub, i + 1);
        }
        return n;
    }

    public static string TryConvertToString(this string str, string defaultValue = "") {
        return str;
    }

    public static int TryConvertToInt(this string str, int defaultValue = 0) {
        int _valueInt = defaultValue;
        if (str.Exist() && int.TryParse(str, out _valueInt))
            return _valueInt;
        else
            return defaultValue;
    }

    public static long TryConvertToLong(this string str, long defaultValue = 0) {
        long _valueInt = defaultValue;
        if (str.Exist() && long.TryParse(str, out _valueInt))
            return _valueInt;
        else
            return defaultValue;
    }

    public static bool TryConvertToBool(this string str, bool defaultValue = false) {
        bool _valueBool = defaultValue;
        if (bool.TryParse(str, out _valueBool))
            return _valueBool;
        else
            return defaultValue;
    }

    public static float TryConvertToFloat(this string str, float defaultValue = 0) {
        float _valueFloat = defaultValue;
        if (float.TryParse(str, out _valueFloat))
            return _valueFloat;
        else
            return defaultValue;

    }

    public static bool Exist(this string s) {
        return !string.IsNullOrEmpty(s);
    }

    public static T TryConvertToEnum<T>(this string type, T defaultValue = default(T)) {
        System.Type t = typeof(T);
        if (!t.IsEnum) return defaultValue;
        try {
            return (T)System.Enum.Parse(t, type);
        }
        catch { return defaultValue; }

    }
    public static string ToFirstUpperString(this string str) {
        if (str.Exist()) {
            if (str.Length <= 1) str = str.ToUpper();
            else str = str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }
        return str;
    }

    public static StringBuilder stringBuilder = new StringBuilder();
    public static void ClearStringBuilder() { stringBuilder.Remove(0, stringBuilder.Length); }

    public static string BatchReplace(this string origin, string[] from, params string[] to) {
        ClearStringBuilder();
        for (int i = 0; i < origin.Length; i++) {
            bool _contains = false;
            for (int j = 0; j < from.Length && j < to.Length; j++) {
                if (origin.beginWith(i, from[j])) {
                    stringBuilder.Append(to[j]);
                    i += from[j].Length - 1;
                    _contains = true;
                    break;
                }
            }
            if (!_contains) stringBuilder.Append(origin[i]);
        }
        return stringBuilder.ToString();
    }

    public static string BatchReplaceWithCallBack(this string origin, string[] from, string[] to, Action<string, int> WhenReplace) {
        ClearStringBuilder();

        int remCount = 0;
        for (int i = 0; i < origin.Length; i++) {
            bool _contains = false;
            for (int j = 0; j < from.Length && j < to.Length; j++) {
                if (origin.beginWith(i, from[j])) {
                    stringBuilder.Append(to[j]);
                    WhenReplace(from[j], i - remCount);
                    i += from[j].Length - 1;
                    remCount += (from[j].Length - to[j].Length);
                    _contains = true;
                    break;
                }
            }
            if (!_contains) stringBuilder.Append(origin[i]);
        }
        return stringBuilder.ToString();
    }

    public static bool beginWith(this string str, int start, string match) {
        int i = start;
        while (i <= str.Length) // for every char
        {
            if (i - start >= match.Length) {
                return true;
            }
            if (i >= str.Length || str[i] != match[i - start]) break;
            i++;
        }
        return false;
    }

    public static Seq<string> Debine(this string str, string split = ",") {
        if (split.Length == 1) {
            string[] events = str.Split(split[0]);
            return Seq<string>.Parse(events);
        }
        else {
            string[] events = str.Split(new string[] { split }, System.StringSplitOptions.None);
            return Seq<string>.Parse(events);
        }
    }

    public static int GetStringLastNumber(this string str) {
        int result = 0;
        if (str != null && str != string.Empty) {
            Match match = Regex.Match(str, @"(^.+?)(\d+$)");
            if (match.Success) result = int.Parse(match.Groups[2].Value);
        }
        return result;
    }

    public static string IncLastNumber(this string str, int defaultZero = 0) {
        int result = 0;
        ClearStringBuilder();
        if (str != null && str != string.Empty) {
            Match match = Regex.Match(str, @"(^.+?)(\d+$)");
            if (match.Success) {
                Capture group = match.Groups[2];
                result = int.Parse(group.Value) + 1;
                int newlength = result.ToString().Length;

                stringBuilder
                    .Append(str)
                    .Remove(group.Index, group.Length)
                    .AppendFormat("{0:d" + (newlength > group.Length ? newlength : group.Length) + "}", result); //超过时会增加, 小于时则保持位数
                return stringBuilder.ToString();
            }
        }

        if (defaultZero > 0) return stringBuilder.Append(str).AppendFormat("{0:d" + defaultZero + "}", 0).ToString();
        else return str;
    }
}


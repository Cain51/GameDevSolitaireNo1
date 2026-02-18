using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;

public class LocalProfilerTools
{
    public static string filePath = @"E:\output\quoteInfo.txt";
    public static string gcTreeFilePath = @"E:\output\gcTree.txt";
    public static string dumpPath = @"E:\output\dump.json";

    public static UInt64 objectScaned = 0;
    public static long checkedIdx = 0;
    public static Dictionary<object, long> checkedObject = new Dictionary<object, long>();
    public static Dictionary<object, List<string>> fieldPathDic = new Dictionary<object, List<string>>();
    public static Dictionary<Type, int> instanceHistoryDic = new Dictionary<Type, int>();

    public static Dictionary<Type, object> referenceDic = new Dictionary<Type, object>();
    public static JsonWriter writer = null;
    public static string checkType = null;
    public static float startTime;
    public static void Clean()
    {
        objectScaned = 0;
        checkedIdx = 0;
        instanceHistoryDic.Clear();
        checkedObject.Clear();
        fieldPathDic.Clear();
        GC.Collect();
    }

    public static void LogAllFieldTable(object root, string type = null)
    {
        filePath = Application.dataPath.Replace("/Assets", "/quoteInfo.txt");
        gcTreeFilePath = Application.dataPath.Replace("/Assets", "/gcTree.txt");
        startTime = Time.time;
        //fieldDic.Clear();
        Clean();
        checkType = type;
        deepFind(root, root.ToString());
        LogGcTree();
        LogQuoteInfo();
        Clean();
    }
    public static void LogGcTree()
    {
        float totalTime = Time.time - startTime;

        StringBuilder sb = new StringBuilder();
        sb.Append("cacheSnapshotTime = " + totalTime +
            "\r\nobjectScaned = " + objectScaned +
            "\r\ncheckType = " + checkType +
            "\r\n");


        int index = 0;
        foreach (KeyValuePair<object, List<string>> kv in fieldPathDic)
        {
            try
            {
                sb.Append("\r\n--------[" + fieldPathDic[kv.Key].Count + "]" + kv.Key.GetType().ToString());

            }
            catch
            {
                Debug.LogError(kv.Key.ToString());
            }
            foreach (var item in kv.Value)
            {
                sb.Append("\r\n" + item);
            }
            index++;
            if (index > 1000) { break; }
        }

        if (File.Exists(gcTreeFilePath))
        {
            File.Delete(gcTreeFilePath);
        }
        File.Create(gcTreeFilePath).Dispose();
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(gcTreeFilePath, true))
        {
            file.WriteLine(sb);
        }
        Debug.Log("Profiler Log Over!!!");
    }
    public static void LogDump(string srcType)
    {
        //dumpPath = Application.dataPath.Replace("/Assets", "/dump.txt");
        writer = new JsonWriter();
        //寻找起点
        string[] dotInst = srcType.Split('.');

        Type type = Type.GetType(dotInst[0]);
        //起点(或当前节点)
        object cur = type;
        PropertyInfo info = null;
        for (int i = 1; i < dotInst.Length; i++)
        {
            info = type.GetProperty(dotInst[i], BindingFlags.Public |
           BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Instance |
           BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Static);
            cur = info.GetValue(cur);
            type = cur.GetType();
        }
        DateTime time = DateTime.Now;
        deepFind(cur, cur.ToString());
        Debug.LogError((DateTime.Now - time));
        writer.WriteObjectStart();
        //开始dump
        writer.WritePropertyName("Heap");
        writer.WriteObjectStart();
        foreach (var item in checkedObject)
        {
            Dump(item.Key, false, true);
        }
        writer.WriteObjectEnd();



        writer.WritePropertyName("PTable");
        writer.WriteObjectStart();
        foreach (var item in checkedObject)
        {
            Type t = item.Key.GetType();
            writer.WritePropertyName(t.Name);
            if (item.Key is ICollection)
            {
                writer.Write(t.FullName);
            }
            else
            {
                writer.WriteObjectStart();
                FieldInfo[] infos = t.GetFields();
                for (int i = 0, len = infos.Length; i < len; i++)
                {
                    FieldInfo finfo = infos[i];
                    writer.WritePropertyName(finfo.Name);
                    object attr = infos[i].GetValue(item.Key);
                    writer.Write(attr == null ? "null" : attr.GetType().Name);
                }
                writer.WriteObjectEnd();
            }
        }
        writer.WriteObjectEnd();
        writer.WriteObjectEnd();


        if (File.Exists(dumpPath))
        {
            File.Delete(dumpPath);
        }
        File.Create(dumpPath).Dispose();
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(dumpPath, true))
        {
            file.WriteLine(writer.ToString());
        }
        Debug.Log("Dump Log Over!!!");
    }
    public static void Dump(object node, bool isAry = false, bool isRoot = false)
    {
        if (node == null) { return; }

        //判断是否需要记录
        do
        {
            Type srcType = node.GetType();
            if (WriteBaseObject(node) == true)
            {
                break;
            }
            long idx = checkedObject.ContainsKey(node) ? checkedObject[node] : -1;

            if (isRoot == false && (node == null || idx != -1))
            {
                writer.Write(srcType.Name + "#" + idx);
                break;
            }

            if (isAry == false)
            {
                string name = srcType.Name;
                if (isRoot == true)
                {
                    name += "#" + idx;
                }
                writer.WritePropertyName(name);
            }

            writer.WriteObjectStart();

            Type t = node.GetType();

            FieldInfo[] infos = t.GetFields();
            for (int i = 0, len = infos.Length; i < len; i++)
            {
                FieldInfo info = infos[i];
                object val = info.GetValue(node);
                writer.WritePropertyName(info.Name);
                if (val == null)
                {
                    writer.Write(null);
                }
                else if (val is ICollection)
                {
                    //检查是否为集合，例如：List，Dictionary
                    int index = 0;
                    writer.WriteArrayStart();
                    foreach (var item in (ICollection)val)
                    {
                        if (item == null) { continue; }
                        //如果集合是双键值对，例：Dictionary，Map
                        Type valueType = item.GetType();
                        bool isKeyValuePair = false;
                        if (valueType.IsGenericType)
                        {
                            Type baseType = valueType.GetGenericTypeDefinition();
                            if (baseType == typeof(KeyValuePair<,>))
                            {
                                isKeyValuePair = true;
                                Type[] argTypes = baseType.GetGenericArguments();
                                object kvpKey = valueType.GetProperty("Key").GetValue(item, null);
                                object kvpValue = valueType.GetProperty("Value").GetValue(item, null);
                                if (filterUselessClass(kvpKey) == false)
                                {
                                    writer.Write(kvpKey.ToString());
                                }
                                else
                                {
                                    Dump(kvpKey, true);
                                }
                                if (filterUselessClass(kvpValue) == false)
                                {
                                    writer.Write(kvpValue.ToString());
                                }
                                else
                                {
                                    Dump(kvpValue, true);
                                }
                            }
                        }
                        if (isKeyValuePair == false)
                        {
                            if (item != null)
                            {
                                Dump(item, true);
                            }
                        }
                        index++;
                    }
                    writer.WriteArrayEnd();
                }
                else
                {
                    if (WriteBaseObject(val) == true)
                    {
                        continue;
                    }

                    Dump(val);
                }
            }
            writer.WriteObjectEnd();
        } while (false);
    }
    public static bool WriteBaseObject(object node)
    {
        if (node is int)
        {
            writer.Write((int)node); return true;
        }
        else if (node is float)
        {
            writer.Write((float)node); return true;
        }
        else if (node is double)
        {
            writer.Write((double)node); return true;
        }
        else if (node is uint)
        {
            writer.Write((uint)node); return true;
        }
        else if (node is byte)
        {
            writer.Write((byte)node); return true;
        }
        else if (node is short)
        {
            writer.Write((short)node); return true;
        }
        else if (node is ushort)
        {
            writer.Write((ushort)node); return true;
        }
        else if (node is string)
        {
            writer.Write((string)node); return true;
        }
        else if (node is bool)
        {
            writer.Write((bool)node); return true;
        }
        else if (node is long)
        {
            writer.Write((long)node); return true;
        }
        else if (node is ulong)
        {
            writer.Write((ulong)node); return true;
        }
        else if (node is char)
        {
            writer.Write((char)node); return true;
        }
        else if (node is Enum)
        {
            writer.Write(node.ToString()); return true;
        }
        else if (node is Type)
        {
            writer.Write(node.ToString()); return true;
        }
        else if (node is XmlElement)
        {
            writer.Write(node.ToString()); return true;
        }
        else if (node is System.Xml.NameTable)
        {
            writer.Write(node.ToString()); return true;
        }
        return false;
    }
    public static void LogQuoteInfo()
    {
        float totalTime = Time.time - startTime;

        StringBuilder sb = new StringBuilder();
        sb.Append("cacheSnapshotTime = " + totalTime +
            "\r\nobjectScaned = " + objectScaned +
            "\r\ncheckType = " + checkType +
            "\r\n");

        var dicSort = from objDic in instanceHistoryDic orderby objDic.Value descending select objDic;

        foreach (KeyValuePair<Type, int> kv in dicSort)
        {
            sb.Append("\r\n--------[" + kv.Value + "]" + kv.Key.ToString());

        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        File.Create(filePath).Dispose();
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
        {
            file.WriteLine(sb);
        }
        Debug.Log("Profiler Log Over!!!");
    }

    public static void deepFind(object root, string path)
    {
        FieldInfo[] fields = root.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        int len = fields.Length;
        for (int i = 0; i < len; i++)
        {
            FieldInfo info = fields[i];

            object attr = info.GetValue(root);

            if (attr == null) { continue; }
            if (root == attr) { continue; }

            //try
            //{
            AddField(attr, path, info.Name.ToString());
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError(attr.GetType().ToString());
            //}
        }
    }
    public static void AddField(object attr, string path, string pname)
    {
        if (filterUselessClass(attr))
        {
            return;
        }

        //if (fieldDic.ContainsKey(attr) == true)
        //{
        //    fieldDic[attr]++;
        //    return;
        //}
        string pathPlus = path + "/" + pname;
        Type attrType = attr.GetType();

        if (instanceHistoryDic.ContainsKey(attrType))
        {
            instanceHistoryDic[attrType]++;
        }
        else
        {
            instanceHistoryDic.Add(attrType, 1);
        }
        //如果是目标类型,则记录
        if (checkType == string.Empty || attr.GetType().Name == checkType)
        {
            if (fieldPathDic.ContainsKey(attr))
            {
                fieldPathDic[attr].Add(pathPlus);
            }
            else
            {
                fieldPathDic[attr] = new List<string>() { pathPlus };
            }
        }

        //检查是否存在，此处不能用Contains，因为类型可能会使Function，略迷
        try
        {
            if (attr is object && checkedObject.ContainsKey((object)attr))
            {
                return;
            }
        }
        catch (Exception e)
        {
            if (checkedObject.Any((t) => { return t.Key == attr; }))
            {
                return;
            }
        }
        //没有查询过则进入已查询列表
        checkedObject.Add(attr, checkedIdx);
        checkedIdx++;
        objectScaned++;

        //检查是否为集合，例如：List，Dictionary
        if (attr is ICollection)
        {
            int index = 0;
            foreach (var item in (ICollection)attr)
            {
                if (item == null) { continue; }
                //如果集合是双键值对，例：Dictionary，Map
                Type valueType = item.GetType();
                bool isKeyValuePair = false;
                if (valueType.IsGenericType)
                {
                    Type baseType = valueType.GetGenericTypeDefinition();
                    if (baseType == typeof(KeyValuePair<,>))
                    {
                        isKeyValuePair = true;
                        Type[] argTypes = baseType.GetGenericArguments();
                        object kvpKey = valueType.GetProperty("Key").GetValue(item, null);
                        object kvpValue = valueType.GetProperty("Value").GetValue(item, null);

                        AddField(kvpKey, path, pname + "[" + index + "]KEY:" + attr.GetType().Name);

                        AddField(kvpValue, path, pname + "[" + index + "]VALUE:" + attr.GetType().Name);

                    }
                }
                if (isKeyValuePair == false)
                {

                    if (item != null)
                    {
                        AddField(item, path, pname + "[" + index + "]" + attr.ToString());
                    }
                }
                index++;
            }
            return;
        }
        deepFind(attr, pathPlus);
    }

    public static void logDomain()
    {
        //var type = typeof(IJob);
        var types = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var item in types)
        {
            foreach (var module in item.Modules)
            {
                Debug.LogError(module.Name);
            }
        }
        //foreach (Type t in types)
        //{
        //    FieldInfo[] fields = t.GetFields();
        //    int len = fields.Length;
        //    for (int i = 0; i < len; i++)
        //    {
        //        FieldInfo info = fields[i];
        //        object attr = info.GetValue(Mono.DataConverter);
        //        if (attr == null) { continue; }
        //        if (fieldDic.ContainsKey(attr) == true)
        //        {
        //            fieldDic[attr]++;
        //            continue;
        //        }
        //        else
        //        {
        //            fieldDic.Add(attr, 1);
        //        }

        //        if (attr is ICollection)
        //        {
        //            int index = 0;
        //            foreach (var item in (ICollection)attr)
        //            {
        //                AddField(item, "/" + info.Name.ToString() + "[" + index + "]" + attr.ToString());
        //                index++;
        //            }
        //        }
        //        AddField(attr, "/" + info.Name.ToString());
        //    }
        //}
    }

    static bool filterUselessClass(object from)
    {
        if (from == null) { return true; }
        var t = from.GetType();

        if (t.IsValueType || t.IsEnum || t.IsPrimitive)
        {
            return true;
        }

        if (from == null ||
            from is int[] ||
            from is int[][] ||
            from is uint[] ||
            from is double[] ||
            from is float[] ||
            from is byte[] ||
            from is string[] ||
            from is string ||
            from is System.Reflection.Pointer ||
            from is System.Globalization.CompareInfo ||
            from is System.Text.RegularExpressions.Regex ||
            from is System.Text.RegularExpressions.Match ||
            from is System.Globalization.TextInfo ||
            from is System.Type
            || from is XmlElement
            || from is System.Xml.NameTable
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    static void rec_logGcTreeByType(Type targetType, object from, ref List<object> rt, ref Dictionary<object, string> workspace, string parentPrefix)
    {
        if (from == null)
        {
            return;
        }

        if (filterUselessClass(from))
        {
            return;
        }

        if (from.GetType() == targetType)
        {
            if (!workspace.ContainsKey(from))
            {
                workspace[from] = parentPrefix;
                rt.Add(from);
            }
            else
            {
                workspace[from] += parentPrefix;
            }

            return;
        }
        else
        {
            if (workspace.ContainsKey(from))
            {
                return;
            }
            workspace[from] = parentPrefix;
        }

        workspace["numObjectScaned"] = workspace["numObjectScaned"] + 1;



        FieldInfo[] info = null;
        info = from.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);


    }
}

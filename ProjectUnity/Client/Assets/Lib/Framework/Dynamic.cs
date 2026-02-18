using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

    /// <summary>
    /// 工具类，处理反射取值赋值
    /// </summary>
    public class Dynamic
    {
        /// <summary>
        /// 动态赋值
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="fieldName">变量名称</param>
        /// <param name="value">值</param>
        public static void SetValue(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("NoValue:" + fieldName);
#endif
            }
        }
        /// <summary>
        /// 动态赋值
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="fieldName">变量名称</param>
        /// <param name="value">值</param>
        public static void SetValue<T>(object obj, string fieldName, T value)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("NoValue:" + type.Name + "#" + fieldName);

#endif
            }
        }
        /// <summary>
        /// 动态取值
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="fieldName">变量名称</param>
        /// <returns></returns>
        public static object GetValue(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName);
			if(fieldInfo == null) {
                if (obj is Dictionary<string, object> dic && dic.TryGetValue(fieldName, out obj))
                {
                    return obj;
                }

                return null;
            }
            object rt = fieldInfo.GetValue(obj);
            return rt;
        }
        /// <summary>
        /// 动态取值
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="fieldName">变量名称</param>
        /// <returns></returns>
        public static T GetValue<T>(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName);
            T rt = (T)fieldInfo.GetValue(obj);
            return rt;
        }
    }
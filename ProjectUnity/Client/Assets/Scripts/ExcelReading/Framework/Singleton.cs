using UnityEngine;

namespace ExcelReading.Framework
{
    /// <summary>
    /// 单例模式基类，用于创建全局唯一的组件实例
    /// 使用示例：public class GameManager : Singleton<GameManager> { ... }
    /// </summary>
    /// <typeparam name="T">继承该类的具体类型</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// 获取单例实例，如果不存在则创建一个
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] 应用程序正在退出，返回null：" + typeof(T));
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (FindObjectsOfType<T>().Length > 1)
                        {
                            Debug.LogError("[Singleton] 在场景中发现多个" + typeof(T).Name + "实例！");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject(typeof(T).Name + "(Singleton)");
                            _instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                            Debug.Log("[Singleton] 创建了新的" + typeof(T).Name + "实例：" + singleton);
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[Singleton] 发现重复的实例，正在销毁：" + gameObject.name);
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }
}

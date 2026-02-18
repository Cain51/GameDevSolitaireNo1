using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using DG.Tweening;
/// <summary>
/// 单例
/// </summary>
public class CBus
{
	public List<string> factoryNameList = new List<string>();
	//释放时临时使用
	private List<string> _removeList = new List<string>();
	/// <summary>
	/// 切换场景的回调
	/// </summary>
	public Action callbackChangeSence;

	/// <summary>
	/// MainGameObject
	/// </summary>
	public GameObject mainGO;


	/// <summary>
	/// 报错信息
	/// </summary>
	public string errorMsg;
	/// <summary>
	/// 仅由C#计算、且不适合展示给玩家的报错信息，会附带到彩码和对后台提交报错协议中
	/// </summary>
	public string errorExtension = "";

	/// <summary>
	/// 报错信息列表
	/// </summary>
	public Dictionary<string, string> _errorHandle = new Dictionary<string, string>();

	/// <summary>
	/// GMTool Init
	/// </summary>
	public bool isInitGMTool = false;

	/// <summary>
	/// 正在销毁，防止重复销毁
	/// </summary>
	private bool _isDestroying;
	public bool IsDestroying
	{
		get
		{
			return _isDestroying;
		}
	}

	/// <summary>
	/// 工厂列表
	/// </summary>
	private Dictionary<string, FactoryBase> _factoryDic;
	public Dictionary<string, FactoryBase> FactoryDic
	{
		get
		{
			return _factoryDic;
		}
	}
	private Dictionary<string, ManagerBase> _managerDic;

	// [ThreadStatic]
	public static CBus _instance;
	public static CBus Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new CBus();
			}
			return _instance;
		}
	}
	/// <summary>
	/// 当前游戏执行帧数
	/// </summary>
	public int currentFrame
	{
		get
		{
			return _currentframe;
		}
	}
	private int _currentframe;
	/// <summary>
	/// 当前帧生产产品索引，用于给同一帧产生的产品排序
	/// </summary>
	public int produceIndex;

	private CBus()
	{
		Init();
	}
	/// <summary>
	/// 初始化
	/// </summary>
	private void Init()
	{
		_factoryDic = new Dictionary<string, FactoryBase>();
		_managerDic = new Dictionary<string, ManagerBase>();
		_currentframe = 0;
		errorMsg = "";
		OEF.Instance.Add(this, Update);
	}
	/// <summary>
	/// 初始化参数
	/// 场景开始的时候执行
	/// </summary>
	public void InitParams()
	{
		_isDestroying = false;

		foreach (FactoryBase factory in _factoryDic.Values)
		{
			factory.InitParams();
		}
		//工厂注册完之后执行
		foreach (ManagerBase manager in _managerDic.Values)
		{
			manager.InitParams();
		}
	}
	/// <summary>
	/// 外部注册工厂，Lua用
	/// </summary>
	public void RegFactoryOutside(string name, string nameCN, int id)
	{
#if UNITY_EDITOR
		if (factoryNameList.Contains(name))
		{
			Debug.LogError(name + "工厂重名");
			return;
		}
#endif
		FactoryBase factory = new FactoryBase();
		RegFactory(name, factory);
		factory.InitParams();
	}

	private void Update()
	{
		++_currentframe;
		produceIndex = 0;//每帧归零，重新计数

	}

	/// <summary>
	/// 清理，只清理需要清理的工厂和管理器
	/// </summary>
	public void Clear()
	{
		if (_isDestroying)
		{
			return;
		}
		_isDestroying = true;

		//清理工厂
		_removeList.Clear();
		foreach (string name in _factoryDic.Keys)
		{
			FactoryBase factory = _factoryDic[name];
			switch (factory.changeSceneOperation)
			{
				case OperationType.Clear://清理
					factory.Clear();
					break;

				case OperationType.Destroy://销毁
					factory.Destroy();
					_removeList.Add(name);
					break;

				default:
					break;
			}
		}
		//去除工厂
		int length = _removeList.Count;
		for (int i = 0; i < length; i++)
		{
			_factoryDic.Remove(_removeList[i]);
			factoryNameList.Remove(_removeList[i]);
		}
		//清理Manager
		_removeList.Clear();
		foreach (string name in _managerDic.Keys)
		{
			ManagerBase manager = _managerDic[name];
			switch (manager.changeSceneOperation)
			{
				case OperationType.Clear://清理
					manager.Clear();
					break;

				case OperationType.Destroy://销毁
					manager.Destroy();
					_removeList.Add(name);
					break;

				default:
					break;
			}
		}
		//去除Manager
		length = _removeList.Count;
		for (int i = 0; i < length; i++)
		{
			_managerDic.Remove(_removeList[i]);
		}
#if NOT_SERVER
        //清理DOTween
        DOTween.Clear();
#endif
	}
	/// <summary>
	/// 销毁,全部销毁
	/// </summary>
	public void Destroy()
	{
		if (_isDestroying)
		{
			return;
		}
		_isDestroying = true;
		//销毁工厂
		foreach (FactoryBase factory in _factoryDic.Values)
		{
			factory.Destroy();
		}
		_factoryDic.Clear();
		_factoryDic = null;
		factoryNameList.Clear();
		factoryNameList = null;
		_removeList.Clear();
		_removeList = null;
		_errorHandle.Clear();
		_errorHandle = null;
		//销毁Manager
		foreach (ManagerBase manager in _managerDic.Values)
		{
			manager.Destroy();
		}
		_managerDic.Clear();
		_managerDic = null;

		OEF.Instance.Destory();
		_instance = null;
#if NOT_SERVER
        //清理DOTween
        DOTween.Clear();
        luaEnv.Dispose();
        luaEnv = null;
#endif
	}
	/// <summary>
	/// 注册工厂
	/// </summary>
	/// <param name="name"></param>
	/// <param name="factory"></param>
	public void RegFactory(string name, FactoryBase factory, OperationType operation = OperationType.None)
	{
#if UNITY_EDITOR
		if (_factoryDic.ContainsKey(name))
		{
			Debug.LogError("重复注册工厂:" + name);
		}
#endif
		if (!_factoryDic.ContainsKey(name))
		{
			factoryNameList.Add(name);
		}
		_factoryDic[name] = factory;
		factory.changeSceneOperation = operation;
	}


	public bool HasFactory(string name)
	{
		return _factoryDic.ContainsKey(name);
	}

	/// <summary>
	/// 获取注册的工厂
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public FactoryBase GetFactory(string name)
	{
		FactoryBase rt = _factoryDic[name];
		return rt;
	}

	public void ThrowException(string msg)
	{
		//为Lua提供, 错误会被ErrorDisplay捕获并附带更多信息传递回来
		throw new Exception(msg);
	}

	public bool errored = false;
	public void ShowError(string msg)
	{
		if (errored)
		{
			//避免衍生错误信息覆盖主要错误
			return;
		}
		//停止触发入口避免重复抛错或在抛错上下文处死循环、或反复创建对象
		OEF.Instance.Clear();

		errored = true;
		Instance.errorMsg = msg;

	}

	public void RegManager(string name, ManagerBase manager, OperationType operation = OperationType.None)
	{
#if NOT_SERVER
#if UNITY_EDITOR
        if (name == ManagerBase.name)
        {
            Debug.LogError("未命名的管理器:" + manager);
            return;
        }
        if (_managerDic.ContainsKey(name))
        {
            Debug.LogError("重复注册管理器:" + name);
        }
#endif
#endif
		_managerDic[name] = manager;
		manager.changeSceneOperation = operation;
	}
	/// <summary>
	/// 获取注册的管理器
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public ManagerBase GetManager(string name, bool allowNull = false)
	{
#if UNITY_EDITOR
        if (!_managerDic.ContainsKey(name))
        {
            Debug.LogError("未注册的管理器：" + name);
            return null;
        }
#endif
		if (allowNull == true && _managerDic.ContainsKey(name) == false) { return null; }
		ManagerBase rt = _managerDic[name];
		return rt;
	}
	public Dictionary<string, FactoryBase> GetFactoryDic()
	{
		return _factoryDic;
	}
	/// <summary>
	/// 场景跳转
	/// </summary>
	public void ChangeScene(string sceneName, Action action = null)
	{
#if NOT_SERVER
#if UNITY_EDITOR
        Debug.Log("-----------------场景切换：" + sceneName);
#endif
        if (!_isDestroying)
        {
            ResManager resManager = GetManager(ManagerName.ResManager) as ResManager;
            resManager.UnloadRes(ABUnit.DisposType.ChangeScene);
        }
        Clear();
        //场景切换 需要打开指定界面的回调赋值
        callbackChangeSence = action;
        MGameManager.OnSceneEnd();
        MGameManager.OnSceneStart(sceneName);
        SceneManager.LoadScene(sceneName);
#endif
	}

#if UNITY_EDITOR
	/// <summary>
	/// 编辑器用,重启cbus
	/// </summary>
	public void RestartOnEditor()
	{
		_instance = new CBus();
	}
#endif

	/// <summary>
	/// 重启游戏
	/// </summary>
	private void RestartPrivate()
	{
		if (_isDestroying)
		{
			return;
		}
		_isDestroying = true;
		//销毁工厂
		foreach (FactoryBase factory in _factoryDic.Values)
		{
			factory.Destroy();
		}
		_factoryDic.Clear();
		factoryNameList.Clear();
		_removeList.Clear();
		//销毁Manager
		foreach (ManagerBase manager in _managerDic.Values)
		{
			manager.Destroy();
		}
		_managerDic.Clear();

		OEF.Instance.Clear();
		OEF.Instance.Add(this, Update);
		//清理DOTween
		DOTween.Clear();
	}
}

using UnityEngine;
using System;
using System.Collections.Generic;
using RG.Basic;

/// <summary>
/// 统一管理Update
/// 一个对象只能注册一个Update
/// </summary>
public class OEF
{
    private OEF()
    {
        Init();
    }
    [ThreadStatic]
    private static OEF _instance;
    public static OEF Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = new OEF();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 回调方法队列
    /// </summary>
    private LinkHead<Action> _timeScalableLink;
    private LinkHead<Action> _defaultLink;
    //不暂停队列
    private LinkHead<Action> _methodCantPauseLink;
    /// <summary>
    /// 用于查询<对象,链表node>
    /// </summary>
    private Dictionary<object, LinkNode<Action>> _timeScalableDic;
    private Dictionary<object, LinkNode<Action>> _defaultDic;
    //不暂停字典
    private Dictionary<object, LinkNode<Action>> _methodCantPauseDic;
    /// <summary>
    /// 暂停开关
    /// </summary>
    public bool isPause;
    /// <summary>
    /// 清理标记，中断Update
    /// </summary>
    private bool _isClear;

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        _timeScalableLink = new LinkHead<Action>();
        _timeScalableDic = new Dictionary<object, LinkNode<Action>>();

        _defaultLink = new LinkHead<Action>();
        _defaultDic = new Dictionary<object, LinkNode<Action>>();

        _methodCantPauseLink = new LinkHead<Action>();
        _methodCantPauseDic = new Dictionary<object, LinkNode<Action>>();
        _isClear = false;
    }

    //对报错调试有重要意义
    public LinkNode<Action> currentNode;
    //每帧清理，报错时输出此对象
    public Product debuggingObject;

    public object getCurrentTarget()
    {
        if (currentNode == null)
        {
            return null;
        }
        //根据currentNode反查当前正在运行OEF的对象，对报错定位有重要意义
        foreach (object key in _timeScalableDic.Keys)
        {
            if (_timeScalableDic[key] == currentNode)
            {
                return key;
            }
        }
        foreach (object key in _defaultDic.Keys)
        {
            if (_defaultDic[key] == currentNode)
            {
                return key;
            }
        }
        foreach (object key in _methodCantPauseDic.Keys)
        {
            if (_methodCantPauseDic[key] == currentNode)
            {
                return key;
            }
        }

        return null;
    }

    public void Update()
    {
        //在此值为true时会暂停更新动画
        //其它预编译参数将会生效
        for (int i = 0; i < GameSetting.Instance.processSpeed; i++)
        {
            UpdateTimeScalableLink();
        }
        UpdateDefaultLink();
        UpdateCantPauseLink();
        _isClear = false;
        currentNode = null;
        debuggingObject = null;
    }


    private void UpdateDefaultLink()
    {
        if (isPause)
        {
            return;
        }
        LinkNode<Action> node = _defaultLink.first;
        LinkNode<Action> next;
        while (node != null)
        {
            if (_isClear == true)
            {
                break;
            }
            next = node.next;
            //销毁标记销毁的对象
            if (node.isReadyToDestroy)
            {
                node.Remove();
                node = next;
                continue;
            }
            currentNode = node;
            node.item();
            node = next;
        }
    }

    /// <summary>
    /// 单独写成一个函数为使在profiler中查看便捷
    /// </summary>
    private void UpdateTimeScalableLinkLastTime()
    {
        UpdateTimeScalableLink();
    }

    private void UpdateTimeScalableLink()
    {
        if (isPause)
        {
            return;
        }
        LinkNode<Action> node = _timeScalableLink.first;
        LinkNode<Action> next;
        while (node != null)
        {
            if (_isClear == true)
            {
                break;
            }
            next = node.next;
            //销毁标记销毁的对象
            if (node.isReadyToDestroy)
            {
                node.Remove();
                node = next;
                continue;
            }

            currentNode = node;
            node.item();
            node = next;
        }
    }

    private void UpdateCantPauseLink()
    {
        LinkNode<Action> node = _methodCantPauseLink.first;
        LinkNode<Action> next;
        while (node != null)
        {
            if (_isClear == true)
            {
                break;
            }
            next = node.next;
            //销毁标记销毁的对象
            if (node.isReadyToDestroy)
            {
                node.Remove();
                node = next;
                continue;
            }

            currentNode = node;
            node.item();
            node = next;
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        _defaultLink.Clear();
        _defaultDic.Clear();

        _timeScalableLink.Clear();
        _timeScalableDic.Clear();

        _methodCantPauseLink.Clear();
        _methodCantPauseDic.Clear();
        _isClear = true;
    }
    /// <summary>
    /// 销毁
    /// </summary>
    public void Destory()
    {
        Clear();
        _defaultDic = null;
        _defaultLink = null;

        _timeScalableLink = null;
        _timeScalableDic = null;

        _methodCantPauseLink = null;
        _methodCantPauseDic = null;
        _instance = null;
    }

    /// <summary>
    /// 添加到此处的回调，不会应为游戏速度播放改变而改变，
    /// 添加到此处的回调可以被暂停
    /// </summary>
    /// <param name="target"></param>
    /// <param name="method"></param>
    public void AddUnscalable(object target, Action method)
    {
#if NOT_SERVER
#if UNITY_EDITOR
		if (_defaultDic.ContainsKey(target)) {
			Debug.LogWarning("OEF重复添加：" + target.ToString());
			return;
		}
#endif
#endif
        if (!_defaultDic.ContainsKey(target))
        {
            _defaultDic[target] = _defaultLink.AddLast(method);
        }
    }

    public void Add(object target, Action method)
    {

#if NOT_SERVER
#if UNITY_EDITOR
        if (_timeScalableDic.ContainsKey(target))
        {
            Debug.LogWarning("OEF重复添加：" + target.ToString());
            return;
        }
#endif
#endif
        if (!_timeScalableDic.ContainsKey(target))
        {
            _timeScalableDic[target] = _timeScalableLink.AddLast(method);
        }
    }


    /// <summary>
    /// 添加不暂停列表
    /// </summary>
    /// <param name="target"></param>
    /// <param name="method"></param>
    public void AddCantPauseLink(object target, Action method)
    {
#if NOT_SERVER
#if UNITY_EDITOR
        if (_methodCantPauseDic.ContainsKey(target))
        {
            Debug.LogWarning("OEF重复添加：" + target.ToString());
            return;
        }
#endif
#endif
        if (!_methodCantPauseDic.ContainsKey(target))
        {
            _methodCantPauseDic[target] = _methodCantPauseLink.AddLast(method);
        }
    }

    /// <summary>
    /// 清理对应对象注册的Update
    /// </summary>
    /// <param name="target"></param>
    public void Remove(object target)
    {
        if (_defaultDic.ContainsKey(target))
        {
            //标记去除，在下一帧去除，避免遍历出错（遍历时在当前节点中销毁了下一个节点，导致next.item == null）
            _defaultDic[target].isReadyToDestroy = true;
            _defaultDic.Remove(target);
        }
        if (_timeScalableDic.ContainsKey(target))
        {
            //标记去除，在下一帧去除，避免遍历出错（遍历时在当前节点中销毁了下一个节点，导致next.item == null）
            _timeScalableDic[target].isReadyToDestroy = true;
            _timeScalableDic.Remove(target);
        }
        if (_methodCantPauseDic.ContainsKey(target))
        {
            //标记去除，在下一帧去除，避免遍历出错（遍历时在当前节点中销毁了下一个节点，导致next.item == null）
            _methodCantPauseDic[target].isReadyToDestroy = true;
            _methodCantPauseDic.Remove(target);
        }
    }
}
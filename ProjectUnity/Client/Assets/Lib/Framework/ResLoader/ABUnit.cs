using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 创建日期： 2021/2/20 12:33:02
/// AssetBundle素材单元
/// </summary>
public class ABUnit
{
    /// <summary>
    /// 保存的素材
    /// </summary>
    public AssetBundle assetBundle;
    /// <summary>
    /// bundle的地址
    /// </summary>
    public string bundleName;
    /// <summary>
    /// 异步加载Request
    /// </summary>
    public AssetBundleCreateRequest request;
    /// <summary>
    /// 引用计数
    /// </summary>
    public int refCount;
    /// <summary>
    /// 加载完毕后的回调函数列表
    /// </summary>
    public List<Action> callBackList;
    /// <summary>
    /// 依赖列表
    /// </summary>
    public List<ABUnit> dependList;
    /// <summary>
    /// 当前正在加载中的依赖素材数量
    /// </summary>
    public int dependLoadingCount;
    /// <summary>
    /// 卸载类型
    /// </summary>
    public DisposType disposType;
    /// <summary>
    /// 卸载类型
    /// </summary>
    public enum DisposType
    {
        Immediately,//没有引用后立即卸载，默认卸载方式
        Restart,//重启卸载
        ChangeScene//切换场景卸载
    }
    /// <summary>
    /// 卸载倒计时
    /// </summary>
    public int unloadCountdown;

    public ABUnit()
    {
        Init();
    }
    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        refCount = 0;
        unloadCountdown = 0;
        dependLoadingCount = 0;
        disposType = DisposType.Immediately;
        dependList = new List<ABUnit>();
        callBackList = new List<Action>();
    }
    public void Destroy()
    {
#if UNITY_EDITOR && RESLOADER
        Debug.Log("<color=#FF0000>素材管理：销毁</color>" + bundleName);
#endif
        if (assetBundle != null)
        {
            assetBundle.Unload(true);
            assetBundle = null;
        }
        request = null;
        callBackList = null;
        dependList = null;
    }
    /// <summary>
    /// 依赖项加载完毕
    /// </summary>
    public void AfterDependLoaded()
    {
        --dependLoadingCount;
    }

    public Dictionary<string, int> resPathCnt = new Dictionary<string, int>();
    /// <summary>
    /// 修改引用数量
    /// </summary>
    public void ChangeRefCount(int count, string resPath)
    {

#if UNITY_EDITOR
        if (count > 0)
        {
            if (resPathCnt.ContainsKey(resPath))
            {
                resPathCnt[resPath] += count;
            }
            else
            {
                resPathCnt.Add(resPath, count);
            }

        }
        else
        {
            if (resPathCnt.ContainsKey(resPath))
            {
                resPathCnt[resPath] -= count;
            }
            else
            {
                resPathCnt.Add(resPath, -count);
            }
        }
#endif
        refCount += count;
    }
}
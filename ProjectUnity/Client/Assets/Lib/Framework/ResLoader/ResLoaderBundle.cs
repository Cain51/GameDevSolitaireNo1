using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 创建日期： 2020/10/20 16:50:51
/// 加载AssetBundle中的素材
/// 从AssetBundleManager中加载对应的素材
/// </summary>
public class ResLoaderBundle : IResLoader
{
    /// <summary>
    /// bundle文件后缀
    /// </summary>
    public static string extension = "asset";
    /// <summary>
    /// 关联列表
    /// </summary>
    private Dictionary<string, string[]> _dependList;

    /// <summary>
    /// 等待异步加载的队列，方便反查
    /// </summary>
    public Dictionary<string, ABUnit> _loadAsyncWaitDic;
    /// <summary>
    /// 正在加载的队列
    /// </summary>
    public Dictionary<string, ABUnit> _loadingAsyncDic;
    /// <summary>
    /// 已经加载好的素材列表
    /// </summary>
    public Dictionary<string, ABUnit> _loadedAssetDic;
    /// <summary>
    /// 等待被删除的素材列表
    /// 在等待删除的途中要是需要加载就拿出来用
    /// </summary>
    public Dictionary<string, ABUnit> _unloadWaitList;
    /// <summary>
    /// 用于临时存储要操作的资源名称
    /// </summary>
    private List<ABUnit> _tempABUnitList;

    /// <summary>
    /// 已经加载好的素材列表
    /// </summary>
    public Dictionary<string, int> fileNames = new Dictionary<string, int>();


    public ResLoaderBundle()
    {
        Init();
    }
    /// <summary>
    /// 获取关联列表
    /// </summary>
    private void GetDependList()
    {
        _dependList = new Dictionary<string, string[]>();
        //获取关联
        AssetBundle main;
        // AssetBundle.SetAssetBundleDecryptKey(HotfixSetting.Instance.bundleKey);
        if (GameSetting.Instance.IsResInPatch("Asset"))
        {
            main = AssetBundle.LoadFromFile(GameSetting.Instance.abPatchAddress + "Asset");
        }
        else
        {
            main = AssetBundle.LoadFromFile(GameSetting.Instance.abAddressLocal + "Asset");
        }

        //AssetBundle.SetAssetBundleDecryptKey(null);
        if (main != null)
        {
            AssetBundleManifest manifest = main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest != null)
            {
                string[] bundleList = manifest.GetAllAssetBundles();
                string[] depend;
                string bundleName;
                int length = bundleList.Length;
                for (int i = 0; i < length; i++)
                {
                    bundleName = bundleList[i];
                    depend = manifest.GetAllDependencies(bundleName);
                    if (depend.Length > 0)
                    {
                        _dependList[bundleName] = depend;
                    }
                }
            }
        }
        main.Unload(true);
    }
    private void Init()
    {
        _loadAsyncWaitDic = new Dictionary<string, ABUnit>();
        _loadingAsyncDic = new Dictionary<string, ABUnit>();
        _loadedAssetDic = new Dictionary<string, ABUnit>();
        _unloadWaitList = new Dictionary<string, ABUnit>();
        _tempABUnitList = new List<ABUnit>();
        fileNames = new Dictionary<string, int>();
        //GetDependList();
        OEF.Instance.AddCantPauseLink(this, Update);
    }

    public void InitParams()
    {
    }
    public void Clear()
    {

    }

    public void Destroy()
    {
        OEF.Instance.Remove(this);
        //释放所有未释放的bundle
        AssetBundle.UnloadAllAssetBundles(true);
        _loadAsyncWaitDic = null;
        _loadingAsyncDic = null;
        _loadedAssetDic = null;
        _unloadWaitList = null;
        _tempABUnitList = null;
#if UNITY_EDITOR
        fileNames = null;
#endif
    }

    private void Update()
    {
        UpdateWaitList();
        UpdateLoadingList();
        UpdateUnloadList();
    }
    /// <summary>
    /// 更新等待加载队列
    /// </summary>
    private void UpdateWaitList()
    {
        if (_loadAsyncWaitDic.Count < 1)
        {
            return;
        }
        //同时加载中数量超过上限
        if (_loadingAsyncDic.Count > GameSetting.Instance.abLoadingMax)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _loadAsyncWaitDic.Values)
        {
            if (aBUnit.refCount > 0)
            {
                StartLoadAsync(aBUnit);
            }
            _tempABUnitList.Add(aBUnit);
            //超出加载数量
            if (_loadingAsyncDic.Count > GameSetting.Instance.abLoadingMax)
            {
                break;
            }
        }
        //从等待队列中去除
        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            _loadAsyncWaitDic.Remove(_tempABUnitList[i].bundleName);
        }
    }
    /// <summary>
    /// 更新加载中队列
    /// </summary>
    private void UpdateLoadingList()
    {
        if (_loadingAsyncDic.Count < 1)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _loadingAsyncDic.Values)
        {
            //引用都加载完了才算加载
            if (aBUnit.dependLoadingCount < 1 && aBUnit.request != null && aBUnit.request.isDone == true)
            {
                _tempABUnitList.Add(aBUnit);
            }
        }
        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            AfterLoaded(_tempABUnitList[i]);
        }
    }
    /// <summary>
    /// 更新卸载队列
    /// </summary>
    private void UpdateUnloadList()
    {
        if (_unloadWaitList.Count < 1)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _unloadWaitList.Values)
        {
            if (aBUnit.refCount > 0)//还在使用，取消卸载
            {
                _tempABUnitList.Add(aBUnit);
            }
            else if (--aBUnit.unloadCountdown < 0)
            {
                aBUnit.Destroy();
                if (_loadedAssetDic.ContainsKey(aBUnit.bundleName))
                {
                    _loadedAssetDic.Remove(aBUnit.bundleName);
                }

                _tempABUnitList.Add(aBUnit);
            }
        }

        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            _unloadWaitList.Remove(_tempABUnitList[i].bundleName);
        }
    }

    /// <summary>
    /// 获取一个ABUnit
    /// </summary>
    /// <returns></returns>
    private ABUnit GetABUnit(string bundleName)
    {
        ABUnit rt = new ABUnit();
        rt.bundleName = bundleName;
        return rt;
    }
    /// <summary>
    /// 根据素材名称获取对应的Bundle名称
    /// </summary>
    /// <returns></returns>
    private string GetBundleName(string resPath)
    {
        string path = resPath.ToLower().Replace("\\", "/");
        int index = path.LastIndexOf("/");
        string abName = path.Substring(0, index) + "." + extension;
        return abName;
    }
    /// <summary>
    /// 获取一个素材
    /// 如果AB不存在则同步加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iPath"></param>
    /// <returns></returns>
    public T GetRes<T>(string iPath) where T : Object
    {
#if UNITY_EDITOR
        if (fileNames != null && iPath != null)
        {
            if (fileNames.ContainsKey(iPath))
            {
                fileNames[iPath]++;
            }
            else
            {
                fileNames.Add(iPath, 1);
            }
        }
#endif
        if (iPath == null)
        {
            return null;
        }
        if (iPath.Length < 1)
        {
            return null;
        }
        return Resources.Load<T>(iPath);
        //string path = iPath.ToLower().Replace("\\", "/");
        //int index = path.LastIndexOf("/");
        //string abName = path.Substring(0, index) + "." + extension;
        //string fileName = path.Substring(index + 1);
        //AssetBundle ab = LoadAssetBundle(abName, path).assetBundle;
        //T t = ab.LoadAsset<T>(fileName);
        //return t;
    }
    /// <summary>
    /// 卸载素材
    /// </summary>
    public void UnloadRes(string resPath)
    {
        return;
        if (string.IsNullOrEmpty(resPath))
        {
            return;
        }
        ABUnit aBUnit = null;
        string bundleName = GetBundleName(resPath);

#if UNITY_EDITOR
        if (fileNames.ContainsKey(resPath))
        {
            fileNames[resPath]--;
        }
#endif


        if (_loadedAssetDic.ContainsKey(bundleName))
        {
            aBUnit = _loadedAssetDic[bundleName];
        }
        else if (_loadingAsyncDic.ContainsKey(bundleName))
        {
            aBUnit = _loadingAsyncDic[bundleName];
        }
        else if (_loadAsyncWaitDic.ContainsKey(bundleName))
        {
            aBUnit = _loadAsyncWaitDic[bundleName];
        }
        RemoveRefCount(aBUnit, resPath);
    }
    /// <summary>
    /// 卸载指定卸载类型的素材，
    /// 只卸载Loaded列表中的素材
    /// </summary>
    /// <param name="disposType"></param>
    public void UnloadRes(ABUnit.DisposType disposType)
    {
        switch (disposType)
        {
            case ABUnit.DisposType.Restart://重启游戏
                UnloadResRestart();
                break;
            case ABUnit.DisposType.ChangeScene://切换场景
            default:
                UnloadResChangeScene();
                break;
        }
    }
    /// <summary>
    /// 重启卸载所有素材
    /// </summary>
    private void UnloadResRestart()
    {
        //清空等待列表
        _loadAsyncWaitDic.Clear();
        //加载中列表清理
        _loadingAsyncDic.Clear();
        //清理加载完成列表,直接卸载
        foreach (ABUnit aBUnit in _loadedAssetDic.Values)
        {
            aBUnit.assetBundle.Unload(true);
        }
        _loadedAssetDic.Clear();
        //立即清理卸载列表
        foreach (ABUnit aBUnit in _unloadWaitList.Values)
        {
            aBUnit.assetBundle.Unload(true);

        }
#if UNITY_EDITOR
        fileNames.Clear();
#endif
        _unloadWaitList.Clear();
    }
    /// <summary>
    /// 切换场景，除了Restart的素材，全部放入卸载队列
    /// </summary>
    private void UnloadResChangeScene()
    {
        _loadAsyncWaitDic.Clear();
        foreach (ABUnit aBUnit in _loadingAsyncDic.Values)
        {
            if (aBUnit.disposType != ABUnit.DisposType.Restart)
            {
                AddUnloadWaitList(aBUnit);
            }
        }
        foreach (ABUnit aBUnit in _loadedAssetDic.Values)
        {
            if (aBUnit.disposType != ABUnit.DisposType.Restart)
            {
                AddUnloadWaitList(aBUnit);
            }
        }
#if UNITY_EDITOR
        fileNames.Clear();
#endif
    }
    /// <summary>
    /// 获取依赖列表
    /// 没有依赖返回null
    /// </summary>
    private string[] GetDepends(string bundleName)
    {
        if (_dependList.ContainsKey(bundleName))
        {
            return _dependList[bundleName];
        }
        return null;
    }
    #region 引用计数器
    /// <summary>
    /// 添加一个引用计数
    /// 会给所有引用增加一个引用计数
    /// </summary>
    /// <param name="abName"></param>
    private void AddRefCount(ABUnit aBUnit, string resPath)
    {
        aBUnit.ChangeRefCount(1, resPath);
        if (aBUnit.dependList != null)
        {
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                AddRefCount(aBUnit.dependList[i], resPath);
            }
        }
    }
    /// <summary>
    /// 去除一个引用
    /// </summary>
    /// <param name="aBUnit">需要去除引用的ABUnit</param>
    /// <param name="resPath">具体释放的素材地址</param>
    private void RemoveRefCount(ABUnit aBUnit, string resPath)
    {
        if (aBUnit == null)
        {
            return;
        }
        aBUnit.ChangeRefCount(-1, resPath);
        if (aBUnit.dependList != null)
        {
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                RemoveRefCount(aBUnit.dependList[i], resPath);
            }
        }
        if (aBUnit.refCount < 1)
        {
            AddUnloadWaitList(aBUnit);
        }
    }
    /// <summary>
    /// 添加到等待卸载队列
    /// </summary>
    private void AddUnloadWaitList(ABUnit aBUnit)
    {
        aBUnit.refCount = 0;
        //延时卸载
        aBUnit.unloadCountdown = GameSetting.Instance.abUnloadCountdownBase + _unloadWaitList.Count;
#if UNITY_EDITOR
        Debug.LogWarning("<color=#0000FFFF>加入等待卸载列表:</color>" + aBUnit.bundleName + "，倒计时：" + aBUnit.unloadCountdown);
#endif
        if (!_unloadWaitList.ContainsKey(aBUnit.bundleName))
        {
            _unloadWaitList.Add(aBUnit.bundleName, aBUnit);
        }
    }
    #endregion

    #region 同步加载
    /// <summary>
    /// 同步加载AssetBundle
    /// </summary>
    /// <returns></returns>
    private ABUnit LoadAssetBundle(string bundleName, string resPath)
    {
        ABUnit aBUnit;
        //AssetBundle.SetAssetBundleDecryptKey(HotfixSetting.Instance.bundleKey);
        if (_loadedAssetDic.ContainsKey(bundleName))//已经加载
        {
            aBUnit = _loadedAssetDic[bundleName];
            //记录引用
            AddRefCount(aBUnit, resPath);
            return aBUnit;
        }
        else if (_loadingAsyncDic.ContainsKey(bundleName))//正在加载
        {
            aBUnit = _loadingAsyncDic[bundleName];
            //依赖项加载的时候会计算引用
            aBUnit.ChangeRefCount(1, resPath);
            //加载依赖
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                LoadAssetBundle(aBUnit.dependList[i].bundleName, resPath);
            }
            AfterLoaded(aBUnit);
            return aBUnit;
        }
        else if (_loadAsyncWaitDic.ContainsKey(bundleName))//等待加载
        {
            aBUnit = _loadAsyncWaitDic[bundleName];
            _loadAsyncWaitDic.Remove(bundleName);
            //依赖项加载的时候会计算引用
            aBUnit.ChangeRefCount(1, resPath);
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                LoadAssetBundle(aBUnit.dependList[i].bundleName, resPath);
            }
            AddRefCount(aBUnit, resPath);
            string path1;
            if (GameSetting.Instance.IsResInPatch(bundleName))
            {
                path1 = GameSetting.Instance.abPatchAddress + bundleName;
            }
            else
            {
                path1 = GameSetting.Instance.abAddressLocal + bundleName;
            }
            AssetBundle ab1 = AssetBundle.LoadFromFile(path1);
            aBUnit.assetBundle = ab1;
            AfterLoaded(aBUnit);
            return aBUnit;
        }
        aBUnit = GetABUnit(bundleName);
        //记录引用
        AddRefCount(aBUnit, resPath);
        string path;
        if (GameSetting.Instance.IsResInPatch(bundleName))
        {
            path = GameSetting.Instance.abPatchAddress + bundleName;
        }
        else
        {
            path = GameSetting.Instance.abAddressLocal + bundleName;
        }
        AssetBundle ab = AssetBundle.LoadFromFile(path);
        aBUnit.assetBundle = ab;
        //加载依赖
        LoadDependency(aBUnit, resPath);
        AfterLoaded(aBUnit);
        //AssetBundle.SetAssetBundleDecryptKey(null);
        return aBUnit;
    }
    /// <summary>
    /// 加载依赖的bundle
    /// </summary>
    /// <param name="key"></param>
    private void LoadDependency(ABUnit aBUnit, string resPath)
    {
        string[] abs = GetDepends(aBUnit.bundleName);
        if (abs != null)
        {
            int length = abs.Length;
            for (int i = 0; i < length; i++)
            {
                aBUnit.dependList.Add(LoadAssetBundle(abs[i], resPath));
            }
        }
    }
    #endregion

    #region 异步加载
    /// <summary>
    /// 异步加载一个资源
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="callback"></param>
    public void LoadAsync(string resPath, System.Action callback)
    {
        string bundleName = GetBundleName(resPath);
        LoadAssetBundleAsync(bundleName, resPath, callback);
    }
    /// <summary>
    /// 异步加载素材，添加到等待队列
    /// </summary>
    /// <param name="bundleName"></param>
    private ABUnit LoadAssetBundleAsync(string bundleName, string resPath, System.Action callback)
    {
        //添加到等待队列
        ABUnit aBUnit = GetABUnit(bundleName);
        AddRefCount(aBUnit, resPath);
        //先加载依赖
        LoadDependencyAsync(aBUnit, resPath);
        _loadAsyncWaitDic.Add(bundleName, aBUnit);
        return aBUnit;
    }
    /// <summary>
    /// 加载依赖的bundle
    /// </summary>
    /// <param name="key"></param>
    private void LoadDependencyAsync(ABUnit aBUnit, string resPath)
    {
        string[] abs = GetDepends(aBUnit.bundleName);
        int length = abs.Length;
        for (int i = 0; i < length; i++)
        {
            aBUnit.dependList.Add(LoadAssetBundleAsync(abs[i], resPath, aBUnit.AfterDependLoaded));
        }
    }
    /// <summary>
    /// 从等待队列中获取开始加载
    /// </summary>
    /// <param name="aBUnit"></param>
    private void StartLoadAsync(ABUnit aBUnit)
    {
        string path = GameSetting.Instance.abAddressLocal + aBUnit.bundleName;
        aBUnit.request = AssetBundle.LoadFromFileAsync(path);
        _loadingAsyncDic.Add(aBUnit.bundleName, aBUnit);
    }
    #endregion
    /// <summary>
    /// 加载完毕要执行的方法
    /// </summary>
    private void AfterLoaded(ABUnit aBUnit)
    {
#if UNITY_EDITOR && RESLOADER
        Debug.Log("<color=#CC6411ff>素材管理：加载</color>" + aBUnit.bundleName);
#endif
        if (aBUnit.request != null)
        {
            aBUnit.assetBundle = aBUnit.request.assetBundle;//异步加载的会转成同步加载
        }
        if (_loadingAsyncDic.ContainsKey(aBUnit.bundleName))
        {
            _loadingAsyncDic.Remove(aBUnit.bundleName);
        }
        _loadedAssetDic.Add(aBUnit.bundleName, aBUnit);
        //先移到Loaded队列再执行callBack，可能回调中会加载当前素材，顺序反过来就错误的不会加载后续素材了
        if (aBUnit.callBackList.Count > 0)
        {
            System.Action callBack;
            int length = aBUnit.callBackList.Count;
            for (int i = 0; i < length; i++)
            {
                callBack = aBUnit.callBackList[i];
                if (callBack != null)
                {
                    callBack();
                }
            }
            aBUnit.callBackList.Clear();
        }
    }
}
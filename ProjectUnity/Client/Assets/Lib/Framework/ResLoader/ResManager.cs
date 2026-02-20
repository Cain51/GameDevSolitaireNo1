using System.Collections.Generic;
using UnityEngine;

public class ResManager : ManagerBase
{
    #region 预加载
    /// <summary>
    /// 预加载列表
    /// </summary>
    private PreloadHandler _preloadHandler;
    /// <summary>
    /// 预加载列表
    /// </summary>
    private List<int> _preloadList;
    #endregion

    private IResLoader _resloader;

#if UNITY_EDITOR

    public IResLoader resloader
    {
        get { return _resloader; }
    }

#endif

    /// <summary>
    /// BuildIn的Material素材，只在这里存一份
    /// </summary>
    private Dictionary<string, Material> _buildInMaterilDic;

    protected override void Init()
    {
        base.Init();
        _preloadHandler = new PreloadHandler();
        _preloadList = new List<int>();
        _buildInMaterilDic = new Dictionary<string, Material>();
#if UNITY_EDITOR
        if (GameSetting.Instance.isUseAB)
        {
            _resloader = new ResLoaderBundle();
        }
        else
        {
            _resloader = new ResLoaderEditor();
        }
#else
        _resloader = new ResLoaderBundle();
        
#endif
    }
    public override void InitParams()
    {
        base.InitParams();
        _resloader.InitParams();
        _preloadHandler.InitParams();
    }
    public override void Clear()
    {
        base.Clear();
        _resloader.Clear();
        _preloadHandler.Clear();
        _preloadList.Clear();
    }
    public override void Destroy()
    {
        base.Destroy();
        _buildInMaterilDic.Clear();
        _buildInMaterilDic = null;
        _resloader.Destroy();
        _resloader = null;
        _preloadHandler.Destroy();
        _preloadHandler = null;
        _preloadList = null;
    }

    /// <summary>
    /// 预加载素材
    /// </summary>
    public void LoadRes()
    {

    }
    /// <summary>
    /// 存储一个BuildInMaterial
    /// </summary>
    public void AddBuildInMaterial(string name, Material material)
    {
        if (!_buildInMaterilDic.ContainsKey(name))
        {
            _buildInMaterilDic.Add(name, material);
        }
    }
    /// <summary>
    /// 确认BuildInMaterial列表里面有没有存
    /// </summary>
    public bool CheckBuildInMaterial(string name)
    {
        return _buildInMaterilDic.ContainsKey(name);
    }
    /// <summary>
    /// 获取一个BuildInMaterial
    /// 默认认为必然在列表里
    /// </summary>
    public Material GetBuildInMaterial(string name)
    {
        return _buildInMaterilDic[name];
    }
    /// <summary>
    /// 获取素材
    /// </summary>
    /// <returns></returns>
    public T GetRes<T>(string path) where T : Object
    {
        return _resloader.GetRes<T>(path);
    }
    /// <summary>
    /// 卸载指定素材，引用计数器-1
    /// </summary>
    public void UnloadRes(string resPath)
    {
        _resloader?.UnloadRes(resPath);
    }
    /// <summary>
    /// 卸载指定卸载类型的素材，
    /// 直接卸载
    /// 只卸载Loaded列表中的素材
    /// </summary>
    /// <param name="disposType"></param>
    public void UnloadRes(ABUnit.DisposType disposType)
    {
        _resloader.UnloadRes(disposType);
    }

    #region 预加载
    /// <summary>
    /// 预加载之前的初始化
    /// </summary>
    public void PreloadInit()
    {
        _preloadList.Clear();
    }
    /// <summary>
    /// 添加一个预加载产品的id
    /// </summary>
    public void AddPreloadProductID(int id)
    {
        _preloadList.Add(id);
    }
    /// <summary>
    /// 生成预加载素材列表
    /// </summary>
    public void CreatePreloadList()
    {
        _preloadHandler.Prepare();
        int length = _preloadList.Count;
        int id;
        for (int i = 0; i < length; i++)
        {
            id = _preloadList[i];
            _preloadHandler.AddId(id);
        }
    }
    #endregion
}
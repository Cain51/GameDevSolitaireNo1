using System.Collections.Generic;

public class PreloadHandler
{
    private CBus _cBus;
    /// <summary>
    /// 预加载素材列表
    /// </summary>
    private List<string> _preloadList;
    /// <summary>
    /// 已经处理过的ID列表，防止重复处理
    /// </summary>
    private List<int> _idList;
    public PreloadHandler()
    {
        _preloadList = new List<string>();
        _idList = new List<int>();
    }
    public void InitParams()
    {
        _cBus = CBus.Instance;

    }
    /// <summary>
    /// 准备加载
    /// </summary>
    public void Prepare()
    {
        _preloadList.Clear();
        _idList.Clear();
    }
    /// <summary>
    /// 初始化
    /// 在开始获取素材列表之前需要初始化一次
    /// </summary>
    public void Clear()
    {
        _cBus = null;
        _preloadList.Clear();
        _idList.Clear();
    }
    /// <summary>
    /// 销毁
    /// </summary>
    public void Destroy()
    {
        _preloadList = null;
        _idList = null;
    }
    /// <summary>
    /// 添加一个id
    /// </summary>
    public void AddId(int id)
    {
        if (!_idList.Contains(id))
        {
            _idList.Add(id);
            //FactoryBase factory;
            //factory = _cBus.GetFactory(_dataManager.GetFactoryNameById(id));
            //factory.GetBattleResPath(this, id);
        }
    }
    /// <summary>
    /// 添加素材
    /// </summary>
    public void AddRes(string res)
    {
        _preloadList.Add(res);
    }
    /// <summary>
    /// 获取预加载素材列表
    /// </summary>
    /// <returns></returns>
    public List<string> GetResList()
    {
        return _preloadList;
    }
}
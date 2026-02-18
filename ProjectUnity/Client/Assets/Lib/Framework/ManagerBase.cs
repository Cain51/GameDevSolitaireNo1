/// <summary>
/// 工厂和管理器在切换场景时的操作
/// </summary>
public enum OperationType : int
{
    None = 0,//不清理不销毁
    Clear = 1,//清理
    Destroy = 2//销毁
}
public class ManagerBase
{
    /// <summary>
    /// 管理器名称
    /// </summary>
    public const string name = "Default";
    /// <summary>
    /// 切换场景的时候是否需要清理
    /// </summary>
    public OperationType changeSceneOperation;

    protected CBus _cbus;

    public ManagerBase()
    {
        changeSceneOperation = OperationType.None;
        Init();
    }
    protected virtual void Init()
    {
        _cbus = CBus.Instance;
    }
    /// <summary>
    /// 初始化参数
    /// 场景开始时执行
    /// </summary>
    public virtual void InitParams()
    {


    }
    /// <summary>
    /// 清理
    /// </summary>
    public virtual void Clear()
    {
        OEF.Instance.Remove(this);
    }
    /// <summary>
    /// 释放
    /// </summary>
    public virtual void Destroy()
    {
        Clear();
        _cbus = null;
    }
}

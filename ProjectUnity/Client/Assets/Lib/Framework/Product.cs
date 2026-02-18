public class Product
{
    /// <summary>
    /// 记录产生的帧数
    /// </summary>
    public int produceFrame;
    /// <summary>
    /// 上一次回收的帧数
    /// </summary>
    public int lastRecycleFrame;
    /// <summary>
    /// 记录同一帧中这是第几个生产的产品
    /// </summary>
    public int produceIndex;
    /// <summary>
    /// 唯一Id，caid+ 产生帧数+产生索引
    /// </summary>
    public string uniqueId;

    protected CBus _cbus;
    public bool isRecycled;
    public CABase ca;
    public Product(CABase ca)
    {
        this.ca = ca;
        Init();
        InitParams();
    }

    public virtual string GetDebugInfo()
    {
        return ca.id.ToString();
    }


    /// <summary>
    /// 创建变量实例
    /// </summary>
    protected virtual void Init()
    {
        OEF.Instance.debuggingObject = this;
        _cbus = CBus.Instance;
    }
    /// <summary>
    /// 初始化参数，Reuse和构造函数调用
    /// </summary>
    protected virtual void InitParams()
    {
        OEF.Instance.debuggingObject = this;
    }
    /// <summary>
    /// 回收
    /// 内存池满了或者工厂销毁自动调用Destroy
    /// </summary>
    public void Recycle()
    {
        //防止重复回收
        if (isRecycled)
        {
            return;
        }
        isRecycled = true;
        lastRecycleFrame = CBus.Instance.currentFrame;
        OEF.Instance.debuggingObject = this;
        RecyclePrivate();
        ca.factory.Recycle(this, Destroy);
    }
    /// <summary>
    /// 子类需要做的回收重写这个方法
    /// 解决回收的顺序
    /// </summary>
    protected virtual void RecyclePrivate()
    {
        OEF.Instance.Remove(this);
    }
    /// <summary>
    /// 销毁
    /// 只在工厂的Recycle方法中调用
    /// </summary>
    public virtual void Destroy()
    {
        OEF.Instance.debuggingObject = this;
        _cbus = null;
    }
    /// <summary>
    /// 重用赋值
    /// </summary>
    public virtual void Reuse()
    {
        isRecycled = false;
        InitParams();
    }
}
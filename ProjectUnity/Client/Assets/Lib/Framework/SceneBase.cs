
using RG.Basic;
using UnityEngine;
/// <summary>
/// 场景基类
/// </summary>
public class SceneBase : MonoBehaviour
{
    protected CBus _cbus;

    private void Awake()
    {
        Files.CopyFolder(Application.streamingAssetsPath, FileTools.CachePath);

        SceneAwake();
        _cbus = CBus.Instance;
        //注册场景需要的工厂和Manager
        Reg();
        //初始化工厂和Manager
        _cbus.InitParams();
        //初始化
        Init();
    }
    private void OnDestroy()
    {
        Destory();
        _cbus = null;
    }
    /// <summary>
    /// 场景唤醒时执行
    /// </summary>
    protected virtual void SceneAwake()
    {

    }
    /// <summary>
    /// 初始化，执行在Awake中
    /// </summary>
    protected virtual void Init()
    {

    }
    /// <summary>
    /// 销毁，执行在OnDestroy中
    /// </summary>
    protected virtual void Destory()
    {

    }
    /// <summary>
    /// 注册工厂和Manager
    /// </summary>
    protected virtual void Reg()
    {

    }
    private void Start()
    {
        StartScene();
    }
    /// <summary>
    /// 场景开始
    /// </summary>
    protected virtual void StartScene()
    {

    }
}
using UnityEngine;
/// <summary>
/// 创建日期： 2020/10/20 16:23:05
/// 资源加载接口,加载器都需要实现这个接口
/// </summary>
public interface IResLoader
{
    /// <summary>
    /// 初始化
    /// </summary>
    void InitParams();
    /// <summary>
    /// 清理
    /// </summary>
    void Clear();
    /// <summary>
    /// 释放
    /// </summary>
    void Destroy();
    /// <summary>
    /// 获取指定类型的素材
    /// 如果没有素材直接阻塞加载
    /// 如果正在异步加载，改成同步加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    T GetRes<T>(string path) where T : Object;
    /// <summary>
    /// 异步加载一个素材
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    void LoadAsync(string path, System.Action callback);
    /// <summary>
    /// 卸载指定素材，引用计数器-1
    /// </summary>
    void UnloadRes(string resPath);
    /// <summary>
    /// 卸载指定卸载类型的素材，
    /// 直接卸载
    /// 只卸载Loaded列表中的素材
    /// </summary>
    /// <param name="disposType"></param>
    void UnloadRes(ABUnit.DisposType disposType);
}
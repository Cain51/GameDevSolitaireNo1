using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ResLoaderEditor : IResLoader
{
    public ResLoaderEditor()
    {
    }

    public void Clear()
    {

    }

    public void Destroy()
    {

    }

    public T GetRes<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        string finalPath = GameSetting.Instance.resAddress + path;
        System.Type type = typeof(T);
        switch (type.Name)
        {
            case "GameObject":
                finalPath = "Assets\\" + finalPath + ".prefab";
                break;
            case "AudioClip":
                finalPath = "Assets\\" + finalPath + ".mp3";
                break;
            case "Sprite":
            case "Texture2D":
                finalPath = "Assets\\" + finalPath + ".png";
                break;
            case "SkeletonDataAsset":
                finalPath = "Assets\\" + finalPath + ".asset";
                break;
            case "Material":
                finalPath = "Assets\\" + finalPath + ".mat";
                break;
            case "VideoClip":
                finalPath = "Assets\\" + finalPath + ".mp4";
                break;
            case "Shader":
                finalPath = "Assets\\" + finalPath + ".shader";
                break;
            case "RuntimeAnimatorController":
                finalPath = "Assets\\" + finalPath + ".controller";
                break;
            case "TextAsset":
                finalPath = "Assets\\" + finalPath + ".txt";
                break;
            default:
                Debug.LogError("未知素材类型：" + type.Name);
                finalPath = "Assets\\" + finalPath;
                break;
        }

        T t = AssetDatabase.LoadAssetAtPath<T>(finalPath);

        if (t == null && !string.IsNullOrEmpty(path))
        {
            Debug.LogError("获取素材失败：" + path);
        }

        return t;
#endif
        throw new System.NotImplementedException();
    }

    public void InitParams()
    {

    }

    public void LoadAsync(string path, System.Action callback)
    {

    }
    /// <summary>
    /// 卸载指定素材，引用计数器-1
    /// </summary>
    public void UnloadRes(string resPath)
    {

    }
    /// <summary>
    /// 卸载指定卸载类型的素材，
    /// 直接卸载
    /// 只卸载Loaded列表中的素材
    /// </summary>
    /// <param name="disposType"></param>
    public void UnloadRes(ABUnit.DisposType disposType)
    {

    }
}
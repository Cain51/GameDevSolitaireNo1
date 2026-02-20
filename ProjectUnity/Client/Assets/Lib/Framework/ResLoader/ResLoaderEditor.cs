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
        Resources.UnloadUnusedAssets();
    }

    public void Destroy()
    {
        Resources.UnloadUnusedAssets();
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
                Debug.LogError("δ֪�ز����ͣ�" + type.Name);
                finalPath = "Assets\\" + finalPath;
                break;
        }

        T t = AssetDatabase.LoadAssetAtPath<T>(finalPath);

        if (t == null && !string.IsNullOrEmpty(path))
        {
            Debug.LogError("��ȡ�ز�ʧ�ܣ�" + path);
        }

        return t;
#endif
        throw new System.NotImplementedException();
    }

    public void InitParams()
    {
        Resources.UnloadUnusedAssets();
    }

    public void LoadAsync(string path, System.Action callback)
    {
        GetRes<Object>(path);
        callback?.Invoke();
    }
    /// <summary>
    /// ж��ָ���زģ����ü�����-1
    /// </summary>
    public void UnloadRes(string resPath)
    {
        Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// ж��ָ��ж�����͵��زģ�
    /// ֱ��ж��
    /// ֻж��Loaded�б��е��ز�
    /// </summary>
    /// <param name="disposType"></param>
    public void UnloadRes(ABUnit.DisposType disposType)
    {
        Resources.UnloadUnusedAssets();
    }
}

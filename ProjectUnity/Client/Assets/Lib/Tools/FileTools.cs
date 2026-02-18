using RG.Basic;
using RG.Basic.Helper;
using System.IO;
using UnityEngine;

public static class FileTools
{
    public static bool usingOnlineDocument = false;
    public static string developDocPath = "../../res/client_res/";
   
    public static string CachePath
    {
        get
        {
#if NETFX_CORE
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return Application.streamingAssetsPath + "/";
#endif
        }
    }
    public static string GetDocumentPath(string route)
    {
#if UNITY_EDITOR
        if (usingOnlineDocument)
        {
            return Path.Combine(CachePath, route);
        }
        else
        {
            return GetEDocumentPath(route);
        }
#else
  return Path.Combine(CachePath, route);
#endif
	}
	public static string ShrinkPath(string str_origin, char splitor = '/')
    {
        Seq<string> strs = str_origin.Split(splitor);
        for (int i = 0; i < strs.Count; i++)
        {
            if (strs[i] == "..")
            {
                strs.RemoveAt(i--);
                strs.RemoveAt(i--);
            }
            else if (strs[i] == ".")
            {
                strs.RemoveAt(i--);
            }
        }
        return strs.Reduce((a, b) => a + splitor + b);
    }
    /// <summary>
    /// For Editor Only
    /// </summary>
    public static string EAssetPath { get { return Application.dataPath + "/"; } }
    /// <summary>
    /// For Editor Only
    /// </summary>
    public static string EDocumentPath
    {
        get
        {
            string edoc_path = ShrinkPath(EAssetPath + developDocPath);
            //string edoc_path = EAssetPath + Preference.developDocPath;
            return edoc_path;
        }
    }
    public static bool IsDocumentPathExist { get { return Files.DirExist(EDocumentPath); } }

    public static string GetEDocumentPath(string route)
    {
        string doc_path = StrGen.Start(EDocumentPath).Append(route).End;
#if UNITY_EDITOR
        if (!IsDocumentPathExist)
        {
            throw new System.Exception("Error: 'Doc' Not Exist. Please Check: " + EDocumentPath + "\nRequest: " + doc_path);

        }
        else
#else
            if (!IsDocumentPathExist) {
               // _Log.Fatal("EDocumentPath cannot be called in non-editor procedure");
                return StrGen.Start(CachePath).Append(route).End;
            }
#endif
            return doc_path;
    }
    public static void Save(string fullFileName, string jsonCode)
    {
        //文件流信息
        StreamWriter sw;
        FileInfo t = new FileInfo(fullFileName);


        //如果此文件不存在则创建
        if (t.Exists)
        {//判断文件是否存在
            t.Delete();
        }

        sw = t.CreateText();//不存在，创建
        //以行的形式写入信息
        sw.WriteLine(jsonCode);
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();

    }
    public static string CheckExsitDeleteDirectory(string DirectoryName, string dataPath)
    {
        string path = Application.dataPath + dataPath + DirectoryName + "/";

        if (Directory.Exists(path))
        {
            DeleteFolder(path);
        }
        else
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    public static string ReadStringIfFileExist(string path)
    {
        if (!File.Exists(path)) return null;
        string text = "";
#if (UNITY_WP8 || UNITY_METRO) && !UNITY_EDITOR
			byte[] buffer = UnityEngine.Windows.File.ReadAllBytes (path);
			Text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
#else
        text = Files.ReadStringIfFileExist(path);
#endif
        return text;
    }

    public static byte[] ReadAllBytesIfFileExist(string path)
    {
        if (!File.Exists(path)) return null;
#if (UNITY_WP8 || UNITY_METRO) && !UNITY_EDITOR
			return UnityEngine.Windows.File.ReadAllBytes (path); 
#else
        return Files.ReadAllBytesIfFileExist(path);
#endif
    }

    /// <summary>
    /// 清空指定的文件夹，但不删除文件夹
    /// </summary>
    /// <param name="dir"></param>
    private static void DeleteFolder(string dir)
    {
        foreach (string d in Directory.GetFileSystemEntries(dir))
        {
            if (File.Exists(d))
            {
                try
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件 
                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
                catch
                {

                }
            }
        }
    }

}

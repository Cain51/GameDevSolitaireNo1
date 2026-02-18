using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
/// <summary>
/// 存放默认设置
/// </summary>
public class GameSetting
{
    /// <summary>
    /// 设置当前版本号
    /// </summary>
    public string version = Application.version;

    /// <summary>
    /// 配置文件更新地址
    /// </summary>
    public string configPatchAddress = Application.persistentDataPath + "/Patch/Config/";
    /// <summary>
    /// lua文件更新地址
    /// </summary>
    public string luaPatchAddress = Application.persistentDataPath + "/Patch/Script/";
    /// <summary>
    /// 配置文件地址
    /// </summary>
    public string configFileAddress = Application.dataPath + "/GameRes/Config/";
    /// <summary>
    /// 压缩bin格式配置文件地址
    /// </summary>
    public string binConfigFileAddress = Application.dataPath + "/GameRes/BinaryConfig/";
    /// <summary>
    /// Lua文件夹地址
    /// </summary>
    public string luaFileAddress = Application.dataPath + "/GameRes/Script/";

    /// <summary>
    /// 素材文件地址
    /// </summary>
    public string resAddress = "GameRes/Res/";
    /// <summary>
    /// assetBundle沙盒路径
    /// </summary>
    public string abPatchAddress = Application.persistentDataPath + "/Patch/Asset/";
    /// <summary>
    /// assetBundle本地路径
    /// </summary>
    public string abAddressLocal = Application.streamingAssetsPath + "/Asset/";
    /// <summary>
    /// assetBundle卸载的基础帧数
    /// </summary>
    public int abUnloadCountdownBase = 180;
    /// <summary>
    /// 同时加载数量上限
    /// </summary>
    public int abLoadingMax = 5;
    /// <summary>
    ///单帧时间
    /// </summary>
    public float frameTime = 0.0167f;
    /// <summary>
    ///单帧时间SafeNumber
    /// </summary>
    public long frameTime_SN = 167;
    /// <summary>
    /// 游戏FPS
    /// </summary>
    public int fps = 60;
    /// <summary>
    /// 模型层播放速度倍数。希望减速播放请使用bulletTime
    /// 此值在OEF中读取
    /// </summary>
    public int processSpeed = 1;
    /// <summary>
    /// 队伍角色数量
    /// </summary>
    public int teamRoleNum = 5;
    /// <summary>
    /// 重力，每帧Y向下加速度
    /// </summary>
    public long gravitySN = 1000;
    /// <summary>
    /// 摩擦力，每帧被击飞速度衰减值
    /// </summary>
    public long frictionSN = 1000;

    //资源管理------------------------------------

    /// <summary>
    /// 支持值"xml", "bin",此值需要通过反射从GameSetting.txt读取，并因此不能使用枚举。
    /// </summary>
    public string factoryDataType = "xml";


    public bool isUseAB = false;
    /// <summary>
    /// 补丁素材列表
    /// </summary>
    private List<string> _patchResList;
    /// <summary>
    /// 读取本地GameSetting
    /// </summary>
    public void LoadSetting()
    {
#if UNITY_EDITOR
        TextAsset ta = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameRes/GameSetting.txt");

        if (ta == null)
        {
            Debug.Log("缺少Assets/GameRes/GameSetting.txt");
            return;
        }

        string[] arr = Regex.Split(ta.text, @"[\r\n]+");

        int length = arr.Length;
        for (int i = 0; i < length; i++)
        {
            string line = arr[i];
            if (!String.IsNullOrEmpty(line))
            {
                var firstEqualIndex = line.IndexOf('=');
                if (firstEqualIndex != -1)
                {
                    var key = line.Substring(0, firstEqualIndex).Trim();
                    var value = line.Substring(firstEqualIndex + 1);

                    //去注释
                    if (value.IndexOf("//") != -1)
                    {
                        if (line.LastIndexOf("://") != -1 && line.LastIndexOf("://") == line.LastIndexOf("//") - 1)
                        {

                        }
                        else if (line.LastIndexOf("///") != -1 && line.LastIndexOf("///") == line.LastIndexOf("//") - 1)
                        {

                        }
                        else
                        {
                            value = value.Substring(0, value.IndexOf("//"));
                        }
                    }
                    if (value.IndexOf(";") != -1)
                    {
                        value = value.Substring(0, value.LastIndexOf(";"));
                    }
                    //去空格
                    value = value.Trim();
                    //去引号
                    if (value.IndexOf('"') == 0)
                    {
                        value = value.Substring(1, value.Length - 2);
                        Dynamic.SetValue<string>(this, key, value);
                    }
                    else
                    {
                        if (value == "false" || value == "False")
                        {
                            Dynamic.SetValue<bool>(this, key, false);
                        }
                        else if (value == "true" || value == "True")
                        {
                            Dynamic.SetValue<bool>(this, key, true);
                        }
                        else
                        {
                            Dynamic.SetValue<int>(this, key, int.Parse(value));
                        }
                    }
                }
            }
        }
#endif
    }

    /// <summary>
    /// 素材是否是在Patch中
    /// </summary>
    /// <returns></returns>
    public bool IsResInPatch(string name)
    {
        if (_patchResList.Contains(name))
        {
            //patch地址
            return true;
        }
        //本地地址
        return false;
    }
    private static GameSetting _instance;
    public static GameSetting Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameSetting();

                //加载本地设置
                _instance.LoadSetting();

            }
            return _instance;
        }
    }



    private GameSetting()
    {
       
    }
}

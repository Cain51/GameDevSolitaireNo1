using System.Collections.Generic;
using Godot;

public enum Attributes
{
    智力 = 0,
    体力 = 1,
    美貌 = 2,
    劳动 = 3,
    武德 = 4,
    幸运 = 5,
}

public static class AttributeExtensions
{
    static Dictionary<Attributes, List<string>> _displayNames = new Dictionary<Attributes, List<string>>()
    {
        { Attributes.智力, new List<string> { "黑洞无毛", "一学就废", "人间清醒", "博学多才", "多智近妖"} },
        { Attributes.体力, new List<string> { "体弱霍金", "黛玉之躯", "不咳不喘", "身强体健", "肌肉虬结"} },
        { Attributes.美貌, new List<string> { "辟邪驱鬼", "平平无奇", "氛围帅哥", "清新脱俗", "倾国倾城"} },
        { Attributes.劳动, new List<string> { "一无是处", "手懒脚软", "勤勤恳恳", "心灵手巧", "巧匠天工" } },
        { Attributes.武德, new List<string> { "无耻下流", "兵不厌诈", "嘴强王者", "心怀正义", "圣人再世" } },
        { Attributes.幸运, new List<string> { "倒霉透顶", "喝水塞牙", "不好不坏", "逢凶化吉", "好运连连" } },
    };

    public static string GetDisplayName(this Attributes attribute)
    {
        return attribute switch
        {
            Attributes.智力 => "智力",
            Attributes.体力 => "体力",
            Attributes.美貌 => "美貌",
            Attributes.劳动 => "劳动",
            Attributes.武德 => "武德",
            Attributes.幸运 => "幸运",
            _ => attribute.ToString()
        };
    }

    public static string GetLevelName(this Attributes attribute, int level)
    {
        var list = _displayNames[attribute];
        return list[Mathf.Clamp(level, 0, list.Count - 1)];
    }
}
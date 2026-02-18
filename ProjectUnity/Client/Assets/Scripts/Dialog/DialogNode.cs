using RG.Basic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

public class DialogNode : Node
{
	[Input] public string input;  // 上一个节点
	public int speakerid;
	public string ani;
	public List<Pair<int,int>> rewards;  // 对话文本列表，逐段显示
	public List<Pair<int, int>> cost;  // 对话文本列表，逐段显示
	[TextArea] public List<string> dialogText;  // 对话文本列表，逐段显示
	public List<DialogChoice> choices = new List<DialogChoice>();  // 选择项列表

	// 动态输出端口，数量取决于choices
	[Output(dynamicPortList = true)] public DialogNode nextNode;

	public string scene;  // 是否是结局节点
    public string prefab;  // 是否是结局节点
    public string endingDescription;  // 结局描述
	public int eventid;
	// 返回输出端口的类型
	public override object GetValue(NodePort port)
	{
		return null;  // 此处不需要返回具体的值
	}

	// 动态更新输出端口
	public override void OnCreateConnection(NodePort from, NodePort to)
	{
		base.OnCreateConnection(from, to);
		UpdateDynamicPorts();
	}

	// 动态创建或删除端口，基于choices数量
	public void UpdateDynamicPorts()
	{
		int choiceCount = choices.Count;

		// 移除多余的动态端口
		while (DynamicPorts.Count() > choiceCount)
		{
			RemoveDynamicPort("nextNode " + (DynamicPorts.Count() - 1));
		}

		// 添加缺少的动态端口
		while (DynamicPorts.Count() < choiceCount)
		{
			AddDynamicOutput(typeof(DialogNode), fieldName: "nextNode " + DynamicPorts.Count());
		}
	}

}
[System.Serializable]
public class DialogChoice
{
	public string choiceText;  // 选择文本
	public List<Condition> conditions;  // 条件列表

	// 检查该选择是否可用
	public bool IsChoiceAvailable(Dictionary<int, int> playerStats)
	{
		foreach (var condition in conditions)
		{
			if (!playerStats.ContainsKey(condition.id) || !condition.IsConditionMet(playerStats[condition.id]))
			{
				return false;  // 有任何一个条件不满足，该选择项不可用
			}
		}
		return true;  // 所有条件均满足，该选择项可用
	}
}

[System.Serializable]
public class Condition
{
	public int id;  // 条件对应的ID
	public ComparisonType comparisonType;  // 比较运算符
	public int value;  // 目标值

	public enum ComparisonType
	{
		GreaterThan,    // >
		LessThan,       // <
		EqualTo,        // ==
		GreaterOrEqual, // >=
		LessOrEqual     // <=
	}

	// 检查条件是否满足
	public bool IsConditionMet(int currentValue)
	{
		switch (comparisonType)
		{
			case ComparisonType.GreaterThan:
				return currentValue > value;
			case ComparisonType.LessThan:
				return currentValue < value;
			case ComparisonType.EqualTo:
				return currentValue == value;
			case ComparisonType.GreaterOrEqual:
				return currentValue >= value;
			case ComparisonType.LessOrEqual:
				return currentValue <= value;
			default:
				return false;
		}
	}
}
using Godot;
using System;
using System.Linq;

public partial class CreateActor : ColorRect
{
	// RadarChart引用
	private RadarChart _radarChart;
	
	// 按钮引用
	private Button _confirmButton;
	private Button _randomButton;
	private Label _nameLabel;
	private Label _label;
	[Export] private int MinAttribute = 5;
	[Export] private int MaxAttribute = 10;
	
	private int[] currentAttributes;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 获取RadarChart节点
		_radarChart = GetNode<RadarChart>("RadarChart");
		
		// 获取按钮节点
		_confirmButton = GetNode<Button>("Confirm");
		_randomButton = GetNode<Button>("Random");
		_nameLabel = GetNode<Label>("NameLabel");
		_label = GetNode<Label>("Label");
		
		// 连接按钮信号
		if (_confirmButton != null)
		{
			_confirmButton.Pressed += OnConfirmButtonPressed;
		}
		
		if (_randomButton != null)
		{
			_randomButton.Pressed += OnRandomButtonPressed;
		}
		
		// 初始随机生成一组属性值
		RandomizeAttributes();
	}
	
	// 确认按钮点击方法
	private void OnConfirmButtonPressed()
	{
		GD.Print("确认按钮被点击");
		
		// 获取当前雷达图的属性值
		float[] currentValues = _radarChart?.AttributeValues;
		
		if (currentValues != null)
		{
			GD.Print("当前属性值:");
			for (int i = 0; i < currentValues.Length; i++)
			{
				GD.Print($"  {_radarChart.AttributeNames[i]}: {currentValues[i]}");
			}
			
			// 在这里添加确认逻辑
			// 例如：保存角色数据，切换到其他场景等
			ConfirmActor();
		}
	}
	
	// 重新随机按钮点击方法
	private void OnRandomButtonPressed()
	{
		GD.Print("重新随机按钮被点击");
		RandomizeAttributes();
	}
	
	// 随机生成属性值
	private void RandomizeAttributes()
	{
		if (_radarChart == null)
			return;

		Random random = new Random();
		int fullAttributePoints = random.Next(MinAttribute, MaxAttribute + 1);
		int[] randomValues = new int[6];

		// 随机选择起始索引 (0-5)
		int startIndex = random.Next(0, 6);

		// 从随机起始位置开始循环分配
		for (int offset = 0; offset < 6; offset++)
		{
			int i = (startIndex + offset) % 6; // 循环索引

			int maxForCurrent = Math.Min(fullAttributePoints, 100);
			randomValues[i] = random.Next(0, 5);
			fullAttributePoints -= randomValues[i];

			// 如果点数分配完，剩余属性设为0
			if (fullAttributePoints <= 0)
			{
				// 继续分配剩余的属性（从下一个位置开始）
				for (int remainingOffset = offset + 1; remainingOffset < 6; remainingOffset++)
				{
					int j = (startIndex + remainingOffset) % 6;
					randomValues[j] = 0;
				}
				break;
			}
		}

		// 使用动画过渡到新值
		_radarChart.AnimateToValues([.. randomValues.Select(x => x == 0 ? 0.001f : (float)x)], 1.0f);
		currentAttributes = randomValues;

		_nameLabel.Text = Find.Player.GetName(currentAttributes);
		GD.Print($"已随机生成新属性值 (起始索引: {startIndex}): [{string.Join(", ", randomValues)}]");
	}
	
	// 确认角色创建的方法
	private void ConfirmActor()
	{
		Find.Game.Player.AttributeValues = currentAttributes;
		Find.Game.Player.Name = Find.Player.GetName(currentAttributes);
		GD.Print($"角色已确认: {Find.Game.Player.Name} with attributes [{string.Join(", ", Find.Game.Player.AttributeValues)}]");

		Color = new Color(0.3f, 0.3f, 0.3f, 0.0f);
		Size = new Vector2(300, 300);
		_radarChart.Scale = new Vector2(0.4f, 0.4f);
		_confirmButton.Visible = false;
		_randomButton.Visible = false;
		_nameLabel.Visible = false;
		_label.Visible = false;
		Find.Game.StartGame();
	}
	
	// 设置特定属性的值
	public void SetAttributeValue(int index, float value)
	{
		if (_radarChart != null && index >= 0 && index < 6)
		{
			float[] newValues = (float[])_radarChart.AttributeValues.Clone();
			newValues[index] = Mathf.Clamp(value, 0, 100);
			_radarChart.SetAttributeValues(newValues);
		}
	}
	
	// 获取当前属性值
	public float[] GetCurrentAttributes()
	{
		return _radarChart?.AttributeValues;
	}
	
	// 重置为默认值
	public void ResetToDefault()
	{
		if (_radarChart != null)
		{
			float[] defaultValues = { 80, 60, 90, 70, 50, 85 };
			_radarChart.AnimateToValues(defaultValues, 1.0f);
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

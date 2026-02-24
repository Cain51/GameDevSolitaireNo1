using Godot;
using System;

[Tool]
public partial class RadarChart : Control
{
	const float MaxValue = 4.0f;

	// 六个属性的名称
	[Export]
	public string[] AttributeNames { get; set; } = new string[] { "智", "体", "美", "劳", "德", "运" };

	// 六个属性的值（0-5）
	[Export]
	public float[] AttributeValues { get; set; } = new float[] { 1, 2, 3, 4, 5, 0 };

	// 颜色设置
	[Export]
	public Color GridColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.5f);

	[Export]
	public Color AxisColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 0.8f);

	[Export]
	public Color DataColor { get; set; } = new Color(0.2f, 0.6f, 0.9f, 0.6f);

	[Export]
	public Color BorderColor { get; set; } = new Color(0.1f, 0.4f, 0.8f, 1.0f);

	[Export]
	public Color LabelColor { get; set; } = new Color(0f, 0f, 0f, 1.0f);

	// 内部变量
	private Vector2 _center;
	private float _radius;
	private Vector2[] _points = new Vector2[6];

	// 网格层级数
	private const int GridLevels = 5;

	public override void _Ready()
	{
		InitializeChart();
	}

	private void InitializeChart()
	{
		_center = Size / 2;
		_radius = Mathf.Min(Size.X, Size.Y) * 0.4f;
		
		// 如果在编辑器中，需要手动触发重绘
		if (Engine.IsEditorHint())
		{
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		if (AttributeValues.Length != 6)
			return;

		CalculatePoints();
		DrawBackgroundGrid();
		DrawAxisLines();
		DrawDataArea();
		DrawLabels();
	}

	// 计算六个顶点的位置
	private void CalculatePoints()
	{
		for (int i = 0; i < 6; i++)
		{
			float angle = Mathf.DegToRad(90 - i * 60);  // 从12点钟方向开始
			_points[i] = _center + new Vector2(
				Mathf.Cos(angle),
				-Mathf.Sin(angle)
			) * _radius;
		}
	}

	// 绘制背景网格
	private void DrawBackgroundGrid()
	{
		for (int level = GridLevels; level <= GridLevels; level++)
		{
			Vector2[] levelPoints = new Vector2[6];
			float levelRadius = _radius * ((float)level / GridLevels);

			// 计算当前层的所有点
			for (int i = 0; i < 6; i++)
			{
				float angle = Mathf.DegToRad(90 - i * 60);
				levelPoints[i] = _center + new Vector2(
					Mathf.Cos(angle),
					-Mathf.Sin(angle)
				) * levelRadius;
			}

			// 绘制多边形网格
			for (int i = 0; i < 6; i++)
			{
				int next = (i + 1) % 6;
				DrawLine(levelPoints[i], levelPoints[next], GridColor, 1.0f);
			}

			// 绘制中心到顶点的连接线（仅在最大层绘制）
			if (level == GridLevels)
			{
				for (int i = 0; i < 6; i++)
				{
					DrawLine(_center, levelPoints[i], GridColor, 1.0f);
				}
			}
		}
	}

	// 绘制轴线
	private void DrawAxisLines()
	{
		for (int i = 0; i < 6; i++)
		{
			DrawLine(_center, _points[i], AxisColor, 2.0f);
		}
	}

	// 绘制数据区域
	private void DrawDataArea()
	{
		Vector2[] dataPoints = new Vector2[6];

		// 计算数据点位置
		for (int i = 0; i < 6; i++)
		{
			float valueRatio = AttributeValues[i] / MaxValue;
			float angle = Mathf.DegToRad(90 - i * 60);
			dataPoints[i] = _center + new Vector2(
				Mathf.Cos(angle),
				-Mathf.Sin(angle)
			) * _radius * valueRatio;
		}

		// 绘制填充区域
		if (dataPoints.Length >= 3)
		{
			DrawPolygon(dataPoints, new Color[] { DataColor });
		}

		// 绘制边框
		for (int i = 0; i < 6; i++)
		{
			int next = (i + 1) % 6;
			DrawLine(dataPoints[i], dataPoints[next], BorderColor, 2.0f);
		}

		// 绘制数据点
		// foreach (Vector2 point in dataPoints)
		// {
		// 	DrawCircle(point, 4, BorderColor);
		// }
	}

	// 绘制标签
	private void DrawLabels()
	{
		for (int i = 0; i < 6; i++)
		{
			float angle = Mathf.DegToRad(90 - i * 60);
			Vector2 labelPos = _center + new Vector2(
				Mathf.Cos(angle),
				-Mathf.Sin(angle)
			) * (_radius + 30);

			// 创建或更新标签
			string labelName = "Label" + i;
			Label label = GetNodeOrNull<Label>("Labels/" + labelName);

			if (label == null)
			{
				label = new Label();
				label.Name = labelName;
				GetNode<Control>("Labels").AddChild(label);
			}

			label.Text = AttributeNames[i];
			label.Position = labelPos - new Vector2(label.Size.X / 2, label.Size.Y / 2);
			label.Modulate = LabelColor;
		}
	}

	// 更新属性值
	public void SetAttributeValues(float[] newValues)
	{
		if (newValues.Length == 6)
		{
			AttributeValues = newValues;
			QueueRedraw();
		}
	}

	// 更新属性名称
	public void SetAttributeNames(string[] newNames)
	{
		if (newNames.Length == 6)
		{
			AttributeNames = newNames;
			QueueRedraw();
		}
	}

	// 处理大小变化
	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			_center = Size / 2;
			_radius = Mathf.Min(Size.X, Size.Y) * 0.4f;
			QueueRedraw();
		}
	}

	// 动画过渡到新数值
	public void AnimateToValues(float[] targetValues, float duration = 1.0f)
	{
		if (targetValues.Length != 6)
			return;

		float[] startValues = (float[])AttributeValues.Clone();

		Tween tween = CreateTween();
		tween.TweenMethod(Callable.From<float>(t =>
		{
			for (int i = 0; i < 6; i++)
			{
				AttributeValues[i] = Mathf.Lerp(startValues[i], targetValues[i], t);
			}
			QueueRedraw();
		}), 0.0f, 1.0f, duration);
	}
}

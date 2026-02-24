using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class Game : Node
{
	public Actor Player = new Actor();
    public RadarChart RadarChart;
    private GodotObject config;
    private Node plot;
    public EventCheckMessage EventCheckMessage;
    public HashSet<string> Flags { get; set; } = new HashSet<string>();

    Dictionary<int, Curtain> curtains = new Dictionary<int, Curtain>()
    {
        {100, new C100()},
        {104, new C104()},
        {108, new C108()},
        {112, new C112()},
        {115, new C115()},
        {121, new C121()},
        {124, new C124()},
        {140, new C140()},
        {302, new C302()},
        {10301, new C10301()}
    };
    Curtain currentCurtain = Curtain.NULL;

    public override void _EnterTree()
    {
        Find.Game = this;
    }

    public override void _Ready()
    {
        var configScript = GD.Load<GDScript>("res://2Gove/配置/config.gd");
        config = (GodotObject)configScript.New();
        config.Call("load_config", "res://2Gove/配置/接龙Godot-工作表1.csv");

        plot = GetNode<Node>("UI/Plot");
        plot.Connect("on_choice_selected", Callable.From<int, int>(OnChoiceSelected));

        EventCheckMessage = GetNode<EventCheckMessage>("UI/EventCheck");

        RadarChart = GetNode<RadarChart>("UI/CreateActor/RadarChart");
    }
    public void StartGame()
    {
        if (config == null)
        {
            GD.PrintErr("config 为空");
            return;
        }
        
        ShowCurtain(100);
    }

    public void OnChoiceSelected(int index, int curtainId)
    {
        _ = currentCurtain.OnSelect(index, curtainId);
    }

    public void ShowCurtain(int curtainId)
    {
        var curtain = config.Call("get_curtain", curtainId).AsGodotObject();
        if (curtain == null)
        {
            GD.PrintErr($"获取幕布对象失败，ID: {curtainId}");
            return;
        }
        var oldCurtain = currentCurtain;
        currentCurtain = curtains.TryGetValue(curtainId, out var curtainObj) ? curtainObj : Curtain.NULL;

        var content = curtain.Get("contenxt").AsString();
        content = content.Replace("${Name}", Find.Player.Name);
        curtain.Set("contenxt", content);
        
        GD.Print($"获取到幕布对象 (ID: {curtainId}): {curtain}");
        
        if (plot != null)
        {
            _ = oldCurtain.OnExit();
            _ = currentCurtain.OnEnter();
            plot.Call("load_data", curtain);
            GD.Print($"已将幕布对象 (ID: {curtainId}) 传递给 Plot");
            plot.Set("visible", true);
        }
        else
        {
            GD.PrintErr("Plot 节点为空");
        }
    }
}
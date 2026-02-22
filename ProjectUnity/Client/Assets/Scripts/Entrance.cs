using RG.Zeluda;
using UnityEngine;

public class Entrance : MonoBehaviour
{
	private Character p;
	public bool enableGM = true;
	private bool showGM = false;
	private string gmDayInput = "1";
	private string gmGrassInput = "1";
	private void Awake()
	{
		LoadConfig();
		Reg();
	}
	private void Start()
	{
		ParameterFactory parameterFactory = CBus.Instance.GetFactory(FactoryName.ParameterFactory) as ParameterFactory;

		UIManager ui = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		ui.OpenPanel("LobbyPanel");
	
	
	}
	private void LoadConfig()
	{
		DataCenter.Init();
	}
	public void Reg()
	{
		CBus.Instance.RegFactory(FactoryName.ParameterFactory, new ParameterFactory());
		CBus.Instance.RegFactory(FactoryName.CharacterFactory, new CharacterFactory());
		CBus.Instance.RegFactory(FactoryName.MapFactory,new MapFactory()) ;
		CBus.Instance.RegFactory(FactoryName.AssetFactory, new AssetFactory());
		CBus.Instance.RegFactory(FactoryName.WorkFactory, new WorkFactory());
		CBus.Instance.RegFactory(FactoryName.DailyFactory, new DailyFactory());


		CBus.Instance.RegManager(ManagerName.ResManager, new ResManager());
		CBus.Instance.RegManager(ManagerName.UIManager, new UIManager());
		CBus.Instance.RegManager(ManagerName.GameManager, new GameManager());
		CBus.Instance.RegManager(ManagerName.DialogManager, new DialogManager());
		CBus.Instance.RegManager(ManagerName.SceneLoadManager, new SceneLoadManager());
		CBus.Instance.RegManager(ManagerName.GroundManager, new GroundManager());
		CBus.Instance.RegManager(ManagerName.AssetManager, new AssetManager());
		CBus.Instance.RegManager(ManagerName.LevelManager, new LevelManager());
		CBus.Instance.InitParams();
	}
	private void Update()
	{
		if (OEF.Instance.isPause == true) { return; }
		OEF.Instance.Update();
		if (enableGM && Input.GetKeyDown(KeyCode.F10))
		{
			showGM = !showGM;
		}
		if (enableGM && Input.GetKeyDown(KeyCode.F9))
		{
			GM_SkipDialog();
			LobbyPanel lobbyPanel = FindObjectOfType<LobbyPanel>();
			if (lobbyPanel != null)
			{
				lobbyPanel.SkipStartVideo();
			}
		}
	}
	private void OnGUI()
	{
		if (enableGM == false || showGM == false) { return; }
		float baseWidth = 260f;
		float baseHeight = 260f;
		float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1080f, Screen.height / 720f), 0.85f, 1.6f);
		float width = baseWidth * scale;
		float height = baseHeight * scale;
		float padding = 10f * scale;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
		GUILayout.BeginArea(new Rect(padding / scale, padding / scale, width / scale, height / scale), "GM", GUI.skin.window);
		if (GUILayout.Button("跳过剧情")) { GM_SkipDialog(); }
		GUILayout.Space(8);
		GUILayout.Label("跳到某一天");
		gmDayInput = GUILayout.TextField(gmDayInput, 4, GUILayout.MinHeight(28));
		if (GUILayout.Button("确认跳转")) { GM_JumpToDay(); }
		GUILayout.Space(8);
		GUILayout.Label("获得草料");
		gmGrassInput = GUILayout.TextField(gmGrassInput, 4, GUILayout.MinHeight(28));
		if (GUILayout.Button("添加草料")) { GM_AddGrass(); }
		GUILayout.Space(8);
		GUILayout.Label("快捷键：F10 开关GM，F9 跳过剧情");
		GUILayout.EndArea();
		GUI.matrix = Matrix4x4.identity;
	}
	private void GM_SkipDialog()
	{
		DialogPanel dialogPanel = FindObjectOfType<DialogPanel>();
		if (dialogPanel != null)
		{
			dialogPanel.SkipAll();
		}
	}
	private void GM_JumpToDay()
	{
		int targetDay;
		if (int.TryParse(gmDayInput, out targetDay) == false) { return; }
		targetDay = Mathf.Max(1, targetDay);
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm == null) { return; }
		gm.day = targetDay;
		gm.time = gm.start_time;
		UIManager ui = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		MainPanel main = ui.GetPanel("MainPanel") as MainPanel;
		if (main != null)
		{
			main.SetDay(gm.day);
			main.SetTime(gm.time);
			if (gm.day >= 2)
			{
				main.ismatchlocked = true;
			}
		}
	}
	private void GM_AddGrass()
	{
		int amount;
		if (int.TryParse(gmGrassInput, out amount) == false) { return; }
		if (amount == 0) { return; }
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am == null) { return; }
		am.Add(1100003, amount);
	}
}

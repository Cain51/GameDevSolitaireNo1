using RG.Zeluda;
using UnityEngine;

public class Entrance : MonoBehaviour
{
	private Character p;
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
	}
}

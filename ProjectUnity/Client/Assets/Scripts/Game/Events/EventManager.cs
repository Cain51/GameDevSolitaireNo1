using RG.Zeluda;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EventManager : ManagerBase
{
	public EventsFactory eventsFactory;
	public Dictionary<int, int> chrEventDic;
	public override void InitParams()
	{
		base.InitParams();
		chrEventDic = new Dictionary<int, int>();
		eventsFactory = CBus.Instance.GetFactory(FactoryName.EventsFactory) as EventsFactory;
	}
	//public EventsCA GetNextEvents(CharacterCA ca) {
	//	if (chrEventDic.ContainsKey(ca.id) == false) { 
	//	return eventsFactory.GetCA(ca.cardid);
	//	}
	//}
	public void TriggerEvent(int eid)
	{
		EventsCA eca = eventsFactory.GetCA(eid) as EventsCA;
		if (eca.scene != null)
		{
			SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
			slm.Load(eca.scene);
			UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			DialogPanel dp = uiManager.OpenFloat("DialogPanel") as DialogPanel;
			dp.StartDialog(eca.dialog);

			GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
			gm.CostTime(eca.cost);
			gm.SetNextAwake(eca.awake);
		}
	}
}

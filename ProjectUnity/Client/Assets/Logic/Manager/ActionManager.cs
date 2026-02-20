using RG.Zeluda;
using System.Collections.Generic;

public class ActionManager : ManagerBase
{
	public List<ActionCA> actions = new List<ActionCA>();
	public override void InitParams()
	{
		base.InitParams();
		Refresh();
	}
	public void Refresh()
	{
		actions.Clear();
		if (CBus.Instance.HasFactory("ActionFactory") == false) { return; }
		ActionFactory factory = CBus.Instance.GetFactory("ActionFactory") as ActionFactory;
		if (factory == null) { return; }
		CABase[] cas = factory.GetAllCA();
		for (int i = 0; i < cas.Length; i++)
		{
			ActionCA ca = cas[i] as ActionCA;
			if (ca != null)
			{
				actions.Add(ca);
			}
		}
	}
	public ActionCA GetAction(int id)
	{
		for (int i = 0; i < actions.Count; i++)
		{
			if (actions[i].id == id) { return actions[i]; }
		}
		if (CBus.Instance.HasFactory("ActionFactory") == false) { return null; }
		ActionFactory factory = CBus.Instance.GetFactory("ActionFactory") as ActionFactory;
		return factory != null ? factory.GetCA(id) as ActionCA : null;
	}
	public List<ActionCA> GetAllActions()
	{
		if (actions.Count == 0)
		{
			Refresh();
		}
		return actions;
	}
}

using RG.Basic.DataType;
using RG.Zeluda;
using System.Collections.Generic;

public class GroundManager : ManagerBase
{
	public List<Ground> grounds;
	public override void InitParams()
	{
		base.InitParams();
		grounds = new List<Ground>();
	}
	public void BuildGround(int cnt)
	{
		UIManager uimanager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		GroundPanel panel = uimanager.OpenPanel("GroundPanel") as GroundPanel;
		for (int i = 0; i < cnt; i++)
		{
			Ground g = new Ground();
			panel.CreateGround(g);
			grounds.Add(g);
		}
	}
	public int HoeGround(int cnt)
	{
		int hoeCount = 0;

		for (int i = 0; i < grounds.Count; i++)
		{
			Ground g = grounds[i];
			if (g.gtype == GroundType.uncultivated)
			{
				g.gtype = GroundType.empty;
				g.view.Refresh(g);
				hoeCount++;
			}

			if (hoeCount >= cnt)
				break;
		}

		return hoeCount;
	}
	public int WaterGround(int cnt)
	{
		int wateredCount = 0;

		for (int i = 0; i < grounds.Count; i++)
		{
			Ground g = grounds[i];
			if (g.gtype == GroundType.empty)
			{
				g.gtype = GroundType.wet;
				g.view.Refresh(g);
				wateredCount++;
			}

			if (wateredCount >= cnt)
				break;
		}

		return wateredCount;
	}
	public int Plant(int cnt, int id)
	{
		int planted = 0;

		for (int i = 0; i < grounds.Count; i++)
		{
			Ground g = grounds[i];
			if ((g.gtype == GroundType.empty || g.gtype == GroundType.wet) && g.id == 0)
			{
				g.id = id;
				g.process = 3;
				g.view.Refresh(g);
				planted++;
			}

			if (planted >= cnt)
				break;
		}

		return planted;
	}
	public void DayEnd()
	{
		for (int i = 0; i < grounds.Count; i++)
		{
			Ground g = grounds[i];
			if (g.gtype == GroundType.wet )
			{
				g.gtype = GroundType.empty;
				if (g.id != 0) {
					g.process--;
					if (g.process == 0)
					{
						g.id = 0;
						AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
						am.Add(1100003, 1);
					}
				}
				
			}
		
			g.view.Refresh(g);

		}
	}
}

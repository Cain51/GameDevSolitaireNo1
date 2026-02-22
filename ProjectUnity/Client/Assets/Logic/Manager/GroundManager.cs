using RG.Basic.DataType;
using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundManager : ManagerBase
{
	public List<Ground> grounds;
	private List<ChickenState> chickens;
	private Sprite chickenSprite;
	private Transform chickenRoot;
	private class ChickenState
	{
		public bool isFull;
		public GroundItem view;
	}
	public override void InitParams()
	{
		base.InitParams();
		grounds = new List<Ground>();
		chickens = new List<ChickenState>
		{
			new ChickenState { isFull = false },
			new ChickenState { isFull = false }
		};
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
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		MapCA mapCa = slm != null ? slm.mapCA : null;
		if (mapCa == null)
		{
			GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
			MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
			if (gm != null && mf != null)
			{
				mapCa = mf.GetCA(gm.prisonRooms[gm.roomIdx]) as MapCA;
			}
		}
		if (mapCa != null && mapCa.name == "牧场")
		{
			uimanager.HidePanel("GroundPanel");
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
						am.Add(1100002, 2);
					}
				}
				
			}
		
			g.view.Refresh(g);

		}
	}
	public void UpdateChickenSprites()
	{
		if (IsChickenMap() == false)
		{
			SetGroundVisible(true);
			if (chickenRoot != null) { chickenRoot.gameObject.SetActive(false); }
			return;
		}
		SetGroundVisible(false);
		if (chickens == null || chickens.Count == 0)
		{
			if (chickenRoot != null) { chickenRoot.gameObject.SetActive(false); }
			return;
		}
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		// 使用 OpenPanelIgnoreToggle 避免触发 MapPanel 的 Hide，从而避免递归 StackOverflow
		GroundPanel panel = uiManager != null ? uiManager.OpenPanelIgnoreToggle("GroundPanel") as GroundPanel : null;
		if (panel == null || panel.pfb_item == null || panel.tran_content == null) { return; }
		if (chickenSprite == null)
		{
			chickenSprite = Resources.Load<Sprite>("UI/ground/鸡");
		}
		if (chickenSprite == null) { return; }
		EnsureChickenRoot(panel);
		if (chickenRoot == null) { return; }
		chickenRoot.gameObject.SetActive(true);
		for (int i = 0; i < chickens.Count; i++)
		{
			ChickenState chicken = chickens[i];
			if (chicken.view == null)
			{
				GameObject go = GameObject.Instantiate(panel.pfb_item);
				go.transform.SetParent(chickenRoot, false);
				chicken.view = go.GetComponent<GroundItem>();
			}
			if (chicken.view != null)
			{
				chicken.view.gameObject.SetActive(true);
				chicken.view.RefreshChicken(chickenSprite, chicken.isFull);
			}
		}
	}
	public bool FeedHungryChicken()
	{
		if (chickens == null) { return false; }
		for (int i = 0; i < chickens.Count; i++)
		{
			if (chickens[i].isFull == false)
			{
				chickens[i].isFull = true;
				UpdateChickenSprites();
				return true;
			}
		}
		return false;
	}
	public bool KillChicken()
	{
		if (chickens == null || chickens.Count == 0) { return false; }
		ChickenState last = chickens[chickens.Count - 1];
		if (last.view != null)
		{
			GameObject.Destroy(last.view.gameObject);
		}
		chickens.RemoveAt(chickens.Count - 1);
		UpdateChickenSprites();
		return true;
	}
	public int GetFullChickenCount()
	{
		if (chickens == null) { return 0; }
		int count = 0;
		for (int i = 0; i < chickens.Count; i++)
		{
			if (chickens[i].isFull) { count++; }
		}
		return count;
	}
	public void SetAllChickenHungry()
	{
		if (chickens == null) { return; }
		for (int i = 0; i < chickens.Count; i++)
		{
			chickens[i].isFull = false;
		}
		UpdateChickenSprites();
	}
	public void AddChickens(int count)
	{
		if (count <= 0) { return; }
		if (chickens == null) { chickens = new List<ChickenState>(); }
		for (int i = 0; i < count; i++)
		{
			chickens.Add(new ChickenState { isFull = false });
		}
		UpdateChickenSprites();
	}
	private bool IsChickenMap()
	{
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		MapCA mapCa = slm != null ? slm.mapCA : null;
		if (mapCa == null)
		{
			GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
			MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
			if (gm != null && mf != null)
			{
				mapCa = mf.GetCA(gm.prisonRooms[gm.roomIdx]) as MapCA;
			}
		}
		return mapCa != null && mapCa.name == "养鸡场";
	}
	private void EnsureChickenRoot(GroundPanel panel)
	{
		if (chickenRoot != null) { return; }
		Transform existing = panel.tran_content.Find("ChickenRoot");
		if (existing != null)
		{
			chickenRoot = existing;
			return;
		}
		GameObject root = new GameObject("ChickenRoot", typeof(RectTransform));
		chickenRoot = root.transform;
		chickenRoot.SetParent(panel.tran_content, false);
		RectTransform rect = root.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		GridLayoutGroup grid = root.AddComponent<GridLayoutGroup>();
		grid.cellSize = new Vector2(100f, 100f);
		grid.spacing = new Vector2(10f, 10f);
		grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		grid.constraintCount = 5;
		grid.childAlignment = TextAnchor.UpperLeft;
	}
	private void SetGroundVisible(bool visible)
	{
		if (grounds == null) { return; }
		for (int i = 0; i < grounds.Count; i++)
		{
			Ground g = grounds[i];
			if (g != null && g.view != null)
			{
				g.view.gameObject.SetActive(visible);
			}
		}
	}
}

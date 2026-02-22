using RG.Basic;
using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform trans_content;
	public MapCA[] cas;
	public int sceneIndex;
	public bool isOpen = false;
	public void InitMap()
	{
		if (isOpen == true)
		{
			isOpen = false;

			Close();
			return;
		}
		isOpen = true;

		foreach (Transform c in trans_content)
		{
			Destroy(c.gameObject);
		}
		MapFactory mapFactory = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
		CABase[] ca = mapFactory.GetAllCA();

		GameManager gameManager = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		int currentMapId = slm.mapCA != null ? slm.mapCA.id : gameManager.prisonRooms[gameManager.roomIdx];
		List<MapCA> availableMaps = ca
			.Select(caItem => caItem as MapCA)
			.Where(map => map != null)
			.Where(map => map.id != currentMapId)
			.Where(map => map.name == "海边" || (map.unlockday <= gameManager.day && map.opentime != null && map.opentime.Contains(gameManager.GetCurTime())))
			.OrderBy(map => map.id)
			.ToList();
		cas = availableMaps.ToArray();
		for (int i = 0; i < availableMaps.Count; i++)
		{
			GameObject obj = GameObject.Instantiate(pfb_item, trans_content);
			obj.SetActive(true);
			MapItem item = obj.GetComponent<MapItem>();
			item.Init(availableMaps[i]);
		}
		if (trans_content.childCount == 0)
		{
			TipManager.Tip("此时没有可移动的地点！");
			Close();
		}

	}
	public override void Open()
	{
		gameObject.SetActive(true);
		base.Open();
	}
	public override void Close()
	{
		isOpen = false;
		AudioManager.Inst.Play("BGM/点击按钮");
		gameObject.SetActive(false);
		base.Close();
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
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
		if (uiManager != null)
		{
			bool showGroundPanel = false;
			if (mapCa != null)
			{
				string name = mapCa.name != null ? mapCa.name.Trim() : string.Empty;
				showGroundPanel = name == "农场" || name == "你的农场" || name == "养鸡场";
			}
			if (showGroundPanel)
			{
				uiManager.OpenPanelIgnoreToggle("GroundPanel");
			}
			else
			{
				uiManager.HidePanel("GroundPanel");
			}
		}
		MainPanel mainPanel = uiManager != null ? uiManager.GetPanel("MainPanel") as MainPanel : null;
		if (mainPanel != null)
		{
			mainPanel.RefreshActionButtons();
		}
	}


}

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
			.Where(map => 
			{
                string name = map.name != null ? map.name.Trim() : "";

				// 第一天没有地图开放（按钮也是灰的）
				if (gameManager.day == 1) return false;

				// 第二天只能去养鸡场和牧场 (根据用户要求保持该逻辑)
				if (gameManager.day == 2)
				{
					if (name == "养鸡场" || name == "你的牧场" || name == "牧场" || map.id == 1800002 || map.id == 1800004)
					{
						return map.opentime == null || map.opentime.Length == 0 || map.opentime.Contains(gameManager.time);
					}
					return false;
				}

                // 第三天特殊逻辑
                if (gameManager.day == 3)
                {
                    // 1. 继承第二天的地点：养鸡场、牧场
                    if (name == "养鸡场" || name == "你的牧场" || name == "牧场" || map.id == 1800002 || map.id == 1800004)
                    {
                        return true;
                    }
                    
                    // 2. 第三天默认新增地点：海边
                    if (name == "海边" || map.id == 1800009)
                    {
                        return true;
                    }
                    
                    // 3. 满足洗澡剧情后解锁：医院
                    if (gameManager.isBathStoryFinished && (name == "医院" || map.id == 1800005))
                    {
                        return true;
                    }
                    
                    // 其他地点暂不开放
                    return false;
                }

                // 第四天特殊逻辑
                if (gameManager.day == 4)
                {
                    // 第四天开放：杂货铺、海边、牧场、你的农场、养鸡场、医院
                    if (name == "杂货铺" || name == "海边" || name == "牧场" || name == "你的农场" || name == "养鸡场" || name == "医院" ||
                        map.id == 1800003 || map.id == 1800009 || map.id == 1800004 || map.id == 1800001 || map.id == 1800002 || map.id == 1800005)
                    {
                        return true;
                    }
                    return false;
                }
                // ID 145 特殊解锁
                if (gameManager != null && gameManager.isSpecialUnlockTriggered)
                {
                    return true;
                }

                // 第五天及以后（通用逻辑）
                if (gameManager.day >= 5)
                {
                    // 第五天解锁：农场、牧场、养鸡场、杂货铺、医院、矿洞、海边、图书馆、教堂
                    // 这里直接返回 true 开放所有配置的地点
                    return true;
                }

				// 兜底逻辑
				return map.unlockday <= gameManager.day && (map.opentime == null || map.opentime.Length == 0 || map.opentime.Contains(gameManager.time));
			})
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

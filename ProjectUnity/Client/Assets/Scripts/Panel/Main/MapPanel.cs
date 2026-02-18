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
		cas = new MapCA[ca.Length];

		GameManager gameManager = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;


		MapCA mca = mapFactory.GetCA(gameManager.prisonRooms[gameManager.roomIdx]) as MapCA;
		if (slm.curScene.name != null)
		{
			if (slm.curScene.name != mca.scene)
			{
				List<int> open = new List<int>(mca.opentime);
				if (open.Contains(gameManager.GetCurTime()) == true)
				{
					GameObject obj = GameObject.Instantiate(pfb_item, trans_content);
					obj.SetActive(true);
					MapItem item = obj.GetComponent<MapItem>();
					item.Init(mca);
				}
			}
		}

		int idx = 0;
		foreach (var caItem in ca)
		{
			MapCA map = caItem as MapCA;
			if (slm.curScene.name != null)
			{
				if (slm.curScene.name == map.scene) { continue; }
			}
			if (map.unlockday > gameManager.day) { continue; }

			List<int> open = new List<int>(map.opentime);
			if (open.Contains(gameManager.GetCurTime()) == false) { continue; }
			cas[idx] = map;
			GameObject obj = GameObject.Instantiate(pfb_item, trans_content);
			obj.SetActive(true);
			MapItem item = obj.GetComponent<MapItem>();
			item.Init(map);
			idx++;
		}
		if (trans_content.childCount == 0)
		{
			TipManager.Tip("暂时没有可移动的地点！");
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
		AudioManager.Inst.Play("BGM/点击按钮");
		gameObject.SetActive(false);
		base.Close();
	}


}

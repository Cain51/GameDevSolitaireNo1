using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform tran_content;

	public Dictionary<int, FoodItem> itemDic;
	private readonly HashSet<int> extraIds = new HashSet<int> { 1100002, 1100007, 1100008, 1200001 };
	public override void Init()
	{
		itemDic = new Dictionary<int, FoodItem>();
	}
	public void Refresh()
	{
		AssetManager assetManager = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
		HashSet<int> targetIds = new HashSet<int>(extraIds);
		foreach (var item in assetManager.assetDic)
		{
			if (item.Key == 0) { continue; }
			AssetCA ca = assetFactory.GetCA(item.Key) as AssetCA;
			if (ca == null) { continue; }
			if (ca.sptype == AssetType.Food)
			{
				targetIds.Add(ca.id);
			}
		}
		foreach (int id in targetIds)
		{
			AssetCA ca = assetFactory.GetCA(id) as AssetCA;
			if (ca == null) { continue; }
			int count = assetManager.assetDic.ContainsKey(id) ? assetManager.assetDic[id] : 0;
			if (itemDic.ContainsKey(ca.id) == false)
			{
				CreateItem(ca, count);
			}
			else
			{
				itemDic[ca.id].Refresh(count);
			}
		}
		if (itemDic.Count > 0)
		{
			List<int> keys = new List<int>(itemDic.Keys);
			for (int i = 0; i < keys.Count; i++)
			{
				int id = keys[i];
				if (targetIds.Contains(id) == false)
				{
					itemDic[id].Refresh(0);
				}
			}
		}
	}
	public void CreateItem(AssetCA ca, int num)
	{
		GameObject obj = GameObject.Instantiate(pfb_item);
		obj.transform.SetParent(tran_content);
		obj.SetActive(true);
		FoodItem item = obj.GetComponent<FoodItem>();
		item.Init(ca, num);
		itemDic.Add(ca.id, item);

	}
}

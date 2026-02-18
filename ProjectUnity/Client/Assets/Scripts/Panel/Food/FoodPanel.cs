using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform tran_content;

	public Dictionary<int, FoodItem> itemDic;
	public override void Init()
	{
		itemDic = new Dictionary<int, FoodItem>();
	}
	public void Refresh()
	{
		AssetManager assetManager = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
	
		foreach (var item in assetManager.assetDic)
		{
			if (item.Key == 0) { continue; }
			AssetCA ca = assetFactory.GetCA(item.Key) as AssetCA;
			if (ca.sptype !=  AssetType.Food) { continue; }
			if (itemDic.ContainsKey(ca.id) == false)
			{
				CreateItem(ca, item.Value);
			}
			else
			{
				itemDic[ca.id].Refresh(item.Value);
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

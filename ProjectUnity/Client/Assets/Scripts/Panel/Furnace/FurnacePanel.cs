using DG.Tweening;
using RG.Basic;
using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;

public class FurnacePanel : PanelBase
{
	public GameObject pfb_item;
	public List<FillItem> items;
	public Transform tran_content;
	public void NextDay(DailyFurnaceCA ca)
	{
		
		int len = ca.costids.Length + ca.costtypes.Length;
		int totalNeed = len - items.Count;
		for (int i = 0; i < totalNeed; i++)
		{
			GameObject item = GameObject.Instantiate(pfb_item);
			item.transform.SetParent(tran_content);
			item.transform.localPosition = Vector3.zero;
			item.transform.localEulerAngles = Vector3.zero;
			items.Add(item.gameObject.GetComponent<FillItem>());
		}
		int idsLen = ca.costids.Length;
		int typesLen = ca.costtypes.Length;

		for (int i = 0; i < idsLen; i++)
		{
			FillItem item = items[i];
			item.gameObject.SetActive(true);
			Pair<int, int> p = ca.costids[i];
			item.Init(p.k, p.v);
		}
		int index = 0;
		for (int i = idsLen; i < typesLen; i++)
		{
			FillItem item = items[i];
			item.gameObject.SetActive(true);
			Pair<int, int> p = ca.costtypes[index];
			item.Init((CardType)p.k, p.v);
			index++;
		}
		for (int i = len; i < items.Count; i++)
		{
			items[i].gameObject.SetActive(false);
		}

	}
	public void InitData()
	{

	}
	public bool CheckCanFire()
	{
		foreach (FillItem item in items)
		{
			if (item.CheckAble() == false) { return false; }
		}
		return true;
	}
	public void FireCard()
	{
		if (CheckCanFire() == false) { return; }
		foreach (FillItem item in items)
		{
			item.DestroyCard();
		}
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
	}
	public override void Open()
	{
		base.Open();
		gameObject.SetActive(true);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.transform.DOKill();
		gameObject.transform.DOScale(Vector3.one, 0.2F);
	}
	public override void Close()
	{
		base.Close();
		foreach (FillItem item in items)
		{
			item.ReleaseCard();
		}
		gameObject.transform.DOKill();
		gameObject.transform.DOScale(Vector3.zero, 0.2F).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}
}

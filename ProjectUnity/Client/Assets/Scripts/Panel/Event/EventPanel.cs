using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPanel : PanelBase
{
	public GameObject pfb_sell;
	public GameObject pfb_buy;

	public Transform tran_sellGrid;
	public Transform tran_buyGrid;

	public List<EventItem> sellItem;
	public List<EventItem> buyItem;

	public List<Card> bagCard;
	public override void Init()
	{
		base.Init();
		sellItem = new List<EventItem>();
		buyItem = new List<EventItem>();
	}
	public override void SetData(object o)
	{
		Events e = (Events)o;
		//////////出售//////////////
		int cnt = sellItem.Count;
		int sub = e.sellCards.Count - cnt;
		for (int i = 0; i < sub; i++)
		{
			GameObject obj = GameObject.Instantiate(pfb_sell);
			EventItem item = obj.GetComponent<EventItem>();
			sellItem.Add(item);
		}
		for (int i = 0; i < e.sellCards.Count; i++)
		{
			EventItem item = sellItem[i];
			item.gameObject.SetActive(true);
			item.SetData(e.sellCards[i]);
		}
		for (int i = e.sellCards.Count; i < cnt; i++)
		{
			EventItem item = sellItem[i];
			item.gameObject.SetActive(false);
		}
		//////////获得//////////////
		 cnt = buyItem.Count;
		 sub = e.buyCards.Count - cnt;
		for (int i = 0; i < sub; i++)
		{
			GameObject obj = GameObject.Instantiate(pfb_buy);
			EventItem item = obj.GetComponent<EventItem>();
			buyItem.Add(item);
		}
		for (int i = 0; i < e.buyCards.Count; i++)
		{
			EventItem item = buyItem[i];
			item.gameObject.SetActive(true);
			item.SetData(e.buyCards[i]);
		}
		for (int i = e.buyCards.Count; i < cnt; i++)
		{
			EventItem item = buyItem[i];
			item.gameObject.SetActive(false);
		}
		Open();
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
		gameObject.transform.DOKill();
		gameObject.transform.DOScale(Vector3.zero, 0.2F).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}

}

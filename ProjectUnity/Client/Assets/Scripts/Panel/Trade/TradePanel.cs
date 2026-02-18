using DG.Tweening;
using RG.Basic;
using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;

public class TradePanel : PanelBase
{
	public List<Card> bagCard;
	public List<FillItem> guestItem;
	public List<FillItem> selfItem;
	public GameObject pfb_item;

	public Transform tran_guest;
	public Transform tran_self;
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
		gameObject.transform.DOScale(Vector3.zero, 0.2F).OnComplete(() =>
		{
			gameObject.SetActive(false);
		});
	}
	public void SetCharacter(CharacterCA e)
	{
		
		ResetItems();
	}
	public void SetEvent(EventsCA ca)
	{
		
	}
	public void ResetItems()
	{
		if (guestItem == null)
		{
			guestItem = new List<FillItem>();
			for (int i = 0; i < 8; i++)
			{
				GameObject item = GameObject.Instantiate(pfb_item);
				item.transform.SetParent(tran_guest);
				item.transform.localPosition = Vector3.zero;
				item.transform.localEulerAngles = Vector3.zero;
				item.transform.localScale = Vector3.one;
				guestItem.Add(item.gameObject.GetComponent<FillItem>());
			}
		}
		else
		{
			foreach (var item in guestItem)
			{
				item.ReleaseCard();
			}
		}
		//if (selfItem == null)
		//{
		//	selfItem = new List<FillItem>();
		//	for (int i = 0; i < 8; i++)
		//	{
		//		GameObject item = GameObject.Instantiate(pfb_item);
		//		item.transform.SetParent(tran_self);
		//		item.transform.localPosition = Vector3.zero;
		//		item.transform.localEulerAngles = Vector3.zero;
		//		selfItem.Add(item.gameObject.GetComponent<FillItem>());
		//	}
		//}
		//else
		//{
		//	foreach (var item in selfItem)
		//	{
		//		item.ReleaseCard();
		//	}
		//}
	}
	public void Trade()
	{
		if (CheckTrade() == false) { return; }
		foreach (var item in guestItem)
		{
			item.card.SetOwner(true);
			item.ReleaseCard();
		}
		foreach (var item in selfItem)
		{
			item.card.SetOwner(false);
			item.DestroyCard();
		}
		Close();
	}
	public bool CheckTrade()
	{
		if (guestItem == null || selfItem == null) { return false; }
		int guestCost = 0;
		foreach (var item in guestItem)
		{
			AssetCA ca = item.GetCardAssetCA();
			guestCost += ca.cost;
		}
		int selfCost = 0;
		foreach (var item in selfItem)
		{
			AssetCA ca = item.GetCardAssetCA();
			selfCost += ca.cost;
		}
		if (selfCost < guestCost)
		{
			//µ¯ÌáÊ¾
			return false;
		}
		return true;
	}
}

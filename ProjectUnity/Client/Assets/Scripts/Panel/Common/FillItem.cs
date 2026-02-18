using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FillItem : MonoBehaviour
{
	public int cardid;
	public Card card;
	public Text lbl_type;

	public CardType typeLimit = CardType.None;
	public int idLimit = 0;
	public int numLimit = 0;
	public void Init(CardType cardType, int num)
	{
		typeLimit = cardType;
		idLimit = 0;
		numLimit = num;
		lbl_type.gameObject.SetActive(true);
		lbl_type.text = cardType.ToString();
	}
	public void Init(int id, int num)
	{
		typeLimit = CardType.None;
		idLimit = id;
		numLimit = num;
		lbl_type.gameObject.SetActive(false);
	}
	public bool CheckAble()
	{
		if (card == null) { return false; }

		return true;
	}
	public void SetCard(Card c)
	{
		if (typeLimit != CardType.None && typeLimit != c.type) { return; }
		if (idLimit != 0 && idLimit != c.id) { return; }
		if (card != null) { return; }
		cardid = c.id;
		card = c;
		card.transform.parent = transform;
		card.transform.localPosition = Vector3.zero;
	}
	public void DestroyCard() {
		if (card == null) { return; }
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		CardPanel cardPanel = uiManager.GetPanel("CardPanel") as CardPanel;
		card.transform.parent = cardPanel.transform;
		cardPanel.RecycleCard(card);
		card = null;
		cardid = 0;
	}
	public void ReleaseCard()
	{
		if (card == null) { return; }
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		CardPanel cardPanel = uiManager.GetPanel("CardPanel") as CardPanel;
		card.transform.parent = cardPanel.transform;
		card = null;
		cardid = 0;
	}
	public AssetCA GetCardAssetCA() {
		if (card == null) { return null; }
		if (card.srcid.ToString().StartsWith("100") == false) { return null; }
		AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
		return assetFactory.GetCA(card.srcid) as AssetCA;
	}
}

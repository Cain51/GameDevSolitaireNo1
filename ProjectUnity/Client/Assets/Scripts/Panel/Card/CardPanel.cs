using RG.Basic;
using RG.Basic.DataType;
using System.Collections.Generic;
using UnityEngine;

public class CardPanel : PanelBase
{
	public GameObject pfb_card;
	public GameObject pfb_furniture;

	public Dictionary<CardType, Queue<Card>> recycleCards = new Dictionary<CardType, Queue<Card>>();
	public Queue<Card> recycleFurnitureCards = new Queue<Card>();
	public Card InitCard(int srcid,int id, CardType c)
	{
		CardFactory cardFactory = CBus.Instance.GetFactory(FactoryName.CardFactory) as CardFactory;
		CardCA ca = cardFactory.GetCA(id) as CardCA;
		Card card = null;
		Queue<Card> queue = null;
		if (recycleCards.ContainsKey(c) == false)
		{
			queue = new Queue<Card>();
			recycleCards.Add(c, queue);
		}
		else {
			queue = recycleCards[c];
		}

		if (queue.Count == 0)
		{
			card = CreateCard(c);
		}
		else
		{
			card = queue.Dequeue();
		}
		card.gameObject.name = c.ToString() + id;
		card.gameObject.SetActive(true);
		ResManager resManager = CBus.Instance.GetManager(ManagerName.ResManager) as ResManager;
		Sprite s = resManager.GetRes<Sprite>(ca.resPath);
		card.Init(srcid,id, c, s, ca.name);
		return card;
	}
	public Card CreateCard(CardType c)
	{
		GameObject obj = GameObject.Instantiate(c == CardType.Furniture? pfb_furniture:pfb_card);
		obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		return obj.GetComponent<Card>();
	}
	public void RecycleCard(Card c)
	{
		c.Recycle();
		recycleCards[c.type].Enqueue(c);
	}
}

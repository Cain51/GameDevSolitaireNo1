using RG.Basic;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
	None,
	Asset,
	Character,
	Furniture
}
public class Card : MonoBehaviour
{
	public int srcid;
	public int id;
	public CardType type = CardType.None;
	public Image img_bg;
	public Image img_icon;
	public Text lbl_name;
	public Text lbl_num;
	public Action OnOpenEvent;
	public Action OnMessageShow;
	public bool isOwner = true;

	
	public void Init(int sid, int id, CardType t, Sprite s, string name)
	{
		this.id = id;
		srcid = sid;
		img_icon.sprite = s;
		lbl_name.text = name;
		type = t;
	}
	public void Pop()
	{

	}
	public void Clone()
	{

	}
	public void SetOwner(bool isPlayer) {
		isOwner = isPlayer;
		if (isPlayer == true)
		{
			img_bg.color = Color.white;
		}
		else {
			img_bg.color = Color.black;
		}
	}
	public void MessageShow()
	{
		if (OnMessageShow == null) { return; }
		OnMessageShow();
	}
	public void OpenEvent()
	{
		if (OnOpenEvent == null) { return; }
		OnOpenEvent();
	}
	public void Recycle()
	{
		img_icon.sprite = null;
		gameObject.SetActive(false);
	}
}

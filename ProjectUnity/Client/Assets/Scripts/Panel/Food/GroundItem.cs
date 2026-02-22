using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundItem : MonoBehaviour
{
	public Image img_icon;
	public Image img_seed;
	public void Refresh(Ground g)
	{
		if (g.gtype == GroundType.uncultivated)
		{
			img_icon.color = Color.gray;
		}
		else if (g.gtype == GroundType.empty)
		{
			img_icon.color = new Color(187f / 255, 104f / 255, 0);
		}
		else if (g.gtype == GroundType.wet)
		{
			img_icon.color = new Color(90f / 255, 50f / 255, 0);
		}
		g.view.img_seed.gameObject.SetActive(g.id != 0);
	}
	public void RefreshChicken(Sprite sprite, bool isFull)
	{
		if (img_icon != null)
		{
			img_icon.sprite = sprite;
			img_icon.color = isFull ? new Color(1f, 0.75f, 0.75f, 1f) : Color.white;
		}
		if (img_seed != null)
		{
			img_seed.gameObject.SetActive(false);
		}
	}
}

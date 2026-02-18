using DG.Tweening;
using RG.Zeluda;
using UnityEngine;
using UnityEngine.UI;

public class FoodItem : MonoBehaviour
{
	public Transform tran_content;
	public Image img_icon;
	public Text lbl_num;
	public int number;
	public void Init(AssetCA ca, int num)
	{
		ResManager res = CBus.Instance.GetManager(ManagerName.ResManager) as ResManager;
		img_icon.sprite = res.GetRes<Sprite>(ca.respath);
		Refresh(num);
	}
	public void Refresh(int num)
	{
		if (number == num) { return; }
		if (num > number) { Shake(); }
		number = num;
		lbl_num.text = "x" + num;
		if (number <= 0)
		{
			Hide();
		}
		else {
			Show();
		}
	}
	public void Show()
	{
		if (gameObject.activeSelf == true) { return; }
		gameObject.SetActive(true);
	}
	public void Hide()
	{
		if (gameObject.activeSelf == false) { return; }
		gameObject.SetActive(false);
	}
	public void Shake()
	{
		tran_content.DOKill(true);
		tran_content.DOPunchScale(Vector3.one * 1.05f, 0.2f, 1, 1);
	}
}

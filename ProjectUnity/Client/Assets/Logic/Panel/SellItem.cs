using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SellItem : MonoBehaviour
{
    public Text lbl_name;
    public Text lbl_price;
    public ItemCA item;
    public void Init(ItemCA data)
    {
        item = data;
        lbl_name.text = item.Name;
        lbl_price.text = "$" + item.Price;
    }
    public void OnClick()
    {
        if (item == null) { return; }
        if (item.Price == 0) { return; }
		GameManager gameManager = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
    }
}

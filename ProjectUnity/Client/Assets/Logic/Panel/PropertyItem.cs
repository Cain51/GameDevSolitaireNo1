using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyItem : MonoBehaviour
{
    public Text lbl_name;
    public Text lbl_cnt;
    public ItemCA item;
    public void Init(ItemCA data, int cnt)
    {
        item = data;
        lbl_name.text = item.Name;
        lbl_cnt.text = "x" + cnt;
    }
    public void OnClick()
    {
    
    }
}

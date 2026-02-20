using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildItem : MonoBehaviour
{
    public int index;
    public Text lbl_gr;
    public Text lbl_name;
    public Image img_icon;
    public Asset model;

    public void Init(int idx, Asset m)
    {
        index = idx;
        model = m;
        lbl_name.text = model.ca.name;
    }
    public void OnClick()
    {
       
    }
}

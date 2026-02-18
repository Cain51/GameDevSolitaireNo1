using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetItem : MonoBehaviour
{
    public int index;
    public Text lbl_name;
    public void Init(int idx ,string name) {
        index = idx;
        lbl_name.text = name;
    }
    public void OnClick() {
       
    }
}

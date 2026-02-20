using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DayItem : MonoBehaviour, ISelectItem
{
    public ActionCA actionData;
    public AssetCA baseData;
    public Text lbl_name;


    public void Refresh()
    {
        string result = string.Empty;
        if (actionData == null)
        {
            result = "+";
        }
        else if (baseData == null)
        {
            result = actionData.name;
        }
        else
        {
            result = actionData.name + "\n" + baseData.name;
        }
        lbl_name.text = result;
    }
    public void OnClick()
    {
		
    
    }

    public void Select(CABase data)
    {
		
    }
}

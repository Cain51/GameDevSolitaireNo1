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
		DownSelectPanel panel = Object.FindObjectOfType<DownSelectPanel>();
		if (panel == null) { return; }
		panel.item = this;
		if (actionData == null)
		{
			if (CBus.Instance.HasFactory("ActionFactory") == false) { return; }
			ActionFactory factory = CBus.Instance.GetFactory("ActionFactory") as ActionFactory;
			if (factory == null) { return; }
			panel.Draw(transform.position, factory.GetAllCA());
		}
		else
		{
			AssetFactory factory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
			if (factory == null) { return; }
			panel.Draw(transform.position, factory.GetAllCA());
		}
    }

    public void Select(CABase data)
    {
		if (data == null) { return; }
		ActionCA action = data as ActionCA;
		if (action != null)
		{
			actionData = action;
			baseData = null;
			Refresh();
			return;
		}
		AssetCA asset = data as AssetCA;
		if (asset != null)
		{
			baseData = asset;
			Refresh();
		}
    }
}

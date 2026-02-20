using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorePanel : PanelBase
{
	public Character buyer;

	public override void Init()
	{
		buyer = null;
	}

	public void OnClickOK()
	{
		Close();
		TipManager.Tip("交易已确认");
	}
	public void OnClickCancel()
	{
		Close();
	}
    public void OnClickClose()
    {
		Close();
    }
}

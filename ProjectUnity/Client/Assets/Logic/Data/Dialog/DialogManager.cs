using RG.Zeluda;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogManager : ManagerBase
{
	public void ShowDialog(string name,Action a = null) {
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		string graphName = $"{name}";
		string data = PlayerPrefs.GetString(graphName, string.Empty);
		if (data != string.Empty)
		{
			DialogPanel dp2 = uiManager.OpenFloat("DialogPanel") as DialogPanel;
			dp2.ShowSimple(string.Empty, data);
			return;
		}
		
		DialogPanel dp = uiManager.OpenFloat("DialogPanel") as DialogPanel;
		dp.StartDialog(graphName);
		dp.OnCloseCallback = a;
	}
	public void OnCharacterClick(Character c)
	{
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;

		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		string graphName = $"{c.ca.name}{c.ca.id}/{c.ca.id}_{slm.curScene.name}_{gm.day}";
		string data = PlayerPrefs.GetString(graphName, string.Empty);
		if (data != string.Empty)
		{
			DialogPanel dp2 = uiManager.OpenFloat("DialogPanel") as DialogPanel;
			dp2.ShowSimple(c.ca.name, data);
			return;
		}
		if (gm.CheckTime(1) == false)
		{
			TipManager.Tip("时间不足1小时");
			return;
		}
		DialogPanel dp = uiManager.OpenFloat("DialogPanel") as DialogPanel;

		dp.StartDialog(graphName);
		dp.OnCallback = () =>
		{
			GameManager gm2 = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
			gm2.CostTime(1);
		};
	}
}

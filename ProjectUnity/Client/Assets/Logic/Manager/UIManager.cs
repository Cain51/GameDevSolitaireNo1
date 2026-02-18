using System.Collections.Generic;
using System;
using UnityEngine;

public class UIManager : ManagerBase
{
	public Transform _tran_canvas;

	public Transform tran_canvas
	{
		get
		{
			if (_tran_canvas == null)
			{
				_tran_canvas = GameObject.Find("Canvas").transform;
			}
			return _tran_canvas;
		}
	}
	public Transform _tran_float;

	public Transform tran_float
	{
		get
		{
			if (_tran_float == null)
			{
				_tran_float = GameObject.Find("Float_Canvas").transform;
			}
			return _tran_float;
		}
	}
	private Dictionary<string, PanelBase> panelDic;
	private Dictionary<string, PanelBase> floatDic;
	public GameObject go_uibase;
	protected override void Init()
	{
		base.Init();
		panelDic = new Dictionary<string, PanelBase>();
		floatDic = new Dictionary<string, PanelBase>();
	}
	public PanelBase GetPanel(string key)
	{
		if (panelDic.ContainsKey(key) == false)
		{
			CreatePanel(key);
		}
		return panelDic[key];
	}
	public void OpenBase()
	{
		go_uibase.SetActive(true);
	}
	public void CloseBase()
	{
		go_uibase.SetActive(false);
	}
	public PanelBase OpenPanel(string key, Action closeAction = null)
	{
		if (panelDic.ContainsKey(key))
		{
			PanelBase p = panelDic[key];
			p.Open();
			return p;
		}
		PanelBase panel = CreatePanel(key, closeAction);
		panel.gameObject.SetActive(true);
		panel.Open();
		return panel;
	}

	public PanelBase CreatePanel(string key, Action closeAction = null)
	{

		GameObject obj = Resources.Load<GameObject>("Prefab/UI/" + key);
		if (obj == null) { return null; }
		GameObject go_panel = GameObject.Instantiate(obj);
		go_panel.name = key;
		PanelBase panel = go_panel.GetComponent<PanelBase>();
		panel.Init();
		panel.OnCloseCallback = closeAction;
		go_panel.transform.SetParent(tran_canvas);

		go_panel.transform.localPosition = Vector3.zero;
		go_panel.transform.localRotation = Quaternion.identity;
		go_panel.transform.SetSiblingIndex(go_panel.transform.childCount);
		go_panel.gameObject.SetActive(false);
		panelDic.Add(key, panel);
		return panel;
	}

	public PanelBase OpenFloat(string key, Action closeAction = null)
	{
		if (floatDic.ContainsKey(key))
		{
			PanelBase p = floatDic[key];
			p.Open();
			return p;
		}
		if (floatDic.ContainsKey(key)) { return floatDic[key]; }
		GameObject obj = Resources.Load<GameObject>("Prefab/UI/" + key);
		if (obj == null) { return null; }
		GameObject go_panel = GameObject.Instantiate(obj);
		PanelBase panel = go_panel.GetComponent<PanelBase>();
		panel.Init();
		panel.OnCloseCallback = closeAction;
		go_panel.transform.parent = tran_float;
		go_panel.transform.localPosition = Vector3.zero;
		floatDic.Add(key, panel);
		return panel;
	}
	public void ClosePanel(string key)
	{
		if (panelDic.ContainsKey(key) == false) { return; }
		PanelBase panel = panelDic[key];
		panelDic.Remove(key);
		panel.Close();
		GameObject.Destroy(panel.gameObject);

	}

	public void CloseAll()
	{
		foreach (PanelBase p in panelDic.Values)
		{
			p.Close();
			GameObject.Destroy(p.gameObject);
		}
		panelDic.Clear();
	}
}

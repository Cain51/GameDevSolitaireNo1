using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
	public Action OnCloseCallback;
	public virtual void Init()
	{

	}
	public virtual void SetData(object obj) {
	
	}
	public virtual void Open()
	{
		//OEF.Instance.isPause = true;
	}
	public virtual void Close()
	{
		//OEF.Instance.isPause = false;
		if (OnCloseCallback != null)
		{
			OnCloseCallback();
			OnCloseCallback = null;
		}
	}

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkItem : MonoBehaviour
{
	public Text lbl_name;
	public Action<WorkCA> onClick;
	public WorkCA work;
	public void Init(WorkCA ca)
	{
		work = ca;
		lbl_name.text = ca.name;

	}
	public void OnClick() {
		if (onClick == null) { return; }
		AudioManager.Inst.Play("BGM/µã»÷°´Å¥");
		onClick.Invoke(work);
	}
}

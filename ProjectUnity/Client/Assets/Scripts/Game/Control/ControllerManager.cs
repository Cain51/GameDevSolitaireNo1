using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : ManagerBase
{
	public Character target;
	public override void InitParams()
	{
		base.InitParams();
		OEF.Instance.Add(this, Update);
	}
	public void Update()
	{
		if (target == null) { return; }
		Vector3 dir = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			dir.z = 1;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			dir.z = -1;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			dir.x = -1;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			dir.x = 1;
		}
		
	}
}

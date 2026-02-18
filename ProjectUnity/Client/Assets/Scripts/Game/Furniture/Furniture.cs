using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : Product
{
	public new FurnitureCA ca;
	public Furniture(CABase ca) : base(ca)
	{
	}
	protected override void Init()
	{
		base.Init();
		this.ca = (FurnitureCA)base.ca;
	}
	protected override void InitParams()
	{
		base.InitParams();

	}
	public virtual void OpenEventPanel()
	{

	}

}

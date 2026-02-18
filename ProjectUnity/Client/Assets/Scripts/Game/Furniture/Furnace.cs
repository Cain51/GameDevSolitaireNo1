using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Furniture
{
	public int ligth = 10;
	public Furnace(CABase ca) : base(ca)
	{
	}
	protected override void Init()
	{
		base.Init();
		this.ca = (FurnitureCA)base.ca;
	}
}

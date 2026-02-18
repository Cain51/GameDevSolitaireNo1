using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GroundType
{
	uncultivated,
	empty,
	wet
}
public class Ground
{
	public GroundType gtype;
	public int id;
	public int process;
	public GroundItem view;
}

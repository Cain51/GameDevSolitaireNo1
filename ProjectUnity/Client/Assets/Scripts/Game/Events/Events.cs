using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : Product
{
	public new EventsCA ca;
	public List<Card> sellCards;
	public List<Card> buyCards;
	public Events(CABase ca) : base(ca)
	{
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : ManagerBase
{
	public Queue<CharacterCA> waitQueue = new Queue<CharacterCA>();

	public CharacterCA GetQueue()
	{
		if (waitQueue.Count == 0) { return null; }
		return waitQueue.Dequeue();
	}
	public void SetQueue(int[] ids)
	{
		CharacterFactory cf = CBus.Instance.GetFactory(FactoryName.CharacterFactory) as CharacterFactory;
		for (int i = 0; i < ids.Length; i++)
		{
			waitQueue.Enqueue(cf.GetCA(ids[i]) as CharacterCA);
		}
	}
}

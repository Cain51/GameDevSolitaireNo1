using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : ManagerBase
{

	public void InitMap(int id) {
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		if (slm != null)
		{
			slm.Load(id);
		}
	}
}

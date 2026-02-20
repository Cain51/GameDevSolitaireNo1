using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCenter : MonoBehaviour
{
    public static GameCenter inst;
	[System.Serializable]
	private class SaveData
	{
		public int day;
		public int time;
		public List<int> bagKeys = new List<int>();
		public List<int> bagValues = new List<int>();
		public List<int> assetKeys = new List<int>();
		public List<int> assetValues = new List<int>();
	}
	private void Awake()
	{
		inst = this;
	}

    public void Save()
    {
		SaveData data = new SaveData();
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm != null)
		{
			data.day = gm.day;
			data.time = gm.time;
			foreach (var kv in gm.bag)
			{
				data.bagKeys.Add(kv.Key);
				data.bagValues.Add(kv.Value);
			}
		}
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am != null)
		{
			foreach (var kv in am.assetDic)
			{
				data.assetKeys.Add(kv.Key);
				data.assetValues.Add(kv.Value);
			}
		}
		string json = JsonUtility.ToJson(data);
		PlayerPrefs.SetString("SaveData", json);
		PlayerPrefs.Save();
		TipManager.Tip("已保存");
    }
}

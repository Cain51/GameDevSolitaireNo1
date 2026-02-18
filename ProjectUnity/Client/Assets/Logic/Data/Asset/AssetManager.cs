
using RG.Basic.DataType;
using System.Collections.Generic;
using UnityEngine;

namespace RG.Zeluda
{
	public struct MatPair
	{
		public int id;
		public int cnt;
	}
	public class AssetManager : ManagerBase
	{
		public static List<int> baseAseetList = new List<int> { 1200001, 1210019, 1210018, 1210019 };
		public Transform tran_coin;
		public Transform tran_wood;
		public Transform tran_stone;

		public Transform tran_coinicon;
		public Transform tran_woodicon;
		public Transform tran_stoneicon;
		public bool isUpdate = false;
		public Dictionary<int, int> assetDic = new Dictionary<int, int>();

		private Dictionary<int, Dictionary<int, int>> dayIDPrice = new Dictionary<int, Dictionary<int, int>>();
	
		protected override void Init()
		{
			isUpdate = false;
			base.Init();
			OEF.Instance.Add(this,Update);
		}
		public Vector3 GetCoinPos()
		{
			return Camera.main.ScreenToWorldPoint(tran_coin.position);
		}
		public Vector3 GetWoodPos()
		{
			return Camera.main.ScreenToWorldPoint(tran_wood.position);
		}
		public Vector3 GetStonePos()
		{
			return Camera.main.ScreenToWorldPoint(tran_stone.position);
		}

		public bool Remove(int id, int cnt)
		{
			if (assetDic.ContainsKey(id) == false || assetDic[id] < cnt)
			{
				return false;
			}
			assetDic[id] -= cnt;
			isUpdate = true;
			return true;
		}

		public bool RemoveList(MatPair[] pairs)
		{
			int len = pairs.Length;
			for (int i = 0; i < len; i++)
			{
				MatPair mat = pairs[i];
				if (assetDic.ContainsKey(mat.id) == false || assetDic[mat.id] < mat.cnt)
				{
					return false;
				}
			}
			for (int i = 0; i < len; i++)
			{
				MatPair mat = pairs[i];
				assetDic[mat.id] -= mat.cnt;
			}
			isUpdate = true;
			return true;
		}

		public void Add(int id, int cnt, bool needUpdate = true)
		{
			if (assetDic.ContainsKey(id) == false)
			{
				assetDic.Add(id, cnt);
			}
			else
			{
				assetDic[id] += cnt;
			}

			isUpdate = needUpdate;
		}
		public void AddList(MatPair[] pairs)
		{
			int len = pairs.Length;
			for (int i = 0; i < len; i++)
			{
				MatPair mat = pairs[i];
				Add(mat.id, mat.cnt);
			}
			isUpdate = true;
		}


		public void AddDayPrice(int day, int id, int price)
		{
			Dictionary<int, int> idPriceDic = null;
			if (dayIDPrice.ContainsKey(day) == true)
			{
				idPriceDic = dayIDPrice[day];
			}
			else
			{
				idPriceDic = new Dictionary<int, int>();
				idPriceDic.Add(id, price);
				dayIDPrice.Add(day, idPriceDic);
			}
			if (idPriceDic.ContainsKey(id) == false)
			{
				idPriceDic.Add(id, price);
			}
			else
			{
				idPriceDic[id] = price;
			}
		}
		public int GetDayPrice(int day, int id)
		{
			if (dayIDPrice.ContainsKey(day) == false) { return 0; }
			Dictionary<int, int> priceDic = dayIDPrice[day];
			if (priceDic.ContainsKey(id) == false) { return 0; }
			return priceDic[id];
		}
		public Dictionary<int, int> GetAssetWithoutBook()
		{
			Dictionary<int, int> asset = new Dictionary<int, int>();

			foreach (var item in assetDic)
			{
				int rel = item.Value;
				asset.Add(item.Key, rel);
			}
			return asset;
		}
		public int AssetCount(int assetid)
		{
			if (assetDic.ContainsKey(assetid) == false)
			{
				return 0;
			}
			return assetDic[assetid];
		}
		public bool CheckAsset(MatPair[] mats)
		{
			int len = mats.Length;
			for (int i = 0; i < len; i++)
			{
				MatPair mat = mats[i];
				if (assetDic.ContainsKey(mat.id) == false || assetDic[mat.id] < mat.cnt)
				{
					return false;
				}
			}
			return true;
		}
		public bool CheckAsset(MatPair[] mats, Dictionary<int, int> asset)
		{
			int len = mats.Length;
			for (int i = 0; i < len; i++)
			{
				MatPair mat = mats[i];
				if (asset.ContainsKey(mat.id) == false || asset[mat.id] < mat.cnt)
				{
					return false;
				}
			}
			return true;
		}
		public bool CheckAsset(int id, int cnt)
		{
			if (assetDic.ContainsKey(id) == false || assetDic[id] < cnt)
			{
				return false;
			}
			return true;
		}
		public bool CheckCoint(int cnt)
		{
			if (assetDic.ContainsKey(1200001) == false || assetDic[1200001] < cnt)
			{
				return false;
			}
			return true;
		}
		public bool RemoveCoin(int cnt)
		{
			return Remove(1200001, cnt);
		}
		public void GetCoin(int cnt, bool needUpdate = true)
		{
			Add(1200001, cnt, needUpdate);
		}
		//public void InitEffect(Vector3 pos, int assetid, int count)
		//{
		//	if (baseAseetList.Contains(assetid) == false) { return; }
		//	ResManager resManager = CBus.Instance.GetManager(ManagerName.ResManager) as ResManager;
		//	GameObject go = GameObject.Instantiate(resManager.GetRes<GameObject>("FX/Coin"));
		//	go.transform.position = pos;
		//	ParticleTargetMove move = go.GetComponent<ParticleTargetMove>();
		//	move.manager = this;
		//	move.Play(tran_coin, tran_coinicon, assetid, count /10);//
		//}
		public void Update()
		{
			if (isUpdate == false) { return; }
			UIManager uimanager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			FoodPanel foodPanel = uimanager.OpenPanel("FoodPanel") as FoodPanel;
			foodPanel.Refresh();
			isUpdate = false;
		}
	}
}
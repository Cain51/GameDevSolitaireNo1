using System;
using System.Data;

namespace RG.Zeluda
{
	public class AssetFactory : FactoryBase
	{
		protected override void Init()
		{
			base.Init();
			_memoryPoolCapacityEach = 0;
			_memoryPoolCapacityTotal = 0;
		}
		protected override Product CreateClass(int id)
		{
			AssetCA ca = GetCA(id) as AssetCA;
			return new Asset(ca);
		}
		public override void CreateCA(int id)
		{
			AssetCA ca = new AssetCA();
			ca.factory = this;
			DataRow row = DataCenter.GetData("asset", id);
			if (row == null) { _caDic[id] = null; return; }
			ca.id = id;
			ca.name = Convert.ToString(row["name"]);
			ca.cost = Convert.ToInt32(row["cost"]);
			ca.respath = Convert.ToString(row["resPath"]);
			_caDic[id] = ca;
		}
	}
}
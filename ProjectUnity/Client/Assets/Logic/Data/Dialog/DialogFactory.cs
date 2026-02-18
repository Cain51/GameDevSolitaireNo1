using System;
using System.Data;

namespace RG.Zeluda
{
	public class DialogFactory : FactoryBase
	{
		protected override void Init()
		{
			base.Init();
			_memoryPoolCapacityEach = 0;
			_memoryPoolCapacityTotal = 0;
		}
		public override void CreateCA(int id)
		{
			DialogCA ca = new DialogCA();
			ca.factory = this;
			DataRow row = DataCenter.GetData("dialog", id);
			ca.id = id;
			ca.ani = Convert.ToString("ani");
			ca.content = Convert.ToString("content");
			ca.suc = Convert.ToInt32("suc");
			ca.fail = Convert.ToInt32("fail");
			_caDic[id] = ca;
		}

	}
}
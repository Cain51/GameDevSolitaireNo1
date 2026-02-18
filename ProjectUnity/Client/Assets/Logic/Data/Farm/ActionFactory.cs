using System;

namespace RG.Zeluda
{
    public class ActionFactory : FactoryBase
	{
		protected override void Init()
		{
			base.Init();
			_memoryPoolCapacityEach = 0;
			_memoryPoolCapacityTotal = 0;
		}
		public override void CreateCA(int id)
		{
			ActionCA ca = new ActionCA();
			ca.factory = this;
			var row = DataCenter.GetData("ction", id);
			if (row == null) { return; }
			ca.id = id;
			ca.name = Convert.ToString("name"); 
			ca.power = Convert.ToInt32("power"); 
			_caDic[id] = ca;
		}
	}
}
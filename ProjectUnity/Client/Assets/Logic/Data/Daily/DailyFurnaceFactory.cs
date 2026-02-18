using System;

public class DailyFurnaceFactory : FactoryBase
{
	protected override Product CreateClass(int id)
	{
		DailyFurnaceCA ca = GetCA(id) as DailyFurnaceCA;
		return new DailyFurnace(ca);
	}
	public override void CreateCA(int id)
	{
		DailyFurnaceCA ca = new DailyFurnaceCA();
		ca.factory = this;
		var row = DataCenter.GetData("dailyfurnace", id);
		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString("name");
		ca.costids = DataUtil.StringToIntPairAry(Convert.ToString("costids"));
		ca.costtypes = DataUtil.StringToIntPairAry(Convert.ToString("costtypes"));
		ca.guest = DataUtil.StringToIntAry(Convert.ToString("guest"));
		_caDic[id] = ca;
	}
}

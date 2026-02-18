using System;

public class WorkFactory : FactoryBase
{
	public override int GetFactoryCode()
	{
		return 160;
	}
	public override void CreateCA(int id)
	{
		WorkCA ca = new WorkCA();
		ca.factory = this;
		var row = DataCenter.GetData("work", id);
	
		if (row == null) {  return; }
		ca.id = id;
		ca.name = Convert.ToString("name");
		ca.starttime = Convert.ToInt32("starttime");
		ca.endtime = Convert.ToInt32("endtime");
		ca.reward = DataUtil.StringToIntPairAry(Convert.ToString("reward"));
		ca.alert = Convert.ToString("alert");
		ca.work = Convert.ToString("work");
		_caDic[id] = ca;
	}
}

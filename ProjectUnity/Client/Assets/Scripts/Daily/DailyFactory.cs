using System;

public class DailyFactory : FactoryBase
{
	public override void CreateCA(int id)
	{
		DailyCA ca = new DailyCA();
		ca.factory = this;
		var row = DataCenter.GetData("daily", id);
		ca.id = id;
	
		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString(row["name"]);
		ca.dialog = Convert.ToString(row["dialog"]);
		ca.scene = Convert.ToString(row["scene"]);
		_caDic[id] = ca;
	}
}

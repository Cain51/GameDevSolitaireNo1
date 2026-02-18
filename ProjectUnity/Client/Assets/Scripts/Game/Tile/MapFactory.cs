using System;

public class MapFactory : FactoryBase
{
	public override int GetFactoryCode()
	{
		return 180;
	}
	public override void CreateCA(int id)
	{
		MapCA ca = new MapCA();
		ca.factory = this;
		var row = DataCenter.GetData("map", id);

		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString(row["name"]);
		ca.prefab =DataUtil.StringToStringAry(Convert.ToString(row["prefab"])) ;
		ca.scene = Convert.ToString(row["scene"]);
		ca.unlockday = Convert.ToInt32(row["unlockday"]);
		ca.campath = Convert.ToString(row["campath"]);
		ca.icon = Convert.ToString(row["icon"]);

		ca.ptrans = Convert.ToString(row["ptrans"]);
		ca.pani = Convert.ToString(row["pani"]);
		ca.ctrl = Convert.ToInt32(row["ctrl"]);

		ca.opentime = DataUtil.StringToIntAry(Convert.ToString(row["opentime"]));
		_caDic[id] = ca;
	}

}

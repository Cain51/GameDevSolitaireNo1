using System;

public class CardFactory : FactoryBase
{
	public override int GetFactoryCode()
	{
		return 110;
	}
	public override void CreateCA(int id)
	{
		CardCA ca = new CardCA();
		ca.factory = this;
		var row = DataCenter.GetData("card", id);
		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString("name"); 
		ca.resPath = Convert.ToString("resPath");
		_caDic[id] = ca;
	}
}

using System;

public class FurnitureFactory : FactoryBase
{
	protected override Product CreateClass(int id)
	{
		FurnitureCA ca = GetCA(id) as FurnitureCA;

		switch (ca.mod)
		{
			case "Furnace":
				return new Furnace(ca);
			case "TradeTable":
				return new TradeTable(ca);
		}
		return new Furniture(ca);
	}
	public override void CreateCA(int id)
	{
		FurnitureCA ca = new FurnitureCA();
		ca.factory = this;

		var row = DataCenter.GetData("furniture", id);
		ca.id = id;

		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString("name");
		ca.mod = Convert.ToString("mod");
		ca.resPath = Convert.ToString("resPath");
		ca.panel = Convert.ToString("panel");
		ca.cardid = Convert.ToInt32("cardid");
		_caDic[id] = ca;
	}
}

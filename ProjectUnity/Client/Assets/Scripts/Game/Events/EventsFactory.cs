using System;

public class EventsFactory : FactoryBase
{
	protected override Product CreateClass(int id)
	{
		EventsCA ca = GetCA(id) as EventsCA;
		return new Events(ca);
	}
	public override void CreateCA(int id)
	{
		EventsCA ca = new EventsCA();
		ca.factory = this;
		var row = DataCenter.GetData("events", id);
		ca.id = id;
		ca.name = Convert.ToString("name");
		ca.scene = Convert.ToString("scene");
		ca.dialog = Convert.ToString("dialog");
		ca.cost = Convert.ToInt32("cost");
		ca.awake = Convert.ToInt32("awake");

		_caDic[id] = ca;
	}
}

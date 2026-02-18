using System;

public class ParameterFactory : FactoryBase
{
	protected override Product CreateClass(int id)
	{
		ParameterCA ca = GetCA(id) as ParameterCA;
		return new Parameter(ca);
	}
	public override void CreateCA(int id)
	{
		ParameterCA ca = new ParameterCA();
		ca.factory = this;
		var row = DataCenter.GetData("parameter", id);
		ca.id = id;
		ca.value = Convert.ToInt32("value");
		_caDic[id] = ca;
	}
}
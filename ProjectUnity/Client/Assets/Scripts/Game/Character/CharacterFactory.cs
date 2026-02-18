using System;

public class CharacterFactory : FactoryBase
{
	protected override Product CreateClass(int id)
	{
		CharacterCA ca = GetCA(id) as CharacterCA;
		return new Character(ca);
	}
	public override void CreateCA(int id)
	{
		CharacterCA ca = new CharacterCA();
		ca.factory = this;

		var row = DataCenter.GetData("character", id);
		
		if (row == null) { return; }
		ca.id = id;
		ca.name = Convert.ToString(row["name"]);
		ca.path = Convert.ToString(row["path"]);
		_caDic[id] = ca;
	}
}

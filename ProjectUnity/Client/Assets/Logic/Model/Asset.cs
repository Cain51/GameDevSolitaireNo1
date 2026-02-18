using RG.Zeluda;
using System.Collections.Generic;
public enum AssetType
{
	None,
	Gold,
	Food,
	Warm,
	Health,
	Item,
}
public class Asset : Product
{
	public new AssetCA ca;
	public Dictionary<int, int> actionCnt = new Dictionary<int, int>();

	public int rewardCnt = 0;
	public int cnt;

	public Asset(CABase ca) : base(ca)
	{
	}
	protected override void Init()
	{
		base.Init();
		this.ca = (AssetCA)base.ca;
	}
	protected override void InitParams()
	{
		base.InitParams();

	}

}

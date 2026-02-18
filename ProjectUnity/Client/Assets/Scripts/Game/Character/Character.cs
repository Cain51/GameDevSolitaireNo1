using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Product
{
	#region Fields
	public GameObject view;
	public new CharacterCA ca;
	public Card card;
	#endregion
	#region Properties
	#endregion
	public Character(CharacterCA ca) : base(ca)
	{

	}
	/// 只在新建时调用此方法
	/// </summary>
	protected override void Init()
	{
		this.ca = (CharacterCA)base.ca;
		base.Init();
	}
	/// <summary>
	/// 数据初始化
	/// 新建和重用的时候调用此方法
	/// 新建时在Init之后执行
	/// </summary>
	protected override void InitParams()
	{
		base.InitParams();
		ca = (CharacterCA)base.ca;
		OEF.Instance.Add(this, Update);
	}
	protected void Update()
	{
		int time = (int)Time.timeScale;

		for (int j = 0; j < time; j++)
		{

		}
	}
	public void OpenEventPanel() {
		//UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		//EventPanel eventsPanel = uiManager.OpenPanel("EventPanel") as EventPanel;
		//eventsPanel.SetData();
	}
	/// <summary>
	/// 回收
	/// 释放引用对象
	/// 内存池满了或者工厂销毁自动调用Destroy
	/// </summary>
	protected override void RecyclePrivate()
	{
		//释放写在Base之前，Base可能触发Destroy，导致对象为null

		GameObject.Destroy(view);
		base.RecyclePrivate();
	}
	/// <summary>
	/// 销毁
	/// 所有对象置null
	/// 只在Recycle方法中调用
	/// </summary>
	public override void Destroy()
	{

		base.Destroy();
	}
}

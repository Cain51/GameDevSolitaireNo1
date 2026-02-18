using DG.Tweening;
using RG.Zeluda;
using UnityEngine;

public class WorkPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform trans_content;
	public WorkCA[] cas;

	public void InitWork()
	{
		foreach (Transform child in trans_content)
		{
			Destroy(child.gameObject);
		}
		WorkFactory workFactory = CBus.Instance.GetFactory(FactoryName.WorkFactory) as WorkFactory;
		CABase[] ca = workFactory.GetAllCA();
		cas = new WorkCA[ca.Length];

		int idx = 0;
		foreach (var caItem in ca)
		{
			WorkCA work = caItem as WorkCA;
			cas[idx] = work;
			GameObject obj = GameObject.Instantiate(pfb_item, trans_content);
			obj.SetActive(true);
			WorkItem item = obj.GetComponent<WorkItem>();
			item.Init(work);
			item.onClick = OnClick;
			idx++;
		}
	}
	public override void Open()
	{
		base.Open();
		gameObject.SetActive(true);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.transform.DOKill();
		gameObject.transform.DOScale(Vector3.one, 0.2F);
		InitWork();
	}
	public override void Close()
	{
		base.Close();
	
		gameObject.transform.DOKill();
		gameObject.transform.DOScale(Vector3.zero, 0.2F).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}
	public void OnClick(WorkCA c)
	{
		Close();
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		MainPanel mp = uiManager.GetPanel("MainPanel") as MainPanel;
		mp.SetTimeSlice(c.starttime, c.endtime , Color.red);
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		gm.work = c;
	}
}

using UnityEngine;

public class GroundPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform tran_content;

	public void CreateGround(Ground g)
	{
		GameObject obj = GameObject.Instantiate(pfb_item);
		obj.transform.SetParent(tran_content);
		obj.SetActive(true);
		GroundItem item = obj.GetComponent<GroundItem>();
		g.view = item;
		item.Refresh(g);
	}
}

using UnityEngine;

public class DragManager : ManagerBase
{
	private Camera camera;
	private RectTransform canvasRect;
	public DragObject obj;
	public override void InitParams()
	{
		base.InitParams();
		OEF.Instance.Add(this, Update);
	}
	public void SelectObj(DragObject o) {
		ReleaseObj();
		obj = o;
	}
	public void ReleaseObj() {
		if (obj == null)
		{
			return;
		}
		obj.Release();
		obj = null;
	}

	// Update is called once per frame
	void Update()
	{
		if (obj == null) { return; }
		if (camera == null) { camera = Camera.main; }
		if (camera == null) { return; }
		if (canvasRect == null) { canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>(); }
		if (canvasRect == null) { return; }
		Vector2 localPoint;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, camera, out localPoint))
		{
			// 转换为世界坐标
			Vector3 worldPoint = canvasRect.TransformPoint(localPoint);
			worldPoint.y = worldPoint.z;
			worldPoint.z = 0;
			obj.transform.localPosition = worldPoint;
		}
	}
}

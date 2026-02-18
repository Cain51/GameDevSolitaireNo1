using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public Camera cam;
	public Transform player;
	public Vector3 offset = new Vector3(0, 10, 0);

	public float followSpeed = 5f; // 跟随速度
	public Vector2 minMaxX; // X轴限制范围
	public Vector2 minMaxY; // Y轴限制范围
	public float zoomSpeed = 10f; // 缩放速度
	public float minZoom = 30f; // 最小缩放
	public float maxZoom = 75f; // 最大缩放
	private void Start()
	{
		cam = GetComponent<Camera>();
	}

	public void Update()
	{
		if (player == null)
		{
			GameObject p = GameObject.Find("Player");
			if (p == null) { return; }
			player = p.transform;
			return;
		}
	
		// 计算相机目标位置
		Vector3 targetPosition = player.position + offset;
		targetPosition.x = Mathf.Clamp(targetPosition.x, minMaxX.x, minMaxX.y);
		targetPosition.y = Mathf.Clamp(targetPosition.y, minMaxY.x, minMaxY.y);
		targetPosition.z = cam.transform.position.z; // 保持相机的 Z 轴不变

		// 平滑跟随主角
		cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, followSpeed * Time.deltaTime);

		// 获取滚轮输入
		float scrollInput = Input.GetAxis("Mouse ScrollWheel");
		if (scrollInput != 0)
		{
			cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scrollInput * zoomSpeed, minZoom, maxZoom);
		}
	}
}

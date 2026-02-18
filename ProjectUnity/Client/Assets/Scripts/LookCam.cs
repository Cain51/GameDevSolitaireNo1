using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCam : MonoBehaviour
{
	private Transform tran_cam;

	// Update is called once per frame
	void Update()
	{
		if (tran_cam == null)
		{
			Camera[] cameras = FindObjectsOfType<Camera>();
			foreach (Camera c in cameras)
			{
				CameraManager cm = c.GetComponent<CameraManager>();
				if (cm != null)
				{
					tran_cam = c.transform;
					break;
				}
			}
		}
		if (tran_cam == null)
		{ return; }
		transform.forward = transform.position - tran_cam.position ;
	}
}

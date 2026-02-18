using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;

public class CameraRaycasterAdder : EditorWindow
{
	[MenuItem("Tools/Add Physics Raycaster to Cameras in Active Scene")]
	public static void AddPhysicsRaycasterToCameras()
	{
		// 获取当前活动的场景
		var activeScene = EditorSceneManager.GetActiveScene();

		// 遍历场景中的所有根GameObject
		foreach (GameObject obj in activeScene.GetRootGameObjects())
		{
			// 查找Camera组件
			Camera camera = obj.GetComponent<Camera>();
			if (camera != null)
			{
				// 检查是否已存在PhysicsRaycaster
				if (camera.GetComponent<PhysicsRaycaster>() == null)
				{
					// 添加PhysicsRaycaster组件
					camera.gameObject.AddComponent<PhysicsRaycaster>();
					Debug.Log($"Added PhysicsRaycaster to Camera: {camera.name} in scene: {activeScene.name}");
				}
			}
		}
	}
}

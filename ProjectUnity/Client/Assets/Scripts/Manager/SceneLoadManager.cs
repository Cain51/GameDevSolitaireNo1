using RG.Basic;
using RG.Zeluda;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : ManagerBase
{
	public Scene curScene;
	public GameObject pfb_obj;
	public MapCA mapCA;
	public void Load(string scene)
	{
		if (curScene.name != null)
		{
			if (curScene.name == scene) { return; }
			SceneManager.UnloadScene(curScene);
		}
		LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
		curScene = SceneManager.LoadScene(scene, parameters);
	}
	public void Load(MapCA ca)
	{
		mapCA = ca;

		string sceneName = ca.scene;
		if (string.IsNullOrEmpty(sceneName) || sceneName == "Shop")
		{
			LoadPrefab();
			return;
		}

		if (curScene.name != null)
		{

			if (curScene.name == sceneName)
			{
				LoadPrefab();
				return;
			}
			if (pfb_obj != null)
			{
				GameObject.Destroy(pfb_obj);
			}
			SceneManager.UnloadScene(curScene);
		}
		LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
		curScene = SceneManager.LoadScene(sceneName, parameters);
		LoadPrefab(); // ���óɹ��ص�
	}
	public void Load(int id)
	{
		MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
		mapCA = mf.GetCA(id) as MapCA;
		if (mapCA != null && (string.IsNullOrEmpty(mapCA.scene) || mapCA.scene == "Shop"))
		{
			LoadPrefab();
			return;
		}
		if (curScene.name != null)
		{
			if (curScene.name == mapCA.scene)
			{
				LoadPrefab();
				return;
			}
			if (pfb_obj != null)
			{
				GameObject.Destroy(pfb_obj);
			}
			SceneManager.UnloadScene(curScene);
		}
		LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
		curScene = SceneManager.LoadScene(mapCA.scene, parameters);
		LoadPrefab(); // ���óɹ��ص�
	}

	public void LoadPrefab()
	{
		if (pfb_obj != null)
		{
			GameObject.Destroy(pfb_obj);
			pfb_obj = null;

		}
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		int day = Mathf.Clamp(gm.day - 1, 0, mapCA.prefab.Length - 1);
        //�����������ӣ���ͼ����ز�ͬ��Ԥ����?
        UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		um.OpenPanel(mapCA.prefab[day]);
		GroundPanel groundPanel = um.GetPanel(mapCA.prefab[day]) as GroundPanel;
		Sprite backgroundSprite = null;
		if (groundPanel != null)
		{
			backgroundSprite = groundPanel.GetBackgroundSprite();
		}
		if (backgroundSprite != null)
		{
			UpdateLobbyBackground(backgroundSprite);
		}
		MainPanel mainPanel = um.GetPanel("MainPanel") as MainPanel;
		if (mainPanel != null)
		{
			mainPanel.RefreshActionButtons();
		}
		if (gm != null)
		{
			bool tutorialShown = gm.TryShowMapTutorial(mapCA);
			if (!tutorialShown)
			{
				gm.TryTriggerMapClue(mapCA);
			}
		}
		//GameObject obj = Resources.Load<GameObject>(mapCA.prefab[day]);
		//pfb_obj = GameObject.Instantiate(obj);
		//pfb_obj.transform.position = Vector3.zero;
		//Transform startPoint = pfb_obj.transform.Find(mapCA.ptrans);

		//if (startPoint == null)
		//{
		//	PlayerCharacterController.Inst.transform.position = Vector3.zero;
		//	PlayerCharacterController.Inst.transform.eulerAngles = Vector3.zero;
		//	PlayerCharacterController.Inst.agent.destination = Vector3.zero;
		//}
		//else {
		//	PlayerCharacterController.Inst.transform.position = startPoint.position;
		//	PlayerCharacterController.Inst.transform.eulerAngles = startPoint.eulerAngles;
		//	PlayerCharacterController.Inst.agent.destination = startPoint.position;
		//}

		//PlayerCharacterController.Inst.ani.Play(mapCA.pani);
		//PlayerCharacterController.Inst.moveLock = mapCA.ctrl == 0;

	}
	public void LoadSimplePrefab(string prefab)
	{
		if (pfb_obj != null)
		{
			GameObject.Destroy(pfb_obj);
			pfb_obj = null;

		}
		GameObject obj = Resources.Load<GameObject>(prefab);
		pfb_obj = GameObject.Instantiate(obj);
		pfb_obj.transform.position = Vector3.zero;
	}

	private void UpdateLobbyBackground(Sprite backgroundSprite)
	{
		if (backgroundSprite == null)
		{
			Debug.LogWarning("SceneLoadManager: Background sprite is null.");
			return;
		}
		Scene lobbyScene = SceneManager.GetSceneByName("Lobby");
		if (!lobbyScene.IsValid() || !lobbyScene.isLoaded)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded) { continue; }
				if (scene.name == "Lobby" || scene.path.Replace("\\", "/").EndsWith("/Lobby.unity"))
				{
					lobbyScene = scene;
					break;
				}
			}
		}
		if (!lobbyScene.IsValid() || !lobbyScene.isLoaded)
		{
			Debug.LogError("SceneLoadManager: Lobby scene not found or not loaded.");
			return;
		}
		GameObject[] roots = lobbyScene.GetRootGameObjects();
		bool found = false;
		for (int i = 0; i < roots.Length; i++)
		{
			GameObject root = roots[i];
			Transform canvasTransform = root.name == "Canvas" ? root.transform : root.transform.Find("Canvas");
			if (canvasTransform == null) { continue; }
			Transform backgroundTransform = canvasTransform.Find("Image");
			if (backgroundTransform == null) { continue; }
			Image backgroundImage = backgroundTransform.GetComponent<Image>();
			if (backgroundImage == null) { continue; }
			backgroundImage.sprite = backgroundSprite;
			found = true;
			return;
		}
		if (!found)
		{
			Debug.LogError("SceneLoadManager: Could not find Canvas/Image in Lobby scene.");
		}
	}
}

using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class LobbyPanel : PanelBase
{
	public GameObject go_start;
	public VideoPlayer v_start;
	public void OnStartGameClick()
	{
		go_start.SetActive(false);
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm != null)
		{
			gm.PrepareStartGame();
		}
		v_start.gameObject.SetActive(true);
		v_start.loopPointReached += OnVideoFinished;
		AudioManager.Inst.Play("BGM/�����ť");
	}

	public override void Close()
	{
		gameObject.SetActive(false);
		base.Close();
	}
	void OnVideoFinished(VideoPlayer vp)
	{
		v_start.gameObject.SetActive(false);
		v_start.loopPointReached -= OnVideoFinished;
		Close();
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm != null)
		{
			gm.StartFromLobby();
		}
	}
	public void SkipStartVideo()
	{
		if (v_start == null) { return; }
		if (v_start.gameObject.activeSelf == false) { return; }
		v_start.Stop();
		OnVideoFinished(v_start);
	}
	public void OnExitGameClick()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}

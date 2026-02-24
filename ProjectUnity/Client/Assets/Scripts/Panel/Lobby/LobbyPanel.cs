using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using DG.Tweening;

using ExcelReading;
using ExcelReading.Framework;

public class LobbyPanel : PanelBase
{
	public GameObject go_start;
	public VideoPlayer v_start;
    public string introTextContent = "【第一天：不安的种子】";
    public string outroTextContent = "少女把马牵进马厩。马看起来很虚弱，有点怕光。";
    
    // 引用新的剧情系统Prefab
    public GameObject newStorySystemPrefab;

    private GameObject currentBgObj;
    private Coroutine introCoroutine;

	public void OnStartGameClick()
	{
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm != null)
		{
			gm.PrepareStartGame();
		}
		
		// 立即隐藏开始界面的按钮，防止重复点击
		if (go_start != null) go_start.SetActive(false);

        introCoroutine = StartCoroutine(PlayIntroSequence());
	}

    private IEnumerator PlayIntroSequence()
    {
        Debug.Log("[LobbyPanel] 开始播放序幕流程...");
        // 1. 创建黑色背景
        GameObject bgObj = new GameObject("IntroBlackScreen");
        bgObj.transform.SetParent(transform, false);
        currentBgObj = bgObj; // 记录引用
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0); // 初始透明
        bgImg.raycastTarget = true; // 阻挡点击

        // 2. 创建文字
        GameObject textObj = new GameObject("IntroText");
        textObj.transform.SetParent(transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        Text txt = textObj.AddComponent<Text>();
        txt.text = introTextContent;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 40;
        txt.color = new Color(1, 1, 1, 0); // 初始透明

        // 尝试获取字体
        if (go_start != null)
        {
            Text btnText = go_start.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                txt.font = btnText.font;
            }
        }
        
        if (txt.font == null)
        {
            Debug.Log("[LobbyPanel] 未找到默认字体，尝试加载 LegacyRuntime.ttf");
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // 3. 调整层级
        bgObj.transform.SetAsLastSibling();
        textObj.transform.SetAsLastSibling();

        // 4. 执行动画序列
        Debug.Log("[LobbyPanel] 正在淡入黑屏...");
        if (bgImg != null) yield return bgImg.DOFade(1f, 1f).WaitForCompletion();

        Debug.Log("[LobbyPanel] 正在显示第一段文字...");
        if (txt != null) yield return txt.DOFade(1f, 1f).WaitForCompletion();
        yield return new WaitForSeconds(1f);
        if (txt != null) yield return txt.DOFade(0f, 1f).WaitForCompletion();

        // 播放视频
        if (v_start != null)
        {
            Debug.Log("[LobbyPanel] 正在播放视频...");
            bool isVideoFinished = false;
            v_start.gameObject.SetActive(true);
            v_start.transform.SetAsLastSibling();
            
            void OnVideoEnd(VideoPlayer vp) { isVideoFinished = true; }
            v_start.loopPointReached += OnVideoEnd;
            v_start.Play();

            float timeout = 60f;
            float timer = 0f;
            while (!isVideoFinished && timer < timeout)
            {
                if (v_start.isPlaying) timer += Time.unscaledDeltaTime;
                else timer += Time.unscaledDeltaTime * 0.1f;
                yield return null;
            }
            v_start.loopPointReached -= OnVideoEnd;
            v_start.gameObject.SetActive(false);
        }

        Debug.Log("[LobbyPanel] 正在显示第二段文字...");
        if (txt != null)
        {
            txt.text = outroTextContent;
            yield return txt.DOFade(1f, 1f).WaitForCompletion();
            yield return new WaitForSeconds(1f);
            yield return txt.DOFade(0f, 1f).WaitForCompletion();
        }

        Debug.Log("[LobbyPanel] 视频与文字结束，准备移交给 GameManager...");
        
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm != null)
        {
            gm.NewStorySystemPrefab = newStorySystemPrefab; // 将预制体传给 GameManager
            Debug.Log("[LobbyPanel] 正在执行 gm.StartFromLobby()...");
            gm.StartFromLobby();
        }
        else
        {
            Debug.LogError("[LobbyPanel] 未找到 GameManager 实例！");
            Close();
        }
    }

	public override void Close()
	{
		gameObject.SetActive(false);
		base.Close();
	}
	
	public void SkipStartVideo()
	{
        Debug.Log("[LobbyPanel] 跳过序幕视频...");
        // 停止协程
        if (introCoroutine != null) StopCoroutine(introCoroutine);
        
		if (v_start != null && v_start.gameObject.activeSelf) 
        { 
            v_start.Stop();
            v_start.gameObject.SetActive(false);
        }
        
        if (currentBgObj != null) Destroy(currentBgObj);

        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm != null)
        {
            gm.PrepareStartGame(); // 确保跳过时也执行清除注册表和初始化逻辑
            gm.NewStorySystemPrefab = newStorySystemPrefab;
            gm.StartFromLobby();
        }
        else
        {
            Close();
        }
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

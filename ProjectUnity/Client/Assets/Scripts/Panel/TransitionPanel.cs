using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionPanel : PanelBase
{
	public CanvasGroup canvasGroup; // 需要设置为黑色 Image 的 CanvasGroup
	public float transitionTime = 0.5f;
	public Action OnClose;
	public Action OnFinished;
	private Text textComponent;

	public void StartTransition(Action action, string text = "", Action finishAction = null, bool stayBlack = false)
	{
		Debug.Log($"[TransitionPanel] 开始过渡动画, 文本: {text}, stayBlack: {stayBlack}");
		OnClose = action;
		OnFinished = finishAction;
		canvasGroup.alpha = 0;
		gameObject.SetActive(true);
		
		if (!string.IsNullOrEmpty(text))
		{
			if (textComponent == null)
			{
				textComponent = GetComponentInChildren<Text>();
				if (textComponent == null)
				{
					GameObject textObj = new GameObject("TransitionText");
					textObj.transform.SetParent(transform, false);
					textComponent = textObj.AddComponent<Text>();
					textComponent.alignment = TextAnchor.MiddleCenter;
					// 关闭 BestFit，直接使用固定字号，防止因容器过小导致字体变小
					textComponent.resizeTextForBestFit = false;
					textComponent.fontSize = 40;
					textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
					textComponent.verticalOverflow = VerticalWrapMode.Overflow;
					textComponent.color = Color.white;
					
					RectTransform rt = textComponent.rectTransform;
					rt.anchorMin = new Vector2(0.5f, 0.5f);
					rt.anchorMax = new Vector2(0.5f, 0.5f);
					rt.pivot = new Vector2(0.5f, 0.5f);
					rt.sizeDelta = new Vector2(1920, 1080); // 强制设置一个足够大的尺寸
					rt.anchoredPosition = Vector2.zero;
					
					// 强制使用 Arial 字体以保持一致性
					textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
			}
			textComponent.text = text;
			textComponent.gameObject.SetActive(false);
		}
		
		StartCoroutine(Transition(text, stayBlack));
	}

	private IEnumerator Transition(string text, bool stayBlack)
	{
		AudioManager.Inst.Play("BGM/场景切换");
		// 拉黑
		yield return Fade(1);
		
		// 这里可以执行需要的操作，比如加载新场景
		if (OnClose != null)
		{
			try
			{
				OnClose();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Transition OnClose Error: {ex}");
			}
		}

		if (!string.IsNullOrEmpty(text) && textComponent != null)
		{
			textComponent.gameObject.SetActive(true);
			textComponent.canvasRenderer.SetAlpha(0);
			textComponent.CrossFadeAlpha(1f, 0.5f, true);
			yield return new WaitForSecondsRealtime(0.5f);
			
			yield return new WaitForSecondsRealtime(0.5f);
			
			textComponent.CrossFadeAlpha(0f, 0.5f, true);
			yield return new WaitForSecondsRealtime(0.5f);
			
			textComponent.gameObject.SetActive(false);
		}
		else
		{
			yield return new WaitForSecondsRealtime(0.5f); // 等待一段时间
		}
		
		if (!stayBlack)
		{
			yield return Fade(0);
			
			// 场景变亮后再触发结束动作
			if (OnFinished != null)
			{
				try
				{
					OnFinished();
				}
				catch (Exception ex)
				{
					Debug.LogError($"Transition OnFinished Error: {ex}");
				}
			}
			
			Close();
		}
		else
		{
			// 如果需要保持黑屏，直接触发结束动作而不淡出
			if (OnFinished != null)
			{
				try
				{
					OnFinished();
				}
				catch (Exception ex)
				{
					Debug.LogError($"Transition OnFinished Error: {ex}");
				}
			}
			// 不调用 Close()，保持 gameObject 激活和 canvasGroup.alpha = 1
		}
	}
	public void FlashRed(float duration = 0.5f)
	{
		gameObject.SetActive(true);
		Image bg = canvasGroup.GetComponent<Image>();
		if (bg == null) bg = canvasGroup.GetComponentInChildren<Image>();
		
		if (bg != null)
		{
			bg.color = Color.red;
		}
		
		StartCoroutine(FlashRedCoroutine(duration));
	}

	private IEnumerator FlashRedCoroutine(float duration)
	{
		canvasGroup.alpha = 1f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			canvasGroup.alpha = 1f - (elapsed / duration);
			yield return null;
		}
		canvasGroup.alpha = 0f;
		
		// 恢复原色，防止下次拉黑也是红色
		Image bg = canvasGroup.GetComponent<Image>();
		if (bg == null) bg = canvasGroup.GetComponentInChildren<Image>();
		if (bg != null) bg.color = Color.black;
		
		gameObject.SetActive(false);
	}

	public void FadeOut(Action onFinished = null)
	{
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[TransitionPanel] FadeOut called but object is inactive. Force activating.");
            gameObject.SetActive(true);
        }
		StopAllCoroutines();
		StartCoroutine(DoFadeOut(onFinished));
	}

	private IEnumerator DoFadeOut(Action onFinished)
	{
		yield return Fade(0);
		onFinished?.Invoke();
		Close();
	}

	public override void Open()
	{
		base.Open();
		gameObject.SetActive(true);
	}

	public override void Close()
	{
		base.Close();
		gameObject.SetActive(false);
	}

	private IEnumerator Fade(float targetAlpha)
	{
		float startAlpha = canvasGroup.alpha;
		float time = 0;

		while (time < transitionTime)
		{
			time += Time.unscaledDeltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / transitionTime);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha; // 确保最终值准确
	}
}

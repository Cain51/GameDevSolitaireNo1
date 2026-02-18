using System;
using System.Collections;
using UnityEngine;

public class TransitionPanel : PanelBase
{
	public CanvasGroup canvasGroup; // 需要设置为黑色 Image 的 CanvasGroup
	public float transitionTime = 1f;
	public Action OnClose;

	public void StartTransition(Action action)
	{
		OnClose = action;
		canvasGroup.alpha = 0;
		gameObject.SetActive(true);
		StartCoroutine(Transition());
	}

	private IEnumerator Transition()
	{
		AudioManager.Inst.Play("BGM/场景切换");
		// 拉黑
		yield return Fade(1);
		// 这里可以执行需要的操作，比如加载新场景
		OnClose();
		yield return new WaitForSeconds(1f); // 等待一段时间
		
		yield return Fade(0);
		Close();
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
			time += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / transitionTime);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha; // 确保最终值准确
	}
}

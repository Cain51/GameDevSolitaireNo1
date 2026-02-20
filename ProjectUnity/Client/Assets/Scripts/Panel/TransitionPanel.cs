using System;
using System.Collections;
using UnityEngine;

public class TransitionPanel : PanelBase
{
	public CanvasGroup canvasGroup; // ��Ҫ����Ϊ��ɫ Image �� CanvasGroup
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
		// ����
		yield return Fade(1);
		// �������ִ����Ҫ�Ĳ�������������³���
		OnClose();
		yield return new WaitForSeconds(1f); // �ȴ�һ��ʱ��
		
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

		canvasGroup.alpha = targetAlpha; // ȷ������ֵ׼ȷ
	}
}

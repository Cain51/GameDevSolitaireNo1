using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // 确保已引入 DoTween 命名空间

public class TipPanel : PanelBase
{
	public GameObject tipPrefab;  // 提示框预制件
	public Transform tipContainer;  // 用于存放提示框的容器
	public float tipDuration = 2.0f;  // 提示持续时间
	public float moveDistance = 100.0f;  // 提示框向上移动的距离

	public void TipLog(string message)
	{
		// 创建提示框
		GameObject tipObject = Instantiate(tipPrefab, tipContainer);
		Text tipText = tipObject.GetComponentInChildren<Text>();  // 获取提示框中的 Text 组件用于显示信息
		tipText.text = message;  // 设置提示内容

		// 使用动画让提示框向上移动并逐渐消失
		RectTransform tipRect = tipObject.GetComponent<RectTransform>();
		CanvasGroup canvasGroup = tipObject.AddComponent<CanvasGroup>();  // 为提示框添加 CanvasGroup 以便控制透明度

		// 开始动画
		tipRect.DOAnchorPosY(tipRect.anchoredPosition.y + moveDistance, tipDuration).SetEase(Ease.OutCubic);  // 提示框移动
		canvasGroup.DOFade(0, tipDuration).SetEase(Ease.Linear).OnComplete(() =>
		{
			// 销毁提示框
			Destroy(tipObject);
		});
	}
}

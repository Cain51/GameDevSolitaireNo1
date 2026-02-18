using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // 确保引用 DoTween 命名空间

public class TipPanel : PanelBase
{
	public GameObject tipPrefab;  // 提示框预制件
	public Transform tipContainer;  // 放置提示框的容器
	public float tipDuration = 2.0f;  // 提示持续时间
	public float moveDistance = 100.0f;  // 提示框向上移动的距离

	public void TipLog(string message)
	{
		// 生成提示框
		GameObject tipObject = Instantiate(tipPrefab, tipContainer);
		Text tipText = tipObject.GetComponentInChildren<Text>();  // 假设提示框中有一个 Text 组件显示消息
		tipText.text = message;  // 设置提示内容

		// 动画：提示框向上移动并逐渐消失
		RectTransform tipRect = tipObject.GetComponent<RectTransform>();
		CanvasGroup canvasGroup = tipObject.AddComponent<CanvasGroup>();  // 为提示框添加 CanvasGroup 用于控制透明度

		// 开始动画
		tipRect.DOAnchorPosY(tipRect.anchoredPosition.y + moveDistance, tipDuration).SetEase(Ease.OutCubic);  // 向上移动
		canvasGroup.DOFade(0, tipDuration).SetEase(Ease.Linear).OnComplete(() =>
		{
			// 动画结束后销毁提示框
			Destroy(tipObject);
		});
	}
}

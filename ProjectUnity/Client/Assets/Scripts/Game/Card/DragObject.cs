using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	public Card card;
	public FillItem fillItem;
	public void Release()
	{
		if (card == null) { return; }
		card.transform.localPosition = new Vector3((int)card.transform.localPosition.x, (int)card.transform.localPosition.y, (int)card.transform.localPosition.z);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		DragManager dragManager = CBus.Instance.GetManager(ManagerName.DragManager) as DragManager;
		dragManager.SelectObj(this);
		if (fillItem != null) {
			fillItem.ReleaseCard();
			fillItem = null;
		}
		
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		DragManager dragManager = CBus.Instance.GetManager(ManagerName.DragManager) as DragManager;
		dragManager.ReleaseObj();
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;

		if (card == null) { return; }
		// 用于存储射线检测结果
		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, raycastResults);
		foreach (var result in raycastResults)
		{
			// 检测结果是否是 FillItem
			if (result.gameObject.CompareTag("FillItem"))
			{
				FillItem item = result.gameObject.GetComponent<FillItem>();
				item.SetCard(card);
				fillItem = item;
				return;  // 找到一个 FillItem 后，结束操作
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (card == null) { return; }
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			card.OpenEvent();
		}
		else if (eventData.button == PointerEventData.InputButton.Left)
		{
			card.MessageShow();
		}
	}
}

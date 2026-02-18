using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;

namespace RG.Zeluda
{
    public class TouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public object data;
        public Action<object, InputButton> clickDownAction;
        public Action<object, InputButton> clickUpAction;
        public Action<object, InputButton> clickClickAction;
        private Vector3 pos = Vector3.zero;
        public void OnPointerDown(PointerEventData eventData)
        {
            pos = eventData.position;
            if (clickDownAction == null) { return; }
            clickDownAction(data,eventData.button);

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData != null && Vector3.Distance(eventData.position, pos) > 2) { return; }
            if (clickUpAction == null) { return; }
            clickUpAction(data, eventData.button);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickClickAction == null) { return; }
            clickClickAction(data, eventData.button);
        }
    
    }
}
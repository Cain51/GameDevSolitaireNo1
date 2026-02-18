using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoeSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MatchPanel matchPanel;
    public KeyCode skillkey;
    public string skillname ;
    public string skillscript ;

    public int level = 0;

    public bool skillActive = false;
    public UnityEvent myevent;
    public void Cast()
    {
        if (skillActive)
        {
            myevent.Invoke();
            skillActive = false;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (matchPanel != null)
        {
            matchPanel.skillHintText.text = $"|{skillname}|\n{skillscript}";
            matchPanel.isSkllShowing = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (matchPanel != null)
        {
            matchPanel.skillHintText.text = "";
            matchPanel.isSkllShowing = false;
        }
    }
}

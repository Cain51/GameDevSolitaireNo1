using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoeSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MatchPanel matchPanel;
    public KeyCode skillkey;
    public string skillname ;
    public string skillscript ;
    public GameObject mask;
    public int level = 0;
    public bool unlock = false;
    public bool skillActive {
        get { return isacitve; }
        set { 
            isacitve = value;
            mask.SetActive(!isacitve);
        }

    }
    bool isacitve = false;
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

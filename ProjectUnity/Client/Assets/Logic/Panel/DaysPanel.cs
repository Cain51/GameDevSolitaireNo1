using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DaysPanel : MonoBehaviour
{
    public GameObject pfn_line;
    public GameObject pfb_item;

    public RectTransform tran_line;
    public Transform tran_content;
    public LineItem curLine;
    public ScrollRect scrollRect;
    public void GoNextDay(int day)
    {
        if (curLine != null)
        {
            curLine.OverDay();
        }
        GameObject line = GameObject.Instantiate(pfn_line);
        line.transform.parent = tran_line;
        line.SetActive(true);
        curLine = line.GetComponent<LineItem>();
        tran_content = curLine.tran_content;
        curLine.lbl_index.text = day.ToString();
        for (int i = 0; i < 5; i++)
        {
            GameObject og = GameObject.Instantiate(pfb_item);
            og.SetActive(true);
            og.transform.parent = tran_content;
            DayItem ditem = og.GetComponent<DayItem>();
            ditem.Refresh();
        }
		
        int child = tran_line.childCount;
        tran_line.sizeDelta = new Vector2(tran_line.sizeDelta.x, child * 100 + 10 * (child - 1));
        scrollRect.velocity = new Vector2(0, child * 100 + 10 * (child - 1));
    }
}

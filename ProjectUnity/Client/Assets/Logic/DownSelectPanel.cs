using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownSelectPanel : MonoBehaviour
{
    public GameObject pfb;
    public RectTransform content;

    public ISelectItem item;

    private CABase[] curData;
    public void Draw(Vector3 pos, ICollection datas)
    {
        gameObject.SetActive(true);
        transform.position = pos;

        for (int i = 0, len = content.childCount; i < len; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        int idx = 0;
		CABase[] baseDatas = new CABase[datas.Count];
        foreach (var i in datas)
        {
			CABase item = (CABase)i;
            GameObject obj = GameObject.Instantiate(pfb);
            obj.SetActive(true);
            obj.transform.parent = content;
            AssetItem asset = obj.GetComponent<AssetItem>();
            string plus = string.Empty;
            if (i is ActionCA)
            {
                plus = ((ActionCA)i).power.ToString();
            }
            asset.Init(idx, item.name + plus);
            baseDatas[idx] = item;
            idx++;
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Clamp(datas.Count * 30 + (datas.Count - 1) * 5, 10, 999));
        curData = baseDatas;
    }
    public void Select(int idx)
    {
        gameObject.SetActive(false);
        item.Select(curData[idx]);
    }
}

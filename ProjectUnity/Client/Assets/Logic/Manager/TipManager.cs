using DG.Tweening;
using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : ManagerBase
{
    private static Queue<string> tipQueue = new Queue<string>();
    private static bool isHelperInit = false;

    public static void Tip(string msg)
    {
        tipQueue.Enqueue(msg);
        InitHelper();
    }

    private static void InitHelper()
    {
        if (isHelperInit) return;
        isHelperInit = true;

        var go = new GameObject("TipManagerHelper");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<TipManagerHelper>();
    }


    public static void TryShowNext()
    {
        if (tipQueue.Count == 0) return;

        string msg = tipQueue.Dequeue();

        UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefab/tip"));
        obj.transform.SetParent(uiManager.tran_float, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.zero;
        Text txt = obj.GetComponentInChildren<Text>();
        txt.text = msg;
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.transform.DOScale(Vector3.one,0.5f));
        seq.Append(obj.transform.DOLocalMoveY(Screen.height / 2, 2));
        seq.AppendCallback(() =>
        {
            GameObject.Destroy(obj);
        });
    }

  
    private class TipManagerHelper : MonoBehaviour
    {
        private float timer = 0.5f;

        void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= 0.5f)
            {
                timer = 0f;
                TipManager.TryShowNext();
            }
        }
    }
}

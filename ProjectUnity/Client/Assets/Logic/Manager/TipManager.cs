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
        obj.transform.localPosition = new Vector3(0, 279, 0);
        obj.transform.localScale = Vector3.zero;
        Text txt = obj.GetComponentInChildren<Text>();
        txt.text = msg;

        // 动态调整宽度
        RectTransform bgRect = obj.GetComponent<RectTransform>();
        // 获取文本的首选宽度（不强制刷新Canvas，直接计算）
        float textWidth = txt.preferredWidth;
        // 加上一些边距，例如左右各50像素
        float newWidth = textWidth + 100f;
        // 也可以设置一个最小宽度，防止太短
        if (newWidth < 200f) newWidth = 200f;
        
        bgRect.sizeDelta = new Vector2(newWidth, bgRect.sizeDelta.y);

        Sequence seq = DOTween.Sequence();
        seq.Append(obj.transform.DOScale(Vector3.one,0.2f));
        // 增加中间的悬停时间：先在中间停留一会，再向上飘动
        seq.AppendInterval(1.0f); // 停留1秒
        // 使用相对移动 DOLocalMoveY 并且加上 "Relative" 参数，或者手动计算目标位置
        // 为了保证每次移动的距离一致，我们让它向上移动固定的距离，比如 200 像素
        seq.Append(obj.transform.DOLocalMoveY(200f, 1).SetRelative(true)); 
        seq.Join(txt.DOFade(0, 0.5f).SetDelay(0.5f)); // 提前淡出
        seq.AppendCallback(() =>
        {
            GameObject.Destroy(obj);
        });
    }

  
    private class TipManagerHelper : MonoBehaviour
    {
        private float timer = 0.2f;

        void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= 0.2f)
            {
                timer -= 0.2f;
                TipManager.TryShowNext();
            }
        }
    }
}

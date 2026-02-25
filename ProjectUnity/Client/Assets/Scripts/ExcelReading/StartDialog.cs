using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelReading.Framework;

namespace ExcelReading
{
    public class StartDialog : MonoBehaviour
    {
        public int StartID = 0; // 修改默认值为0

        void Start()
        {
            // 延迟一帧执行，确保DialogInfo已经完成初始化（Awake中加载表格，Enable中注册事件）
            StartCoroutine(StartDelayed());
        }

        private IEnumerator StartDelayed()
        {
            yield return null; // 等待一帧
            StoryEventManager.Instance.Publish(GameEventNames.DIALOG_START, StartID);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using ExcelReading.Framework;
using ExcelReading.Dialogs;

namespace ExcelReading
{
    [Serializable]
    public class DialogItem
    {
        public string flag; // 标志位：#对话 / &选项 / END结束
        public int id;      // 行ID（对应表格B列）
        public string character; // 人物（C列）
        public string position;  // 位置（左/右，D列）
        public string content;   // 内容（E列）
        public int jumpId;       // 跳转ID（F列）
        public string effect;    // 效果（G列，如"好感度加@1"）
        public string target;    // 目标（H列）
    }

    public class DialogInfo : MonoBehaviour
    {
        private List<DialogItem> dialogList;
        private int currentDialogId; // 当前对话ID
        public int StartDialogId = 0; // 起始对话ID
        public int? EndDialogId = null; // 结束对话ID，为null时使用END标志结束

        [Header("UI References")]
        public GameObject dialogPanel; // 对话面板
        public UnityEngine.UI.Text speakerText; // 说话人名称
        public UnityEngine.UI.Text contentText; // 对话内容
        public Transform optionParent1; // 选项按钮父物体1
        public Transform optionParent2; // 选项按钮父物体2
        public GameObject optionButtonPrefab; // 选项按钮预制体

        [Header("Portrait References")]
        public UnityEngine.UI.Image leftImage;  // 左侧立绘 Image 组件
        public UnityEngine.UI.Image rightImage; // 右侧立绘 Image 组件
        public float fadeDuration = 0.3f;       // 淡入淡出时间

        [Header("Settings")]
        [Range(0.01f, 0.1f)]
        public float typeSpeed = 0.03f; // 打字速度
        public SpeakPronounce speak;   // 引用旧系统的音效组件

        [Header("Special Effects")]
        public GameObject notePrefab; // 纸条预制体
        private GameObject currentNoteInstance; // 当前显示的纸条实例

        private bool isTyping = false; // 是否正在打字
        private string fullText; // 完整的对话内容
        private Coroutine typewriterCoroutine; // 当前运行的打字机协程引用
        private UnityEngine.UI.Button panelButton; // 对话面板按钮

        // 角色配置
        [System.Serializable]
        public class CharacterConfig
        {
            public string characterName; // 角色名称（与Excel中C列匹配）
            public Sprite normalSprite;  // 正常状态立绘
            public Sprite sadSprite;     // 悲伤状态立绘
            public Sprite angrySprite;   // 生气状态立绘
            public Sprite specialSprite; // 特殊状态立绘
        }

        public CharacterConfig[] characterConfigs; 

        private Dictionary<string, CharacterConfig> characterDictionary; 

        private void Awake()
        {
            // 隐藏对话面板，防止未初始化时显示
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            // 初始时隐藏立绘
            if (leftImage != null) leftImage.gameObject.SetActive(false);
            if (rightImage != null) rightImage.gameObject.SetActive(false);

            // 初始化角色字典
            characterDictionary = new Dictionary<string, CharacterConfig>();
            foreach (var config in characterConfigs)
            {
                if (!string.IsNullOrEmpty(config.characterName))
                    characterDictionary[config.characterName] = config;
            }

            // 初始化：加载对话表格
            dialogList = DialogXls.LoadDialogAsList();
        }

        // 辅助方法：根据 ID 查找第一个对话项（通常用于 # 或 END）
        private DialogItem GetDialogById(int id)
        {
            if (dialogList == null) return null;
            return dialogList.Find(x => x.id == id);
        }

        private void OnEnable()
        {
            // 订阅对话开始事件
            StoryEventManager.Instance.Subscribe(GameEventNames.DIALOG_START, OnDialogStartEvent);
            // 订阅对话结束事件
            StoryEventManager.Instance.Subscribe(GameEventNames.DIALOG_END, OnDialogEndEvent);
            // 订阅对话效果事件
            StoryEventManager.Instance.Subscribe(GameEventNames.ON_DIALOG_EFFECT, OnDialogEffectEvent);
        }

        private void OnDisable()
        {
            // 取消订阅对话开始事件
            StoryEventManager.Instance.Unsubscribe(GameEventNames.DIALOG_START, OnDialogStartEvent);
            // 取消订阅对话结束事件
            StoryEventManager.Instance.Unsubscribe(GameEventNames.DIALOG_END, OnDialogEndEvent);
            // 取消订阅对话效果事件
            StoryEventManager.Instance.Unsubscribe(GameEventNames.ON_DIALOG_EFFECT, OnDialogEffectEvent);
        }

        // 处理对话效果事件
        private void OnDialogEffectEvent(object eventData)
        {
            string effect = eventData as string;
            if (string.IsNullOrEmpty(effect))
            {
                Debug.LogWarning("[DialogInfo] 收到空的效果指令");
                return;
            }

            Debug.Log($"[DialogInfo] 收到效果指令: {effect}");

            // 只要包含“羊皮纸”或“纸条”关键词就触发
            if (effect.Contains("羊皮纸") || effect.Contains("纸条"))
            {
                ShowNote();
            }
            else
            {
                Debug.Log($"[DialogInfo] 指令 '{effect}' 未匹配任何已知效果");
            }
        }

        private void ShowNote()
        {
            if (notePrefab == null)
            {
                Debug.LogWarning("[DialogInfo] 未分配 notePrefab，无法显示纸条");
                return;
            }

            if (currentNoteInstance == null)
            {
                // 实例化纸条到 dialogPanel 的父物体下（通常是 Canvas）
                currentNoteInstance = Instantiate(notePrefab, dialogPanel.transform.parent);
                
                // 查找实际的纸条图片对象（根据你层级截图，名字是 "泛黄的羊皮纸"）
                Transform noteTransform = currentNoteInstance.transform.Find("泛黄的羊皮纸");
                if (noteTransform == null)
                {
                    // 如果找不到指定名字的子物体，就尝试找第一个子物体
                    if (currentNoteInstance.transform.childCount > 0)
                        noteTransform = currentNoteInstance.transform.GetChild(0);
                }

                GameObject clickableObj = noteTransform != null ? noteTransform.gameObject : currentNoteInstance;

                // 给实际的纸条对象添加点击关闭功能
                UnityEngine.UI.Button btn = clickableObj.GetComponent<UnityEngine.UI.Button>();
                if (btn == null)
                {
                    btn = clickableObj.AddComponent<UnityEngine.UI.Button>();
                    btn.transition = UnityEngine.UI.Selectable.Transition.None;
                }
                
                UnityEngine.UI.Image img = clickableObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(HideNote);
            }
            currentNoteInstance.SetActive(true);
            currentNoteInstance.transform.SetAsLastSibling();
        }

        private void HideNote()
        {
            if (currentNoteInstance != null)
            {
                currentNoteInstance.SetActive(false);
            }

            // 纸条关闭后，处理后续逻辑
            DialogItem current = GetDialogById(currentDialogId);
            if (current != null)
            {
                if (current.flag == "END")
                {
                    // 如果是结束行触发的纸条，关闭后直接结束剧情
                    StoryEventManager.Instance.Publish(GameEventNames.DIALOG_END, currentDialogId);
                }
                else
                {
                    // 如果是普通对话触发的纸条，关闭后跳转到下一句
                    currentDialogId = current.jumpId;
                    ShowCurrentDialog();
                }
            }
        }

        // 处理对话开始事件
        private void OnDialogStartEvent(object eventData)
        {
            int dialogId = StartDialogId;
            if (eventData is int) dialogId = (int)eventData;
            else if (eventData is string strData && int.TryParse(strData, out int parsedId)) dialogId = parsedId;
            
            StartDialog(dialogId);
        }

        // 开始对话
        public void StartDialog(int startId)
        {
            panelButton = dialogPanel.GetComponent<UnityEngine.UI.Button>();
            if (panelButton == null)
                panelButton = dialogPanel.AddComponent<UnityEngine.UI.Button>();
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(OnDialogPanelClick);
            panelButton.transition = UnityEngine.UI.Selectable.Transition.None;

            currentDialogId = startId;
            ShowCurrentDialog();
        }

        // 显示当前对话
        private void ShowCurrentDialog()
        {
            DialogItem current = GetDialogById(currentDialogId);
            if (current == null)
            {
                Debug.LogError("[DialogInfo] 对话ID不存在：" + currentDialogId);
                return;
            }

            // 播放对话框展开音效（如果是新的一轮对话）
            if (dialogPanel != null && !dialogPanel.activeSelf)
            {
                AudioManager.Inst.Play("BGM/对话框展开");
            }
            
            dialogPanel.SetActive(true);

            // 如果当前 ID 指向的是一个选项，强制进入选项组显示模式
            if (current.flag == "&")
            {
                ShowOptionsGroup(current.id);
                return;
            }

            switch (current.flag)
            {
                case "#": // 普通对话
                    if (speakerText != null) speakerText.text = current.character;
                    fullText = current.content;
                    if (contentText != null) contentText.text = "";
                    
                    // 播放旧系统的“语音”音效
                    if (speak != null)
                    {
                        speak.ConvertAndSpeak(current.content);
                    }

                    StoryEventManager.Instance.Publish(GameEventNames.ON_DIALOG, currentDialogId);

                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = StartCoroutine(TypewriterText());
                    
                    // 更新立绘
                    UpdateCharacterImages(current);
                    break;

                case "&": // 选项
                    ShowOptionsGroup(current.id);
                    break;

                case "END": // 结束
                    dialogPanel.SetActive(false);
                    if (leftImage != null) leftImage.gameObject.SetActive(false);
                    if (rightImage != null) rightImage.gameObject.SetActive(false);
                    StoryEventManager.Instance.Publish(GameEventNames.DIALOG_END, currentDialogId);
                    break;
            }
        }

        // 结束对话处理
        private void OnDialogEndEvent(object eventData)
        {
            if (panelButton != null) panelButton.onClick.RemoveListener(OnDialogPanelClick);
        }

        /// <summary>
        /// 核心修改：根据 Excel 里的 D列(position) 自动显示图片到指定位置
        /// 默认显示在左侧
        /// </summary>
        private void UpdateCharacterImages(DialogItem dialog)
        {
            string baseName = ExtractBaseCharacterName(dialog.character);
            
            // 查找配置中的图片
            CharacterConfig config = null;
            if (!string.IsNullOrEmpty(baseName))
                characterDictionary.TryGetValue(baseName, out config);

            // 处理位置逻辑
            if (dialog.position == "右")
            {
                ShowOnPosition(rightImage, config, dialog.content);
                HidePosition(leftImage);
            }
            else // 如果填了“左”，或者未指定位置，都默认显示在左侧
            {
                // 只有当有角色配置且名字不为空时才显示，否则（如旁白）隐藏
                if (config != null && !string.IsNullOrEmpty(baseName))
                {
                    ShowOnPosition(leftImage, config, dialog.content);
                    HidePosition(rightImage);
                }
                else
                {
                    // 旁白或未定义角色，隐藏全部
                    HidePosition(leftImage);
                    HidePosition(rightImage);
                }
            }
        }

        private void ShowOnPosition(UnityEngine.UI.Image targetImg, CharacterConfig config, string content)
        {
            if (targetImg == null) return;
            if (config == null)
            {
                targetImg.gameObject.SetActive(false);
                return;
            }

            // 设置图片并激活
            targetImg.sprite = DetermineExpressionSprite(config, content);
            targetImg.preserveAspect = true;
            
            if (!targetImg.gameObject.activeSelf)
            {
                targetImg.gameObject.SetActive(true);
                // 简单的淡入
                CanvasGroup cg = targetImg.GetComponent<CanvasGroup>();
                if (cg != null) StartCoroutine(FadeIn(cg, fadeDuration));
            }
        }

        private void HidePosition(UnityEngine.UI.Image targetImg)
        {
            if (targetImg != null && targetImg.gameObject.activeSelf)
            {
                CanvasGroup cg = targetImg.GetComponent<CanvasGroup>();
                if (cg != null) 
                    StartCoroutine(FadeOut(cg, fadeDuration, () => targetImg.gameObject.SetActive(false)));
                else 
                    targetImg.gameObject.SetActive(false);
            }
        }

        private string ExtractBaseCharacterName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "";
            int index = fullName.IndexOf('（');
            if (index == -1) index = fullName.IndexOf('(');
            return index > 0 ? fullName.Substring(0, index) : fullName;
        }

        private Sprite DetermineExpressionSprite(CharacterConfig config, string content)
        {
            if (content.Contains("害怕") || content.Contains("颤抖") || content.Contains("哭"))
                return config.sadSprite ?? config.normalSprite;
            if (content.Contains("生气") || content.Contains("愤怒") || content.Contains("可恶"))
                return config.angrySprite ?? config.normalSprite;
            if (content.Contains("特殊") || content.Contains("关键"))
                return config.specialSprite ?? config.normalSprite;
            return config.normalSprite;
        }

        private System.Collections.IEnumerator FadeIn(CanvasGroup group, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                group.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            group.alpha = 1;
        }

        private System.Collections.IEnumerator FadeOut(CanvasGroup group, float duration, Action onComplete = null)
        {
            float startAlpha = group.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                group.alpha = Mathf.Lerp(startAlpha, 0, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            group.alpha = 0;
            onComplete?.Invoke();
        }

        private void ShowOptionsGroup(int groupId)
        {
            ClearOptions();
            List<DialogItem> options = new List<DialogItem>();
            if (dialogList != null)
            {
                foreach (var item in dialogList)
                {
                    // 改为只要 ID 相同且标志为 &，就认为属于同一组选项
                    if (item.flag == "&" && item.id == groupId)
                        options.Add(item);
                }
            }

            for (int i = 0; i < options.Count; i++)
            {
                Transform parent = i == 0 ? optionParent1 : optionParent2;
                CreateOptionButton(options[i], parent);
            }
        }

        private void CreateOptionButton(DialogItem option, Transform parent)
        {
            if (optionButtonPrefab == null || parent == null) return;
            GameObject btn = Instantiate(optionButtonPrefab, parent);
            var txt = btn.GetComponentInChildren<UnityEngine.UI.Text>();
            if (txt != null) txt.text = option.content;

            btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                // 记录当前点击的选项 ID
                currentDialogId = option.id; 

                // 清除选项按钮
                ClearOptions();

                Debug.Log($"[DialogInfo] 点击选项 ID: {option.id}, 内容: {option.content}, Effect: {option.effect}");

                // 如果该选项自带效果（如弹出纸条），则先触发效果并隐藏对话框
                if (!string.IsNullOrEmpty(option.effect))
                {
                    if (dialogPanel != null) dialogPanel.SetActive(false);
                    StoryEventManager.Instance.Publish(GameEventNames.ON_DIALOG_EFFECT, option.effect);
                }
                else
                {
                    // 没有效果，直接跳转
                    currentDialogId = option.jumpId;
                    ShowCurrentDialog();
                }
            });
        }

        private void ClearOptions()
        {
            if (optionParent1 != null) foreach (Transform child in optionParent1) Destroy(child.gameObject);
            if (optionParent2 != null) foreach (Transform child in optionParent2) Destroy(child.gameObject);
        }

        private void OnDialogPanelClick()
        {
            DialogItem current = GetDialogById(currentDialogId);
            if (current == null) return;

            if (current.flag == "#")
            {
                if (isTyping)
                {
                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    if (contentText != null) contentText.text = fullText;
                    isTyping = false;
                }
                else
                {
                    // 如果有效果（如羊皮纸），点击先触发效果并隐藏对话框
                    if (!string.IsNullOrEmpty(current.effect))
                    {
                        if (dialogPanel != null) dialogPanel.SetActive(false);
                        StoryEventManager.Instance.Publish(GameEventNames.ON_DIALOG_EFFECT, current.effect);
                    }
                    else
                    {
                        // 没有效果，正常跳转
                        currentDialogId = current.jumpId;
                        ShowCurrentDialog();
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator TypewriterText()
        {
            isTyping = true;
            if (contentText != null) contentText.text = "";
            
            for (int i = 0; i < fullText.Length; i++)
            {
                if (contentText != null) contentText.text += fullText[i];
                char currentChar = fullText[i];
                if (!char.IsWhiteSpace(currentChar))
                    StoryEventManager.Instance.Publish(GameEventNames.DIALOG_TYPE_SOUND, currentChar);
                
                yield return new WaitForSeconds(typeSpeed);
            }
            isTyping = false;
        }
    }
}

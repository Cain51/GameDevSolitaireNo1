using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class MainPanel : PanelBase
{
	public Transform tran_clock;
	public Button btn_map;
	public Text lbl_day;
	public GameObject go_slice;
	public Transform tran_content;
	public List<Image> imgList = new List<Image>();

	public GameObject go_item;
	public Transform tran_item;
	public Dictionary<int, BegItem> begItem = new Dictionary<int, BegItem>();

	public Text lbl_lv;
	public Image img_exp;
	public Text text_exp;
	public Image img_ma;
    public GameObject pfb_ma_normal;    // 初始马预制体
    public GameObject pfb_ma_small_dry; // 小枯枝马预制体
    public GameObject pfb_ma_dry;       // 枯枝马预制体
    private GameObject inst_ma_normal;    // 初始马实例
    private GameObject inst_ma_small_dry; // 小枯枝马实例
    private GameObject inst_ma_dry;       // 枯枝马实例
    private GameObject currentHorseObj;   // 当前马实例

    private void SetupUIObj(GameObject obj)
    {
        if (obj == null) return;
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            // 彻底重置为全屏填充模式，确保预制体内容能撑开并对齐
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
        }
        else
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
        }
        
        // 确保实例被正确激活
        obj.SetActive(true);
    }

	private Transform tran_action;
	private readonly List<Button> actionButtons = new List<Button>();
	private readonly List<Text> actionTexts = new List<Text>();
	private readonly Dictionary<string, UnityAction> actionHandlers = new Dictionary<string, UnityAction>();
	private readonly Dictionary<string, string[]> mapActionLabels = new Dictionary<string, string[]>();
	public override void Init()
	{
		base.Init();
		if (btn_map == null)
		{
			Button[] buttons = GetComponentsInChildren<Button>(true);
			foreach (var btn in buttons)
			{
				if (btn.onClick != null)
				{
					for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
					{
						if (btn.onClick.GetPersistentMethodName(i) == "OnMapClick")
						{
							btn_map = btn;
							break;
						}
					}
				}
				if (btn_map != null) break;
			}
		}
		InitActionHandlers();
		CacheActionButtons();
		RefreshActionButtons();
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (gm != null)
		{
			SetDay(gm.day);
			SetTime(gm.time);
		}
	}
	public void InitClock()
	{
		imgList.Clear();
		for (int i = 0; i < 24; i++)
		{
			GameObject obj = GameObject.Instantiate(go_slice, tran_content);
			obj.SetActive(true);
			obj.transform.localEulerAngles = new Vector3(0, 0, i * -15);
			imgList.Add(obj.GetComponent<Image>());
		}
	}
	public void SetTimeSlice(int start, int end, Color c)
	{
		// Ensure imgList is initialized
		if (imgList == null || imgList.Count == 0)
		{
			InitClock();
		}

		for (int i = start; i <= end; i++)
		{
			if (i >= 0 && i < imgList.Count)
			{
				imgList[i].color = c;
			}
		}
	}
	public void SetDay(int day)
	{
		lbl_day.text = $"第{day}天";
		SetTimeSlice(0, 23, Color.white);
    }

    // 动态显示或隐藏天数文本
    public void ToggleDayDisplay(bool visible)
    {
        if (lbl_day != null)
        {
            lbl_day.gameObject.SetActive(visible);
        }
    }

	public void SetTime(int curTime)
	{
        // curTime 是剩余小时数，24 - curTime 才是当前小时数
        int hour = 24 - curTime;
		tran_clock.localEulerAngles = new Vector3(0, 0, hour * -15);
	}
	public void OnRelaxClick()
	{
		AudioManager.Inst.Play("BGM/消磨时间");
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		gm.CostTime(1);
	}
	public void OnOverDayClick()
	{
		EventManager em = CBus.Instance.GetManager(ManagerName.EventManager) as EventManager;
		em.TriggerEvent(1400003);
	}
	public void OnMapClick()
	{
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		MapPanel map = uiManager.GetPanel("MapPanel") as MapPanel;
		if (map.isOpen)
		{
			map.Close();
			return;
		}
		map = uiManager.OpenPanel("MapPanel") as MapPanel;
		AudioManager.Inst.Play("BGM/点击按钮");
		map.InitMap();
	}
	private void InitActionHandlers()
	{
		actionHandlers.Clear();
		actionHandlers["耕地"] = Hoe;
		actionHandlers["浇水"] = Water;
		actionHandlers["种植"] = Plant;
		actionHandlers["播种"] = Plant;
		actionHandlers["喂马"] = Feed;
		actionHandlers["喂鸡"] = FeedChicken;
		actionHandlers["杀鸡"] = KillChicken;
		actionHandlers["挖矿"] = Mine;
		actionHandlers["钓鱼"] = Fish;
		actionHandlers["赛马"] = Match;
		actionHandlers["睡觉"] = Sleep;
        actionHandlers["治疗"] = Treat;
        
        // 添加其他交互占位
        actionHandlers["读书"] = () => TipManager.Tip("这里有很多深奥的书籍...");
        actionHandlers["祷告"] = () => TipManager.Tip("你在内心默默祈祷...");
        actionHandlers["休息"] = OnRelaxClick;

		mapActionLabels.Clear();
		mapActionLabels["农场"] = new[] { "耕地", "浇水", "播种", "赛马", "睡觉" };
		mapActionLabels["你的农场"] = new[] { "耕地", "浇水", "播种", "赛马", "睡觉" };
		mapActionLabels["养鸡场"] = new[] { "喂鸡", "杀鸡", "赛马", "睡觉" };
		mapActionLabels["杂货铺"] = new[] { "赛马", "睡觉" };
		mapActionLabels["牧场"] = new[] { "喂马", "赛马", "睡觉" };
		mapActionLabels["医院"] = new[] { "治疗", "赛马", "睡觉" };
		mapActionLabels["图书馆"] = new[] { "读书", "赛马", "睡觉" };
		mapActionLabels["矿洞"] = new[] { "挖矿", "赛马", "睡觉" };
		mapActionLabels["海边"] = new[] { "钓鱼", "赛马", "睡觉" };
		mapActionLabels["教堂"] = new[] { "祷告", "赛马", "睡觉" };
	}
	private void CacheActionButtons()
	{
		if (tran_action == null)
		{
			tran_action = transform.Find("go_action");
		}
		actionButtons.Clear();
		actionTexts.Clear();
		if (tran_action == null) { return; }
		for (int i = 0; i < tran_action.childCount; i++)
		{
			Transform child = tran_action.GetChild(i);
			Button button = child.GetComponent<Button>();
			if (button == null) { continue; }
			actionButtons.Add(button);
			Text txt = child.GetComponentInChildren<Text>(true);
			actionTexts.Add(txt);
		}
	}
	public void RefreshActionButtons()
	{
		if (actionButtons.Count == 0)
		{
			CacheActionButtons();
		}
		string mapName = GetCurrentMapName();
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
        
        int currentDay = gm != null ? gm.day : 1;
        bool isDay1 = currentDay == 1;
        bool isDay3 = currentDay == 3;
        bool isHayEmpty = am != null && am.AssetCount(1100003) <= 0;
        bool isMorningStory = gm != null && gm.isMorningStoryPlaying;

        Debug.Log($"[MainPanel] RefreshActionButtons: Day={currentDay}, isMorningStory={isMorningStory}, isDay1={isDay1}");

        // 控制地图按钮：Day 1 禁用，Day 2+ 启用，剧情播放时禁用
        if (btn_map == null)
        {
            // 兜底：如果 Init 没找着，尝试直接按名字找
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b.name == "btn_map")
                {
                    btn_map = b;
                    break;
                }
            }
        }

        if (btn_map != null)
        {
            // 重写地图按钮控制逻辑：
            // 1. 第一天强制禁用。
            // 2. 第二天剧情播放时禁用，剧情结束后启用。
            // 3. 从第三天开始，地图按钮始终保持开启状态，不受剧情锁干扰。
            bool shouldEnableMap;
            if (currentDay == 1)
            {
                shouldEnableMap = false;
            }
            else if (currentDay == 2)
            {
                shouldEnableMap = !isMorningStory;
            }
            else
            {
                // 第三天及以后，地图永远亮起
                shouldEnableMap = true;
            }

            btn_map.interactable = shouldEnableMap;
            Debug.Log($"[MainPanel] 设置地图按钮状态: {shouldEnableMap} (currentDay={currentDay}, isMorningStory={isMorningStory})");
        }
        else
        {
            Debug.LogWarning("[MainPanel] 未找到 btn_map 引用！");
        }

		if (img_ma != null)
		{
            bool isAtRanch = string.IsNullOrEmpty(mapName) || mapName.Trim() == "牧场";
            img_ma.gameObject.SetActive(isAtRanch);

            if (isAtRanch)
            {
                // 彻底透明化父 Image，仅将其作为容器，不渲染白色背景
                img_ma.color = Color.clear;
                img_ma.enabled = true; // 保持启用以显示子预制体，但颜色设为透明

                // --- 进化状态机切换逻辑 ---
                // 根据天数确定目标预制体
                GameObject targetPrefab = null;
                if (currentDay == 4) targetPrefab = pfb_ma_small_dry;
                else if (currentDay >= 5) targetPrefab = pfb_ma_dry;
                else targetPrefab = pfb_ma_normal;

                // 统一管理实例化逻辑
                if (targetPrefab != null)
                {
                    // 如果当前显示的物体不对，则重置所有子物体并重新实例化
                    bool needsRefresh = currentHorseObj == null || currentHorseObj.name != targetPrefab.name + "(Clone)";
                    if (needsRefresh)
                    {
                        Debug.Log($"[HorseLogic] 正在切换马匹进化形态: {targetPrefab.name} (Day {currentDay})");
                        // 清除所有旧的马
                        for (int i = img_ma.transform.childCount - 1; i >= 0; i--)
                        {
                            Transform child = img_ma.transform.GetChild(i);
                            // 只有带有 (Clone) 标记或者是我们创建的实例才删除，防止误删编辑器中手动挂载的其他组件
                            if (child.name.Contains("(Clone)") || child.gameObject == currentHorseObj)
                            {
                                Destroy(child.gameObject);
                            }
                        }
                        
                        // 创建新的马
                        currentHorseObj = Instantiate(targetPrefab, img_ma.transform);
                        currentHorseObj.name = targetPrefab.name + "(Clone)";
                        SetupUIObj(currentHorseObj);
                    }
                }
                else
                {
                    // 兜底逻辑：如果连基础预制体都没挂载，恢复 Image 显示以确保不留黑洞
                    img_ma.color = Color.white;
                    img_ma.enabled = true;
                }
            }
		}
		GroundManager groundManager = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		if (groundManager != null)
		{
			groundManager.UpdateChickenSprites();
		}
		bool showGroundPanel = false;
		if (!string.IsNullOrEmpty(mapName))
		{
			string trimmedName = mapName.Trim();
			showGroundPanel = trimmedName == "农场" || trimmedName == "你的农场" || trimmedName == "养鸡场";
		}
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		if (uiManager != null)
		{
			if (showGroundPanel)
			{
				uiManager.OpenPanelIgnoreToggle("GroundPanel");
			}
			else
			{
				uiManager.HidePanel("GroundPanel");
			}
		}
		UIManager panelManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		if (panelManager != null)
		{
			if (mapName != null && mapName.Trim() == "杂货铺")
			{
				panelManager.HidePanel("GroundPanel");
				panelManager.OpenPanelIgnoreToggle("StorePanel");
			}
			else
			{
				panelManager.HidePanel("StorePanel");
			}
		}
		string[] labels;
		if (mapName == null || mapActionLabels.TryGetValue(mapName.Trim(), out labels) == false)
		{
			labels = mapActionLabels["牧场"];
		}

		for (int i = 0; i < actionButtons.Count; i++)
		{
			Button btn = actionButtons[i];
			btn.transform.DOKill(); // 重置动画
			btn.transform.localScale = Vector3.one; // 重置缩放
			Text txt = i < actionTexts.Count ? actionTexts[i] : null;
			if (i < labels.Length)
			{
				btn.gameObject.SetActive(true);
				// 禁用键盘导航，防止空格/回车误触
				Navigation nav = new Navigation();
				nav.mode = Navigation.Mode.None;
				btn.navigation = nav;

				string actionName = labels[i];
                // 第三天海边特殊处理：钓鱼按钮文字变更为“洗澡”
                if (isDay3 && mapName.Trim() == "海边" && actionName == "钓鱼")
                {
                    actionName = "洗澡";
                }

				if (txt != null) { txt.text = actionName; }
				btn.onClick = new Button.ButtonClickedEvent();
				UnityAction handler;

				if (actionHandlers.TryGetValue(actionName, out handler) || actionName == "洗澡")
				{
                    // 1. 剧情锁基础逻辑：如果是早晨剧情期间，全部禁用
                    if (isMorningStory)
                    {
                        btn.interactable = false;
                    }
                    else
                    {
                        // 0. 特殊解锁逻辑：如果 ID 145 已经触发，无视天数限制全部开启
                        if (gm != null && gm.isSpecialUnlockTriggered)
                        {
                            btn.interactable = true;
                            if (actionName == "赛马")
                            {
                                btn.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                            }
                        }
                        else
                        {
                            // 2. 根据天数应用特定规则
                            switch (currentDay)
                            {
                                case 1:
                                    if (actionName == "赛马") btn.interactable = false;
                                    else if (actionName == "睡觉") btn.interactable = isHayEmpty;
                                    else btn.interactable = true;
                                    break;

                                case 2:
                                    if (actionName == "赛马") btn.interactable = false;
                                    else if (actionName == "睡觉")
                                    {
                                        bool hasChickenMeat = am != null && am.AssetCount(1100005) > 0;
                                        btn.interactable = hasChickenMeat;
                                        if (hasChickenMeat)
                                        {
                                            btn.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                                        }
                                    }
                                    else btn.interactable = true;
                                    break;

                                case 3:
                                    if (actionName == "赛马") btn.interactable = false;
                                    else if (actionName == "睡觉")
                                    {
                                        bool isDay3StoryDone = gm != null && gm.isDay3FinalStoryFinished;
                                        btn.interactable = isDay3StoryDone;
                                        if (isDay3StoryDone)
                                        {
                                            btn.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                                        }
                                    }
                                    else btn.interactable = true;
                                    break;

                                case 4:
                                    if (actionName == "赛马") btn.interactable = false;
                                    else if (actionName == "睡觉")
                                    {
                                        bool isDay4StoryDone = gm != null && gm.isDay4FinalStoryFinished;
                                        btn.interactable = isDay4StoryDone;
                                        if (isDay4StoryDone)
                                        {
                                            btn.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                                        }
                                    }
                                    else btn.interactable = true;
                                    break;

                                case 5:
                                    // 第五天：赛马依然禁用，其他全部开启
                                    if (actionName == "赛马") btn.interactable = false;
                                    else btn.interactable = true;
                                    break;

                                default: // 第六天及以后
                                    // 第六天及以后：所有按钮开启，但睡觉按钮永久禁用
                                    if (actionName == "睡觉")
                                    {
                                        btn.interactable = false;
                                    }
                                    else
                                    {
                                        btn.interactable = true;
                                        // 赛马按钮增加呼吸提示
                                        if (actionName == "赛马")
                                        {
                                            btn.transform.DOScale(1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    // 3. 绑定点击回调
                    if (actionName == "洗澡")
                    {
                        btn.onClick.AddListener(() => {
                            Debug.Log("[MainPanel] 触发洗澡剧情: ID 45");
                            if (gm != null) gm.isBathStoryPlaying = true;
                            ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 45);
                        });
                    }
                    else if (handler != null)
                    {
                        btn.onClick.AddListener(handler);
                    }
				}
				else
				{
					btn.interactable = false;
				}
			}
			else
			{
				btn.gameObject.SetActive(false);
			}
		}
	}
	private string GetCurrentMapName()
	{
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		if (slm != null && slm.mapCA != null)
		{
			return slm.mapCA.name.Trim();
		}
		return null;
	}
	public void Hoe()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("时间不足1小时");
			return;
		}
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		int num = gm.HoeGround(1);

		ggm.CostTime(num);
	}
	public void Water()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("时间不足1小时");
			return;
		}
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		int num = gm.WaterGround(1);
		ggm.CostTime(num);
	}
	public void Plant()
	{
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am.CheckAsset(1100002, 1) == false) { TipManager.Tip("种子不足1"); return; }
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		// 此处应该弹出菜单，选择一个需要种植的物品
		int num = gm.Plant(1, 1100002);
		if (num > 0)
		{
			am.Add(1100002, -1);
			TipManager.Tip("播种成功");
		}
		else
		{
			TipManager.Tip("没有可播种的土地");
		}
	}
	// 静态变量，只在本次游戏运行期间有效，重启游戏后重置
	private static bool hasShownFirstFeedDialog = false;
    private static bool hasShownFeedGrassDepletedTip = false;

    public void Feed() {
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		
		// 第一天和第二天逻辑：消耗牧草
		if (gm != null && (gm.day == 1 || gm.day == 2))
		{
			if (am.CheckAsset(1100003, 1) == false) { TipManager.Tip("牧草不足1"); return; }

			// 触发第一天第一次喂干草的剧情对话 (ID 13)
			if (gm.day == 1 && !hasShownFirstFeedDialog)
			{
				Debug.Log("[MainPanel] 触发第一次喂食剧情: ID 13");
				hasShownFirstFeedDialog = true;
				ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 13);
			}

			int beforeCount = am.AssetCount(1100003);
			am.Add(1100003, -1);
			int afterCount = am.AssetCount(1100003);
			if (beforeCount > 0 && afterCount == 0 && gm.day == 1)
			{
				ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 15);
				RefreshActionButtons();

				if (!hasShownFeedGrassDepletedTip)
				{
					hasShownFeedGrassDepletedTip = true;
					UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
					DialogPanel dialog = uiManager != null ? uiManager.OpenFloat("DialogPanel") as DialogPanel : null;
					if (dialog != null)
					{
						dialog.ShowSimple("少女", "干草喂完了，现在去睡觉吧", 1200001);
					}
				}
			}
            else if (gm.day == 2)
            {
                TipManager.Tip("马儿勉强吃下了牧草...");
            }

            LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
            if (levelManager != null)
            {
                levelManager.AddExp(20);
                TipManager.Tip("Exp+20!");
            }
		}
		// 第三天及以后逻辑：消耗红肉糜 (ID 1100005)
		else
		{
			if (am.CheckAsset(1100005, 1) == false) { TipManager.Tip("红肉糜不足1"); return; }
			
			am.Add(1100005, -1);
			TipManager.Tip("马儿狼吞虎咽地吃下了红肉糜...");
            
            LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
            if (levelManager != null)
            {
                levelManager.AddExp(70);
                TipManager.Tip("Exp+70!");
            }
		}
    }
	public void FeedChicken()
	{
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am.CheckAsset(1100002, 1) == false) { TipManager.Tip("种子不足1"); return; }
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		if (gm != null && gm.FeedHungryChicken())
		{
			am.Add(1100002, -1);
			TipManager.Tip("喂饱一只鸡");
		}
		else
		{
			TipManager.Tip("没有饥饿的鸡");
		}
	}
	public void KillChicken()
	{
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		if (gm != null && gm.KillChicken())
		{
			AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
			am.Add(1100005, 1);
			TipManager.Tip("获得1个鸡肉");

			// 屏幕红光一闪
			UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			TransitionPanel tp = uiManager.OpenFloat("TransitionPanel") as TransitionPanel;
			if (tp != null)
			{
				tp.FlashRed(0.3f);
			}

			// 播放 ID 为 27 的剧情对话
			ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 27);

			// UI 效果：喂食按钮变灰，睡觉按钮急促呼吸
			for (int i = 0; i < actionButtons.Count; i++)
			{
				if (i >= actionTexts.Count) continue;
				string actionName = actionTexts[i].text;
				if (actionName == "喂鸡")
				{
					actionButtons[i].interactable = false;
				}
				else if (actionName == "睡觉")
				{
					actionButtons[i].interactable = true;
					// 停止旧动画（如果有）
					actionButtons[i].transform.DOKill();
					// 开始急促呼吸效果 (Scale 1.0 -> 1.1)
					actionButtons[i].transform.DOScale(1.1f, 0.3f)
						.SetLoops(-1, LoopType.Yoyo)
						.SetEase(Ease.InOutSine);
				}
			}
		}
		else
		{
			TipManager.Tip("没有鸡可以杀");
		}
	}
	public void Mine()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("时间不足1小时");
			return;
		}
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		am.Add(1100007, 1);
		ggm.CostTime(1);
		TipManager.Tip("获得1个矿物");
	}
	public void Fish()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("时间不足1小时");
			return;
		}
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		am.Add(1100008, 1);
		ggm.CostTime(1);
		TipManager.Tip("获得1条鱼");
	}
    public void Treat()
    {
        Debug.Log("[MainPanel] 触发医院治疗剧情: ID 54");
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm != null) gm.isHospitalStoryPlaying = true;
        ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 54);
    }
	public bool ismatchlocked = false;
    public void Match() {
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm.day < 3)
        {
            TipManager.Tip("还没有人给我指过去赛马场的路呢");
            return;
        }
        SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
        slm.Load("match");
    }

    public void Sleep() {
        if (GameManager.isTransitioning) return; // 防止重复触发
        
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm != null && gm.day >= 6)
        {
            TipManager.Tip("赛马的日子不能睡过头了");
            return;
        }

        Debug.Log("[MainPanel] Sleep 按钮被手动点击");
        // 显式开启锁，允许进入下一天
        GameManager.isSleepClicked = true;
		gm.NextDayDailog();
	}
	public void RefreshBeg()
	{
		AudioManager.Inst.Play("BGM/失去道具");
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		AssetFactory af = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
		foreach (var item in begItem)
		{
			item.Value.isRefresh = false;
		}
		foreach (var item in gm.bag)
		{
			BegItem bitem = null;
			if (begItem.ContainsKey(item.Key))
			{
				bitem = begItem[item.Key];

			}
			else
			{
				AssetCA ac = af.GetCA(item.Key) as AssetCA;
				GameObject item_beg = GameObject.Instantiate(go_item, tran_item);
				bitem = item_beg.GetComponent<BegItem>();
				bitem.img_icon.sprite = Resources.Load<Sprite>(ac.respath);
				bitem.lbl_name.text = ac.name;
				bitem.lbl_num.text = item.Value.ToString();
				begItem.Add(item.Key, bitem);
			}
			if (item.Value > 0)
			{
				bitem.isRefresh = true;
				bitem.gameObject.SetActive(true);
				bitem.lbl_num.text = item.Value.ToString();
			}
			else
			{
				bitem.gameObject.SetActive(false);
				bitem.isRefresh = true;
			}
		}
		foreach (var item in begItem)
		{
			if (item.Value.isRefresh == false)
			{
				item.Value.gameObject.SetActive(false);
			}
		}
	}
}

using DG.Tweening.Core.Easing;
using RG.Zeluda;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace RG.Zeluda
{
	public class GameManager : ManagerBase
	{
		[Serializable]
		private class SaveItem
		{
			public int id;
			public int cnt;
		}
		[Serializable]
		private class SaveData
		{
			public int day;
			public int time;
			public int roomIdx;
			public int nextDayMapID;
			public int matchWinStreak;
			public bool matchStreakDialogShown;
			public int storyStage;
			public bool firstRaceWin;
			public bool pendingLiMorning;
			public bool clueCollectionEnabled;
			public bool pendingStage3Morning;
			public bool storyEnded;
			public bool pendingStartRanchTutorial;
			public List<int> collectedClues;
			public List<SaveItem> bag;
			public List<SaveItem> assets;
			public int level;
			public int exp;
		}
		public enum StoryStage
		{
			Stage1 = 1,
			Stage2 = 2,
			Stage3 = 3,
			Ended = 4
		}
		public Dictionary<int, int> bag = new Dictionary<int, int>();

		public int day = 0;
		public int time = 12;
		public int max_time = 24;
		public int start_time = 18; // 18 对应早上 6 点 (24 - 18 = 6)
		public int over_time = 3;
		public Transform uiManager;
		public WorkCA work;

		public int[] prisonRooms = new int[4] { 1800004, 1800002, 1800003, 1800005 };
		public int roomIdx = 0;
		public int nextDayMapID = 0;
		public int matchWinStreak = 0;
		public bool matchStreakDialogShown = false;
		private AudioClip horseLevelUpClip;
		public StoryStage storyStage = StoryStage.Stage1;
		public bool firstRaceWin = false;
		public bool pendingLiMorning = false;
		public bool clueCollectionEnabled = false;
		public bool pendingStage3Morning = false;
		public bool storyEnded = false;
		public bool pendingStartRanchTutorial = false;
        public bool isMorningStoryPlaying = false; // 增加标志位，防止 UI 按钮状态冲突
        public bool isBathStoryPlaying = false; // 第三天特殊逻辑：是否正在洗澡剧情中
        public bool isBathStoryFinished = false; // 第三天特殊逻辑：是否洗完澡（ID 45 结束）
        public bool isHospitalStoryPlaying = false; // 第三天特殊逻辑：是否正在医院剧情中
        public bool isDay3FinalStoryPlaying = false; // 第三天特殊逻辑：是否正在进行 ID 74 - 90 剧情
        public bool isDay3FinalStoryFinished = false; // 第三天特殊逻辑：ID 90 结束后解锁睡觉
        public bool isDay4BuyStoryPlaying = false; // 第四天特殊逻辑：是否正在购买红肉糜剧情中 (ID 111)
        public bool isDay4PostBuyStoryPlaying = false; // 第四天特殊逻辑：是否正在购买后回到牧场的剧情中 (ID 113)
        public bool isDay4FinalStoryFinished = false; // 第四天特殊逻辑：ID 113 结束后解锁睡觉
        public bool isDay5MorningStoryPlaying = false; // 第五天特殊逻辑：是否正在播放 ID 122
        public bool isDay5StoryFinished = false; // 第五天特殊逻辑：ID 122 剧情结束标志
        public bool isSpecialUnlockTriggered = false; // 新增：是否已触发 ID 145 的特殊解锁逻辑
        public bool isFinalStoryPlaying = false; // 结局特殊逻辑：是否正在播放最终对话 (ID 152+)
        public bool isFinalStoryFinished = false; // 结局特殊逻辑：结局对话是否已播完
        public static bool isTransitioning = false; // 增加静态标志，防止连点导致跳天
        public static bool isSleepClicked = false; // 新增：显式锁，只有点击睡觉按钮才允许进入下一天
		public GameObject NewStorySystemPrefab; // 保存来自 LobbyPanel 的剧情系统预制体
		private HashSet<int> collectedClues = new HashSet<int>();
		private Dictionary<int, string> mapClueTexts = new Dictionary<int, string>();
		private Dictionary<int, string> dayTitles = new Dictionary<int, string>();
		private string[] kaneEncouragements = new string[0];
		private const float clueTriggerChance = 0.35f;
		private SaveData pendingSaveData;
		private bool pendingSaveExists;
		public void PrepareStartGame()
		{
            // 彻底清除注册表（PlayerPrefs），确保没有任何持久化数据干扰
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[GameManager] 已彻底清除 PlayerPrefs 数据。");

			// 确保订阅了剧情事件
			ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_START, OnNewStoryStart);
			ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_START, OnNewStoryStart);
			ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, OnNewStoryEnd);
			ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, OnNewStoryEnd);

			// 不再加载存档，每次都开启新游戏
			pendingSaveData = BuildNewGameSaveData();
			pendingSaveExists = false;
		}
		public void StartFromLobby()
		{
			Debug.Log("[GameManager] StartFromLobby 被触发...");
            // 每次从大厅开始都强制清理一遍，双重保险
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[GameManager] StartFromLobby: 已清理 PlayerPrefs");
			if (pendingSaveData == null)
			{
				Debug.Log("[GameManager] 未发现待加载存档数据，调用 PrepareStartGame...");
				PrepareStartGame();
			}

			// 无论是否有存档，统一使用 TransitionPanel 进行背景切换
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			TransitionPanel tp = um.OpenFloat("TransitionPanel") as TransitionPanel;
			if (tp == null)
			{
				Debug.LogError("[GameManager] 无法加载 TransitionPanel，强制执行后续逻辑...");
				ExecuteLobbyTransition();
				return;
			}

			Debug.Log("[GameManager] 开始 Lobby 到游戏场景的黑屏过渡...");
			tp.StartTransition(() =>
			{
				ExecuteLobbyTransition();
				pendingSaveData = null;
				pendingSaveExists = false;
			}, "", () => {
				// 屏幕变亮后，如果是第一天且是新游戏流程，触发新剧情系统 ID 0
				Debug.Log($"[GameManager] 过渡完成，当前天数: {day}, 剧情阶段: {storyStage}");
				if (day == 1 && storyStage == StoryStage.Stage1)
				{
					TriggerOpeningStory();
				}
			});
		}

		private void ExecuteLobbyTransition()
		{
			// 既然不持久化，每次都是开启新游戏
			Debug.Log("[GameManager] 不再加载存档，开启新游戏状态...");
			StartNewGame();
		}

		private void TriggerOpeningStory()
		{
			Debug.Log("[GameManager] 正在启动开场剧情系统 (ID 0)...");
			if (NewStorySystemPrefab != null)
			{
				Debug.Log("[GameManager] 正在实例化 NewStorySystemPrefab...");
				GameObject storySystem = GameObject.Instantiate(NewStorySystemPrefab);
				GameObject.DontDestroyOnLoad(storySystem);
			}
			else
			{
				Debug.LogWarning("[GameManager] NewStorySystemPrefab 为空，尝试直接发布 DIALOG_START 事件...");
				ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 0);
			}
		}

		protected override void Init()
		{
			base.Init();
			// 订阅剧情事件
			ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_START, OnNewStoryStart);
			ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, OnNewStoryEnd);
		}

		private void OnNewStoryStart(object eventData)
		{
			int dialogId = 0;
			if (eventData is int) dialogId = (int)eventData;
			else if (eventData is string strData && int.TryParse(strData, out int parsedId)) dialogId = parsedId;

			// 特殊逻辑：对话 ID 136 开始时播放“叫声”
			if (dialogId == 136)
			{
				Debug.Log("[GameManager] 监听到对话 ID 136 开始，播放叫声音效");
				if (AudioManager.Inst != null)
				{
					AudioClip clip = Resources.Load<AudioClip>("BGM/叫声");
					if (clip != null)
					{
						AudioManager.Inst.Play(clip);
					}
					else
					{
						Debug.LogWarning("[GameManager] 找不到音效资源: BGM/叫声");
					}
				}
			}
		}

		private void OnNewStoryEnd(object eventData)
		{
			int dialogId = 0;
			if (eventData is int) dialogId = (int)eventData;
			else if (eventData is string strData && int.TryParse(strData, out int parsedId)) dialogId = parsedId;

			Debug.Log($"[GameManager] OnNewStoryEnd 监听到对话结束, ID: {dialogId}, 当前天数: {day}, 剧情阶段: {storyStage}");

			// 只有在游戏初始阶段且 ID 12 结束时触发凯恩对话
			if (dialogId == 12 && day <= 1 && storyStage == StoryStage.Stage1)
			{
				Debug.Log("[GameManager] 满足凯恩对话触发条件，正在弹出对话框...");
				UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
				DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
				// 显式清除之前的回调，防止触发旧的 NextDay 逻辑导致自动睡觉
				dp.OnCallback = null;
				// 播放凯恩对话
				dp.StartDialog("第2日起床");
			}

            // 第三天特殊逻辑：洗澡对话 (ID 45) 结束后，标记并弹出医院开放提示
            // 无论 ID 是多少，只要是正在播放洗澡剧情中结束了，就触发解锁
            if (isBathStoryPlaying && day == 3)
            {
                Debug.Log($"[GameManager] 洗澡对话结束(ID:{dialogId})，解锁医院场景。");
                isBathStoryPlaying = false;
                isBathStoryFinished = true;
                TipManager.Tip("医院开放了!");
                
                // 刷新主界面按钮状态
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                if (main != null) main.RefreshActionButtons();
            }

            // 第三天特殊逻辑：医院治疗对话 (ID 54) 结束后，触发过渡回到牧场并播放 ID 74
            if (isHospitalStoryPlaying && day == 3)
            {
                Debug.Log($"[GameManager] 医院治疗对话结束(ID:{dialogId})，准备回到牧场。");
                isHospitalStoryPlaying = false;
                
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                TransitionPanel tp = um.OpenFloat("TransitionPanel") as TransitionPanel;
                if (tp != null)
                {
                    tp.StartTransition(() => {
                        // 1. 切换回牧场
                        roomIdx = 0;
                        SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
                        if (slm != null)
                        {
                            slm.Load(prisonRooms[roomIdx]);
                        }
                        
                        // 2. 在黑屏状态下触发 ID 74 剧情
                        Debug.Log("[GameManager] 回到牧场过渡中，触发 ID 74 剧情");
                        isDay3FinalStoryPlaying = true; // 开启终场剧情追踪
                        isMorningStoryPlaying = true; // 锁定 UI
                        ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 74);
                    }, "回到牧场...");
                }
            }

            // 第三天特殊逻辑：ID 90 剧情结束后解锁睡觉
            // 更加鲁棒的判断：只要在 Day3 终场剧情进行中结束，就尝试解锁
            if ((dialogId == 90 || isDay3FinalStoryPlaying) && day == 3)
            {
                Debug.Log($"[GameManager] 第三天终场剧情结束(ID:{dialogId})，解锁睡觉按钮。");
                isDay3FinalStoryPlaying = false;
                isMorningStoryPlaying = false; // 解锁 UI
                isDay3FinalStoryFinished = true;
                
                // 刷新 UI 状态
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                if (main != null) main.RefreshActionButtons();
            }

            // 第四天特殊逻辑：购买红肉糜对话 (ID 111) 结束后，自动回到牧场并触发 ID 113
            if (isDay4BuyStoryPlaying && day == 4)
            {
                Debug.Log($"[GameManager] 第四天购买红肉糜对话结束(ID:{dialogId})，准备回到牧场。");
                isDay4BuyStoryPlaying = false;
                isMorningStoryPlaying = true; // 保持锁定 UI 直到 ID 113 结束
                
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                TransitionPanel tp = um.OpenFloat("TransitionPanel") as TransitionPanel;
                if (tp != null)
                {
                    tp.StartTransition(() => {
                        // 切换回牧场
                        roomIdx = 0;
                        SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
                        if (slm != null)
                        {
                            slm.Load(prisonRooms[roomIdx]);
                        }
                    }, "回到牧场...", () => {
                        // 回到牧场后，自动播放 ID 113 对话
                        Debug.Log("[GameManager] 回到牧场完成，触发 ID 113 剧情");
                        isDay4PostBuyStoryPlaying = true;
                        ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 113);
                    });
                }
            }

            // 第四天特殊逻辑：ID 113 剧情结束后解锁睡觉
            if (isDay4PostBuyStoryPlaying && day == 4)
            {
                Debug.Log($"[GameManager] 第四天终场剧情结束(ID:{dialogId})，解锁睡觉按钮。");
                isDay4PostBuyStoryPlaying = false;
                isMorningStoryPlaying = false; // 解锁 UI
                isDay4FinalStoryFinished = true;
                
                // 刷新 UI 状态
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                if (main != null) main.RefreshActionButtons();
            }

            // 第五天特殊逻辑：ID 122 剧情结束后刷新 UI
            if (isDay5MorningStoryPlaying && day == 5)
            {
                Debug.Log($"[GameManager] 第五天晨间剧情结束(ID:{dialogId})");
                isDay5MorningStoryPlaying = false;
                isMorningStoryPlaying = false; // 解锁 UI
                isDay5StoryFinished = true;
                
                // 刷新 UI 状态
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                if (main != null) main.RefreshActionButtons();
            }

            // 特殊逻辑：对话 ID 145 结束后解锁所有按钮和场景
            if (dialogId == 145)
            {
                Debug.Log("[GameManager] 监听到对话 ID 145 结束，触发全按钮/全场景解锁");
                isSpecialUnlockTriggered = true;
                
                // 立即刷新当前界面的 UI 状态
                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                if (main != null) main.RefreshActionButtons();
            }

            // 结局特殊逻辑：最终对话结束后淡出回到牧场
            if (isFinalStoryPlaying)
            {
                Debug.Log("[GameManager] 最终结局对话结束，正在淡出返回牧场...");
                isFinalStoryPlaying = false;
                isFinalStoryFinished = true; // 标记结局已完成

                UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                if (um != null)
                {
                    // 对话结束，恢复天数显示
                    MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                    if (main != null) main.ToggleDayDisplay(true);

                    TransitionPanel tp = um.GetPanel("TransitionPanel") as TransitionPanel;
                    if (tp != null)
                    {
                        // 调用新增的 FadeOut 方法，平滑恢复场景亮度
                        tp.FadeOut(() => {
                            Debug.Log("[GameManager] 结局淡出完成，玩家已回到牧场视角");
                        });
                    }
                }
            }
		}

		private void StartNewGame()
		{
			Debug.Log("[GameManager] StartNewGame 被调用...");
			InitStoryData();
			ResetMapTutorialFlags();
			InitDayCountUI();
			day = 1;
			time = 18; // 强制从 6 点开始 (24 - 18 = 6)
			roomIdx = 0;
			nextDayMapID = 0;
			matchWinStreak = 0;
			matchStreakDialogShown = false;
			storyStage = StoryStage.Stage1;
			firstRaceWin = false;
			pendingLiMorning = false;
			clueCollectionEnabled = false;
			pendingStage3Morning = false;
            isBathStoryFinished = false;
            isFinalStoryFinished = false;
			storyEnded = false;
			pendingStartRanchTutorial = false;
			bag = new Dictionary<int, int>();
			collectedClues = new HashSet<int>();

			SetupAfterLobby(true);
			SaveToDisk(BuildCurrentSaveData());
		}

		private void StartFromSave(SaveData data)
		{
			Debug.Log("[GameManager] StartFromSave 被调用...");
			InitStoryData();
			InitDayCountUI();
			ApplySaveData(data);
			SetupAfterLobby(false);
		}

		private void SetupAfterLobby(bool isNewGame)
		{
			Debug.Log($"[GameManager] SetupAfterLobby(isNewGame: {isNewGame}) 开始执行...");
			
            // 开启背景音乐
            if (AudioManager.Inst != null)
            {
                AudioManager.Inst.PlayBGM("BGM/MainBGM", 0.15f);
            }

			// 先处理资源初始化，确保 MainPanel 打开时资源数量正确
			AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
			if (isNewGame && am != null)
			{
                am.RemoveCoin(am.AssetCount(1100001)); // 清零（如果有默认值）
                am.GetCoin(100); // 初始给 100 钱币
				am.Add(1100002, 10);
				am.Add(1100003, 3);
			}

			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			um.ClosePanel("LobbyPanel");
			MainPanel main = um.OpenPanel("MainPanel") as MainPanel;
			LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
			levelManager.OnExpChanged += (c, m) =>
			{
                if (main == null || main.gameObject == null || main.img_exp == null) return;
				main.img_exp.fillAmount = c * 1f / m;
				main.text_exp.text = $"{c}/{m}";
			};
			levelManager.OnLevelUp += (l) =>
			{
                if (main == null || main.gameObject == null || main.lbl_lv == null) return;
				main.lbl_lv.text = $"Lv.{l}";
				if (l == 2 || l == 3 || l == 5 || l == 7)
				{
					TipManager.Tip("马儿学会了新的技能！");
				}
				if (AudioManager.Inst != null)
				{
					AudioManager.Inst.Play(GetHorseLevelUpClip());
				}
			};
			levelManager.updateexp();
			main.SetDay(day);
			main.SetTime(time);
			main.InitClock();
			main.RefreshBeg();
			GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
			gm.BuildGround(9);
			MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
			if (mf != null)
			{
				MapCA mapCa = mf.GetCA(prisonRooms[roomIdx]) as MapCA;
				TryShowMapTutorial(mapCa);
			}
		}
		private SaveData BuildNewGameSaveData()
		{
			SaveData data = new SaveData();
			data.day = 1;
			data.time = start_time;
			data.roomIdx = 0;
			data.nextDayMapID = 0;
			data.matchWinStreak = 0;
			data.matchStreakDialogShown = false;
			data.storyStage = (int)StoryStage.Stage1;
			data.firstRaceWin = false;
			data.pendingLiMorning = false;
			data.clueCollectionEnabled = false;
			data.pendingStage3Morning = false;
            // data.isBathStoryFinished = false; // SaveData 结构里目前没加这个，因为不持久化所以暂时不加也没关系
			data.storyEnded = false;
			data.pendingStartRanchTutorial = false;
			data.collectedClues = new List<int>();
			data.bag = new List<SaveItem>();
			data.assets = new List<SaveItem>
			{
				new SaveItem { id = 1100002, cnt = 10 },
				new SaveItem { id = 1100003, cnt = 3 }
			};
			data.level = 1;
			data.exp = 0;
			return data;
		}
		private SaveData BuildCurrentSaveData()
		{
			SaveData data = new SaveData();
			data.day = day;
			data.time = time;
			data.roomIdx = roomIdx;
			data.nextDayMapID = nextDayMapID;
			data.matchWinStreak = matchWinStreak;
			data.matchStreakDialogShown = matchStreakDialogShown;
			data.storyStage = (int)storyStage;
			data.firstRaceWin = firstRaceWin;
			data.pendingLiMorning = pendingLiMorning;
			data.clueCollectionEnabled = clueCollectionEnabled;
			data.pendingStage3Morning = pendingStage3Morning;
			data.storyEnded = storyEnded;
			data.pendingStartRanchTutorial = pendingStartRanchTutorial;
			data.collectedClues = collectedClues != null ? new List<int>(collectedClues) : new List<int>();
			data.bag = new List<SaveItem>();
			if (bag != null)
			{
				foreach (var item in bag)
				{
					data.bag.Add(new SaveItem { id = item.Key, cnt = item.Value });
				}
			}
			data.assets = new List<SaveItem>();
			AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
			if (am != null && am.assetDic != null)
			{
				foreach (var item in am.assetDic)
				{
					data.assets.Add(new SaveItem { id = item.Key, cnt = item.Value });
				}
			}
			LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
			if (levelManager != null)
			{
				data.level = levelManager.Level;
				data.exp = levelManager.CurrentExp;
			}
			return data;
		}
		private void ApplySaveData(SaveData data)
		{
			if (data == null) { return; }
			day = data.day;
			time = data.time;
			roomIdx = data.roomIdx;
			nextDayMapID = data.nextDayMapID;
			matchWinStreak = data.matchWinStreak;
			matchStreakDialogShown = data.matchStreakDialogShown;
			storyStage = (StoryStage)data.storyStage;
			firstRaceWin = data.firstRaceWin;
			pendingLiMorning = data.pendingLiMorning;
			clueCollectionEnabled = data.clueCollectionEnabled;
			pendingStage3Morning = data.pendingStage3Morning;
			storyEnded = data.storyEnded;
			pendingStartRanchTutorial = data.pendingStartRanchTutorial;
			collectedClues = data.collectedClues != null ? new HashSet<int>(data.collectedClues) : new HashSet<int>();
			bag = new Dictionary<int, int>();
			if (data.bag != null)
			{
				foreach (var item in data.bag)
				{
					bag[item.id] = item.cnt;
				}
			}
			AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
			if (am != null)
			{
				am.assetDic = new Dictionary<int, int>();
				if (data.assets != null)
				{
					foreach (var item in data.assets)
					{
						am.assetDic[item.id] = item.cnt;
					}
				}
				am.isUpdate = true;
			}
			LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
			if (levelManager != null)
			{
				levelManager.SetState(data.level, data.exp);
			}
		}
		private string GetSavePath()
		{
			string folder = Path.Combine(Application.persistentDataPath, "Save");
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			return Path.Combine(folder, "game_save.json");
		}
		private SaveData LoadSave()
		{
			// 禁用存档读取
			return null;
		}
		private void SaveToDisk(SaveData data)
		{
			// 禁用存档保存
		}
		public int GetCurTime()
		{
			return max_time - time;
		}
		public void RefreshAll()
		{

		}

		private void InitDayCountUI()
		{
			GameObject go = GameObject.Find("DayCountUI");
			if (go == null)
			{
				go = new GameObject("DayCountUI");
				GameObject.DontDestroyOnLoad(go);
				go.AddComponent<DayCountHelper>();
			}
		}

		private class DayCountHelper : MonoBehaviour
		{
			private Text txt_day;
			private GameManager gm;

			void Start()
			{
				gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
				
				GameObject canvasObj = new GameObject("DayCanvas");
				canvasObj.transform.SetParent(transform, false);
				Canvas canvas = canvasObj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = 999;
				CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.referenceResolution = new Vector2(1920, 1080);
				canvasObj.AddComponent<GraphicRaycaster>();

				GameObject textObj = new GameObject("DayText");
				textObj.transform.SetParent(canvasObj.transform, false);
				txt_day = textObj.AddComponent<Text>();
				
				GameObject tipPrefab = Resources.Load<GameObject>("Prefab/tip");
				if (tipPrefab != null)
				{
					Text tipText = tipPrefab.GetComponentInChildren<Text>();
					if (tipText != null)
					{
						txt_day.font = tipText.font;
					}
				}
				if (txt_day.font == null)
				{
					txt_day.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}

				txt_day.fontSize = 40;
				txt_day.color = Color.white;
				txt_day.alignment = TextAnchor.UpperLeft;
				txt_day.horizontalOverflow = HorizontalWrapMode.Overflow;
				txt_day.verticalOverflow = VerticalWrapMode.Overflow;
				textObj.AddComponent<Outline>().effectColor = Color.black;

				RectTransform rt = textObj.GetComponent<RectTransform>();
				rt.anchorMin = new Vector2(0, 1);
				rt.anchorMax = new Vector2(0, 1);
				rt.pivot = new Vector2(0, 1);
				rt.anchoredPosition = new Vector2(500, -50);
				rt.sizeDelta = new Vector2(300, 100);
			}

			void Update()
			{
				if (gm != null && txt_day != null)
				{
					// 获取当前场景
					//string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
					// 如果是赛马场景（match），则隐藏文本
					UnityEngine.SceneManagement.Scene matchScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("match");
					if (matchScene.IsValid() && matchScene.isLoaded)
					{
						txt_day.enabled = false;
					}
					else
					{
						txt_day.enabled = true;
						txt_day.text = $"第 {gm.day} 天";
					}
				}
			}
		}

		public void PlayerInit()
		{
			//Money = StartData.inst.money;
			//playerAsset = new PlayerAsset();

			//foreach (var item in StartData.inst.item)
			//{
			//	string[] inum = item.Split(':');
			//	playerAsset.AddItem(int.Parse(inum[0]), int.Parse(inum[1]));
			//}
		}
		public void NextDayDailog()
		{
            if (isTransitioning) return;
            if (!isSleepClicked)
            {
                Debug.LogWarning("[GameManager] NextDayDailog 被调用，但 isSleepClicked 为 false。拒绝弹出对话框。");
                return;
            }

			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
            // 显式清空旧回调，防止残留
            dp.OnCallback = null; 

            if (day == 1)
            {
			    dp.StartDialog("NightDialog");
			    dp.OnCallback = NextDay;
                dp.OnCloseCallback = NextDay;
            }
            else
            {
			    dp.StartDialog("NightDialog");
			    dp.OnCallback = NextDay;
                dp.OnCloseCallback = NextDay;
            }
		}
		public void NextDay()
		{
            if (isTransitioning) return;
            if (!isSleepClicked)
            {
                Debug.LogWarning("[GameManager] NextDay 被调用，但 isSleepClicked 为 false。拒绝执行。");
                return;
            }

            isTransitioning = true;
            isSleepClicked = false; // 重置点击状态
            
            // 睡觉醒来后，强制重置场景到牧场 (prisonRooms[0])
            roomIdx = 0;
            SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
            if (slm != null)
            {
                slm.Load(prisonRooms[roomIdx]);
            }

            // 关键：进入新的一天时，默认开启剧情锁，防止 UI 按钮在剧情加载前瞬间亮起
            isMorningStoryPlaying = true;

            // 增加调用堆栈打印，以便排查是谁触发了自动睡觉
            Debug.Log($"[GameManager] NextDay 执行开始. Day: {day}, Time: {time}\nStackTrace: {System.Environment.StackTrace}");
			GroundManager gdm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
			gdm.DayEnd();

			TipManager.Tip("新的一天开始啦！");
			day++;
			time = start_time;
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			TransitionPanel tp = um.OpenFloat("TransitionPanel") as TransitionPanel;
			
			string title = "";
			if (dayTitles == null || dayTitles.Count == 0) { InitStoryData(); }
			if (dayTitles != null && dayTitles.ContainsKey(day))
			{
				title = dayTitles[day];
			}
			else
			{
				title = $"第{day}天：新的开始";
			}

            Debug.Log($"[GameManager] 开始天数切换过渡: {title}");
			tp.StartTransition(() =>
			{

				AudioManager.Inst.Play("BGM/新的一天开始");

				UIManager um1 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
				MainPanel main = um1.GetPanel("MainPanel") as MainPanel;
				main.SetDay(day);
				main.SetTime(time);
				main.RefreshActionButtons();
				AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
				GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
				if (am != null && gm != null)
				{
					int hatchCount = am.GetNonCurrentDayEggCount(day);
					if (hatchCount > 0)
					{
						gm.AddChickens(hatchCount);
						am.ConsumeNonCurrentDayEggs(day);
					}
					int eggCount = gm.GetFullChickenCount();
					if (eggCount > 0)
					{
						am.AddEggs(eggCount, day);
					}
					gm.SetAllChickenHungry();
				}
			}, title, () => 
			{
                isTransitioning = false; // 过渡完全结束后重置
				UIManager um1 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
				DailyFactory df = CBus.Instance.GetFactory(FactoryName.DailyFactory) as DailyFactory;
				DailyCA daily = df.GetCA(1400000 + day) as DailyCA;
				DialogPanel dp = um1.OpenFloat("DialogPanel") as DialogPanel;
				Action onMorningDialogClose = () =>
				{
					if (storyStage == StoryStage.Stage2 && pendingLiMorning)
					{
						pendingLiMorning = false;
						clueCollectionEnabled = true;
					}
                    
                    // 统一在剧情结束后刷新按钮状态
                    MainPanel main = um1.GetPanel("MainPanel") as MainPanel;
                    if (main != null)
                    {
                        main.RefreshActionButtons();
                    }

					if (day == 2)
					{
						TipManager.Tip("养鸡场开放了!");
					}
                    else if (day == 3)
                    {
                        TipManager.Tip("海边开放了!");
                    }
                    else if (day == 4)
                    {
                        // 第四天特殊逻辑：修改肉的信息
                        AssetFactory af = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
                        if (af != null)
                        {
                            AssetCA chickenMeat = af.GetCA(1100005) as AssetCA;
                            if (chickenMeat != null)
                            {
                                chickenMeat.name = "红肉糜";
                                chickenMeat.describe = "不知名家畜的混合肉，散发着甜腥味";
                                Debug.Log("[GameManager] 第四天：鸡肉已更名为红肉糜");
                            }
                        }
                        TipManager.Tip("杂货铺开放了!");
                    }
				};

                // 直接进入新的一天晨间逻辑，不再自动弹出 DailyCA 对话
				if (TryShowMorningStoryDialog(dp, onMorningDialogClose))
				{
					return;
				}
                // 如果没有晨间剧情，重置标志并执行关闭回调，确保 UI 状态刷新
                isMorningStoryPlaying = false;
				onMorningDialogClose();
                // 关键修复：如果没有晨间剧情，必须关闭 DialogPanel，否则会显示上一次的残留内容（即睡觉对话）
                dp.Close();
			});
		}
		public void SetNextAwake(int scene)
		{
			if (scene < 10)
			{
				roomIdx = Mathf.Clamp(roomIdx + scene, 0, prisonRooms.Length);
			}
			else
			{
				nextDayMapID = scene;
			}
		}
		public void GameOver()
		{

		}
		public void OnFirstRaceWin()
		{
			if (firstRaceWin) { return; }
			firstRaceWin = true;
			storyStage = StoryStage.Stage2;
			pendingLiMorning = true;
		}
		public void TryTriggerMapClue(MapCA mapCA)
		{
			if (storyEnded) { return; }
			if (storyStage != StoryStage.Stage2) { return; }
			if (clueCollectionEnabled == false) { return; }
			if (mapCA == null) { return; }
			if (mapClueTexts == null || mapClueTexts.Count == 0) { InitStoryData(); }
			if (mapClueTexts.ContainsKey(mapCA.id) == false) { return; }
			if (collectedClues.Contains(mapCA.id)) { return; }
			if (UnityEngine.Random.Range(0f, 1f) > clueTriggerChance) { return; }
			string text = mapClueTexts[mapCA.id];
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			if (um == null) { return; }
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
			dp.ShowSimple("少女", text, 1200001);
			dp.OnCloseCallback = () =>
			{
				collectedClues.Add(mapCA.id);
				if (collectedClues.Count >= mapClueTexts.Count)
				{
					storyStage = StoryStage.Stage3;
					pendingStage3Morning = true;
				}
			};
		}
		public bool TryShowMapTutorial(MapCA mapCA)
		{
			return false;
		}
		public void TriggerInitialRanchTutorial()
		{
			return;
		}
		public void TriggerInitialRanchTutorialIfPending()
		{
			if (!pendingStartRanchTutorial) { return; }
			pendingStartRanchTutorial = false;
			TriggerInitialRanchTutorial();
		}
		public void HandleDialogEnding(string dialogName, string endingDescription)
		{
			if (storyEnded) { return; }
			if (string.IsNullOrEmpty(endingDescription)) { return; }
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			if (um == null) { return; }
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
			if (endingDescription == "Ending.Kane")
			{
				storyStage = StoryStage.Ended;
				storyEnded = true;
				dp.ShowSimple("少女", "我接受了凯恩的条件。马儿赢得冠军，真相被我亲手封存。城里依旧繁华，只是少了些清澈。", 1200001);
			}
			else if (endingDescription == "Ending.Rebel")
			{
				storyStage = StoryStage.Ended;
				storyEnded = true;
				dp.ShowSimple("少女", "我赴约来到矿洞，父亲的朋友们把证据公之于众。赌马黑幕被揭开，我与马一起迎来真正的胜利。", 1200001);
			}
		}
		private bool TryShowMorningStoryDialog(DialogPanel dp, Action onStoryComplete)
		{
			InitStoryData();
			if (storyEnded) { return false; }
			
			// 第二天早晨触发特定的马匹异样剧情 (Excel ID 18)
			if (day == 2)
			{
                isMorningStoryPlaying = true;
				// 监听剧情结束事件，确保播完后再执行 onStoryComplete
				Action<object> onDialogEnd = null;
				onDialogEnd = (data) =>
				{
					int endId = (data is int) ? (int)data : 0;
					if (endId == 26) // ID 26 是第二天晨间剧情的结束标志
					{
						ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                        isMorningStoryPlaying = false;
						onStoryComplete?.Invoke();
					}
				};
				ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
				
				ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 18);
				return true;
			}

            // 第三天早晨触发特定的剧情 (Excel ID 40)
            if (day == 3)
            {
                isMorningStoryPlaying = true;
                Action<object> onDialogEnd = null;
                onDialogEnd = (data) =>
                {
                    // 更加健壮的判断：只要是剧情结束事件，且当前是第三天早晨，就尝试释放
                    ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                    Debug.Log($"[GameManager] 第三天晨间剧情结束 (EventData: {data})");
                    isMorningStoryPlaying = false;
                    onStoryComplete?.Invoke();
                };
                ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                
                ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 40);
                return true;
            }

            // 第四天早晨触发特定的剧情 (Excel ID 91 -> 结束)
            if (day == 4)
            {
                isMorningStoryPlaying = true;
                Action<object> onDialogEnd = null;
                onDialogEnd = (data) =>
                {
                    // 只要在 Day 4 晨间剧情状态下，且不是购买剧情，任何对话结束都视为解锁信号
                    if (!isDay4BuyStoryPlaying)
                    {
                        ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                        Debug.Log($"[GameManager] 第四天晨间对话结束，解锁地图");
                        isMorningStoryPlaying = false;
                        onStoryComplete?.Invoke();
                        
                        // 显式强制刷新一次 UI
                        UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                        MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                        if (main != null) main.RefreshActionButtons();
                    }
                };
                ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                
                ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 91);
                return true;
            }

            // 第五天早晨触发特定的剧情 (Excel ID 122)
            if (day == 5)
            {
                isMorningStoryPlaying = true;
                isDay5MorningStoryPlaying = true;
                
                // 监听剧情结束事件，确保播完后再执行 onStoryComplete
                Action<object> onDialogEnd = null;
                onDialogEnd = (data) =>
                {
                    // 只有在 Day 5 晨间剧情状态下，对话结束视为解锁信号
                    if (isDay5MorningStoryPlaying)
                    {
                        ExcelReading.Framework.StoryEventManager.Instance.Unsubscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                        Debug.Log($"[GameManager] 第五天晨间对话结束，准备解锁赛马");
                        // 逻辑已经在 OnNewStoryEnd 中处理，这里只负责关闭锁和刷新
                        isMorningStoryPlaying = false;
                        onStoryComplete?.Invoke();
                        
                        // 显式强制刷新一次 UI
                        UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
                        MainPanel main = um.GetPanel("MainPanel") as MainPanel;
                        if (main != null) main.RefreshActionButtons();
                    }
                };
                ExcelReading.Framework.StoryEventManager.Instance.Subscribe(ExcelReading.Framework.GameEventNames.DIALOG_END, onDialogEnd);
                
                ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 122);
                return true;
            }

			if (storyStage == StoryStage.Stage1 && firstRaceWin == false && day >= 6)
			{
				string text = GetRandomKaneEncouragement();
				dp.ShowSimple("市长凯恩", text, 1200002);
				dp.OnCloseCallback = () => {
                    isMorningStoryPlaying = false; // 剧情结束，解锁 UI
                    onStoryComplete?.Invoke();
                };
				return true;
			}
			if (storyStage == StoryStage.Stage2 && pendingLiMorning)
			{
				dp.ShowSimple("李北镇", "你能赢我不意外，但我也不太开心。你父亲当年的事，城里有些“地方”还留着线索，别被人牵着鼻子走。", 1200003);
				dp.OnCloseCallback = () => {
                    isMorningStoryPlaying = false; // 剧情结束，解锁 UI
                    onStoryComplete?.Invoke();
                };
				return true;
			}
			if (storyStage == StoryStage.Stage3 && pendingStage3Morning)
			{
				pendingStage3Morning = false;
				dp.StartDialog("阶段3_抉择之夜");
				dp.OnCallback = () => {
                    isMorningStoryPlaying = false; // 剧情结束，解锁 UI
                    onStoryComplete?.Invoke();
                };
				return true;
			}
			return false;
		}
		private string GetRandomKaneEncouragement()
		{
			InitStoryData();
			if (kaneEncouragements == null || kaneEncouragements.Length == 0) { return "保持耐心，马是靠日子养出来的。"; }
			int index = UnityEngine.Random.Range(0, kaneEncouragements.Length);
			return kaneEncouragements[index];
		}
		private string GetMapTutorialText(string mapName)
		{
			if (string.IsNullOrEmpty(mapName)) { return string.Empty; }
			if (mapName == "牧场")
			{
				return "这里是牧场，按下左边的按钮喂马，右上角有地图可以切换场景";
			}
			if (mapName == "农场" || mapName == "你的农场")
			{
				return "这里是农场，可以开垦耕地，播种种子，浇水\n成熟后会收获草料和更多的种子\n草料可以喂马，多余的种子可以在养鸡场喂鸡";
			}
			if (mapName == "养鸡场")
			{
				return "这里是养鸡场，可以用种子喂鸡\n鸡吃饱了会生蛋，蛋孵到第二天可以生鸡\n也可以把鸡杀了，在杂货铺里卖鸡肉";
			}
			if (mapName == "杂货铺")
			{
				return "这里是杂货铺，可以卖卖各种物品";
			}
			if (mapName == "矿洞")
			{
				return "这里是矿洞，可以挖矿，挖到的矿石可以去杂货铺里卖掉";
			}
			if (mapName == "海边")
			{
				return "这里是海边，可以钓鱼，钓到的鱼可以去杂货铺里卖掉";
			}
			return string.Empty;
		}
		private void ResetMapTutorialFlags()
		{
            // 不再使用 PlayerPrefs 持久化标志
		}
		private int GetCurrentRanchMapId()
		{
			MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
			if (mf == null) { return 0; }
			MapCA mapCa = mf.GetCA(prisonRooms[roomIdx]) as MapCA;
			return mapCa != null ? mapCa.id : 0;
		}
		private string GetRanchTutorialKey()
		{
			return "MapTutorial_Ranch";
		}
		private void InitStoryData()
		{
			if (kaneEncouragements == null || kaneEncouragements.Length == 0)
			{
				kaneEncouragements = new[]
				{
					"养马贵在恒心，别急，冠军是跑出来的。",
					"多去赛马场见识见识，你和马都会更快成长。",
					"你父亲看重的不是输赢，而是让马跑得坦荡。",
					"别怕失败，真正的驯马师从不回避赛道。",
					"坚持训练，我会在赛马场等你的好消息。"
				};
			}
			if (mapClueTexts == null) { mapClueTexts = new Dictionary<int, string>(); }
			if (mapClueTexts.Count == 0)
			{
				mapClueTexts[1800001] = "农场旧仓房的木盒里有张泛黄的赛马报名单，父亲的名字被划掉了。";
				mapClueTexts[1800002] = "养鸡场老板说父亲曾在夜里匆匆借走一匹快马，像是去追什么人。";
				mapClueTexts[1800003] = "杂货铺账本夹着一张赌票，上面写着“冠军换名”，日期正是父亲出事前一夜。";
				mapClueTexts[1800004] = "牧场旧栏里发现一根断裂的马嚼子，李北镇说那是父亲最喜欢的马具。";
				mapClueTexts[1800005] = "医院护士回忆，父亲曾来过急诊室，说有人逼他“改赛程”。";
				mapClueTexts[1800006] = "图书馆里有本驯马笔记，夹着一页被撕下的证词抄本。";
				mapClueTexts[1800007] = "矿洞墙上刻着一串缩写，和父亲信中提到的地点一致。";
				mapClueTexts[1800008] = "教堂里老人低声提到，当年有人向神父忏悔过赌马的罪。";
				mapClueTexts[1800009] = "海边漂来一只旧瓶，里面的纸条写着“别相信赛马场的人”。";
			}
			if (collectedClues == null) { collectedClues = new HashSet<int>(); }
			if (dayTitles == null) { dayTitles = new Dictionary<int, string>(); }
			if (dayTitles.Count == 0)
			{
				dayTitles[1] = "【第一天：不安的种子】";
				dayTitles[2] = "【第二天：腥甜的渴望】";
				dayTitles[3] = "【第三天：破壳的诊疗单】";
				dayTitles[4] = "【第四天：直立的梦魇】";
				dayTitles[5] = "【第五天：森林的哨音】";
				dayTitles[6] = "【第六天：无声的加冕】";
			}
		}
		public bool CheckTime(int t)
		{
			if (time < t)
			{
				return false;
			}
			return true;
		}
		public bool CostTime(int t)
		{
			if (time < t)
			{

				return false;
			}
			if (work != null && (max_time - time + t) > work.starttime)
			{
				TipManager.Tip($"不能耽误{work.starttime}点的工作");
				return false;
			}
			time -= t;
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			MainPanel main = um.GetPanel("MainPanel") as MainPanel;
			main.SetTime(time);
			if (work != null)
			{
				if ((max_time - time) >= work.starttime)
				{
					//开始工作
					DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
					AudioManager.Inst.Play("BGM/劳动时间开始");
					dp.StartDialog(work.alert);
					dp.OnCallback = () =>
					{

						UIManager um2 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
						DialogPanel dp = um2.OpenFloat("DialogPanel") as DialogPanel;

						dp.StartDialog(work.work);
						dp.OnCallback = WorkOver;
					};
				}
			}
			return true;
		}
		public void WorkOver()
		{
			time = 24 - work.endtime;
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			MainPanel main = um.GetPanel("MainPanel") as MainPanel;
			main.SetTime(time);
			string reward = string.Empty;
			foreach (var item in work.reward)
			{
				AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
				AssetCA ca = assetFactory.GetCA(item.k) as AssetCA;
				reward += $"{ca.name}x{1} ";
				if (bag.ContainsKey(item.k))
				{
					bag[item.k] += item.v;
				}
				else
				{
					bag.Add(item.k, item.v);
				}
			}
			main.RefreshBeg();
			TipManager.Tip($"完成了{work.name} 获得了{reward}");
			work = null;
		}
		public static void Tip(string msg)
		{

			UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			TipPanel tip = uiManager.OpenPanel("TipPanel") as TipPanel;
			tip.TipLog(msg);
		}
		private AudioClip GetHorseLevelUpClip()
		{
			if (horseLevelUpClip != null) { return horseLevelUpClip; }
			AudioClip clip = Resources.Load<AudioClip>("BGM/马儿升级");
			if (clip == null)
			{
				clip = GenerateHorseLevelUpClip();
			}
			horseLevelUpClip = clip;
			return horseLevelUpClip;
		}
		private AudioClip GenerateHorseLevelUpClip()
		{
			int sampleRate = 44100;
			float duration = 0.45f;
			int samples = Mathf.CeilToInt(sampleRate * duration);
			float[] data = new float[samples];
			float startFreq = 880f;
			float endFreq = 1320f;
			for (int i = 0; i < samples; i++)
			{
				float t = i / (float)samples;
				float freq = Mathf.Lerp(startFreq, endFreq, t);
				float sample = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate);
				float envelope = Mathf.Sin(Mathf.PI * t);
				data[i] = sample * envelope * 0.25f;
			}
			AudioClip clip = AudioClip.Create("HorseLevelUpGenerated", samples, 1, sampleRate, false);
			clip.SetData(data, 0);
			return clip;
		}
	}
}

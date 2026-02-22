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
		public int start_time = 18;
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
		private HashSet<int> collectedClues = new HashSet<int>();
		private Dictionary<int, string> mapClueTexts = new Dictionary<int, string>();
		private string[] kaneEncouragements = new string[0];
		private const float clueTriggerChance = 0.35f;
		private SaveData pendingSaveData;
		private bool pendingSaveExists;
		public void PrepareStartGame()
		{
			SaveData data = LoadSave();
			if (data != null)
			{
				pendingSaveData = data;
				pendingSaveExists = true;
				return;
			}
			pendingSaveData = BuildNewGameSaveData();
			pendingSaveExists = false;
			SaveToDisk(pendingSaveData);
		}
		public void StartFromLobby()
		{
			if (pendingSaveData == null)
			{
				PrepareStartGame();
			}
			if (pendingSaveExists)
			{
				StartFromSave(pendingSaveData);
			}
			else
			{
				StartNewGame();
			}
			pendingSaveData = null;
			pendingSaveExists = false;
		}
		public void Start()
		{
			StartNewGame();
		}
		private void StartNewGame()
		{
			InitStoryData();
			ResetMapTutorialFlags();
			day = 1;
			time = start_time;
			roomIdx = 0;
			nextDayMapID = 0;
			matchWinStreak = 0;
			matchStreakDialogShown = false;
			storyStage = StoryStage.Stage1;
			firstRaceWin = false;
			pendingLiMorning = false;
			clueCollectionEnabled = false;
			pendingStage3Morning = false;
			storyEnded = false;
			pendingStartRanchTutorial = false;
			bag = new Dictionary<int, int>();
			collectedClues = new HashSet<int>();
			DialogManager dm = CBus.Instance.GetManager(ManagerName.DialogManager) as DialogManager;
			dm.ShowDialog("游戏开始", () =>
			{
				SetupAfterLobby(true);
				SaveToDisk(BuildCurrentSaveData());
			});
		}
		private void StartFromSave(SaveData data)
		{
			InitStoryData();
			ApplySaveData(data);
			SetupAfterLobby(false);
		}
		private void SetupAfterLobby(bool isNewGame)
		{
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			um.ClosePanel("LobbyPanel");
			MainPanel main = um.OpenPanel("MainPanel") as MainPanel;
			LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
			levelManager.OnExpChanged += (c, m) =>
			{
				main.img_exp.fillAmount = c * 1f / m;
				main.text_exp.text = $"{c}/{m}";
			};
			levelManager.OnLevelUp += (l) =>
			{
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
			AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
			if (isNewGame && am != null)
			{
				am.Add(1100002, 10);
				am.Add(1100003, 3);
			}
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
			string path = GetSavePath();
			if (!File.Exists(path)) { return null; }
			string json = File.ReadAllText(path);
			if (string.IsNullOrEmpty(json)) { return null; }
			return JsonUtility.FromJson<SaveData>(json);
		}
		private void SaveToDisk(SaveData data)
		{
			if (data == null) { return; }
			string path = GetSavePath();
			string json = JsonUtility.ToJson(data, true);
			File.WriteAllText(path, json);
		}
		public int GetCurTime()
		{
			return max_time - time;
		}
		public void RefreshAll()
		{

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
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
			dp.StartDialog("NightDialog");
			dp.OnCallback = NextDay;
		}
		public void NextDay()
		{
			GroundManager gdm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
			gdm.DayEnd();

			TipManager.Tip("新的一天开始啦！");
			day++;
			time = start_time;
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			TransitionPanel tp = um.OpenFloat("TransitionPanel") as TransitionPanel;
			tp.StartTransition(() =>
			{

				AudioManager.Inst.Play("BGM/新的一天开始");

				UIManager um1 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
				MainPanel main = um1.GetPanel("MainPanel") as MainPanel;
				main.SetDay(day);
				main.SetTime(time);
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

				DailyFactory df = CBus.Instance.GetFactory(FactoryName.DailyFactory) as DailyFactory;
				DailyCA daily = df.GetCA(1400000 + day) as DailyCA;
				DialogPanel dp = um1.OpenFloat("DialogPanel") as DialogPanel;
				Action onMorningDialogClose = () =>
				{
					//if (day < 5)
					//{
					//	UIManager um2 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
					//	WorkPanel work = um2.OpenPanel("WorkPanel") as WorkPanel;
					//	if (nextDayMapID == 0)
					//	{
					//		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
					//		slm.Load(prisonRooms[roomIdx]);
					//	}
					//	else
					//	{
					//		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
					//		slm.Load(nextDayMapID);
					//	}
					//}
					//else
					//{
					//	UIManager um2 = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
					//	VideoPanel video = um2.OpenPanel("VideoPanel") as VideoPanel;
					//}
					if (storyStage == StoryStage.Stage2 && pendingLiMorning)
					{
						pendingLiMorning = false;
						clueCollectionEnabled = true;
					}
					if (day == 2)
					{
						main.ismatchlocked = true;
						TipManager.Tip("赛马场开放了!");
                    }
				};
				Action showDailyDialog = () =>
				{
					if (daily == null)
					{
						onMorningDialogClose();
						return;
					}
					DialogPanel dailyPanel = um1.OpenFloat("DialogPanel") as DialogPanel;
					dailyPanel.StartDialog(daily.dialog);
					dailyPanel.OnCallback = onMorningDialogClose;
				};
				if (TryShowMorningStoryDialog(dp, showDailyDialog))
				{
					return;
				}
				showDailyDialog();
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
			if (mapCA == null) { return false; }
			string mapName = mapCA.name != null ? mapCA.name.Trim() : string.Empty;
			string text = GetMapTutorialText(mapName);
			if (string.IsNullOrEmpty(text)) { return false; }
			string key = $"MapTutorial_{mapCA.id}";
			if (mapName == "牧场")
			{
				if (PlayerPrefs.GetInt(GetRanchTutorialKey(), 0) == 1) { return false; }
			}
			if (PlayerPrefs.GetInt(key, 0) == 1) { return false; }
			PlayerPrefs.SetInt(key, 1);
			if (mapName == "牧场")
			{
				PlayerPrefs.SetInt(GetRanchTutorialKey(), 1);
			}
			PlayerPrefs.Save();
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			if (um == null) { return false; }
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
			dp.ShowSimple("少女", text, 1200001);
			return true;
		}
		public void TriggerInitialRanchTutorial()
		{
			string text = GetMapTutorialText("牧场");
			if (string.IsNullOrEmpty(text)) { return; }
			string ranchKey = GetRanchTutorialKey();
			int mapId = GetCurrentRanchMapId();
			string mapKey = mapId > 0 ? $"MapTutorial_{mapId}" : string.Empty;
			if (PlayerPrefs.GetInt(ranchKey, 0) == 1) { return; }
			if (!string.IsNullOrEmpty(mapKey) && PlayerPrefs.GetInt(mapKey, 0) == 1) { return; }
			PlayerPrefs.SetInt(ranchKey, 1);
			if (!string.IsNullOrEmpty(mapKey))
			{
				PlayerPrefs.SetInt(mapKey, 1);
			}
			PlayerPrefs.Save();
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			if (um == null) { return; }
			DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
			dp.ShowSimple("少女", text, 1200001);
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
			if (storyStage == StoryStage.Stage1 && firstRaceWin == false && day >= 4)
			{
				string text = GetRandomKaneEncouragement();
				dp.ShowSimple("市长凯恩", text, 1200002);
				dp.OnCloseCallback = onStoryComplete;
				return true;
			}
			if (storyStage == StoryStage.Stage2 && pendingLiMorning)
			{
				dp.ShowSimple("李北镇", "你能赢我不意外，但我也不太开心。你父亲当年的事，城里有些“地方”还留着线索，别被人牵着鼻子走。", 1200003);
				dp.OnCloseCallback = onStoryComplete;
				return true;
			}
			if (storyStage == StoryStage.Stage3 && pendingStage3Morning)
			{
				pendingStage3Morning = false;
				dp.StartDialog("阶段3_抉择之夜");
				dp.OnCallback = onStoryComplete;
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
			PlayerPrefs.DeleteKey(GetRanchTutorialKey());
			MapFactory mf = CBus.Instance.GetFactory(FactoryName.MapFactory) as MapFactory;
			if (mf == null) { return; }
			CABase[] maps = mf.GetAllCA();
			if (maps == null || maps.Length == 0) { return; }
			for (int i = 0; i < maps.Length; i++)
			{
				MapCA map = maps[i] as MapCA;
				if (map == null) { continue; }
				PlayerPrefs.DeleteKey($"MapTutorial_{map.id}");
			}
			PlayerPrefs.Save();
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
			if (time <= over_time)
			{
				NextDayDailog();
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
				if (time <= over_time)
				{
					NextDayDailog();
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

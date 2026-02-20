using DG.Tweening.Core.Easing;
using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RG.Zeluda
{
	public class GameManager : ManagerBase
	{
		public Dictionary<int, int> bag = new Dictionary<int, int>();

		public int day = 0;
		public int time = 12;
		public int max_time = 24;
		public int start_time = 18;
		public int over_time = 3;
		public Transform uiManager;
		public WorkCA work;

		public int[] prisonRooms = new int[4] { 1800002, 1800003, 1800004, 1800005 };
		public int roomIdx = 0;
		public int nextDayMapID = 0;
		public void Start()
		{
			day = 1;
			time = start_time;

			DialogManager dm = CBus.Instance.GetManager(ManagerName.DialogManager) as DialogManager;
			dm.ShowDialog("游戏开始", () =>
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
				};
                AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
                am.Add(1100003, 3);
                levelManager.updateexp();
                main.SetTime(time);
				main.InitClock();
				GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
				gm.BuildGround(9);
			});



			//WorkPanel work = um.OpenPanel("WorkPanel") as WorkPanel;
			//SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
			//slm.Load(prisonRooms[roomIdx]);
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

				DailyFactory df = CBus.Instance.GetFactory(FactoryName.DailyFactory) as DailyFactory;
				DailyCA daily = df.GetCA(1400000 + day) as DailyCA;
				if (daily == null) { return; }
				DialogPanel dp = um1.OpenFloat("DialogPanel") as DialogPanel;
				dp.StartDialog(daily.dialog);
				dp.OnCallback = () =>
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
					if (day == 2)
					{
						main.ismatchlocked = true;
						TipManager.Tip("赛马场开放了!");
                    }
				};
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
	}
}
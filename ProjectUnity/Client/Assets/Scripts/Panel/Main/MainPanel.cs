using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class MainPanel : PanelBase
{
	public Transform tran_clock;
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
	private Transform tran_action;
	private readonly List<Button> actionButtons = new List<Button>();
	private readonly List<Text> actionTexts = new List<Text>();
	private readonly Dictionary<string, UnityAction> actionHandlers = new Dictionary<string, UnityAction>();
	private readonly Dictionary<string, string[]> mapActionLabels = new Dictionary<string, string[]>();
	public override void Init()
	{
		base.Init();
		InitActionHandlers();
		CacheActionButtons();
		RefreshActionButtons();
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
		for (int i = start; i <= end; i++)
		{
			imgList[i].color = c;
		}
	}
	public void SetDay(int day)
	{
		lbl_day.text = $"第{day}天";
		SetTimeSlice(0, 23, Color.white);
		if ((day + 1) % 3 == 0) cangomatch = true;
		else cangomatch = false;

    }
	public void SetTime(int curTime)
	{
		tran_clock.localEulerAngles = new Vector3(0, 0, (24 - curTime) * -15);
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
		if (img_ma != null)
		{
			if (string.IsNullOrEmpty(mapName))
			{
				img_ma.gameObject.SetActive(true);
			}
			else
			{
				img_ma.gameObject.SetActive(mapName.Trim() == "牧场");
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
			Text txt = i < actionTexts.Count ? actionTexts[i] : null;
			if (i < labels.Length)
			{
				btn.gameObject.SetActive(true);
				if (txt != null) { txt.text = labels[i]; }
				btn.onClick = new Button.ButtonClickedEvent();
				UnityAction handler;
				if (actionHandlers.TryGetValue(labels[i], out handler))
				{
					btn.interactable = true;
					btn.onClick.AddListener(handler);
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
	public void Feed() {
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am.CheckAsset(1100003, 1) == false) { TipManager.Tip("牧草不足1"); return; }
		int beforeCount = am.AssetCount(1100003);
		am.Add(1100003, -1);
		int afterCount = am.AssetCount(1100003);
		if (beforeCount > 0 && afterCount == 0 && PlayerPrefs.GetInt("FeedGrassDepletedTipShown", 0) == 0)
		{
			PlayerPrefs.SetInt("FeedGrassDepletedTipShown", 1);
			UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			DialogPanel dialog = uiManager != null ? uiManager.OpenFloat("DialogPanel") as DialogPanel : null;
			if (dialog != null)
			{
				dialog.ShowSimple("少女", "点击右上角“地图”，再点击“农场”种草料");
			}
		}
		LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
		levelManager.AddExp(20);

        TipManager.Tip("Exp+"+20+"!");
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
	bool cangomatch = false;
	public bool ismatchlocked = false;
    public void Match() {

		if (!ismatchlocked) return;
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (!cangomatch)
		{



            TipManager.Tip($"请等待下次开放时间:第{((((gm.day+1) / 3 + 1) * 3)-1)}天,中午之前");
            return;
		}

        if (gm.time < 12)
        {
			Debug.Log(gm.time);
            TipManager.Tip("比赛只能在中午之前参加！");
            return;
        }

        SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
        slm.Load("match");
		cangomatch = false;
    }

    public void Sleep() {
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
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

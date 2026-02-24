using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RG.Zeluda;

public class StorePanel : PanelBase
{
	public Character buyer;
	private Transform grid;
	private Text lblContent;
	private Button btnBuy;
	private Button btnSell;
	private readonly List<Image> slotImages = new List<Image>();
	private readonly List<Button> slotButtons = new List<Button>();
	private readonly List<StoreItemData> items = new List<StoreItemData>();
	private int selectedIndex = -1;

	private class StoreItemData
	{
		public int id;
		public string name;
		public string desc;
		public int buy;
		public int sell;
		public string resPath;
	}

	public override void Init()
	{
		CacheUI();
		BuildItems();
		RefreshGrid();
		BindButtons();
		SelectItem(0);
	}

	private void CacheUI()
	{
		grid = transform.Find("grid");
		Transform lbl = transform.Find("lbl_content");
		Transform buy = transform.Find("btn_buy");
		Transform sell = transform.Find("btn_sell");
		if (lbl != null) { lblContent = lbl.GetComponent<Text>(); }
		if (buy != null) { btnBuy = buy.GetComponent<Button>(); }
		if (sell != null) { btnSell = sell.GetComponent<Button>(); }
		slotImages.Clear();
		slotButtons.Clear();
		if (grid != null)
		{
			for (int i = 0; i < grid.childCount; i++)
			{
				Transform child = grid.GetChild(i);
				Image img = child.GetComponent<Image>();
				if (img == null) { continue; }
				slotImages.Add(img);
				Button btn = child.GetComponent<Button>();
				if (btn == null)
				{
					btn = child.gameObject.AddComponent<Button>();
					btn.targetGraphic = img;
				}
				slotButtons.Add(btn);
			}
		}
	}

	private void BuildItems()
	{
		items.Clear();
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        bool isSpecialMeatDay = gm != null && gm.day >= 2;

		items.Add(new StoreItemData { id = 1100002, name = "种子", desc = "用来种地或者喂鸡", buy = 8, sell = 4, resPath = "bag/种子" });
		items.Add(new StoreItemData { id = 1100003, name = "草料", desc = "用来喂马", buy = 12, sell = 6, resPath = "bag/草料" });
		items.Add(new StoreItemData { id = 1100006, name = "鸡蛋", desc = "在第下一天早上孵出小鸡", buy = 18, sell = 10, resPath = "bag/鸡蛋" });
		
        if (isSpecialMeatDay)
        {
            items.Add(new StoreItemData { id = 1100005, name = "红肉糜", desc = "不知名家畜的混合肉，散发着甜腥味", buy = 26, sell = 14, resPath = "bag/鸡肉" });
        }
        else
        {
            items.Add(new StoreItemData { id = 1100005, name = "鸡肉", desc = "目前只能用来卖钱", buy = 26, sell = 14, resPath = "bag/鸡肉" });
        }

		items.Add(new StoreItemData { id = 1100007, name = "矿物", desc = "目前只能用来卖钱", buy = 30, sell = 16, resPath = "bag/矿物" });
		items.Add(new StoreItemData { id = 1100008, name = "鱼", desc = "目前只能用来卖钱", buy = 30, sell = 16, resPath = "bag/鱼" });
	}

	private void RefreshGrid()
	{
		ResManager resManager = CBus.Instance.GetManager(ManagerName.ResManager) as ResManager;
		for (int i = 0; i < slotImages.Count; i++)
		{
			if (i < items.Count)
			{
				StoreItemData data = items[i];
				slotImages[i].gameObject.SetActive(true);
				if (resManager != null)
				{
					slotImages[i].sprite = resManager.GetRes<Sprite>(data.resPath);
				}
				int index = i;
				slotButtons[i].onClick = new Button.ButtonClickedEvent();
				slotButtons[i].onClick.AddListener(() => SelectItem(index));
			}
			else
			{
				slotImages[i].gameObject.SetActive(false);
			}
		}
	}

	private void BindButtons()
	{
		if (btnBuy != null)
		{
			btnBuy.onClick = new Button.ButtonClickedEvent();
			btnBuy.onClick.AddListener(BuySelected);
		}
		if (btnSell != null)
		{
			btnSell.onClick = new Button.ButtonClickedEvent();
			btnSell.onClick.AddListener(SellSelected);
		}
	}

	private void SelectItem(int index)
	{
		if (index < 0 || index >= items.Count) { return; }
		selectedIndex = index;
		StoreItemData data = items[index];
		if (lblContent != null)
		{
			lblContent.text = $"{data.name}\n{data.desc}\n购买价格:{data.buy}\n出售价格:{data.sell}";
		}
	}

	private void BuySelected()
	{
		if (selectedIndex < 0 || selectedIndex >= items.Count) { return; }
		StoreItemData data = items[selectedIndex];
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am == null) { return; }
		if (am.CheckCoint(data.buy) == false)
		{
			TipManager.Tip("钱币不足");
			return;
		}
		am.RemoveCoin(data.buy);
		am.Add(data.id, 1);
		TipManager.Tip($"购买了{data.name}");

        // 第二天及以后购买红肉糜的特殊逻辑
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (gm != null && data.id == 1100005)
        {
            // 第二天购买：自动使用一个并触发剧情
            if (gm.day == 2)
            {
                Debug.Log("[StorePanel] 第二天购买红肉糜，自动使用一个并触发剧情");
                
                // 立即扣除刚才购买的那一个（模拟直接喂食）
                am.Add(data.id, -1);
                
                // 增加经验
                LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
                if (levelManager != null)
                {
                    levelManager.AddExp(70);
                    TipManager.Tip("马儿立即吃下了红肉糜，Exp+70!");
                }

                // 触发剧情（复用之前的 111，或者您可以指定新的）
                gm.isDay4BuyStoryPlaying = true; // 借用这个标志位锁定状态
                ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 111);
                
                OnClickClose();
            }
            // 第四天购买（保留之前的剧情逻辑，但不再强制升到10级，改为加经验）
            else if (gm.day == 4)
            {
                Debug.Log("[StorePanel] 第四天购买红肉糜，触发 ID 111 剧情");
                gm.isDay4BuyStoryPlaying = true;
                
                LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
                if (levelManager != null)
                {
                    levelManager.AddExp(70);
                }

                ExcelReading.Framework.StoryEventManager.Instance.Publish(ExcelReading.Framework.GameEventNames.DIALOG_START, 111);
                OnClickClose();
            }
        }
	}

	private void SellSelected()
	{
		if (selectedIndex < 0 || selectedIndex >= items.Count) { return; }
		StoreItemData data = items[selectedIndex];
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am == null) { return; }
		if (am.CheckAsset(data.id, 1) == false)
		{
			TipManager.Tip($"{data.name}不足");
			return;
		}
		am.Add(data.id, -1);
		am.GetCoin(data.sell);
		TipManager.Tip($"出售了{data.name}");
	}

    public void OnClickClose()
    {
		Close();
		gameObject.SetActive(false);
    }

    public override void Start()
    {
	    transform.localScale=Vector3.one*2.5f;
    }
}

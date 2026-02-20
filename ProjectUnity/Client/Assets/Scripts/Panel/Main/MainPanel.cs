using RG.Zeluda;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
		lbl_day.text = $"��{day}��";
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
	public void Hoe()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("ʱ�䲻��1Сʱ");
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
			TipManager.Tip("ʱ�䲻��1Сʱ");
			return;
		}
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		int num = gm.WaterGround(1);
		ggm.CostTime(num);
	}
	public void Plant()
	{
		GameManager ggm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (ggm.CheckTime(1) == false)
		{
			TipManager.Tip("ʱ�䲻��1Сʱ");
			return;
		}
		GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
		//�˴�Ӧ�ô�����壬ѡ��һ����Ҫ��ֲ�Ķ���
		int num = gm.Plant(1, 1100002);
		ggm.CostTime(num);
	}
	public void Feed() {
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am.CheckAsset(1100003, 1) == false) { TipManager.Tip("���ݲ���1"); return; }
		am.Add(1100003, -1);
		LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
		levelManager.AddExp(20);

        TipManager.Tip("Exp+"+20+"!");
    }
	bool cangomatch = false;
	public bool ismatchlocked = false;
    public void Match() {

		if (!ismatchlocked) return;
        GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
        if (!cangomatch)
		{



            TipManager.Tip($"�������´ο���ʱ��:��{((((gm.day+1) / 3 + 1) * 3)-1)}��,����֮ǰ");
            return;
		}

        if (gm.time < 12)
        {
			Debug.Log(gm.time);
            TipManager.Tip("����ֻ����������ǰ�μӣ�");
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

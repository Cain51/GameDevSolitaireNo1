using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 引入UI命名空间

public class MatchPanel : PanelBase
{
    LevelManager levelManager;

    int Currentscore;
    [SerializeField]
    public List<HoeSkill> hoeSkills = new List<HoeSkill>();

    public RectTransform trackRect;
    public float trackLength = 2000f;
    public List<Image> horseImages = new List<Image>();
    public float horseYSpacing = 80f;
    public RectTransform trackViewport;

    public Text raceStatusText;

    public List<Text> horseInfoTexts = new List<Text>();
    public Text skillHintText;

    public GameObject matchResultPanel; 
    public Text resultText;
    class Horse
    {
        public string name;
        public float position;
        public float speed;
        public float baseSpeed;
        public bool isPlayer;
        public Horse(string name, bool isPlayer)
        {
            this.name = name;
            this.isPlayer = isPlayer;
            this.position = 0f;
            if (!isPlayer) this.baseSpeed = UnityEngine.Random.Range(5f, 6f);
            else this.baseSpeed = 5f;
            this.speed = baseSpeed;
        }
    }
    List<Horse> horses = new List<Horse>();
    bool isRacing = false;
    float finishLine = 100f;
    string winner = "";

    void matchInit()
    {
        //levelManager = CBus.Instance.GetManager("LevelManager") as LevelManager;
        foreach (var s in hoeSkills)
        {
            //if (s.level <= levelManager.Level)
            //{
            //    s.skillActive = true;
            //}
            s.skillActive = true;
        }

        horses.Clear();
        horses.Add(new Horse("玩家", true));
        horses.Add(new Horse("电脑A", false));
        horses.Add(new Horse("电脑B", false));
        horses.Add(new Horse("电脑C", false));
        horses.Add(new Horse("电脑D", false));
        isRacing = false;
        winner = "";
        matchResultPanel.SetActive(false);
    }

    private void Start()
    {
        matchInit();
        matchStart();

        if (trackRect != null)
            trackRect.sizeDelta = new Vector2(trackLength, trackRect.sizeDelta.y);
    }

    public void matchStart()
    {
        isRacing = true;
        foreach (var h in horses)
        {
            h.position = 0f;
            h.speed = h.baseSpeed;
        }
        winner = "";
        UpdateHorseUI();
        UpdateRaceUI();
    }

    public void matchover()
    {
        isRacing = false;
        UpdateRaceUI();
        ShowMatchResultPanel();
    }

    public void CloseMathch()
    {
        SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
        if (slm != null && slm.curScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(slm.curScene);
        }
    }

    private void Update()
    {
        if (!isRacing) return;


        foreach (var h in horses)
        {
            h.position += h.speed * Time.deltaTime;
        }


        if (Input.anyKey)
        {
            foreach (var skill in hoeSkills)
            {
                if (skill.skillActive && Input.GetKeyDown(skill.skillkey))
                {
                    skill.Cast();

                }
            }
        }

        foreach (var h in horses)
        {
            if (h.position >= finishLine && isRacing)
            {
                winner = h.name;
                matchover();
                break;
            }
        }

        UpdateHorseUI();
    }

    void UpdateHorseUI()
    {
        for (int i = 0; i < horses.Count && i < horseImages.Count; i++)
        {
            var h = horses[i];
            var img = horseImages[i];

            float x = Mathf.Clamp01(h.position / finishLine) * trackLength;
            img.rectTransform.anchoredPosition = new Vector2(x, -(i + 1) * horseYSpacing);

            var txt = horseInfoTexts[i];

            float percent = Mathf.Clamp01(h.position / finishLine) * 100f;

            txt.text = $"{h.name} : {percent:F0}%\n速度: {h.speed:F2}";
        }

        var player = horses.Find(x => x.isPlayer);
        if (player != null && trackRect != null && trackViewport != null)
        {
            float playerX = Mathf.Clamp01(player.position / finishLine) * trackLength;
            float viewportWidth = trackViewport.rect.width;
            float trackWidth = trackRect.rect.width;
            float targetTrackX = Mathf.Clamp(viewportWidth / 2 - playerX, viewportWidth - trackWidth, 0);
            trackRect.anchoredPosition = new Vector2(targetTrackX, trackRect.anchoredPosition.y);
        }
        // 技能提示
        if (skillHintText != null && !isSkllShowing)
            skillHintText.text = "按下技能键可使用技能!";
    }

    void UpdateRaceUI()
    {
        // 比赛状态和胜者
        if (raceStatusText != null)
        {
            if (!isRacing)

            {
                foreach (var i in horseInfoTexts)
                {
                    i.gameObject.SetActive(false);
                }
                raceStatusText.gameObject.SetActive(true);
                raceStatusText.text = $"比赛已结束，胜者：{winner}";
                return;
            }
            else
            {
                raceStatusText.gameObject.SetActive(false);
            }
        }

        // 每匹马的信息
        for (int i = 0; i < horses.Count && i < horseInfoTexts.Count; i++)
        {
            var h = horses[i];
            var txt = horseInfoTexts[i];
            txt.gameObject.SetActive(true);
            if (txt != null)
            {

                txt.text = $"{h.name} : 0%  速度:{h.speed:F2}";
            }
        }





    }
    public bool isSkllShowing = false;

    public void PlayerHorseAddProgress10()
    {
        var player = horses.Find(h => h.isPlayer);
        if (player != null && isRacing)
        {
            player.position += finishLine * 0.1f;
            if (player.position > finishLine) player.position = finishLine;
            UpdateHorseUI();
        }
    }


    public void PlayerHorseDoubleSpeed()
    {
        var player = horses.Find(h => h.isPlayer);
        if (player != null && isRacing)
        {
            player.speed = player.baseSpeed * 2f;
            UpdateHorseUI();
        }
    }


    public void PlayerHorseAddProgress20()
    {
        var player = horses.Find(h => h.isPlayer);
        if (player != null && isRacing)
        {
            player.position += finishLine * 0.2f;
            if (player.position > finishLine) player.position = finishLine;
            UpdateHorseUI();
        }
    }


    public void PlayerHorseReachFinish()
    {
        var player = horses.Find(h => h.isPlayer);
        if (player != null && isRacing)
        {
            player.position = finishLine;
            UpdateHorseUI();
        }
    }

    // 比赛结果弹窗
    private void ShowMatchResultPanel()
    {
        if (matchResultPanel == null)
        {
            return;
        }

        matchResultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = $"比赛结束!\n胜者:{winner}";

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/***
 * 
 * 只有前三天的内容，其他场景未制作。

赛马小游戏 :
写四个马匹的技能
马匹经验值进度条展示
鼠标悬浮技能显示技能名称
比赛结束后弹窗显示结果和结束按钮
游戏结束后，弹出对话：好耶！失败后弹出：下次一定之类的对话；
每天只能赛一次马;
赛马每五天一次,每天早上12点以前可以参加;
马匹名称/位置随机化
制作技能图标和赛马/马场的图片;
玩家材质
赛马动画
技能按键提示与使用后的熄灭遮罩，未激活遮罩
技能释放音效;
赛马bgm；
数值调整;
取消注释
技能等级检测设置
提示未解锁

未实现：
马儿升级时的音效；
如果连续三次获胜，下次进入赛场会进入对话：告知输掉比赛会获得xxxx；
游戏结束后按照排名马儿获得经验值；
牧草种植优化

失败数次后，好心邻居帮忙开垦耕地：
GroundManager gm = CBus.Instance.GetManager(ManagerName.GroundManager) as GroundManager;
gm.BuildGround(9);
 * 
 * 
 * 
 * ***/


public class MatchPanel : PanelBase
{
    LevelManager levelManager;

    int Currentscore;
    [SerializeField]
    public List<HoeSkill> hoeSkills = new List<HoeSkill>();

    public RectTransform trackRect;
    public float trackLength = 2000f;
    public List<Image> horseImages = new List<Image>();
    public Material PlayerMat;
    [SerializeField]
    public List<HorseInfo> horseInfoList = new List<HorseInfo>();
    [SerializeField]
    public HorseInfo playerHorseInfo;


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
            if (!isPlayer) this.baseSpeed = UnityEngine.Random.Range(8f, 12f);
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

        LevelManager levelManager = CBus.Instance.GetManager(ManagerName.LevelManager) as LevelManager;
        foreach (var s in hoeSkills)
        {
            s.skillActive = false;
            s.unlock = true;


        }
        foreach (var s in hoeSkills)
        {
            if (s.level <= levelManager.Level)
            {
                s.skillActive = true;
                s.unlock = false;
            }

        }

        horses.Clear();


        List<HorseInfo> allHorseInfos = new List<HorseInfo>();


        allHorseInfos.Add(playerHorseInfo);


        List<HorseInfo> aiHorseInfos = new List<HorseInfo>(horseInfoList);
        for (int i = 0; i < aiHorseInfos.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, aiHorseInfos.Count);
            var tmp = aiHorseInfos[i];
            aiHorseInfos[i] = aiHorseInfos[rnd];
            aiHorseInfos[rnd] = tmp;
        }
        aiHorseInfos = aiHorseInfos.GetRange(0, Mathf.Min(4, aiHorseInfos.Count));
        allHorseInfos.AddRange(aiHorseInfos);


        for (int i = 0; i < allHorseInfos.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, allHorseInfos.Count);
            var tmp = allHorseInfos[i];
            allHorseInfos[i] = allHorseInfos[rnd];
            allHorseInfos[rnd] = tmp;
        }


        for (int i = 0; i < allHorseInfos.Count && i < horseImages.Count; i++)
        {
            bool isPlayer = allHorseInfos[i]._name == playerHorseInfo._name;
            horses.Add(new Horse(allHorseInfos[i]._name, isPlayer));


            if (isPlayer && PlayerMat != null)
            {
                horseImages[i].material = PlayerMat;
            }

        }

        isRacing = false;
        winner = "";
        matchResultPanel.SetActive(false);
    }

    private void Start()
    {


        matchInit();

        ShowDialog(() => matchStart(), "赛马开始");
        //matchStart();
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

        if (winner == "玩家")
        {
            ShowDialog(() => ShowMatchResultPanel(), "赛马胜利");
        }
        else
        {
            ShowDialog(() => ShowMatchResultPanel(), "赛马失利");
        }

    }

    public void CloseMatch()
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
                if (Input.GetKeyDown(skill.skillkey))
                {
                    AudioManager.Inst.Play("BGM/点击按钮");
                    if (skill.skillActive)
                    {
                        skill.Cast();

                    }
                    else
                    { 
                        if(skill.unlock)
                            TipManager.Tip("未解锁该技能!");
                        else
                            TipManager.Tip("无法再使用了!");
                    }
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
            img.rectTransform.anchoredPosition = new Vector2(x, img.rectTransform.anchoredPosition.y);

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

        if (skillHintText != null && !isSkllShowing)
            skillHintText.text = "按下技能键可使用技能!\n每个技能只能使用一次";
    }

    void UpdateRaceUI()
    {

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
    #region Skills
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


    private void ShowMatchResultPanel()
    {
        if (matchResultPanel == null)
        {
            return;
        }

        matchResultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = $"比赛结束!\n胜者:{winner}";
        AudioManager.Inst.Play("BGM/新的一天开始");
    }

#endregion

    public void ShowDialog(Action onDialogComplete = null,string DialogName="")
    {

        UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
        DialogPanel dp = um.OpenFloat("DialogPanel") as DialogPanel;
        dp.StartDialog(DialogName);
        if (onDialogComplete != null)
            dp.OnCallback = onDialogComplete;
    }

}

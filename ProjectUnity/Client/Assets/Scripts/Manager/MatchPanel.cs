using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/***
 * 
 * ֻ��ǰ��������ݣ���������δ������

����С��Ϸ :
д�ĸ���ƥ�ļ���
��ƥ����ֵ������չʾ
�������������ʾ��������
���������󵯴���ʾ����ͽ�����ť
��Ϸ�����󣬵����Ի�����Ү��ʧ�ܺ󵯳����´�һ��֮��ĶԻ���
ÿ��ֻ����һ����;
����ÿ����һ��,ÿ������12����ǰ���Բμ�;
��ƥ����/λ�������
��������ͼ�������/����ͼƬ;
��Ҳ���
������
���ܰ�����ʾ��ʹ�ú��Ϩ�����֣�δ��������
�����ͷ���Ч;
����bgm��
��ֵ����;
ȡ��ע��
���ܵȼ��������
��ʾδ����

δʵ�֣�
�������ʱ����Ч��
����������λ�ʤ���´ν������������Ի�����֪�����������xxxx��
��Ϸ�����������������þ���ֵ��
������ֲ�Ż�

ʧ�����κ󣬺����ھӰ�æ���Ѹ��أ�
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

        ShowDialog(() => matchStart(), "����ʼ");
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

        if (winner == "���")
        {
            ShowDialog(() => ShowMatchResultPanel(), "����ʤ��");
        }
        else
        {
            ShowDialog(() => ShowMatchResultPanel(), "����ʧ��");
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
                    AudioManager.Inst.Play("BGM/�����ť");
                    if (skill.skillActive)
                    {
                        skill.Cast();

                    }
                    else
                    { 
                        if(skill.unlock)
                            TipManager.Tip("δ�����ü���!");
                        else
                            TipManager.Tip("�޷���ʹ����!");
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

            txt.text = $"{h.name} : {percent:F0}%\n�ٶ�: {h.speed:F2}";
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
            skillHintText.text = "���¼��ܼ���ʹ�ü���!\nÿ������ֻ��ʹ��һ��";
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
                raceStatusText.text = $"�����ѽ�����ʤ�ߣ�{winner}";
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

                txt.text = $"{h.name} : 0%  �ٶ�:{h.speed:F2}";
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
            resultText.text = $"��������!\nʤ��:{winner}";
        AudioManager.Inst.Play("BGM/�µ�һ�쿪ʼ");
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

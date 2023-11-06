using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class RankBoardCtrl : MonoBehaviour
{
    public class SlotInfo
    {
        public Text text_name;
        public Text text_score;
        public RawImage image_profile;
        public Image image_grade;
    }

    private SlotInfo[] slotInfos;

    private Text text_myRank;

    private Animator anim;

    private Toggle[] toggles_game;
    private Button[] buttons;

    private string gameType;

    private float delayTime = 0f;

    private RectTransform rectTr_toggleSlot;
    private RectTransform rectTr_offset;
    private CanvasGroup canvasGroup;

    private float select_toggle_y = 0f;
    private bool isInit = false;

    private Text text_title;
    private Text[] text_topB_t;

    [System.Serializable]
    public class LeaderBoardInfo
    {
        public UserInfo[] users;

        [System.Serializable]
        public class UserInfo
        {
            public string Rank;
            public string Nick;
            public string Score;
            public string Image;
        }
    }

    private void Update()
    {
        if (delayTime > 0f)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0f)
            {
                canvasGroup.interactable = true;
            }
        }
    }
    private void Start()
    {
        Init();
    }

    void Init()
    {
        if (isInit)
        {
            return;
        }

        anim = GetComponent<Animator>();

        slotInfos = new SlotInfo[11];

        Transform tempTr = transform.Find("Board");
        canvasGroup = tempTr.GetComponent<CanvasGroup>();

        text_title = transform.Find("Board/Image_Window/Text_Title").GetComponent<Text>();

        tempTr = transform.Find("Board/Image_TopBar");
        text_topB_t = new Text[2];
        text_topB_t[0] = tempTr.Find("Text_TopBestScore").GetComponent<Text>();
        text_topB_t[1] = tempTr.Find("Text_TopBestScore_S").GetComponent<Text>();

        tempTr = transform.Find("Board/Image_Bar_Mine");
        text_myRank = tempTr.Find("Text_Rank").GetComponent<Text>();

        slotInfos[0] = new SlotInfo();
        slotInfos[0].text_name = tempTr.Find("Text_Name").GetComponent<Text>();
        slotInfos[0].text_score = tempTr.Find("Text_Score").GetComponent<Text>();
        slotInfos[0].image_profile = tempTr.Find("Image_Profile").GetComponent<RawImage>();
        slotInfos[0].image_grade = tempTr.Find("Image_Grade").GetComponent<Image>();

        tempTr = transform.Find("Board/List");

        for (int i = 0; i < 10; i++)
        {
            slotInfos[i + 1] = new SlotInfo();
            slotInfos[i + 1].text_name = tempTr.GetChild(i).Find("Text_Name").GetComponent<Text>();
            slotInfos[i + 1].text_score = tempTr.GetChild(i).Find("Text_Score").GetComponent<Text>();
            slotInfos[i + 1].image_profile = tempTr.GetChild(i).Find("Image_Profile").GetComponent<RawImage>();
            slotInfos[i + 1].image_grade = tempTr.GetChild(i).Find("Image_Grade").GetComponent<Image>();
        }

        tempTr = transform.Find("Board/Toggles");
        rectTr_toggleSlot = tempTr.GetComponent<RectTransform>();

        UnityEngine.EventSystems.EventTrigger eventTrigger = tempTr.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        UnityEngine.EventSystems.EventTrigger.Entry entry_enter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry_enter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry_enter.callback.AddListener(delegate { StartAnimToggleSlot(true); });
        eventTrigger.triggers.Add(entry_enter);

        UnityEngine.EventSystems.EventTrigger.Entry entry_exit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry_exit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entry_exit.callback.AddListener(delegate { StartAnimToggleSlot(false); });
        eventTrigger.triggers.Add(entry_exit);

        tempTr = transform.Find("Board/Toggles/Offset");
        rectTr_offset = tempTr.GetComponent<RectTransform>();

        toggles_game = new Toggle[tempTr.childCount];
        for (int i = 0; i < toggles_game.Length; i++)
        {
            Toggle toggle = tempTr.GetChild(i).GetComponent<Toggle>();

            toggles_game[i] = toggle;
            toggles_game[i].onValueChanged.RemoveAllListeners();

            toggles_game[i].isOn = (i == 0);
            toggles_game[i].onValueChanged.AddListener(delegate { OnValueToggle(toggle); });
        }

        tempTr = transform.Find("Board/Buttons");

        buttons = new Button[tempTr.childCount];
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = tempTr.GetChild(i).GetComponent<Button>();

            buttons[i] = button;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(delegate { OnClickButton(button); });
        }

        rectTr_toggleSlot.sizeDelta = new Vector2(14.5f, 2.4f);
        rectTr_offset.anchoredPosition = new Vector2(0f, select_toggle_y);
        isInit = true;

        GameSettingCtrl.AddLocalizationChangedEvent(() => SetLocalization());

        if (coroutine_MyScoreUpdate != null)
        {
            StopCoroutine(coroutine_MyScoreUpdate);
        }
        coroutine_MyScoreUpdate = StartCoroutine(Coroutine_Init());
    }

    IEnumerator Coroutine_Init()
    {
        while (GameDataManager.instance.state_userInfo != GameDataManager.UserInfoState.Completed)
        {
            yield return null;
        }
        MyScoreUpdate("All");
    }

    void InitSlot()
    {
        for (int i = 0; i < slotInfos.Length; i++)
        {
            slotInfos[i].text_name.text = "-";
            slotInfos[i].text_score.text = "-";
            slotInfos[i].image_profile.texture = null;
            slotInfos[i].image_grade.enabled = false;
        }
        text_myRank.text = "-";
        anim.Rebind();
    }

    public void MyScoreUpdate(string _gameType, int count = 0)
    {
        Init();

        gameType = _gameType;



        if (coroutine_MyScoreUpdate != null)
        {
            StopCoroutine(coroutine_MyScoreUpdate);
        }
        coroutine_MyScoreUpdate = StartCoroutine(Coroutine_MyScoreUpdate(count));
    }

    Coroutine coroutine_MyScoreUpdate;

    IEnumerator Coroutine_MyScoreUpdate(int count = 0)
    {
        while (GameDataManager.instance.state_userInfo != GameDataManager.UserInfoState.Completed)
        {
            yield return null;
        }

        InitSlot();
        WWWForm form = new WWWForm();

        int gameNum = 2000;
        switch (gameType)
        {
            case "All":
                {
                    gameNum = 2000;
                }
                break;
            case "JetSki":
                {
                    gameNum = 2100;
                }
                break;
            case "BeachVolleyball":
                {
                    gameNum = 2200;
                }
                break;
            case "Fishing":
                {
                    gameNum = 2300;
                }
                break;
            case "FlyingDisc":
                {
                    gameNum = 2400;
                }
                break;
            case "Cycling":
                {
                    gameNum = 2500;
                }
                break;
            case "Swimming":
                {
                    gameNum = 2600;
                }
                break;
            case "ClayShooting":
                {
                    gameNum = 2700;
                }
                break;
            case "WaterPolo":
                {
                    gameNum = 2800;
                }
                break;
            case "PistolShooting":
                {
                    gameNum = 2900;
                }
                break;
            case "Scuba":
                {
                    gameNum = 3000;
                }
                break;
            case "WaterBasketball":
                {
                    gameNum = 3100;
                }
                break;
        }
        Debug.Log(gameNum);
        StartCoroutine(AppnoriWebRequest.API_Ranking(gameNum, "M", SetTextUI));
    }

    void SetTextUI(AppnoriWebRequest.Response_Ranking leaderBoardInfo)
    {
        for (int i = 0; i < leaderBoardInfo.ds.Count; i++)
        {
            slotInfos[i + 1].text_score.text = leaderBoardInfo.ds[i].totLaddrPoint.ToString();
            slotInfos[i + 1].text_name.text = leaderBoardInfo.ds[i].nickNm;
            GameDataManager.instance.SetImage(leaderBoardInfo.ds[i].avatarUrl, slotInfos[i + 1].image_profile);
            PublicGameUIManager.GetInstance.SetGradeImage(leaderBoardInfo.ds[i].userGrade, slotInfos[i + 1].image_grade);
        }

        if (leaderBoardInfo.rs != null)
        {
            text_myRank.text = leaderBoardInfo.rs.userRank.ToString();
            slotInfos[0].text_score.text = leaderBoardInfo.rs.totLaddrPoint.ToString();
        }
        else
        {
            text_myRank.text = "-";
            slotInfos[0].text_score.text = "0";
        }

        slotInfos[0].text_name.text = GameDataManager.instance.userInfo_mine.nick;
        GameDataManager.instance.SetImage(GameDataManager.image_url_mine, slotInfos[0].image_profile);
        int gameNum = 0;
        switch (gameType)
        {
            case "All":
                {
                    gameNum = 0;
                }
                break;
            case "JetSki":
                {
                    gameNum = 1;
                }
                break;
            case "BeachVolleyball":
                {
                    gameNum = 2;
                }
                break;
            case "Fishing":
                {
                    gameNum = 3;
                }
                break;
            case "FlyingDisc":
                {
                    gameNum = 4;
                }
                break;
            case "Cycling":
                {
                    gameNum = 5;
                }
                break;
            case "Swimming":
                {
                    gameNum = 6;
                }
                break;
            case "ClayShooting":
                {
                    gameNum = 7;
                }
                break;
        }
        PublicGameUIManager.GetInstance.SetGradeImage(GameDataManager.instance.userInfo_mine.gameScores[gameNum].userGrade, slotInfos[0].image_grade);
        anim.SetTrigger("OnStart");
    }

    public void Close()
    {
        Init();

        gameObject.SetActive(false);
    }

    public void OnClickButton(Button button)
    {
        if (delayTime > 0f)
        {
            return;
        }
        delayTime = 2f;
        select_toggle_y = 0f;
        toggles_game[0].isOn = true;
        for (int i = 1; i < toggles_game.Length; i++)
        {
            toggles_game[i].isOn = false;
        }
        canvasGroup.interactable = false;
        //switch (button.gameObject.name)
        //{
        //    case "Button_Back":
        //        {
        //            MyScoreUpdate(response.PrevSeasonName, "All");
        //        }
        //        break;
        //    case "Button_Next":
        //        {
        //            MyScoreUpdate(response.NextSeasonName, "All");
        //        }
        //        break;
        //}
    }

    public void OnValueToggle(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            return;
        }
        if (delayTime > 0f)
        {
            return;
        }
        delayTime = 1f;
        select_toggle_y = toggle.transform.localPosition.y * -1f;
        canvasGroup.interactable = false;
        switch (toggle.gameObject.name)
        {
            case "Toggle00":
                {
                    MyScoreUpdate("All");
                }
                break;
            case "Toggle01":
                {
                    MyScoreUpdate("JetSki");
                }
                break;
            case "Toggle02":
                {
                    MyScoreUpdate("BeachVolleyball");
                }
                break;
            case "Toggle03":
                {
                    MyScoreUpdate("Fishing");
                }
                break;
            case "Toggle04":
                {
                    MyScoreUpdate("FlyingDisc");
                }
                break;
            case "Toggle05":
                {
                    MyScoreUpdate("Cycling");
                }
                break;
            case "Toggle06":
                {
                    MyScoreUpdate("Swimming");
                }
                break;
            case "Toggle07":
                {
                    MyScoreUpdate("ClayShooting");
                }
                break;
            case "Toggle08":
                {
                    MyScoreUpdate("WaterPolo");
                }
                break;
        }
    }


    void StartAnimToggleSlot(bool isOpen)
    {
        if (coroutine_animToggleSlot != null)
        {
            StopCoroutine(coroutine_animToggleSlot);
        }

        coroutine_animToggleSlot = StartCoroutine(AnimToggleSlot(isOpen));
    }

    Coroutine coroutine_animToggleSlot;

    IEnumerator AnimToggleSlot(bool isOpen)
    {
        float time = 1f;
        if (isOpen)
        {
            while (true)
            {
                rectTr_toggleSlot.sizeDelta = Vector2.MoveTowards(rectTr_toggleSlot.sizeDelta, new Vector2(14.5f, 4.8f), Time.deltaTime * 10f);
                rectTr_offset.anchoredPosition = Vector2.MoveTowards(rectTr_offset.anchoredPosition, new Vector2(0f, 0f), Time.deltaTime * 10f);
                yield return null;
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    break;
                }
            }
        }
        else
        {
            while (true)
            {
                rectTr_toggleSlot.sizeDelta = Vector2.MoveTowards(rectTr_toggleSlot.sizeDelta, new Vector2(14.5f, 2.4f), Time.deltaTime * 10f);
                rectTr_offset.anchoredPosition = Vector2.MoveTowards(rectTr_offset.anchoredPosition, new Vector2(0f, select_toggle_y), Time.deltaTime * 10f);
                yield return null;
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    break;
                }
            }
        }
    }

    public void SetLocalization()
    {
        switch (gameType)
        {
            case "JetSki":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0060");
                }
                break;
            case "BeachVolleyball":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0061");
                }
                break;
            case "Fishing":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0062");
                }
                break;
            case "FlyingDisc":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0063");
                }
                break;
            case "Cycling":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0064");
                }
                break;
            case "Swimming":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0065");
                }
                break;
            case "ClayShooting":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0070");
                }
                break;
            case "WaterPolo":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0071");
                }
                break;
            case "PistolShooting":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0077");
                }
                break;
            case "Scuba":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0078");
                }
                break;
            case "WaterBasketball":
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0079");
                }
                break;
            default:
                {
                    text_topB_t[0].text = GameSettingCtrl.GetLocalizationText("0080");
                }
                break;
        }

        text_topB_t[1].text = text_topB_t[0].text;

        //if (response == null)
        //{
        //    return;
        //}

        //switch (response.SeasonName)
        //{
        //    case "2022":
        //        {
        //            text_title.text = GameSettingCtrl.GetLocalizationText("0179");
        //        }
        //        break;
        //    case "2023_1Q":
        //        {
        //            text_title.text = GameSettingCtrl.GetLocalizationText("0180");
        //        }
        //        break;
        //    default:
        //        {
        //            text_title.text = response.SeasonName;
        //        }
        //        break;
        //}
    }
}

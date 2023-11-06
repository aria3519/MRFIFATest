using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardCtrl : MonoBehaviour
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

    private bool isInit = false;


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

        Transform tempTr = transform.Find("Board/Image_Bar_Mine");
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

        gameObject.SetActive(false);

        isInit = true;
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
    }

    public void MyScoreUpdate(int count = 0)
    {
        Init();

        InitSlot();

        gameObject.SetActive(true);

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

        WWWForm form = new WWWForm();

        int gameNum = 0;

        switch (GameDataManager.instance.gameType)
        {
            case GameDataManager.GameType.JetSki:
                {
                    gameNum = 2100;
                }
                break;
            case GameDataManager.GameType.BeachVolleyball:
                {
                    gameNum = 2200;
                }
                break;
            case GameDataManager.GameType.Fishing:
                {
                    gameNum = 2300;
                }
                break;
            case GameDataManager.GameType.FlyingDisc:
                {
                    gameNum = 2400;
                }
                break;
            case GameDataManager.GameType.Cycling:
                {
                    gameNum = 2500;
                }
                break;
            case GameDataManager.GameType.Swimming:
                {
                    switch (GameDataManager.level)
                    {
                        case 1:
                            {
                                gameNum = 2610;
                            }
                            break;
                        case 2:
                            {
                                gameNum = 2620;
                            }
                            break;
                        case 3:
                            {
                                gameNum = 2630;
                            }
                            break;
                        case 4:
                            {
                                gameNum = 2640;
                            }
                            break;
                        case 5:
                            {
                                gameNum = 2650;
                            }
                            break;
                        default:
                            {
                                gameNum = 2600;
                            }
                            break;
                    }
                }
                break;
            case GameDataManager.GameType.ClayShooting:
                {
                    gameNum = 2700;
                }
                break;
            case GameDataManager.GameType.WaterPolo:
                {
                    gameNum = 2800;
                }
                break;
            case GameDataManager.GameType.PistolShooting:
                {
                    gameNum = 2900;
                }
                break;
            case GameDataManager.GameType.Scuba:
                {
                    gameNum = 3000;
                }
                break;
            case GameDataManager.GameType.WaterBasketball:
                {
                    gameNum = 3100;
                }
                break;
        }

        if (gameNum != 0)
        {
            if (GameDataManager.instance.gameType == GameDataManager.GameType.Swimming)
            {
                StartCoroutine(AppnoriWebRequest.API_LeaderBoard(gameNum, SetTextUI_Swimming));
            }
            else
            {
                StartCoroutine(AppnoriWebRequest.API_Ranking(gameNum, "S", SetTextUI));
            }
        }
    }

    void SetTextUI_Swimming(AppnoriWebRequest.Response_LeaderBoard leaderBoardInfo)
    {
        for (int i = 0; i < leaderBoardInfo.ds.Count; i++)
        {
            switch (GameDataManager.instance.gameType)
            {
                case GameDataManager.GameType.JetSki:
                case GameDataManager.GameType.Cycling:
                case GameDataManager.GameType.Swimming:
                    {
                        float time_sec = leaderBoardInfo.ds[i].record / 100f;
                        slotInfos[i + 1].text_score.text = PublicGameUIManager.ConvertTimeScore(time_sec);
                    }
                    break;
                default:
                    {
                        slotInfos[i + 1].text_score.text = leaderBoardInfo.ds[i].record.ToString();
                    }
                    break;
            }
            slotInfos[i + 1].text_name.text = leaderBoardInfo.ds[i].nickNm;
            GameDataManager.instance.SetImage(leaderBoardInfo.ds[i].avatarUrl, slotInfos[i + 1].image_profile);
        }

        if (leaderBoardInfo.rs != null)
        {
            text_myRank.text = leaderBoardInfo.rs.userRank.ToString();
            switch (GameDataManager.instance.gameType)
            {
                case GameDataManager.GameType.JetSki:
                case GameDataManager.GameType.Cycling:
                case GameDataManager.GameType.Swimming:
                    {
                        float time_sec = leaderBoardInfo.rs.record / 100f;
                        slotInfos[0].text_score.text = PublicGameUIManager.ConvertTimeScore(time_sec);
                    }
                    break;
                default:
                    {
                        slotInfos[0].text_score.text = leaderBoardInfo.rs.record.ToString();
                    }
                    break;
            }
        }
        else
        {
            text_myRank.text = "-";
            slotInfos[0].text_score.text = "-";
        }

        slotInfos[0].text_name.text = GameDataManager.instance.userInfo_mine.nick;
        GameDataManager.instance.SetImage(GameDataManager.image_url_mine, slotInfos[0].image_profile);
        anim.SetTrigger("OnStart");
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
        int gameNum = 3;
        PublicGameUIManager.GetInstance.SetGradeImage(GameDataManager.instance.userInfo_mine.gameScores[gameNum].userGrade, slotInfos[0].image_grade);
        anim.SetTrigger("OnStart");
    }


    public void Close()
    {
        Init();
        gameObject.SetActive(false);
    }
}

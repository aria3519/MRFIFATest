using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.EventSystems;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine.Audio;

public class PublicGameUIManager : MonoBehaviour
{
    private static PublicGameUIManager Instance;

    public static PublicGameUIManager GetInstance
    {
        get
        {
            if (Instance == null)
            {
                return Instantiate(Resources.Load<Transform>("UI/PublicGameUIManager")).GetComponent<PublicGameUIManager>();
            }
            return Instance;
        }
    }

    public static RenderOriginCamCtrl renderOriginCam;
    public static LeaderBoardCtrl leaderBoard;
    public static GameSettingCtrl gameSetting;
    public static ProfileCaptureCtrl profileCapture;

    private event Action action_lobby;
    private event Action action_replay;
    private event Action<bool> action_menu;
    private event Action action_disconnect;

    private bool isInteractable = false;

    public enum ViewState
    {
        None, Menu, Result
    }
    private ViewState viewState;
    private ViewState viewState_keep;

    private bool isViewCenter = false;
    private bool isOverlay = false;

    private Camera camera_main;
    private XRController[] controllers_main = new XRController[2];
    private Camera camera_overlay;
    private XRController[] controllers_overlay = new XRController[2];


    private MeshButtonCtrl[] buttons_menu;
    private MeshButtonCtrl[] buttons_result;

    private Transform windowTr_menu;
    private Animator anim_result;
    private MeshRenderer renderer_grid;
    private MeshRenderer renderer_arrow;
    private GameObject grid_clone;

    private float gridSize = 2f;
    private float distP_min = 0f;
    private float distP_max = 0f;
    private float distP_pause = 0f;

    private Vector3 targetPos;
    private Quaternion targetRot;

    [System.Serializable]
    public class UserUISlot
    {
        public RectTransform slot;
        public Text text_rank;
        public Text text_nick;
        public Text text_score;
        public Text[] texts_add_point;
        public Image image_back;
        public Image image_rank;
    }

    private UserUISlot[] userUISlots_11;
    private UserUISlot[] userUISlots;

    private int userSlotNum = -1;
    private float backColorP = -1;

    private GameObject typeA;
    private GameObject typeD;

    private Text text_info_A;
    private GameObject eff_win_f;
    private GameObject eff_win_b;
    private Transform eff_win_light;
    private Image[] resultTexts;

    public Sprite[] sprite_resultText_eng;
    public Sprite[] sprite_resultText_cn;
    public Sprite[] sprites_backGround;

    private bool lastButtonState = false;

    public UIHandInfo[] handInfos = new UIHandInfo[2];

    public LayerMask layerMask;

    public AudioClip[] audios_effect;
    public AudioClip[] audios_voice;
    public AudioClip[] audios_voice_cn;

    private AudioSource audioSource_effect;
    private AudioSource audioSource_voice;

    private bool isSetFirst = true;

    public StaticTextsLocalization.DataSlot[] dataSlots_localization;

    public bool isFocus = true;

    public Recorder recorder;

    public enum StartViewState
    {
        Idle, Disconnect, Rematch
    }

    public StartViewState startViewState = StartViewState.Idle;

    public Sprite[] sprites_grade;

    private Color color_mine = new Color(1f, 0.35f, 0f);
    private Color color_other = new Color(0f, 0.5f, 1f);

    private Color color_mine_0 = new Color(0.5f, 1f, 1f);
    private Color color_mine_1 = new Color(0f, 0.7f, 1f);

    private Color color_success = new Color(0f, 1f, 0.5f);
    private Color color_fail = new Color(1f, 0.5f, 0.5f);

    public GameObject[] resultTitles;

    public int[] temp_scores;

    private GameDataManager gameDataManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Debug.Log("Awake_PublicGameUIManager");

        handInfos = new UIHandInfo[2];
        Transform tempTr;
        for (int i = 0; i < handInfos.Length; i++)
        {
            handInfos[i] = new UIHandInfo();
            if (i == 0)
            {
                tempTr = transform.Find("LeftHand Controller");
            }
            else
            {
                tempTr = transform.Find("RightHand Controller");
            }
            handInfos[i].controller = tempTr.GetComponent<XRController>();
            handInfos[i].anim = tempTr.GetChild(0).GetComponent<Animator>();
            handInfos[i].rayDir = handInfos[i].anim.transform.Find("RayDir");
            if (i == 0)
            {
                tempTr = transform.Find("RayL");
            }
            else
            {
                tempTr = transform.Find("RayR");
            }
            handInfos[i].line_transform = tempTr.Find("Line");
            handInfos[i].ball_transform = tempTr.Find("Sphere");
        }

        recorder = PhotonVoiceNetwork.Instance.transform.GetComponent<Recorder>();
        recorder.IsRecording = true;

        audioSource_effect = gameObject.AddComponent<AudioSource>();
        audioSource_effect.loop = false;
        audioSource_effect.volume = 1f;
        audioSource_effect.playOnAwake = false;

        audioSource_voice = gameObject.AddComponent<AudioSource>();
        audioSource_voice.loop = false;
        audioSource_voice.volume = 1f;
        audioSource_voice.playOnAwake = false;

        camera_overlay = transform.GetComponentInChildren<Camera>();
        controllers_overlay = camera_overlay.transform.parent.GetComponentsInChildren<XRController>();
        if (controllers_overlay != null)
        {
            for (int i = 0; i < controllers_overlay.Length; i++)
            {
                controllers_overlay[i].enableInputActions = false;
            }
        }

        renderOriginCam = transform.GetComponentInChildren<RenderOriginCamCtrl>();

        InitSceneCamera();

        windowTr_menu = transform.Find("Window_Menu");
        anim_result = transform.Find("Window_Result").GetComponent<Animator>();
        renderer_grid = transform.Find("Static/Grid").GetComponent<MeshRenderer>();
        renderer_arrow = transform.Find("Static/Arrow").GetComponent<MeshRenderer>();

        buttons_menu = new MeshButtonCtrl[3];

        buttons_menu[0] = windowTr_menu.Find("Button_Play").GetComponent<MeshButtonCtrl>();
        buttons_menu[1] = windowTr_menu.Find("Button_Lobby").GetComponent<MeshButtonCtrl>();
        buttons_menu[2] = windowTr_menu.Find("Button_Exit").GetComponent<MeshButtonCtrl>();

        buttons_result = new MeshButtonCtrl[3];

        buttons_result[0] = anim_result.transform.Find("Button_Restart").GetComponent<MeshButtonCtrl>();
        buttons_result[1] = anim_result.transform.Find("Button_Next").GetComponent<MeshButtonCtrl>();
        buttons_result[2] = anim_result.transform.Find("Button_Lobby").GetComponent<MeshButtonCtrl>();

        leaderBoard = transform.GetComponentInChildren<LeaderBoardCtrl>(true);
        gameSetting = transform.GetComponentInChildren<GameSettingCtrl>(true);

        audioSource_effect.outputAudioMixerGroup = GameSettingCtrl.GetAudioMixerGroup("Effect");
        audioSource_voice.outputAudioMixerGroup = GameSettingCtrl.GetAudioMixerGroup("Effect");

        profileCapture = transform.GetComponentInChildren<ProfileCaptureCtrl>(true);

        typeA = anim_result.transform.Find("Canvas/TypeA").gameObject;
        typeD = anim_result.transform.Find("Canvas/TypeD").gameObject;

        userUISlots_11 = new UserUISlot[2];
        tempTr = null;

        for (int i = 0; i < userUISlots_11.Length; i++)
        {
            userUISlots_11[i] = new UserUISlot();

            if (i == 0)
            {
                tempTr = typeA.transform.Find("User01");
            }
            else
            {
                tempTr = typeA.transform.Find("User02");
            }

            userUISlots_11[i].text_nick = tempTr.Find("Text_Nick").GetComponent<Text>();
            userUISlots_11[i].text_score = tempTr.Find("Text_Score").GetComponent<Text>();
            userUISlots_11[i].image_back = tempTr.Find("Image_Profile/Image_Back").GetComponent<Image>();
        }

        eff_win_b = typeA.transform.Find("Eff_Back").gameObject;
        eff_win_f = typeA.transform.Find("Eff_Front").gameObject;
        eff_win_light = eff_win_f.transform.Find("Image_Mask/Image");
        resultTexts = new Image[3];
        resultTexts[0] = typeA.transform.Find("Image_Win").GetComponent<Image>();
        resultTexts[1] = typeA.transform.Find("Image_Lose").GetComponent<Image>();
        resultTexts[2] = typeA.transform.Find("Image_Draw").GetComponent<Image>();

        text_info_A = typeA.transform.Find("Text_Info").GetComponent<Text>();

        userUISlots = new UserUISlot[8];

        for (int i = 0; i < userUISlots.Length; i++)
        {
            userUISlots[i] = new UserUISlot();

            userUISlots[i].slot = typeD.transform.Find("SubScores/Slot0" + (i + 1)).GetComponent<RectTransform>();

            userUISlots[i].text_rank = userUISlots[i].slot.Find("Text_Rank").GetComponent<Text>();
            userUISlots[i].text_nick = userUISlots[i].slot.Find("Text_Nick").GetComponent<Text>();
            userUISlots[i].text_score = userUISlots[i].slot.Find("Text_Score").GetComponent<Text>();
            userUISlots[i].texts_add_point = new Text[2];
            userUISlots[i].texts_add_point[0] = userUISlots[i].text_score.transform.GetChild(0).GetComponent<Text>();
            userUISlots[i].texts_add_point[1] = userUISlots[i].texts_add_point[0].transform.GetChild(0).GetComponent<Text>();
            userUISlots[i].image_back = userUISlots[i].slot.Find("Image_PlayerInfo/Image_Back").GetComponent<Image>();
            userUISlots[i].image_rank = userUISlots[i].slot.Find("Image_Rank").GetComponent<Image>();
        }

        /////////////////////


        // 레이어 셋팅.
        int layerIndex = 31;

        //camera_overlay.cullingMask = 1 << layerIndex;

        //transform.Find("LeftHand Controller/HandL/HAND").gameObject.layer = layerIndex;
        //transform.Find("RightHand Controller/HandR/HAND").gameObject.layer = layerIndex;

        for (int i = 0; i < 2; i++)
        {
            handInfos[i].line_transform.gameObject.layer = layerIndex;
            handInfos[i].ball_transform.gameObject.layer = layerIndex;
        }

        Transform[] transforms = windowTr_menu.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layerIndex;
        }

        transforms = anim_result.transform.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layerIndex;
        }

        transforms = transform.Find("Static").GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layerIndex;
        }

        transforms = leaderBoard.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layerIndex;
        }

        transforms = gameSetting.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layerIndex;
        }

        // Transform 셋팅.
        gridSize = 2f;
        SetGridSize(gridSize);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            buttons_menu[0].transform.localPosition = new Vector3(0.135f, 0f, 0f);
            buttons_menu[1].gameObject.SetActive(false);
            buttons_menu[2].transform.localPosition = new Vector3(-0.135f, 0f, 0f);
        }
        else
        {
            buttons_menu[0].transform.localPosition = new Vector3(0.19f, 0f, 0f);
            buttons_menu[1].gameObject.SetActive(true);
            buttons_menu[2].transform.localPosition = new Vector3(-0.19f, 0f, 0f);
        }

        // Active 끄기.
        for (int i = 0; i < 2; i++)
        {
            handInfos[i].anim.gameObject.SetActive(false);
            handInfos[i].line_transform.gameObject.SetActive(false);
            handInfos[i].ball_transform.gameObject.SetActive(false);
        }

        isOverlay = false;
        camera_overlay.enabled = false;
        viewState = ViewState.None;
        isViewCenter = false;
        windowTr_menu.gameObject.SetActive(false);
        anim_result.gameObject.SetActive(false);
        renderer_arrow.gameObject.SetActive(false);
        renderer_grid.gameObject.SetActive(false);

        SetCloneGrid();
        isSetFirst = true;
        isInteractable = true;

        XRInteractionManager interactionManager = GameObject.FindObjectOfType<XRInteractionManager>();
        for (int i = 0; i < 2; i++)
        {
            xRRayInteractors[i].interactionManager = interactionManager;
        }

        EventSystem.current = eventSystem;

        if (GameSettingCtrl.localizationInfos == null || GameSettingCtrl.localizationInfos.Count == 0)
        {
            return;
        }
        SetLocaliztion();

#if UNITY_EDITOR
        CreateHotKeyList();
#endif
    }

    public enum StateHotKeyList
    {
        Lobby_Close, Lobby_Menu, Lobby_Mode, Lobby_Level, Lobby_InputCode, Lobby_Random, Lobby_Friend, Lobby_Back, Public_Close, Public_Menu, Public_Result, Setting
    }

#if UNITY_EDITOR
    Photon.Realtime.ClientState clientState;

    public class HotKeyList
    {
        public Text text;
        public Image image;
        public ContentSizeFitter contentSizeFitter;
    }


    private static HotKeyList[] hotKeyLists;

    private static readonly string name_hotKeyList = "IsHotKeyList";

    public static bool IsInitHotKey()
    {
        return (hotKeyLists != null && hotKeyLists.Length == 3 && hotKeyLists[0] != null);
    }
    private static void CreateHotKeyList()
    {
        if (IsInitHotKey())
        {
            return;
        }

        GameObject canvas_hotkeylist = new GameObject("Canvas_HotKey");
        Canvas canvas = canvas_hotkeylist.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        hotKeyLists = new HotKeyList[3];

        for (int i = 0; i < hotKeyLists.Length; i++)
        {
            hotKeyLists[i] = new HotKeyList();

            hotKeyLists[i].image = new GameObject("Image").AddComponent<Image>();
            hotKeyLists[i].image.color = new Color(0, 0, 0, 0.95f);
            hotKeyLists[i].image.transform.SetParent(canvas.transform);

            if (i == 0)
            {
                hotKeyLists[i].image.rectTransform.anchorMin = new Vector2(0f, 1f);
                hotKeyLists[i].image.rectTransform.anchorMax = new Vector2(0f, 1f);
                hotKeyLists[i].image.rectTransform.pivot = new Vector2(0f, 1f);
            }
            else if (i == 1)
            {
                hotKeyLists[i].image.rectTransform.anchorMin = new Vector2(1f, 1f);
                hotKeyLists[i].image.rectTransform.anchorMax = new Vector2(1f, 1f);
                hotKeyLists[i].image.rectTransform.pivot = new Vector2(1f, 1f);
            }
            else
            {
                hotKeyLists[i].image.rectTransform.anchorMin = new Vector2(1f, 0f);
                hotKeyLists[i].image.rectTransform.anchorMax = new Vector2(1f, 0f);
                hotKeyLists[i].image.rectTransform.pivot = new Vector2(1f, 0f);
            }


            hotKeyLists[i].image.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            hotKeyLists[i].image.rectTransform.sizeDelta = new Vector2(10f, 10f);

            hotKeyLists[i].text = new GameObject("Text").AddComponent<Text>();
            hotKeyLists[i].text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            hotKeyLists[i].text.transform.SetParent(hotKeyLists[i].image.transform);

            if (i == 0)
            {
                hotKeyLists[i].text.rectTransform.anchorMin = new Vector2(0f, 1f);
                hotKeyLists[i].text.rectTransform.anchorMax = new Vector2(0f, 1f);
                hotKeyLists[i].text.rectTransform.pivot = new Vector2(0f, 1f);
            }
            else if (i == 1)
            {
                hotKeyLists[i].text.rectTransform.anchorMin = new Vector2(1f, 1f);
                hotKeyLists[i].text.rectTransform.anchorMax = new Vector2(1f, 1f);
                hotKeyLists[i].text.rectTransform.pivot = new Vector2(1f, 1f);
            }
            else
            {
                hotKeyLists[i].text.rectTransform.anchorMin = new Vector2(1f, 0f);
                hotKeyLists[i].text.rectTransform.anchorMax = new Vector2(1f, 0f);
                hotKeyLists[i].text.rectTransform.pivot = new Vector2(1f, 0f);
            }

            hotKeyLists[i].text.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            hotKeyLists[i].text.rectTransform.sizeDelta = new Vector2(10f, 10f);

            hotKeyLists[i].contentSizeFitter = hotKeyLists[i].text.gameObject.AddComponent<ContentSizeFitter>();
            hotKeyLists[i].contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            hotKeyLists[i].contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            hotKeyLists[i].contentSizeFitter.enabled = false;
        }

        DontDestroyOnLoad(canvas_hotkeylist);

        bool isOn = PlayerPrefs.GetInt(name_hotKeyList, 1) == 1 ? true : false;
        ViewHotKeyList(isOn);

    }

    public static void ViewHotKeyList(bool isView)
    {
        if (!IsInitHotKey())
        {
            return;
        }
        for (int i = 0; i < hotKeyLists.Length; i++)
        {
            hotKeyLists[i].image.enabled = isView;
            hotKeyLists[i].text.enabled = isView;
            if (isView)
            {
                hotKeyLists[i].contentSizeFitter.SetLayoutHorizontal();
                hotKeyLists[i].contentSizeFitter.SetLayoutVertical();
                hotKeyLists[i].image.rectTransform.sizeDelta = hotKeyLists[i].text.rectTransform.sizeDelta;
            }
        }
    }

#endif
    public static void SetHotKeyList(StateHotKeyList state)
    {
#if UNITY_EDITOR
        CreateHotKeyList();

        switch (state)
        {
            case StateHotKeyList.Lobby_Close:
                {
                    hotKeyLists[0].text.text = "1) 제트스키\n2) 비치발리볼\n3) 낚시\n4) 프리스비\n5) 사이클링\n6) 수영\n7) 클레이사격\n8) 수구\n9) 권총사격\n0) 스쿠바\nShift+1) 수중농구\nP) 커스텀룸";
                }
                break;
            case StateHotKeyList.Lobby_Menu:
                {
                    hotKeyLists[0].text.text = "1) 혼자하기\n2) 랜덤매치\n3) 비밀방\nB) 뒤로가기";
                }
                break;
            case StateHotKeyList.Lobby_Mode:
                {
                    hotKeyLists[0].text.text = "1) 모드1\n2) 모드2\n3) 모드3\nB) 뒤로가기";
                }
                break;
            case StateHotKeyList.Lobby_Level:
                {
                    hotKeyLists[0].text.text = "1) 레벨1\n2) 레벨2\n3) 레벨3\n4) 레벨4\n5) 레벨5\nB) 뒤로가기";
                }
                break;
            case StateHotKeyList.Lobby_InputCode:
                {
                    hotKeyLists[0].text.text = "1~9) 비번입력\nBack) 지우기\nEnter) 완료\nB) 뒤로가기";
                }
                break;
            case StateHotKeyList.Lobby_Random:
                {
#if SERVER_GLOBAL
                    hotKeyLists[0].text.text = "1) 다음서버\n2) 이전서버\nB) 뒤로가기";
#else
                    hotKeyLists[0].text.text = "B) 뒤로가기";
#endif
                }
                break;
            case StateHotKeyList.Lobby_Friend:
                {
#if SERVER_GLOBAL
                    hotKeyLists[0].text.text = "1) 다음서버\n2) 이전서버\nS) 게임시작\nB) 뒤로가기";
#else
                    hotKeyLists[0].text.text = "S) 게임시작\nB) 뒤로가기";
#endif
                }
                break;
            case StateHotKeyList.Lobby_Back:
                {
                    hotKeyLists[0].text.text = "B) 뒤로가기";
                }
                break;
            case StateHotKeyList.Public_Close:
                {
                    hotKeyLists[1].text.text = "M) 메뉴열기";
                }
                break;
            case StateHotKeyList.Public_Menu:
                {
                    if (GetInstance.buttons_menu[1].gameObject.activeSelf)
                    {
                        hotKeyLists[1].text.text = "Z) 계속하기\nX) 로비로\nS) 게임종료\nM) 메뉴닫기";
                    }
                    else
                    {
                        hotKeyLists[1].text.text = "Z) 계속하기\nS) 게임종료\nM) 메뉴닫기";
                    }
                }
                break;
            case StateHotKeyList.Public_Result:
                {

                    if (GetInstance.buttons_result[0].gameObject.activeSelf && GetInstance.buttons_result[1].gameObject.activeSelf)
                    {
                        hotKeyLists[1].text.text = "Z) 다시하기\nX) 다음레벨\nC) 로비로";
                    }
                    else if (GetInstance.buttons_result[0].gameObject.activeSelf || GetInstance.buttons_result[1].gameObject.activeSelf)
                    {
                        hotKeyLists[1].text.text = "Z) 다시하기\nC) 로비로";
                    }
                    else
                    {
                        hotKeyLists[1].text.text = "C) 로비로";
                    }
                }
                break;
            case StateHotKeyList.Setting:
                {
                    hotKeyLists[2].text.text = "Q) 배경음\nW) 효과음\nE) 진동\nR) 손잡이\nT) 보이스챗\nY) 언어\n+ 방향키) 값변경";
                }
                break;
        }

        for (int i = 0; i < hotKeyLists.Length; i++)
        {
            hotKeyLists[i].contentSizeFitter.SetLayoutHorizontal();
            hotKeyLists[i].contentSizeFitter.SetLayoutVertical();
            hotKeyLists[i].image.rectTransform.sizeDelta = hotKeyLists[i].text.rectTransform.sizeDelta;
        }
#endif
    }
    public static void ClearHotKeyList(int index)
    {
#if UNITY_EDITOR
        if (!IsInitHotKey())
        {
            return;
        }
        hotKeyLists[index].image.rectTransform.sizeDelta = new Vector2(50f, 50f);
        hotKeyLists[index].text.text = "";
#endif
    }

    public XRRayInteractor[] xRRayInteractors;
    public EventSystem eventSystem;
    public XRUIInputModule inputModule;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        gameDataManager = GameDataManager.instance;
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;

        StartCoroutine(AddPlatformManager());
    }

    void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        Debug.Log(deviceWasChanged ? "Device was changed" : "Reset was called");
        if (deviceWasChanged)
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            AudioSettings.Reset(config);
        }

        AudioMixerGroup audioMixerGroup = GameSettingCtrl.GetAudioMixerGroup("BGM");
        AudioSource[] bgms = FindObjectsOfType<AudioSource>();

        for (int i = 0; i < bgms.Length; i++)
        {
            if (bgms[i].outputAudioMixerGroup == audioMixerGroup)
            {
                bgms[i].Play();
            }
        }

        PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording(true);

        Speaker[] speakers = FindObjectsOfType<Speaker>();

        for (int i = 0; i < speakers.Length; i++)
        {
            speakers[i].RestartPlayback();
        }
    }

    void OnEnable()
    {
        InputDevices_Update(new InputDevice());

        InputDevices.deviceConnected += InputDevices_Update;
        InputDevices.deviceDisconnected += InputDevices_Update;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        InputDevices.deviceConnected -= InputDevices_Update;
        InputDevices.deviceDisconnected -= InputDevices_Update;
    }

    private void InputDevices_Update(InputDevice _device)
    {
        List<InputDevice> inputDevices = new List<InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, inputDevices);
        handInfos[0].isControllerOn = false;
        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float velue))
            {
                handInfos[0].isControllerOn = true;
            }
        }

        if (!handInfos[0].isControllerOn)
        {
            handInfos[0].InitData();
        }

        inputDevices.Clear();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        handInfos[1].isControllerOn = false;
        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float velue))
            {
                handInfos[1].isControllerOn = true;
            }
        }
        if (!handInfos[1].isControllerOn)
        {
            handInfos[1].InitData();
        }
    }

    void StartDisconnectCheck()
    {
        if (checkDisconnect != null)
        {
            StopCoroutine(checkDisconnect);
        }
        checkDisconnect = StartCoroutine(DisconnectCheck());
    }

    Coroutine checkDisconnect;

    IEnumerator DisconnectCheck()
    {
        temp_scores = new int[2] {0, 0};
        WaitForSeconds wait_1 = new WaitForSeconds(1f);
        while (true)
        {
            yield return wait_1;

            if (Application.internetReachability != NetworkReachability.NotReachable && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                if (action_disconnect != null)
                {
                    action_disconnect.Invoke();
                }

                OpenResultBoard_Disconnect(true);
                yield break;
            }

            if (!PhotonNetwork.InRoom)
            {
                if (action_disconnect != null)
                {
                    action_disconnect.Invoke();
                }

                OpenResultBoard_Disconnect(false);
                yield break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (clientState != PhotonNetwork.NetworkClientState)
        {
            clientState = PhotonNetwork.NetworkClientState;
            Debug.Log(clientState);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            bool isOn = PlayerPrefs.GetInt(name_hotKeyList, 1) == 1 ? false : true;
            ViewHotKeyList(isOn);
            PlayerPrefs.SetInt(name_hotKeyList, isOn ? 1 : 0);
        }
#endif


        if (userSlotNum != -1)
        {
            backColorP += Time.deltaTime * 2f;
            if (backColorP >= 1f)
            {
                backColorP -= 2f;
            }
            userUISlots[userSlotNum].image_back.color = Color.Lerp(color_mine_0, color_mine_1, Mathf.Abs(backColorP));
        }

#if OCULUS_PLATFORM && !UNITY_EDITOR
        CheckOculusFocus();
#endif
        if (!isInteractable)
        {
            isSetFirst = true;
            return;
        }

        switch (viewState)
        {
            case ViewState.Menu:
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        Click_InputKey("Play");
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        if (buttons_menu[1].gameObject.activeSelf)
                        {
                            Click_InputKey("Lobby");
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        Click_InputKey("Exit");
                    }
                    else if (Input.GetKeyDown(KeyCode.M))
                    {
                        Click_InputKey("Menu");
                    }

                    Vector3 ui_pos = camera_overlay.transform.forward;
                    ui_pos.y = 0f;

                    float dot = Vector3.Dot(ui_pos.normalized, targetRot * -Vector3.forward);

                    Vector3 dir = targetPos - camera_overlay.transform.position;
                    dir.y = 0f;

                    float dist = dir.sqrMagnitude;

                    if (dot < 0.2f || dist < 0.05f || dist > 1f)
                    {
                        ui_pos = ui_pos.normalized * 0.6f;
                        ui_pos.y = -0.3f;

                        targetPos = (camera_overlay.transform.position + ui_pos);
                        targetRot = Quaternion.LookRotation(-ui_pos);
                    }

                    float speedP = (windowTr_menu.position - targetPos).sqrMagnitude + 0.02f;
                    windowTr_menu.position = Vector3.MoveTowards(windowTr_menu.position, targetPos, Time.unscaledDeltaTime * speedP * 30);
                    windowTr_menu.rotation = Quaternion.RotateTowards(windowTr_menu.rotation, targetRot, Time.unscaledDeltaTime * speedP * 3600f);

                    gameSetting.transform.position = windowTr_menu.TransformPoint(Vector3.right * -1.2f + Vector3.forward * -0.3f + Vector3.up * 0.3f);
                    Vector3 gameSetting_dir = camera_overlay.transform.position - gameSetting.transform.position;
                    gameSetting_dir.y = 0f;
                    gameSetting.transform.rotation = Quaternion.LookRotation(gameSetting_dir.normalized);
                }
                break;
            case ViewState.Result:
                {
                    if (Input.GetKeyDown(KeyCode.Z) && buttons_result[0].gameObject.activeSelf)
                    {
                        Click_InputKey("Replay");
                    }
                    else if (Input.GetKeyDown(KeyCode.X) && buttons_result[1].gameObject.activeSelf)
                    {
                        Click_InputKey("Next");
                    }
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        Click_InputKey("Lobby");
                    }

                    Vector3 ui_pos = camera_overlay.transform.forward;
                    ui_pos.y = 0f;

                    float dot = Vector3.Dot(ui_pos.normalized, targetRot * -Vector3.forward);

                    Vector3 dir = targetPos - camera_overlay.transform.position;
                    dir.y = 0f;

                    float dist = dir.sqrMagnitude;

                    if (dot < 0.3f || dist < 0.05f || dist > 2.5f)
                    {
                        ui_pos = ui_pos.normalized * 1.3f;
                        ui_pos.y = -0.1f;

                        targetPos = (camera_overlay.transform.position + ui_pos);
                        targetRot = Quaternion.LookRotation(-ui_pos);
                    }

                    float speedP = (anim_result.transform.position - targetPos).sqrMagnitude + 0.02f;
                    speedP = Mathf.Clamp(speedP, 0f, 1f);
                    anim_result.transform.position = Vector3.MoveTowards(anim_result.transform.position, targetPos, Time.unscaledDeltaTime * speedP * 30);
                    anim_result.transform.rotation = Quaternion.RotateTowards(anim_result.transform.rotation, targetRot, Time.unscaledDeltaTime * speedP * 3600f);
                    if (leaderBoard.gameObject.activeSelf)
                    {
                        leaderBoard.transform.position = anim_result.transform.TransformPoint(Vector3.right * -0.9f + Vector3.forward * 0.1f);
                        Vector3 leaderBoard_dir = camera_overlay.transform.position - leaderBoard.transform.position;
                        leaderBoard_dir.y = 0f;
                        leaderBoard.transform.rotation = Quaternion.LookRotation(leaderBoard_dir.normalized);
                    }
                }
                break;
            case ViewState.None:
                {
                    if (Input.GetKeyDown(KeyCode.M))
                    {
                        Click_InputKey("Menu");
                    }
                }
                break;
        }

        Vector3 trackerPos = camera_overlay.transform.position;
        if (!isOverlay)
        {
            trackerPos.y += 100f;
        }
        renderer_grid.sharedMaterial.SetVector("_TrackerPos00", trackerPos);

        for (int i = 0; i < 2; i++)
        {
            trackerPos = handInfos[i].rayDir.position;
            if (!isOverlay)
            {
                trackerPos.y += 100f;
            }
            if (i == 0)
            {
                renderer_grid.sharedMaterial.SetVector("_TrackerPos01", trackerPos);
            }
            else
            {
                renderer_grid.sharedMaterial.SetVector("_TrackerPos02", trackerPos);
            }
        }

        renderer_grid.sharedMaterial.SetFloat("_TimeP", Time.unscaledTime);
        renderer_arrow.sharedMaterial.SetFloat("_TimeP", Time.unscaledTime);

        Vector3 tracker_pos = camera_overlay.transform.localPosition;
        tracker_pos.y = 0f;
        float tracker_dist = tracker_pos.sqrMagnitude;

        if (isSetFirst)
        {
            if (tracker_dist >= distP_pause)
            {
                SetViewCenter(true);
            }
            else
            {
                SetViewCenter(false);
            }
        }
        else
        {
            if (tracker_dist >= distP_pause && !isViewCenter)
            {
                SetViewCenter(true);
            }
            else if (tracker_dist <= distP_max && isViewCenter)
            {
                SetViewCenter(false);
            }
        }

        if (gridSize > 1.5f)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 temp_dist = handInfos[i].rayDir.position - transform.position;
                temp_dist.y = 0f;

                if (tracker_dist < (temp_dist - temp_dist.normalized * ((gridSize > 1.5f) ? 0.3f : 0.1f)).sqrMagnitude)
                {
                    tracker_dist = (temp_dist - temp_dist.normalized * ((gridSize > 1.5f) ? 0.3f : 0.1f)).sqrMagnitude;
                }
            }
        }

        float lineP = Mathf.Lerp(1.05f, 0.05f, Mathf.Clamp01(Mathf.InverseLerp(distP_min, distP_max, tracker_dist)));

        renderer_grid.sharedMaterial.SetFloat("_LineP", lineP);

        bool tempState = false;
        for (int i = 0; i < handInfos.Length; i++)
        {
            InputDevice device = handInfos[i].controller.inputDevice;
            bool primaryButtonState = false;
            tempState = (device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonState) // did get a value
                        && primaryButtonState)// the value we got
                        || (device.TryGetFeatureValue(CommonUsages.menuButton, out primaryButtonState) // did get a value
                        && primaryButtonState)// the value we got
                        || tempState; // cumulative result from other controllers
        }

        if (tempState != lastButtonState) // Button state changed since last frame
        {
            lastButtonState = tempState;
            if (lastButtonState)
            {
                Click_InputKey("Menu");
            }
        }

        if (viewState == ViewState.Menu || viewState == ViewState.Result)
        {
            for (int i = 0; i < 2; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(handInfos[i].rayDir.position, handInfos[i].rayDir.forward, out hit, 3f, layerMask))
                {
                    Vector3 dir = hit.point - handInfos[i].rayDir.position;

                    if (!handInfos[i].ball_transform.gameObject.activeSelf)
                    {
                        handInfos[i].ball_transform.gameObject.SetActive(true);
                    }
                    if (!handInfos[i].line_transform.gameObject.activeSelf)
                    {
                        handInfos[i].line_transform.gameObject.SetActive(true);
                    }

                    handInfos[i].ball_transform.position = hit.point;
                    handInfos[i].line_transform.position = handInfos[i].rayDir.position;

                    handInfos[i].line_transform.rotation = Quaternion.LookRotation(dir);
                    handInfos[i].line_transform.localScale = Vector3.forward * dir.magnitude;

                    float triggerValue = 0.5f;
                    if (handInfos[i].isControllerOn && handInfos[i].controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue))
                    {
                        bool onTrigger = (triggerValue >= 0.7f && !handInfos[i].isTriggerOn);
                        bool offTrigger = (triggerValue <= 0.3f && handInfos[i].isTriggerOn);

                        if (onTrigger || offTrigger)
                        {
                            handInfos[i].isTriggerOn = onTrigger;

                            if (!handInfos[i].isTriggerOn)
                            {
                                MeshButtonCtrl button = hit.collider.GetComponent<MeshButtonCtrl>();
                                if (button != null && button.IsInteractable())
                                {
                                    button.event_click.Invoke();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (handInfos[i].ball_transform.gameObject.activeSelf)
                    {
                        handInfos[i].ball_transform.gameObject.SetActive(false);
                    }
                    if (handInfos[i].line_transform.gameObject.activeSelf)
                    {
                        handInfos[i].line_transform.gameObject.SetActive(false);
                    }
                }
            }
        }

        isSetFirst = false;

        if (Input.GetKey(KeyCode.Q))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameSetting.SetOnBGM(true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameSetting.SetOnBGM(false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameSetting.SetValueBGM(GameSettingCtrl.settingInfo.value_bgm - 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameSetting.SetValueBGM(GameSettingCtrl.settingInfo.value_bgm + 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameSetting.SetOnEffect(true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameSetting.SetOnEffect(false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameSetting.SetValueEffect(GameSettingCtrl.settingInfo.value_eff - 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameSetting.SetValueEffect(GameSettingCtrl.settingInfo.value_eff + 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameSetting.SetOnHaptic(true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameSetting.SetOnHaptic(false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameSetting.SetValueHaptic(GameSettingCtrl.settingInfo.value_haptic - 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameSetting.SetValueHaptic(GameSettingCtrl.settingInfo.value_haptic + 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.R))
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameSetting.SetRightHanded(true);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameSetting.SetRightHanded(false);
            }
        }
        if (Input.GetKey(KeyCode.T))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameSetting.SetOnVoiceChat(true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameSetting.SetOnVoiceChat(false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameSetting.SetValueVoiceChat(GameSettingCtrl.settingInfo.value_voice - 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameSetting.SetValueVoiceChat(GameSettingCtrl.settingInfo.value_voice + 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.Y))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameSetting.SetLanguageState((int)GameSettingCtrl.languageState - 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameSetting.SetLanguageState((int)GameSettingCtrl.languageState + 1);
            }
        }
    }

    public void SetResultAddPoint(int addPoint)
    {
        string point_str = addPoint.ToString("+0;-#");
        userUISlots[userSlotNum].texts_add_point[0].text = point_str;
        userUISlots[userSlotNum].texts_add_point[1].text = point_str;
        if (addPoint >= 0)
        {
            userUISlots[userSlotNum].texts_add_point[1].color = color_success;
        }
        else
        {
            userUISlots[userSlotNum].texts_add_point[1].color = color_fail;
        }


        userUISlots[userSlotNum].texts_add_point[0].gameObject.SetActive(true);
    }

    public void ClearResultAddPoint()
    {
        for (int i = 0; i < userUISlots.Length; i++)
        {
            userUISlots[i].texts_add_point[0].gameObject.SetActive(false);
        }
    }

    void SetResultTitle(int index)
    {
        for (int r = 0; r < resultTitles.Length; r++)
        {
            resultTitles[r].SetActive(r == index);
        }
    }
    /*  
        //타임 경기.
        ResultUserData resultUserData = new ResultUserData();
        resultUserData.state = ResultUserData.State.Time;
        resultUserData.slot = new List<ResultUserData.DataSlot>();
        for (int i = 0; i < ranNum; i++)
        {
            ResultUserData.DataSlot dataSlot = new ResultUserData.DataSlot();
            dataSlot.userId = GameDataManager.instance.userInfos[i].id;
            dataSlot.time_sec = 65.4321f;
            resultUserData.slot.Add(dataSlot);
        }
        PublicGameUIManager.GetInstance.OpenResultBoard(resultUserData);


        //점수 경기.
        ResultUserData resultUserData = new ResultUserData();
        resultUserData.state = ResultUserData.State.Score;
        resultUserData.slot = new List<ResultUserData.DataSlot>();
        for (int i = 0; i < ranNum; i++)
        {
            ResultUserData.DataSlot dataSlot = new ResultUserData.DataSlot();
            dataSlot.userId = GameDataManager.instance.userInfos[i].id;
            dataSlot.score = 999;
            resultUserData.slot.Add(dataSlot);
        }
        PublicGameUIManager.GetInstance.OpenResultBoard(resultUserData);
    */

    /// <summary>
    /// TypeA1 - 상대가 있을시 사용하는 함수(1:다수).
    /// 순위 순서대로 배열전달.
    /// </summary>
    public void OpenResultBoard(ResultUserData userData)
    {
        if (viewState == ViewState.Result)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == "Scene_Lobby" || SceneManager.GetActiveScene().name == "Scene_Customize")
        {
            return;
        }

        PlayerPrefs.SetString("isDisCheck", "");

        ClearResultAddPoint();

        List<AppnoriWebRequest.Request_GameResult.Infos> playerInfos = new List<AppnoriWebRequest.Request_GameResult.Infos>();

        if (userData.slot.Count == 2)
        {
            for (int i = 0; i < userData.slot.Count; i++)
            {
                if (gameDataManager.userInfos[GameDataManager.GetMyNumber()].id == userData.slot[i].userId)
                {
                    userSlotNum = i;
                }
                AppnoriWebRequest.Request_GameResult.Infos info = new AppnoriWebRequest.Request_GameResult.Infos();
                info.userCd = (userData.slot[i].userId.Contains("CPU_") ? "C" : userData.slot[i].userId);
                info.userRank = i + 1;

                userUISlots_11[i].text_nick.text = gameDataManager.GetUserNick(userData.slot[i].userId);

                if (userData.state == ResultUserData.State.Time)
                {
                    userUISlots_11[i].text_score.text = ConvertTimeScore(userData.slot[i].time_sec);
                    info.record = (int)(userData.slot[i].time_sec * 100);
                }
                else
                {
                    userUISlots_11[i].text_score.text = userData.slot[i].score.ToString();
                    info.record = userData.slot[i].score;
                }
                playerInfos.Add(info);
            }

            text_info_A.text = "";

            bool isWin = (gameDataManager.userInfo_mine.id == userData.slot[0].userId);

            if (isWin)
            {
                switch (GameSettingCtrl.GetLanguageState())
                {
                    case LanguageState.schinese:
                        resultTexts[0].sprite = sprite_resultText_cn[0];
                        break;
                    default:
                        resultTexts[0].sprite = sprite_resultText_eng[0];
                        break;
                }

                resultTexts[0].gameObject.SetActive(true);
                resultTexts[1].gameObject.SetActive(false);
                resultTexts[2].gameObject.SetActive(false);

                Play_Voice(3);
            }
            else
            {
                switch (GameSettingCtrl.GetLanguageState())
                {
                    case LanguageState.schinese:
                        resultTexts[1].sprite = sprite_resultText_cn[1];
                        break;
                    default:
                        resultTexts[1].sprite = sprite_resultText_eng[1];
                        break;
                }

                resultTexts[0].gameObject.SetActive(false);
                resultTexts[1].gameObject.SetActive(true);
                resultTexts[2].gameObject.SetActive(false);

                Play_Voice(4);
            }

            eff_win_b.transform.localPosition = new Vector3(-190f, 55f, 0f);
            eff_win_f.transform.localPosition = eff_win_b.transform.localPosition;
            eff_win_b.SetActive(true);
            eff_win_f.SetActive(true);

            SetResultTitle(0);
            typeA.SetActive(true);
            typeD.SetActive(false);
        }
        else
        {
            for (int i = 0; i < userData.slot.Count; i++)
            {
                if (gameDataManager.userInfos[GameDataManager.GetMyNumber()].id == userData.slot[i].userId)
                {
                    userSlotNum = i;
                    userUISlots[i].image_rank.color = color_mine;
                }
                else
                {
                    userUISlots[i].image_rank.color = color_other;
                }
                AppnoriWebRequest.Request_GameResult.Infos info = new AppnoriWebRequest.Request_GameResult.Infos();
                info.userCd = (userData.slot[i].userId.Contains("CPU_") ? "C" : userData.slot[i].userId);
                info.userRank = i + 1;

                userUISlots[i].text_rank.text = info.userRank.ToString();
                userUISlots[i].text_nick.text = gameDataManager.GetUserNick(userData.slot[i].userId);

                if (userData.state == ResultUserData.State.Time)
                {
                    userUISlots[i].text_score.text = ConvertTimeScore(userData.slot[i].time_sec);
                    info.record = (int)(userData.slot[i].time_sec * 100);
                }
                else
                {
                    userUISlots[i].text_score.text = userData.slot[i].score.ToString();
                    info.record = userData.slot[i].score;
                }
                playerInfos.Add(info);
            }

            if (gameDataManager.playType == GameDataManager.PlayType.Single)
            {
                if (userData.isClear)
                {
                    SetResultTitle(1);
                    Play_Voice(1);
                }
                else
                {
                    SetResultTitle(2);
                    Play_Voice(2);
                }
            }
            else
            {
                SetResultTitle(0);
                Play_Voice(0);
            }

            typeA.SetActive(false);
            typeD.SetActive(true);
        }

        StopDisconnectCheck();

        profileCapture.ShotResultImages(userData, true);
        SetResultSlot(userData.slot.Count);

        if (gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo)
        {
            buttons_result[0].transform.localPosition = new Vector3(0.1f, -0.3f, -0.007f);
            buttons_result[2].transform.localPosition = new Vector3(-0.1f, -0.3f, -0.007f);
            buttons_result[1].gameObject.SetActive(false);
            buttons_result[0].gameObject.SetActive(true);
        }
        else if ((gameDataManager.gameType == GameDataManager.GameType.BeachVolleyball || gameDataManager.gameType == GameDataManager.GameType.FlyingDisc
            || gameDataManager.gameType == GameDataManager.GameType.Swimming || gameDataManager.gameType == GameDataManager.GameType.ClayShooting
            || gameDataManager.gameType == GameDataManager.GameType.WaterPolo || gameDataManager.gameType == GameDataManager.GameType.PistolShooting
            || gameDataManager.gameType == GameDataManager.GameType.Scuba || gameDataManager.gameType == GameDataManager.GameType.WaterBasketball)
            && GameDataManager.level < 5 && gameDataManager.GetClearLevelData() >= GameDataManager.level)
        {

            buttons_result[0].transform.localPosition = new Vector3(0.19f, -0.3f, -0.007f);
            buttons_result[2].transform.localPosition = new Vector3(-0.19f, -0.3f, -0.007f);
            buttons_result[1].gameObject.SetActive(true);
            buttons_result[0].gameObject.SetActive(true);
        }
        else if (gameDataManager.gameType == GameDataManager.GameType.JetSki && GameDataManager.level < 3 && gameDataManager.GetClearLevelData() >= GameDataManager.level)
        {
            buttons_result[0].transform.localPosition = new Vector3(0.19f, -0.3f, -0.007f);
            buttons_result[2].transform.localPosition = new Vector3(-0.19f, -0.3f, -0.007f);
            buttons_result[1].gameObject.SetActive(true);
            buttons_result[0].gameObject.SetActive(true);
        }
        else
        {
            buttons_result[0].transform.localPosition = new Vector3(0.1f, -0.3f, -0.007f);
            buttons_result[2].transform.localPosition = new Vector3(-0.1f, -0.3f, -0.007f);
            buttons_result[1].gameObject.SetActive(false);
            buttons_result[0].gameObject.SetActive(true);
        }

        gameDataManager.SetResultScore(playerInfos);
        SetViewState(ViewState.Result);
    }

    /// <summary>
    /// 상대가 방을 나갈 시. (현재 1:1만 적용).
    /// </summary>
    public void OpenResultBoard_Disconnect(bool isConnected)
    {
        if (viewState == ViewState.Result)
        {
            return;
        }

        string discontYn = "E";
        string dis_check = PlayerPrefs.GetString("isDisCheck", "");

        if (dis_check != "" && JsonUtility.FromJson<AppnoriWebRequest.Request_GameResult>(dis_check).discontYn == "Y")
        {
            discontYn = "Y";
        }

        PlayerPrefs.SetString("isDisCheck", "");

        ClearResultAddPoint();

        List<AppnoriWebRequest.Request_GameResult.Infos> playerInfos = new List<AppnoriWebRequest.Request_GameResult.Infos>();
        ResultUserData userData = new ResultUserData();
        userData.slot = new List<ResultUserData.DataSlot>();
        if (isConnected)
        {
            if (discontYn == "Y")
            {
                discontYn = "N";
            }

            userSlotNum = 0;

            int i = GameDataManager.GetMyNumber();
            AppnoriWebRequest.Request_GameResult.Infos info1 = new AppnoriWebRequest.Request_GameResult.Infos();
            info1.userCd = (gameDataManager.userInfos[i].id.Contains("CPU_") ? "C" : gameDataManager.userInfos[i].id);
            info1.userRank = 1;

            userUISlots_11[0].text_nick.text = gameDataManager.GetUserNick(gameDataManager.userInfos[i].id);
            userUISlots_11[0].text_score.text = "";
            info1.record = 0;
            playerInfos.Add(info1);

            ResultUserData.DataSlot dataSlot1 = new ResultUserData.DataSlot();
            dataSlot1.userId = GameDataManager.instance.userInfos[i].id;
            userData.slot.Add(dataSlot1);


            i = (GameDataManager.GetMyNumber() == 0 ? 1 : 0);
            AppnoriWebRequest.Request_GameResult.Infos info2 = new AppnoriWebRequest.Request_GameResult.Infos();
            info2.userCd = (gameDataManager.userInfos[i].id.Contains("CPU_") ? "C" : gameDataManager.userInfos[i].id);
            info2.userRank = 2;

            userUISlots_11[1].text_nick.text = gameDataManager.GetUserNick(gameDataManager.userInfos[i].id);
            userUISlots_11[1].text_score.text = "";
            info2.record = 0;
            playerInfos.Add(info2);

            ResultUserData.DataSlot dataSlot2 = new ResultUserData.DataSlot();
            dataSlot2.userId = GameDataManager.instance.userInfos[i].id;
            userData.slot.Add(dataSlot2);


            switch (GameSettingCtrl.GetLanguageState())
            {
                case LanguageState.schinese:
                    resultTexts[0].sprite = sprite_resultText_cn[0];
                    break;
                default:
                    resultTexts[0].sprite = sprite_resultText_eng[0];
                    break;
            }

            text_info_A.text = GameSettingCtrl.GetLocalizationText("0031");
            resultTexts[0].gameObject.SetActive(true);
            resultTexts[1].gameObject.SetActive(false);
            resultTexts[2].gameObject.SetActive(false);

            Play_Voice(3);
        }
        else
        {
            userSlotNum = 1;

            int i = (GameDataManager.GetMyNumber() == 0 ? 1 : 0);
            AppnoriWebRequest.Request_GameResult.Infos info1 = new AppnoriWebRequest.Request_GameResult.Infos();
            info1.userCd = (gameDataManager.userInfos[i].id.Contains("CPU_") ? "C" : gameDataManager.userInfos[i].id);
            info1.userRank = 1;

            userUISlots_11[0].text_nick.text = gameDataManager.GetUserNick(gameDataManager.userInfos[i].id);
            userUISlots_11[0].text_score.text = "";
            info1.record = 0;
            playerInfos.Add(info1);

            ResultUserData.DataSlot dataSlot1 = new ResultUserData.DataSlot();
            dataSlot1.userId = GameDataManager.instance.userInfos[i].id;
            userData.slot.Add(dataSlot1);


            i = GameDataManager.GetMyNumber();
            AppnoriWebRequest.Request_GameResult.Infos info2 = new AppnoriWebRequest.Request_GameResult.Infos();
            info2.userCd = (gameDataManager.userInfos[i].id.Contains("CPU_") ? "C" : gameDataManager.userInfos[i].id);
            info2.userRank = 2;

            userUISlots_11[1].text_nick.text = gameDataManager.GetUserNick(gameDataManager.userInfos[i].id);
            userUISlots_11[1].text_score.text = "";
            info2.record = 0;
            playerInfos.Add(info2);

            ResultUserData.DataSlot dataSlot2 = new ResultUserData.DataSlot();
            dataSlot2.userId = GameDataManager.instance.userInfos[i].id;
            userData.slot.Add(dataSlot2);


            switch (GameSettingCtrl.GetLanguageState())
            {
                case LanguageState.schinese:
                    resultTexts[1].sprite = sprite_resultText_cn[1];
                    break;
                default:
                    resultTexts[1].sprite = sprite_resultText_eng[1];
                    break;
            }

            text_info_A.text = GameSettingCtrl.GetLocalizationText("0048");
            resultTexts[0].gameObject.SetActive(false);
            resultTexts[1].gameObject.SetActive(true);
            resultTexts[2].gameObject.SetActive(false);

            Play_Voice(4);
        }

        eff_win_b.transform.localPosition = new Vector3(-190f, 55f, 0f);
        eff_win_f.transform.localPosition = eff_win_b.transform.localPosition;
        eff_win_b.SetActive(true);
        eff_win_f.SetActive(true);
        //////////////

        StopDisconnectCheck();

        profileCapture.ShotResultImages(userData, true);
        SetResultSlot(userData.slot.Count);

        buttons_result[2].transform.localPosition = new Vector3(0f, -0.3f, -0.007f);
        buttons_result[1].gameObject.SetActive(false);
        buttons_result[0].gameObject.SetActive(false);

        typeA.SetActive(true);
        typeD.SetActive(false);

        gameDataManager.SetResultScore(playerInfos, discontYn);
        SetViewState(ViewState.Result);
        SetResultTitle(0);
        //Play_Voice(0);
    }

    public void CloseResultBoard()
    {
        if (viewState != ViewState.Result)
        {
            return;
        }

        SetViewState(ViewState.None);
    }

    public void AddLoadLobbyEvent(Action _action_lobby)
    {
        action_lobby += _action_lobby;
    }

    public void AddReplayEvent(Action _action_replay)
    {
        action_replay += _action_replay;
    }

    public void AddMenuEvent(Action<bool> _action_menu)
    {
        action_menu += _action_menu;
    }

    public void AddDisconnectEvent(Action _action_dis)
    {
        action_disconnect += _action_dis;
    }

    public void SetInteractable(bool _isInteractable)
    {
        isInteractable = _isInteractable;
        if (!isInteractable)
        {
            SetViewState(ViewState.None);
        }
    }

    IEnumerator DelayActive()
    {
        yield return null;

        if (gameDataManager.playType == GameDataManager.PlayType.Single && gameDataManager.gameType == GameDataManager.GameType.Fishing)
        {
            float checkTime = 10f;
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
                checkTime -= Time.deltaTime;
                if (checkTime <= 0f)
                {
                    Click_InputKey("Lobby");
                    yield break;
                }
            }
        }

        if ((gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo) && SceneManager.GetActiveScene().name != "Scene_Lobby" && SceneManager.GetActiveScene().name != "Scene_Customize")
        {
            if (Application.internetReachability == NetworkReachability.NotReachable || !PhotonNetwork.InRoom)
            {
                if (action_lobby != null)
                {
                    action_lobby.Invoke();
                }

                if (leaderBoard.gameObject.activeSelf)
                {
                    leaderBoard.Close();
                }

                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.IsSyncScene = false;
                    PhotonNetwork.Disconnect();
                }

                SetViewState(ViewState.None);
                gameDataManager.playType = GameDataManager.PlayType.None;

                startViewState = StartViewState.Disconnect;
                SceneManager.LoadScene(0);

                yield break;
            }
            else if (gameDataManager.playType == GameDataManager.PlayType.Multi && (gameDataManager.gameType == GameDataManager.GameType.BeachVolleyball || gameDataManager.gameType == GameDataManager.GameType.WaterPolo))
            {
                StartDisconnectCheck();
            }
        }

        while (Camera.main == null)
        {
            yield return null;
        }

        InitSceneCamera();

        XRInteractionManager interactionManager = GameObject.FindObjectOfType<XRInteractionManager>();
        for (int i = 0; i < 2; i++)
        {
            xRRayInteractors[i].interactionManager = interactionManager;
        }

        SetCloneGrid();

        isInteractable = true;

        StartCoroutine(AppnoriWebRequest.API_GameStart());

        if (SceneManager.GetActiveScene().name == "Scene_Lobby" || SceneManager.GetActiveScene().name == "Scene_Customize")
        {
            yield break;
        }

        AppnoriWebRequest.Request_GameResult gameResult = new AppnoriWebRequest.Request_GameResult();

        gameResult.gameTitleNum = AppnoriWebRequest.GetGameTitleNum();

        gameResult.gameId = GameDataManager.game_guid;
        gameResult.gameLvl = GameDataManager.level;

        gameResult.gameResult = new List<AppnoriWebRequest.Request_GameResult.Infos>();
        for (int i = 0; i < GameDataManager.instance.userInfos.Count; i++)
        {
            AppnoriWebRequest.Request_GameResult.Infos info = new AppnoriWebRequest.Request_GameResult.Infos();
            info.userCd = GameDataManager.instance.userInfos[i].id;
            info.userRank = 0;
            info.record = 0;
            gameResult.gameResult.Add(info);
        }

        gameResult.discontYn = "E";

        PlayerPrefs.SetString("isDisCheck", JsonUtility.ToJson(gameResult));

        yield return new WaitForSeconds(3f);

        if (gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo)
        {
            PhotonNetwork.LocalPlayer.CustomProperties["GameReady"] = false;
            PhotonNetwork.LocalPlayer.SetCustomProperties(PhotonNetwork.LocalPlayer.CustomProperties);
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.CurrentSceneProperty] = "";
                PhotonNetwork.CurrentRoom.SetCustomProperties(PhotonNetwork.CurrentRoom.CustomProperties);
            }
            PhotonNetwork.IsSyncScene = false;
            PhotonNetwork.isLoadLevel = false;
            SceneManagerHelper.ActiveSceneName = "";
        }

        yield return new WaitForSeconds(20f);

        gameResult.discontYn = "Y";
        PlayerPrefs.SetString("isDisCheck", JsonUtility.ToJson(gameResult));
    }

    IEnumerator AddPlatformManager()
    {
        while (FindObjectOfType<XROrigin>() == null)
        {
            yield return null;
        }
#if !UNITY_EDITOR && PICO_PLATFORM
        var Xr_Rig = FindObjectOfType<XROrigin>().gameObject;
        var pxrManager = Xr_Rig.AddComponent<Unity.XR.PXR.PXR_Manager>();
        pxrManager.foveationLevel = Unity.XR.PXR.FoveationLevel.Low;
        pxrManager.openMRC = GameSettingCtrl.IsMRC();
        Xr_Rig.AddComponent<PXR_Manager_Support>();
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!CheckLoadScene(scene.name))
        {
            return;
        }

        if (mode == LoadSceneMode.Single)
        {
            isOverlay = false;
            camera_overlay.enabled = false;
            renderer_grid.gameObject.SetActive(false);

            if (scene.name == "Scene_Lobby")
            {
                gridSize = 2f;
                SetGridSize(gridSize);

                GameDataManager.instance.StartPlatformInfo();
            }
            else
            {
                buttons_menu[0].transform.localPosition = new Vector3(0.19f, 0f, 0f);
                buttons_menu[1].gameObject.SetActive(true);
                buttons_menu[2].transform.localPosition = new Vector3(-0.19f, 0f, 0f);
            }

            if (delayActiveCoroutine != null)
            {
                StopCoroutine(delayActiveCoroutine);
            }
            delayActiveCoroutine = StartCoroutine(DelayActive());

            StartCoroutine(AddPlatformManager());

        }
    }

    Coroutine delayActiveCoroutine;

    private void StopDisconnectCheck()
    {
        if (checkDisconnect != null)
        {
            StopCoroutine(checkDisconnect);
        }

        if (delayActiveCoroutine != null)
        {
            StopCoroutine(delayActiveCoroutine);
        }
    }

    private void OnSceneUnloaded(Scene current)
    {
        profileCapture.StopImages();
        SetViewState(ViewState.None);

        isInteractable = false;

        action_lobby = null;
        action_replay = null;
        action_menu = null;
        action_disconnect = null;

        GameSettingCtrl.ClearHandChangedEvent();
        GameSettingCtrl.ClearLocalizationChangedEvent();
    }

    private void SetOverlay(bool _isOverlay)
    {
        if (isOverlay == _isOverlay)
        {
            if (!isSetFirst)
            {
                return;
            }
        }

        isOverlay = _isOverlay;

        if (isOverlay)
        {
            camera_overlay.tag = "MainCamera";
            inputModule.uiCamera = camera_overlay;
            if (camera_main != null)
            {
                camera_main.tag = "Untagged";
                camera_main.enabled = false;
                camera_overlay.enabled = true;
                renderer_grid.gameObject.SetActive(true);
                grid_clone.SetActive(false);
                SetKeepObjectEnable(false);
            }
        }
        else
        {
            if (camera_main != null)
            {
                camera_overlay.tag = "Untagged";
                camera_main.tag = "MainCamera";
                inputModule.uiCamera = camera_main;
                camera_main.enabled = true;
                camera_overlay.enabled = false;
                renderer_grid.gameObject.SetActive(false);
                grid_clone.SetActive(true);
                SetKeepObjectEnable(true);
            }
        }
    }


    public void SetViewState(ViewState setViewState)
    {
        if (viewState == setViewState)
        {
            return;
        }

        if (setViewState != ViewState.Result)
        {
            userSlotNum = -1;
        }

        if (setViewState == ViewState.None && viewState_keep == ViewState.Result)
        {
            setViewState = ViewState.Result;
        }

        viewState_keep = viewState;

        if (setViewState == ViewState.Menu)
        {
            if (action_menu != null)
            {
                action_menu.Invoke(true);
            }
        }
        else if (viewState == ViewState.Menu)
        {
            if (action_menu != null)
            {
                action_menu.Invoke(false);
            }
        }

        viewState = setViewState;

        SetHandState(0);
        SetHandState(1);

        gameSetting.gameObject.SetActive(false);

        if (viewState != ViewState.None)
        {
            if (controllers_main != null)
            {
                for (int i = 0; i < controllers_main.Length; i++)
                {
                    controllers_main[i].enableInputActions = false;
                }
            }

            if (controllers_overlay != null)
            {
                for (int i = 0; i < controllers_overlay.Length; i++)
                {
                    controllers_overlay[i].enableInputActions = true;
                }
            }

            if (!isViewCenter)
            {
                SetOverlay(true);
            }

            Vector3 ui_pos = camera_overlay.transform.forward;
            ui_pos.y = 0f;
            ui_pos = ui_pos.normalized * 0.5f;
            ui_pos.y = -0.3f;

            targetPos = camera_overlay.transform.position;

            switch (viewState)
            {
                case ViewState.Menu:
                    {
                        windowTr_menu.position = camera_overlay.transform.position + camera_overlay.transform.forward * 0.5f + camera_overlay.transform.up * 0.5f;
                        targetRot = Quaternion.LookRotation(camera_overlay.transform.position - windowTr_menu.position);
                        windowTr_menu.rotation = targetRot;

                        if (SceneManager.GetActiveScene().name == "Scene_Lobby")
                        {
                            buttons_menu[0].transform.localPosition = new Vector3(0.135f, 0f, 0f);
                            buttons_menu[1].gameObject.SetActive(false);
                            buttons_menu[2].transform.localPosition = new Vector3(-0.135f, 0f, 0f);
                            buttons_menu[2].gameObject.SetActive(true);
                        }
                        else
                        {
                            SetLocaliztion();
                            gameSetting.View();
                            buttons_menu[0].transform.localPosition = new Vector3(0.19f, 0f, 0f);
                            buttons_menu[1].gameObject.SetActive(true);
                            buttons_menu[2].transform.localPosition = new Vector3(-0.19f, 0f, 0f);
                            buttons_menu[2].gameObject.SetActive(true);
                        }

                        anim_result.gameObject.SetActive(false);
                        windowTr_menu.gameObject.SetActive(true);
                        SetHotKeyList(StateHotKeyList.Public_Menu);
                    }
                    break;
                case ViewState.Result:
                    {
                        anim_result.transform.position = camera_overlay.transform.position + camera_overlay.transform.forward * 0.5f + camera_overlay.transform.up * 0.5f;
                        targetRot = Quaternion.LookRotation(camera_overlay.transform.position - anim_result.transform.position);
                        anim_result.transform.rotation = targetRot;

                        windowTr_menu.gameObject.SetActive(false);
                        anim_result.gameObject.SetActive(true);
                        SetHotKeyList(StateHotKeyList.Public_Result);
                    }
                    break;
            }
        }
        else
        {
            if (controllers_overlay != null)
            {
                for (int i = 0; i < controllers_overlay.Length; i++)
                {
                    controllers_overlay[i].enableInputActions = false;
                }
            }

            if (controllers_main != null)
            {
                for (int i = 0; i < controllers_main.Length; i++)
                {
                    controllers_main[i].enableInputActions = true;
                }
            }

            EndResultAnim();

            windowTr_menu.gameObject.SetActive(false);

            if (!isViewCenter)
            {
                SetOverlay(false);
            }

            SetHotKeyList(StateHotKeyList.Public_Close);
        }
    }

    public void SetViewCenter(bool isActive)
    {
        if (isViewCenter == isActive)
        {
            if (!isSetFirst)
            {
                return;
            }
        }

        isViewCenter = isActive;
        if (isViewCenter)
        {
            if (isSetFirst || viewState == ViewState.None)
            {
                SetOverlay(true);
            }

            renderer_arrow.gameObject.SetActive(true);
        }
        else
        {
            renderer_arrow.gameObject.SetActive(false);
            if (isSetFirst || viewState == ViewState.None)
            {
                SetOverlay(false);
            }
        }
    }

    public void Click_InputKey(string key)
    {
        switch (key)
        {
            case "Lobby":
                {
                    StopDisconnectCheck();

                    if (action_lobby != null)
                    {
                        action_lobby.Invoke();
                    }

                    if (delayActiveCoroutine != null)
                    {
                        StopCoroutine(delayActiveCoroutine);
                    }

                    if (leaderBoard.gameObject.activeSelf)
                    {
                        leaderBoard.Close();
                    }

                    if (gameDataManager.playType == GameDataManager.PlayType.Single && gameDataManager.gameType == GameDataManager.GameType.Fishing)
                    {
                        StartCoroutine(AppnoriWebRequest.API_FishingEnd(GameDataManager.game_guid));
                    }

                    SetViewState(ViewState.None);
                    gameDataManager.playType = GameDataManager.PlayType.None;

                    gameDataManager.state_userInfo = GameDataManager.UserInfoState.Stop;

                    MeshFadeCtrl.instance.LoadScene("Scene_Lobby", 1.5f);
                }
                break;
            case "Exit":
                {
                    Application.Quit();
                }
                break;
        }

        if (!isInteractable)
        {
            return;
        }

        switch (key)
        {
            case "Play":
                {
                    if (viewState == ViewState.Menu)
                    {
                        SetViewState(ViewState.None);
                    }
                }
                break;
            case "Replay":
                {
                    if (leaderBoard.gameObject.activeSelf)
                    {
                        leaderBoard.Close();
                    }

                    if (gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo)
                    {
                        startViewState = StartViewState.Rematch;
                        Click_InputKey("Lobby");
                    }
                    else
                    {
                        StopDisconnectCheck();

                        if (delayActiveCoroutine != null)
                        {
                            StopCoroutine(delayActiveCoroutine);
                        }

                        if (leaderBoard.gameObject.activeSelf)
                        {
                            leaderBoard.Close();
                        }

                        SetViewState(ViewState.None);

                        MeshFadeCtrl.instance.LoadScene("", 1.5f);
                    }
                }
                break;
            case "Menu":
                {
                    if (viewState == ViewState.Menu)
                    {
                        SetViewState(ViewState.None);
                    }
                    else if (viewState == ViewState.None)
                    {
                        SetViewState(ViewState.Menu);
                    }
                }
                break;
            case "Next": // 멀티에는 나오면 안됨.
                {
                    if (leaderBoard.gameObject.activeSelf)
                    {
                        leaderBoard.Close();
                    }

                    SetViewState(ViewState.None);

                    GameDataManager.level += 1;
                    for (int i = 1; i < GameDataManager.instance.userInfos.Count; i++)
                    {
                        //GameDataManager.instance.userInfos[i].customModelData = CustomModelSettingCtrl.GetRandomModelData();
                    }

                    MeshFadeCtrl.instance.LoadScene("", 1.5f);
                }
                break;
        }
    }

    public void SetGridSize(float sizeP)
    {
        gridSize = sizeP;
        renderer_grid.transform.localScale = new Vector3(gridSize, 2f, gridSize);

        if (grid_clone != null)
        {
            grid_clone.transform.localScale = renderer_grid.transform.localScale;
        }

        renderer_grid.sharedMaterial.SetVector("_Tiling", new Vector2(8f * gridSize, 5f));
        renderer_grid.sharedMaterial.SetVector("_TrackerPos00", Vector3.zero);
        renderer_grid.sharedMaterial.SetVector("_TrackerPos01", Vector3.zero);
        renderer_grid.sharedMaterial.SetVector("_TrackerPos02", Vector3.zero);
        if (sizeP > 1.5f)
        {
            distP_pause = gridSize * 0.5f;
            distP_min = distP_pause - 0.4f;
            distP_min *= distP_min;
            distP_max = distP_pause - 0.35f;
            distP_max *= distP_max;
            distP_pause *= distP_pause;
        }
        else
        {
            distP_pause = gridSize * 0.5f;
            distP_min = distP_pause - 0.1f;
            distP_min *= distP_min;
            distP_max = distP_pause - 0.05f;
            distP_max *= distP_max;
            distP_pause *= distP_pause;
        }
    }

    void SetCloneGrid()
    {
        grid_clone = Instantiate(renderer_grid.gameObject, camera_main.transform.parent);
        grid_clone.transform.localPosition = Vector3.zero;
        grid_clone.transform.localRotation = Quaternion.identity;
        grid_clone.transform.localScale = renderer_grid.transform.localScale;
        grid_clone.layer = 0;
        grid_clone.SetActive(true);
    }

    public void EndResultAnim()
    {
        if (viewState_keep != ViewState.Result)
        {
            return;
        }

        if (endResultCoroutine != null)
        {
            StopCoroutine(endResultCoroutine);
        }

        anim_result.SetTrigger("OnClose");
        endResultCoroutine = StartCoroutine(EndResultCoroutine());
    }

    Coroutine endResultCoroutine;

    IEnumerator EndResultCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        while (1f > anim_result.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            yield return null;
        }
        anim_result.gameObject.SetActive(false);
    }

    void SetHandState(int index)
    {
        switch (viewState)
        {
            case ViewState.None:
                {
                    handInfos[index].anim.gameObject.SetActive(false);
                    handInfos[index].line_transform.gameObject.SetActive(false);
                    handInfos[index].ball_transform.gameObject.SetActive(false);
                }
                break;
            case ViewState.Menu:
            case ViewState.Result:
                {
                    handInfos[index].anim.gameObject.SetActive(true);
                    handInfos[index].anim.SetBool("IsPoint", true);
                    handInfos[index].line_transform.gameObject.SetActive(true);
                    handInfos[index].ball_transform.gameObject.SetActive(true);
                }
                break;
        }
    }

    public void Play(int index, bool isLoop = false)
    {
        audioSource_effect.loop = isLoop;
        audioSource_effect.clip = audios_effect[index];
        audioSource_effect.Play();
    }
    public void Stop()
    {
        audioSource_effect.Stop();
    }

    public void Play_Voice(int index, bool isLoop = false)
    {
        audioSource_voice.loop = isLoop;
        switch (GameSettingCtrl.GetLanguageState())
        {
            case LanguageState.schinese:
                {
                    audioSource_voice.clip = audios_voice_cn[index];
                }
                break;
            default:
                {
                    audioSource_voice.clip = audios_voice[index];
                }
                break;
        }
        audioSource_voice.Play();
    }

    public void Stop_Voice()
    {
        audioSource_voice.Stop();
    }

    public void SyncProfileLight(float _time)
    {
        _time %= 1f;  
        eff_win_light.localPosition = Vector3.up * Mathf.Lerp(2000f, -3000f, _time);
    }

    public void SetHaptic(float amplitude = 0.5f, float duration = 0.5f)
    {
        if (controllers_main == null)
        {
            return;
        }

        for (int i = 0; i < controllers_main.Length; i++)
        {
            controllers_main[i].SendHapticImpulse(amplitude, duration);
        }
    }

    public ViewState GetCurrentState()
    {
        return viewState;
    }

    public void SetLocaliztion()
    {
        for (int i = 0; i < dataSlots_localization.Length; i++)
        {
            dataSlots_localization[i].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_localization[i].id);
        }
    }

    private List<Renderer> keep_object = new List<Renderer>();

    public bool IsOverlay()
    {
        return isOverlay;
    }

    private bool isPause = false;

    private void OnApplicationPause(bool pause)
    {
        if (gameDataManager == null)
        {
            return;
        }
        isPause = pause;

        Debug.Log("OnApplicationPause");
        if (gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo)
        {
            recorder.IsRecording = (Application.isFocused && !isPause);
        }
    }

    private float saveTimeScale = 1f;

#if OCULUS_PLATFORM
    private void CheckOculusFocus()
    {
        if (OVRManager.hasInputFocus != isFocus)
        {
            isFocus = OVRManager.hasInputFocus;

            if (gameDataManager.playType != GameDataManager.PlayType.Multi)
            {
                if (isFocus)
                {
                    Time.timeScale = saveTimeScale;
                }
                else
                {
                    saveTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                }
            }

            if (!isFocus && viewState == ViewState.Menu)
            {
                SetViewState(ViewState.None);
                SetOverlay(true);
            }
            else if (!isFocus)
            {
                SetOverlay(true);
            }
            else if (isFocus && viewState == ViewState.None)
            {
                SetOverlay(false);
            }
        }
    }
#endif

    private void OnApplicationFocus(bool focus)
    {
        if (gameDataManager == null)
        {
            return;
        }

        Debug.Log("OnApplicationFocus");
        if (gameDataManager.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo)
        {
            recorder.IsRecording = (focus && !isPause);
        }
    }

    void SetKeepObjectEnable(bool isEnable)
    {
        if (isEnable)
        {
            uint layer = (1 << 0);
            for (int i = 0; i < keep_object.Count; i++)
            {
                if (keep_object[i] != null)
                {
                    keep_object[i].renderingLayerMask = layer;
                }
            }
            keep_object.Clear();
        }
        else
        {
            for (int i = 0; i < controllers_main.Length; i++)
            {
                keep_object.AddRange(controllers_main[i].GetComponentsInChildren<Renderer>(true));
            }

            for (int i = 0; i < keep_object.Count; i++)
            {
                if (keep_object[i] != null)
                {
                    keep_object[i].renderingLayerMask = 0;
                }
            }

        }
    }

    void InitSceneCamera()
    {
        camera_main = Camera.main;

        controllers_main = camera_main.transform.parent.GetComponentsInChildren<XRController>();

        renderOriginCam.Init(camera_main.transform, camera_overlay.transform);
    }

    void SetResultSlot(int count)
    {
        if (count == 2)
        {
            userUISlots[0].slot.anchoredPosition = new Vector2(0f, -65f);
            userUISlots[1].slot.anchoredPosition = new Vector2(0f, -290f);

            userUISlots[0].slot.sizeDelta = new Vector2(0f, 150f);
            userUISlots[1].slot.sizeDelta = new Vector2(0f, 150f);

            userUISlots[0].slot.gameObject.SetActive(true);
            userUISlots[1].slot.gameObject.SetActive(true);
        }
        else
        {
            float rate_pos = -520f / count;
            float rate_size = 450f / count;

            for (int i = 0; i < count; i++)
            {
                userUISlots[i].slot.anchoredPosition = new Vector2(0f, i * rate_pos);
                userUISlots[i].slot.sizeDelta = new Vector2(0f, rate_size);
                userUISlots[i].slot.gameObject.SetActive(true);
            }
        }

        for (int i = count; i < userUISlots.Length; i++)
        {
            userUISlots[i].slot.gameObject.SetActive(false);
        }
    }

    bool CheckLoadScene(string sceneName)
    {
        if (sceneName == "Scene_Lobby" || sceneName == "Scene_Customize"
            || sceneName == "Scene_Game_JetSki" || sceneName == "Scene_Game_VolleyBall" || sceneName == "Scene_Game_Fishing"
            || sceneName == "Scene_Game_FlyingDisc" || sceneName == "Scene_Game_Cycling" || sceneName == "Scene_Game_Swimming"
            || sceneName == "Scene_Game_ClayShooting" || sceneName == "Scene_Game_WaterPolo" || sceneName == "Scene_Game_PistolShooting"
            || sceneName == "Scene_Game_Scuba" || sceneName == "Scene_Game_WaterBasketBall"
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetGradeImage(int gradeNum, Image ui_image)
    {
        try
        {
            ui_image.sprite = sprites_grade[gradeNum];
            ui_image.enabled = true;
        }
        catch (Exception)
        {
            ui_image.sprite = sprites_grade[0];
        }
    }

    public static string ConvertTimeScore(float time_sec)
    {
        int min = (int)time_sec / 60;
        int sec = (int)time_sec % 60;
        int ms = (int)(time_sec * 100 % 100);

        return min.ToString("D2") + ":" + sec.ToString("D2") + ":" + ms.ToString("D2");
    }
}


public class ResultUserData
{
    public State state;
    public bool isClear;
    public List<DataSlot> slot = new List<DataSlot>();

    public enum State
    {
        Score, Time
    }

    public class DataSlot
    {
        public string userId;
        public float time_sec;
        public int score;
    }
}
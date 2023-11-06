using Photon.Pun;
using SingletonBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class LobbyUIManager : Singleton<LobbyUIManager>
{
    public LobbyPlayerCtrl lobbyPlayer;

    public bool isStartGame = false;

    public Text text_code;

    private int codeSize_min = 4;
    private int codeSize_max = 4;
    private string inputString = "";

    public Animator anim_mainUI;
    public Animator anim_keyboardUI;
    public Animator anim_otherGameUI;
    public Animator anim_serverUI;
    public Animator anim_voiceChat;

    public GameObject[] gos_ui;

    private bool isActive = false;

    public GameObject button_connect;
    public GameObject go_connect;
    public GameObject go_disconnect;
    public GameObject go_update;
    public GameObject go_roomOut;
    public GameObject button_match_back;
    public GameObject button_match_start;

    public LobbyUIState lobbyUIState = LobbyUIState.Close;

    public Text text_gameType;

    public Text text_time;

    [System.Serializable]
    public class MultiUserSlot
    {
        public Text text_userNick;
        public Text text_userInfo;
        public Image image_grade;
        public Image image_back;
        public GameObject go_lock;
        public GameObject go_wait;
    }

    public List<MultiUserSlot> multiUserSlots = new List<MultiUserSlot>();

    public MeshButtonCtrl[] meshButtons_gameType;

    public MeshButtonCtrl[] meshButtons_level_05;
    public MeshButtonCtrl[] meshButtons_level_10;

    public Material[] materials_lobbyUI;
    public Mesh[] meshes_voiceChat;
    public MeshFilter meshFilter_voiceChat;

    public Sprite[] sprites_ping;
    public Text text_serverName;
    public Text text_ping;
    public Image image_ping;
    public GameObject pref_page;
    private List<GameObject> list_pageIcon = new List<GameObject>();

    public HorizontalLayoutGroup layoutGroup;

    public AchievementCtrl achievement;
    public GameSettingCtrl gameSetting;

    public XRRayInteractor[] rayInteractors;

    public GameObject[] menuButtons;

    [Serializable]
    public class ButtonMesh
    {
        public Mesh[] meshes;
    }

    public MeshFilter[] levelButtons;
    public ButtonMesh[] mesh_levelButtons;

    [Serializable]
    public class ModeButton
    {
        public Transform tr;
        public Text name;
        public MeshFilter meshFilter;
    }

    public ModeButton[] modeButtons;

    public Mesh[] mesh_modeButtons;

    public enum LobbyUIState
    {
        Close, Menu, Mode, Level, InputCode, Random, Friend, Server, RoomOut, GameStart
    }

    private GameDataManager gameDataManager;

    private int[] single_player_count;

    public Sprite[] sprites_match_slot; // 0: 회색, 1:컬러.

    public enum RootState
    {
        None, Single, Random, Friend
    }
    public RootState rootState = RootState.None;

    void Start()
    {
        Physics.autoSimulation = true;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Physics.bounceThreshold = 1f;
        Physics.sleepThreshold = 0.005f;
        Physics.defaultContactOffset = 0.01f;
        Physics.defaultSolverIterations = 6;
        Physics.defaultSolverVelocityIterations = 1;

        gameDataManager = GameDataManager.instance;
        gameDataManager.playType = GameDataManager.PlayType.None;
        single_player_count = new int[11];
        single_player_count[0] = 5;
        single_player_count[1] = 2;
        single_player_count[2] = 1; // 낚시 특수. 인터넷 연결 필수.
        single_player_count[3] = 2;
        single_player_count[4] = 8;
        single_player_count[5] = 5;
        single_player_count[6] = 2;
        single_player_count[7] = 2;
        single_player_count[8] = 3;
        single_player_count[9] = 4;
        single_player_count[10] = 2;

        lobbyUIState = LobbyUIState.Close;
        for (int i = 0; i < gos_ui.Length; i++)
        {
            gos_ui[i].SetActive(false);
        }

        GameSettingCtrl.AddLocalizationChangedEvent(SetModeLocalization);

        gameSetting.View();

        Invoke("DelayInit", 1f);

#if SERVER_CHINA
        MeshButtonCtrl[] meshButtons = anim_serverUI.GetComponentsInChildren<MeshButtonCtrl>();
        for (int i = 0; i < meshButtons.Length; i++)
        {
            meshButtons[i].gameObject.SetActive(false);
        }
#endif

        achievement.gameObject.SetActive(false);

        if (PublicGameUIManager.GetInstance.startViewState == PublicGameUIManager.StartViewState.Disconnect)
        {
            PublicGameUIManager.GetInstance.startViewState = PublicGameUIManager.StartViewState.Idle;
            SetLobbyUI(LobbyUIState.RoomOut);
        }
        else if (PublicGameUIManager.GetInstance.startViewState == PublicGameUIManager.StartViewState.Rematch)
        {
            PublicGameUIManager.GetInstance.startViewState = PublicGameUIManager.StartViewState.Idle;
            if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random)
            {
                SetLobbyUI(LobbyUIState.Random);
            }
            else
            {
                SetLobbyUI(LobbyUIState.Friend);
            }
            PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Back);
            PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Public_Close);
            PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Setting);
            return;
        }

        gameDataManager.gameType = GameDataManager.GameType.None;
        gameDataManager.playType = GameDataManager.PlayType.None;

        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Close);
        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Public_Close);
        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Setting);
    }
    void DelayInit()
    {
        for (int i = 0; i < rayInteractors.Length; i++)
        {
            rayInteractors[i].enabled = true;
        }

        MeshFadeCtrl.instance.StartFade(true);
    }

    public void OpenStartPage()
    {
        GameDataManager.mode = 1;

        LobbySoundManager.GetInstance.Play(4);

        SetLobbyUI(LobbyUIState.Menu);
    }

    public void SetPageLevel()
    {
        //PublicGameUIManager.profileCapture.ShotSingleImages();
        int gameNum = ((int)gameDataManager.gameType) - 1;
        gameDataManager.SetSinglePlayers(single_player_count[gameNum]);

        if (gameDataManager.gameType == GameDataManager.GameType.Cycling)
        {
            LobbySoundManager.GetInstance.Play(4);
            GameDataManager.level = 1;
            gameDataManager.playType = GameDataManager.PlayType.Single;
            SetLobbyUI(LobbyUIState.GameStart);
            return;
        }

        int count = mesh_levelButtons[gameNum].meshes.Length;

        for (int i = 0; i < count; i++)
        {
            levelButtons[i].mesh = mesh_levelButtons[gameNum].meshes[i];
        }

        if (count == 5)
        {
            meshButtons_level_05[0].transform.localPosition = new Vector3(0.2f, -0.03f, 0f);
            meshButtons_level_05[1].transform.localPosition = new Vector3(0.1f, -0.03f, 0f);
            meshButtons_level_05[2].transform.localPosition = new Vector3(0f, -0.03f, 0f);
            meshButtons_level_05[3].transform.localPosition = new Vector3(-0.1f, -0.03f, 0f);
            meshButtons_level_05[4].transform.localPosition = new Vector3(-0.2f, -0.03f, 0f);

            for (int i = 0; i < meshButtons_level_05.Length; i++)
            {
                meshButtons_level_05[i].gameObject.SetActive(true);
            }
        }
        else if (count == 3)
        {
            meshButtons_level_05[0].transform.localPosition = new Vector3(0.15f, -0.03f, 0f);
            meshButtons_level_05[1].transform.localPosition = new Vector3(0f, -0.03f, 0f);
            meshButtons_level_05[2].transform.localPosition = new Vector3(-0.15f, -0.03f, 0f);

            for (int i = 0; i < meshButtons_level_05.Length; i++)
            {
                meshButtons_level_05[i].gameObject.SetActive(count > i);
            }
        }

        LobbySoundManager.GetInstance.Play(4);
        SetLobbyUI(LobbyUIState.Level);
    }

    public void SetLobbyUI(LobbyUIState setState)
    {
        isActive = false;

        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
        }
        stateCoroutine = StartCoroutine(StateCoroutine(setState));
    }

    Coroutine stateCoroutine;

    IEnumerator StateCoroutine(LobbyUIState setState)
    {
        PublicGameUIManager.ClearHotKeyList(0);
        switch (lobbyUIState)
        {
            case LobbyUIState.InputCode:
                {
                    StopPasswordUI();
                    anim_keyboardUI.SetTrigger("OnClose");
                }
                break;
            case LobbyUIState.Random:
                {
                    if (serverChangeCoroutine != null)
                    {
                        StopCoroutine(serverChangeCoroutine);
                    }
                    anim_otherGameUI.SetTrigger("OnClose");
                    anim_serverUI.SetTrigger("OnClose");
                    if (setState != LobbyUIState.GameStart)
                    {
                        LobbyPhotonManager.GetInstance.StartDisconnect();
                        PublicGameUIManager.profileCapture.StopImages();
                    }
                }
                break;
            case LobbyUIState.Friend:
                {
                    if (serverChangeCoroutine != null)
                    {
                        StopCoroutine(serverChangeCoroutine);
                    }
                    anim_serverUI.SetTrigger("OnClose");
                    if (setState != LobbyUIState.GameStart)
                    {
                        LobbyPhotonManager.GetInstance.StartDisconnect();
                        PublicGameUIManager.profileCapture.StopImages();
                    }
                }
                break;
        }
        if (lobbyUIState != LobbyUIState.Close)
        {
            anim_mainUI.SetTrigger("OnClose");
            yield return new WaitForSeconds(0.1f);
            while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
            {
                yield return null;
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                lobbyPlayer.handInfos[i].anim.SetBool("IsPoint", true);
            }
        }

        for (int i = 0; i < gos_ui.Length; i++)
        {
            gos_ui[i].SetActive(false);
        }
        LobbyUIState tempState = lobbyUIState;
        lobbyUIState = setState;

        if (lobbyUIState == LobbyUIState.Close)
        {
            if (LobbyPropManager.GetInstance.keep_prop != null)
            {
                LobbyPropManager.GetInstance.keep_prop.SetIdlePos();
                LobbyPropManager.GetInstance.keep_prop.SetPropState(PropState.Idle);
                LobbyPropManager.GetInstance.keep_prop = null;
                lobbyPlayer.grab_prop_lobby = null;
                gameDataManager.gameType = GameDataManager.GameType.None;
            }
            for (int i = 0; i < 2; i++)
            {
                lobbyPlayer.handInfos[i].anim.SetBool("IsPoint", false);
            }
            PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Close);
            yield break;
        }
        else if (lobbyUIState == LobbyUIState.GameStart)
        {
            anim_mainUI.SetTrigger("OnClose");
            isActive = false;
            yield return new WaitForSeconds(1f);
            StartGame();
            yield break;
        }

        anim_mainUI.SetTrigger("OnOpen");
        yield return null;

        switch (lobbyUIState)
        {
            case LobbyUIState.Menu:
                {
                    menuButtons[0].transform.localPosition = new Vector3(0.15f, -0.03f, 0f);
                    menuButtons[1].transform.localPosition = new Vector3(0f, -0.03f, 0f);
                    menuButtons[2].transform.localPosition = new Vector3(-0.15f, -0.03f, 0f);
                    menuButtons[0].SetActive(true);
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Menu);


                    gos_ui[0].SetActive(true);
                }
                break;
            case LobbyUIState.Mode:
                {
                    switch (gameDataManager.gameType)
                    {
                        case GameDataManager.GameType.Swimming:
                            {
                                modeButtons[0].tr.gameObject.SetActive(true);
                                modeButtons[1].tr.gameObject.SetActive(true);
                                modeButtons[2].tr.gameObject.SetActive(false);

                                modeButtons[0].tr.localPosition = new Vector3(-0.1f, -0.03f, 0f);
                                modeButtons[1].tr.localPosition = new Vector3(0.1f, -0.03f, 0f);

                                modeButtons[0].meshFilter.mesh = mesh_modeButtons[0];
                                modeButtons[1].meshFilter.mesh = mesh_modeButtons[1];

                                SetModeLocalization();
                            }
                            break;
                    }
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Mode);
                    gos_ui[1].SetActive(true);
                }
                break;
            case LobbyUIState.Level:
                {
                    int clearData = gameDataManager.GetClearLevelData();
                    while (clearData == -1)
                    {
                        yield return null;
                        clearData = gameDataManager.GetClearLevelData();
                    }

                    if (false)
                    {
                        for (int i = 0; i < meshButtons_level_10.Length; i++)
                        {
                            if (i <= clearData)
                            {
                                meshButtons_level_10[i].SetMaterial(materials_lobbyUI[1]);
                                meshButtons_level_10[i].SetInteractable(true);
                            }
                            else
                            {
                                meshButtons_level_10[i].SetMaterial(materials_lobbyUI[0]);
                                meshButtons_level_10[i].SetInteractable(false);
                            }
                        }
                        meshButtons_level_05[0].transform.parent.gameObject.SetActive(false);
                        meshButtons_level_10[0].transform.parent.gameObject.SetActive(true);
                    }
                    else
                    {
                        for (int i = 0; i < meshButtons_level_05.Length; i++)
                        {
                            if (i <= clearData)
                            {
                                meshButtons_level_05[i].SetMaterial(materials_lobbyUI[1]);
                                meshButtons_level_05[i].SetInteractable(true);
                            }
                            else
                            {
                                meshButtons_level_05[i].SetMaterial(materials_lobbyUI[0]);
                                meshButtons_level_05[i].SetInteractable(false);
                            }
                        }
                        meshButtons_level_10[0].transform.parent.gameObject.SetActive(false);
                        meshButtons_level_05[0].transform.parent.gameObject.SetActive(true);
                        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Level);
                    }
                   
                    gos_ui[2].SetActive(true);
                }
                break;
            case LobbyUIState.InputCode:
                {
                    inputString = "";
                    GameDataManager.roomCode = inputString;
                    StartPasswordUI();
                    gos_ui[3].SetActive(true);
                    gos_ui[6].SetActive(true);
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_InputCode);
                }
                break;
            case LobbyUIState.Random:
                {
                    gos_ui[5].SetActive(true);

                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        button_connect.SetActive(true);
                        go_connect.SetActive(false);
                        go_disconnect.SetActive(true);
                        go_update.SetActive(false);
                        go_roomOut.SetActive(false);
                        isActive = true;
                        break;
                    }

                    button_connect.SetActive(false);
                    go_connect.SetActive(true);
                    go_disconnect.SetActive(false);
                    go_update.SetActive(false);
                    go_roomOut.SetActive(false);

                    float countTime = 5f;
                    while (gameDataManager.state_userInfo != GameDataManager.UserInfoState.Completed)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbyPhotonManager.GetInstance.ClearServerInfos();
                    LobbyPhotonManager.GetInstance.StartConnect(true);
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    LobbySoundManager.GetInstance.Play(9, true);

                    countTime = 5f;
                    while (!LobbyPhotonManager.GetInstance.isReady)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbySoundManager.GetInstance.Play(4);

                    if (gameDataManager.gameType == GameDataManager.GameType.Fishing && rootState == RootState.Single)
                    {
                        PhotonNetwork.IsSyncScene = true;
                        PhotonNetwork.isLoadLevel = true;

                        GameDataManager.instance.playType = GameDataManager.PlayType.Single;

                        SetLobbyUI(LobbyUIState.GameStart);
                        yield break;
                    }

                    anim_mainUI.SetTrigger("OnClose");
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    gos_ui[5].SetActive(false);
                    gos_ui[4].SetActive(true);
                    RefrashVoiceChatUI();
                    gos_ui[9].SetActive(true);
                    PublicGameUIManager.profileCapture.PlayImages_Match();
                    anim_mainUI.SetTrigger("OnOpen");
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Random);
                    yield return null;
                }
                break;
            case LobbyUIState.Friend:
                {
                    gos_ui[5].SetActive(true);
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        button_connect.SetActive(true);
                        go_connect.SetActive(false);
                        go_disconnect.SetActive(true);
                        go_update.SetActive(false);
                        go_roomOut.SetActive(false);
                        isActive = true;
                        break;
                    }

                    button_connect.SetActive(false);
                    go_connect.SetActive(true);
                    go_disconnect.SetActive(false);
                    go_update.SetActive(false);
                    go_roomOut.SetActive(false);

                    float countTime = 5f;
                    while (gameDataManager.state_userInfo != GameDataManager.UserInfoState.Completed)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbyPhotonManager.GetInstance.ClearServerInfos();
                    LobbyPhotonManager.GetInstance.StartConnect(false);
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    LobbySoundManager.GetInstance.Play(9, true);

                    countTime = 5f;
                    while (!LobbyPhotonManager.GetInstance.isReady)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbySoundManager.GetInstance.Play(4);

                    if (gameDataManager.gameType == GameDataManager.GameType.Fishing && rootState == RootState.Single)
                    {
                        PhotonNetwork.IsSyncScene = true;
                        PhotonNetwork.isLoadLevel = true;

                        GameDataManager.instance.playType = GameDataManager.PlayType.Single;

                        SetLobbyUI(LobbyUIState.GameStart);
                        yield break;
                    }

                    anim_mainUI.SetTrigger("OnClose");
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    gos_ui[5].SetActive(false);
                    gos_ui[4].SetActive(true);
                    RefrashVoiceChatUI();
                    gos_ui[9].SetActive(true);
                    PublicGameUIManager.profileCapture.PlayImages_Match();
                    anim_mainUI.SetTrigger("OnOpen");
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Friend);
                    yield return null;
                }
                break;
            case LobbyUIState.Server:
                {
                    gos_ui[5].SetActive(true);

                    button_connect.SetActive(false);

                    float countTime = 5f;
                    while (gameDataManager.state_userInfo != GameDataManager.UserInfoState.Completed)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbyPhotonManager.GetInstance.StartConnect(LobbyPhotonManager.GetInstance.GetServerInfo().code);
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    LobbySoundManager.GetInstance.Play(9, true);
                    countTime = 5f;
                    while (!LobbyPhotonManager.GetInstance.isReady)
                    {
                        yield return null;
                        if (countTime > 0f)
                        {
                            countTime -= Time.deltaTime;
                        }
                        else if (!button_connect.activeSelf)
                        {
                            button_connect.SetActive(true);
                            isActive = true;
                        }
                    }
                    isActive = false;
                    LobbySoundManager.GetInstance.Play(4);
                    anim_mainUI.SetTrigger("OnClose");
                    yield return new WaitForSeconds(0.1f);
                    while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        yield return null;
                    }
                    gos_ui[5].SetActive(false);
                    gos_ui[4].SetActive(true);
                    RefrashVoiceChatUI();
                    gos_ui[9].SetActive(true);
                    PublicGameUIManager.profileCapture.PlayImages_Match();
                    anim_mainUI.SetTrigger("OnOpen");
                    lobbyUIState = tempState;
                    if (lobbyUIState == LobbyUIState.Random)
                    {
                        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Random);
                    }
                    else
                    {
                        PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Friend);
                    }
                    yield return null;
                }
                break;
            case LobbyUIState.RoomOut:
                {
                    gameDataManager.playType = GameDataManager.PlayType.None;

                    gos_ui[5].SetActive(true);

                    button_connect.SetActive(true);
                    go_connect.SetActive(false);
                    go_disconnect.SetActive(false);
                    go_update.SetActive(false);
                    go_roomOut.SetActive(true);
                    gameDataManager.userInfos.Clear();
                    LobbyPhotonManager.GetInstance.StartDisconnect();
                    PublicGameUIManager.SetHotKeyList(PublicGameUIManager.StateHotKeyList.Lobby_Back);
                }
                break;
        }

        yield return new WaitForSeconds(0.1f);
        while (1f > anim_mainUI.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            yield return null;
        }
        isActive = true;
    }

    public void SetRoomOutUI()
    {
        isActive = false;

        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
        }
    }
    public void SetModeLocalization()
    {
        return;
        switch (gameDataManager.gameType)
        {
            case GameDataManager.GameType.Swimming:
                {
                    modeButtons[0].name.text = GameSettingCtrl.GetLocalizationText("0095");
                    modeButtons[1].name.text = GameSettingCtrl.GetLocalizationText("0006");
                }
                break;
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    StartCoroutine(AppnoriWebRequest.API_Login());
        //}

        if (lobbyUIState == LobbyUIState.Close)
        {
            if (gameDataManager.state_userInfo != GameDataManager.UserInfoState.Completed)
            {
                return;
            }

            if (((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha1)) || ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Keypad1)))
            {
                Debug.Log("GameType - WaterBasketball");
                gameDataManager.gameType = GameDataManager.GameType.WaterBasketball;
                OpenStartPage();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                Debug.Log("GameType - JetSki");
                gameDataManager.gameType = GameDataManager.GameType.JetSki;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                Debug.Log("GameType - BeachVolleyball");
                gameDataManager.gameType = GameDataManager.GameType.BeachVolleyball;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                Debug.Log("GameType - Fishing");
                gameDataManager.gameType = GameDataManager.GameType.Fishing;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                Debug.Log("GameType - FlyingDisc");
                gameDataManager.gameType = GameDataManager.GameType.FlyingDisc;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                Debug.Log("GameType - Cycling");
                gameDataManager.gameType = GameDataManager.GameType.Cycling;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                Debug.Log("GameType - Swimming");
                gameDataManager.gameType = GameDataManager.GameType.Swimming;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                Debug.Log("GameType - ClayShooting");
                gameDataManager.gameType = GameDataManager.GameType.ClayShooting;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                Debug.Log("GameType - WaterPolo");
                gameDataManager.gameType = GameDataManager.GameType.WaterPolo;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
            {
                Debug.Log("GameType - PistolShooting");
                gameDataManager.gameType = GameDataManager.GameType.PistolShooting;
                OpenStartPage();
            }
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                Debug.Log("GameType - Scuba");
                gameDataManager.gameType = GameDataManager.GameType.Scuba;
                OpenStartPage();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("GameType - CharCustom");
                gameDataManager.playType = GameDataManager.PlayType.None;
                MeshFadeCtrl.instance.LoadScene("Scene_Customize");
            }
        }

        if (!isActive)
        {
            return;
        }

        switch (lobbyUIState)
        {     
            case LobbyUIState.Menu:
                {
                    if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))
                    {
                        Debug.Log("Menu - Single");
                        Click_InputKey("Single");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        Debug.Log("Menu - RandomMatch");
                        Click_InputKey("Random");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        Debug.Log("Menu - FriendMatch");
                        Click_InputKey("Code");
                    }
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
            case LobbyUIState.Mode:
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        Debug.Log("Mode - Mode1");
                        Click_InputKey("Mode1");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        Debug.Log("Mode - Mode2");
                        Click_InputKey("Mode2");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        Debug.Log("Mode - Mode3");
                        Click_InputKey("Mode3");
                    }
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
            case LobbyUIState.Level:
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    Debug.Log("Level_1 Start");
                    Click_InputKey("Level1");
                }
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                {
#if !UNITY_EDITOR
                    if (gameDataManager.GetClearLevelData() < 1)
                    {
                        return;
                    }
#endif
                    Debug.Log("Level_2 Start");
                    Click_InputKey("Level2");
                }
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                {
#if !UNITY_EDITOR
                    if (gameDataManager.GetClearLevelData() < 2)
                    {
                        return;
                    }
#endif
                    Debug.Log("Level_3 Start");
                    Click_InputKey("Level3");
                }
                if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                {
#if !UNITY_EDITOR
                    if (gameDataManager.GetClearLevelData() < 3)
                    {
                        return;
                    }
#endif
                    Debug.Log("Level_4 Start");
                    Click_InputKey("Level4");
                }
                if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                {
#if !UNITY_EDITOR
                    if (gameDataManager.GetClearLevelData() < 4)
                    {
                        return;
                    }
#endif
                    Debug.Log("Level_5 Start");
                    Click_InputKey("Level5");
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    Debug.Log("Back State");
                    Click_InputKey("PageBack");
                }
                break;
            case LobbyUIState.InputCode:
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                    {
                        Click_InputKey("0");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        Click_InputKey("1");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        Click_InputKey("2");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        Click_InputKey("3");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        Click_InputKey("4");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                    {
                        Click_InputKey("5");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        Click_InputKey("6");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
                    {
                        Click_InputKey("7");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        Click_InputKey("8");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                    {
                        Click_InputKey("9");
                    }
                    if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        Click_InputKey("Back");
                    }
                    if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                    {
                        Click_InputKey("Enter");
                    }
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
            case LobbyUIState.Random:
                {
#if SERVER_GLOBAL
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        Click_InputKey("ServerB");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        Click_InputKey("ServerN");
                    }
#endif
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
            case LobbyUIState.Friend:
                {
#if SERVER_GLOBAL
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        Click_InputKey("ServerB");
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        Click_InputKey("ServerN");
                    }
#endif
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        Debug.Log("GameStart_Multi");
                        Click_InputKey("GameStart_Multi");
                    }
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
            case LobbyUIState.RoomOut:
                {
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Debug.Log("Back State");
                        Click_InputKey("PageBack");
                    }
                }
                break;
        }
    }

    public void Click_InputKey(string key)
    {
        switch (key)
        {
            case "Exit":
                {
                    gameSetting.SaveData();
                    Application.Quit();
                }
                break;
            case "Setting":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    PublicGameUIManager.GetInstance.Click_InputKey("Setting");
                }
                break;
        }

        if (!isActive)
        {
            return;
        }

        switch (key)
        {
            case "0":
            case "1":
            case "2":
            case "3":
            case "4":
            case "5":
            case "6":
            case "7":
            case "8":
            case "9":
                {
                    SetCodeText(key);
                }
                break;
            case "ServerB":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    SetServerPage(false);
                }
                break;
            case "ServerN":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    SetServerPage(true);
                }
                break;
            case "Back":
                {
                    if (inputString.Length <= 0)
                    {
                        LobbySoundManager.GetInstance.Play(8);
                        return;
                    }
                    LobbySoundManager.GetInstance.Play(6);
                    inputString = inputString.Remove(inputString.Length - 1);
                    StartPasswordUI();
                }
                break;
            case "Enter":
                {
                    if (inputString.Length < codeSize_min)
                    {
                        LobbySoundManager.GetInstance.Play(8);
                        return;
                    }
                    LobbySoundManager.GetInstance.Play(7);
                    GameDataManager.roomCode = inputString;
                    SetLobbyUI(LobbyUIState.Friend);
                }
                break;
            case "Menu":
                {
                    OpenStartPage();
                }
                break;
            case "Single":
                {
                    LobbySoundManager.GetInstance.Play_Voice(13);
                    rootState = RootState.Single;
                    if (false)// 모드가 있는 종목.
                    {
                        LobbySoundManager.GetInstance.Play(4);
                        SetLobbyUI(LobbyUIState.Mode);
                    }
                    else if (gameDataManager.gameType == GameDataManager.GameType.Fishing)
                    {
                        SetLobbyUI(LobbyUIState.Random);
                    }
                    else
                    {
                        Click_InputKey("Level");
                    }
                }
                break;
            case "Level":
                {
                    SetPageLevel();
                }
                break;
            case "Code":
                {
                    GameDataManager.mode = 1;
                    rootState = RootState.Friend;

                    LobbySoundManager.GetInstance.Play_Voice(15);
                    LobbySoundManager.GetInstance.Play(4);              

                    //if (gameDataManager.gameType == GameDataManager.GameType.Swimming)
                    //{
                    //    SetLobbyUI(LobbyUIState.Mode);
                    //}
                    //else
                    //{
                    //    SetLobbyUI(LobbyUIState.InputCode);
                    //}

                     SetLobbyUI(LobbyUIState.InputCode);
                }
                break;
            case "Random":
                {
                    GameDataManager.mode = 1;
                    rootState = RootState.Random;

                    LobbySoundManager.GetInstance.Play_Voice(14);
                    LobbySoundManager.GetInstance.Play(4);

                    //if (gameDataManager.gameType == GameDataManager.GameType.Swimming)
                    //{
                    //    SetLobbyUI(LobbyUIState.Mode);
                    //}
                    //else
                    //{
                    //    SetLobbyUI(LobbyUIState.Random);
                    //}

                    SetLobbyUI(LobbyUIState.Random);

                }
                break;
            case "Close":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    SetLobbyUI(LobbyUIState.Close);
                }
                break;
            case "Mode1":
                {
                    GameDataManager.mode = 1;
                    switch (rootState)
                    {
                        case RootState.Single:
                            {
                                Click_InputKey("Level");
                            }
                            break;
                        case RootState.Random:
                            {
                                SetLobbyUI(LobbyUIState.Random);
                            }
                            break;
                        case RootState.Friend:
                            {
                                SetLobbyUI(LobbyUIState.InputCode);
                            }
                            break;
                    }
                }
                break;
            case "Mode2":
                {
                    GameDataManager.mode = 2;
                    switch (rootState)
                    {
                        case RootState.Single:
                            {
                                Click_InputKey("Level");
                            }
                            break;
                        case RootState.Random:
                            {
                                SetLobbyUI(LobbyUIState.Random);
                            }
                            break;
                        case RootState.Friend:
                            {
                                SetLobbyUI(LobbyUIState.InputCode);
                            }
                            break;
                    }
                }
                break;
            case "Mode3":
                {
                    GameDataManager.mode = 3;
                    switch (rootState)
                    {
                        case RootState.Single:
                            {
                                Click_InputKey("Level");
                            }
                            break;
                        case RootState.Random:
                            {
                                SetLobbyUI(LobbyUIState.Random);
                            }
                            break;
                        case RootState.Friend:
                            {
                                SetLobbyUI(LobbyUIState.InputCode);
                            }
                            break;
                    }
                }
                break;
            case "Level1":
            case "Level2":
            case "Level3":
            case "Level4":
            case "Level5":
            case "Level6":
            case "Level7":
            case "Level8":
            case "Level9":
                {
                    LobbySoundManager.GetInstance.Play(4);

                    int get_level = int.Parse(key.Substring(5));
                    GameDataManager.level = get_level;

                    gameDataManager.playType = GameDataManager.PlayType.Single;

                    LobbySoundManager.GetInstance.Play_Voice(15 + get_level);
                    SetLobbyUI(LobbyUIState.GameStart);
                }
                break;
            case "JetSki":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.JetSki);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "BeachVolleyball":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.BeachVolleyball);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "Fishing":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.Fishing);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "FlyingDisc":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.FlyingDisc);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "Cycling":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.Cycling);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "Swimming":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.Swimming);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "ClayShooting":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.ClayShooting);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "WaterPolo":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.WaterPolo);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "PistolShooting":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.PistolShooting);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "Scuba":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.Scuba);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "WaterBasketball":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetFindRoom(GameDataManager.GameType.WaterBasketball);
                    SetLobbyUI(LobbyUIState.Random);
                }
                break;
            case "GameStart_Multi":
                {
                    //LobbySoundManager.GetInstance.Play(4);
                    LobbyPhotonManager.GetInstance.SetStartTime();
                }
                break;
            case "PageBack":
                {
                    LobbySoundManager.GetInstance.Play(4);
                    switch (lobbyUIState)
                    {
                        case LobbyUIState.Level:
                            {
                                if (false)// 모드가 있는 종목.
                                {
                                    SetLobbyUI(LobbyUIState.Mode);
                                }
                                else
                                {
                                    SetLobbyUI(LobbyUIState.Menu);
                                }
                            }
                            break;
                        case LobbyUIState.Mode:
                            {
                                SetLobbyUI(LobbyUIState.Menu);
                            }
                            break;
                        case LobbyUIState.InputCode:
                        case LobbyUIState.Random:
                        case LobbyUIState.Friend:
                            {
                                SetLobbyUI(LobbyUIState.Menu);
                            }
                            break;
                        default:
                            {
                                SetLobbyUI(LobbyUIState.Close);
                            }
                            break;
                    }
                }
                break;
        }
    }


    public void SetCodeText(string key)
    {
        if (inputString.Length >= codeSize_max)
        {
            LobbySoundManager.GetInstance.Play(8);
            return;
        }

        LobbySoundManager.GetInstance.Play(5);
        inputString += key;

        if (inputString.Length < codeSize_max)
        {
            StartPasswordUI();
        }
        else
        {
            StopPasswordUI();
        }
    }

    public void StartPasswordUI()
    {
        StopPasswordUI();
        coroutine_textAnim = StartCoroutine(TextAnimCoroutine());
    }

    public void StopPasswordUI()
    {
        if (coroutine_textAnim != null)
        {
            StopCoroutine(coroutine_textAnim);
        }
        text_code.text = inputString;
    }

    Coroutine coroutine_textAnim;
    IEnumerator TextAnimCoroutine()
    {
        while (true)
        {
            text_code.text = inputString + "<size=30> </size><size=25>_</size>";
            for (int i = inputString.Length + 1; i < codeSize_max; i++)
            {
                text_code.text += "<size=20>  </size><size=25>*</size>";
            }
            yield return new WaitForSeconds(0.5f);
            text_code.text = inputString + "  ";
            for (int i = inputString.Length + 1; i < codeSize_max; i++)
            {
                text_code.text += "<size=20>  </size><size=25>*</size>";
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SetMatchBackButton(bool isActive)
    {
        button_match_back.SetActive(isActive);
    }
    public void SetMatchStartButton(bool isActive)
    {
        button_match_start.SetActive(isActive);
    }

    public void SetOtherGames(bool isActive)
    {
        if (isActive)
        {
            gos_ui[7].transform.localScale = Vector3.zero;
        }
        gos_ui[7].SetActive(isActive);
    }

    public bool IsOtherGames()
    {
        return gos_ui[7].activeSelf;
    }

    public void SetVoiceChatUI(bool isActive)
    {
        if (isActive)
        {
            gos_ui[9].transform.localScale = Vector3.zero;
        }
        gos_ui[9].SetActive(isActive);
    }

    public void RefrashVoiceChatUI()
    {
        if (GameSettingCtrl.settingInfo.on_sound_voice)
        {
            meshFilter_voiceChat.sharedMesh = meshes_voiceChat[1];
        }
        else
        {
            meshFilter_voiceChat.sharedMesh = meshes_voiceChat[0];
        }
    }

    public void Click_VoiceChatUI()
    {
        gameSetting.SetOnVoiceChat(!GameSettingCtrl.settingInfo.on_sound_voice);
    }

    public void SetServerUI(bool isActive)
    {
        if (isActive)
        {
            gos_ui[8].transform.localScale = Vector3.zero;
        }
        gos_ui[8].SetActive(isActive);
    }

    public void InitServerUI()
    {
        LobbyPhotonManager.GetInstance.index_server = 0;
        LobbyPhotonManager.ServerInfo serverInfo = LobbyPhotonManager.GetInstance.GetServerInfo();
        text_serverName.text = serverInfo.name;
        text_serverName.rectTransform.sizeDelta = new Vector2(text_serverName.preferredWidth, 80f);
        text_ping.text = serverInfo.ping;
        image_ping.sprite = sprites_ping[serverInfo.grade];
        pref_page.SetActive(true);

        for (int i = list_pageIcon.Count - 1; i >= 0; i--)
        {
            Destroy(list_pageIcon[i]);
        }

        list_pageIcon.Clear();

        for (int i = 0; i < LobbyPhotonManager.GetInstance.serverInfos.Length; i++)
        {
            list_pageIcon.Add(Instantiate(pref_page, pref_page.transform.parent));
        }

        pref_page.SetActive(false);
        SetServerPageIcon();
    }

    public void SetServerPageIcon()
    {
        int index = LobbyPhotonManager.GetInstance.index_server;
        for (int i = 0; i < list_pageIcon.Count; i++)
        {
            if (index == i)
            {
                list_pageIcon[i].transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                list_pageIcon[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SetServerPage(bool isNext)
    {
        int index = LobbyPhotonManager.GetInstance.index_server;
        if (isNext)
        {
            index += 1;
        }
        else
        {
            index -= 1;
        }

        if (index > LobbyPhotonManager.GetInstance.serverInfos.Length - 1)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = LobbyPhotonManager.GetInstance.serverInfos.Length - 1;
        }
        LobbyPhotonManager.GetInstance.index_server = index;
        LobbyPhotonManager.ServerInfo serverInfo = LobbyPhotonManager.GetInstance.GetServerInfo();
        text_serverName.text = serverInfo.name;
        text_serverName.rectTransform.sizeDelta = new Vector2(text_serverName.preferredWidth, 80f);
        text_ping.text = serverInfo.ping;
        image_ping.sprite = sprites_ping[serverInfo.grade];

        SetServerPageIcon();
        StartServerChange();
    }

    public void StartServerChange()
    {
        if (serverChangeCoroutine != null)
        {
            StopCoroutine(serverChangeCoroutine);
        }
        LobbyPhotonManager.GetInstance.StartDisconnect();
        serverChangeCoroutine = StartCoroutine(ServerChangeCoroutine());
    }

    Coroutine serverChangeCoroutine;

    IEnumerator ServerChangeCoroutine()
    {
        yield return new WaitForSeconds(3f);
        SetLobbyUI(LobbyUIState.Server);
    }

    public void SetViewGameType()
    {
        switch (gameDataManager.gameType)
        {
            case GameDataManager.GameType.JetSki:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0060");
                break;
            case GameDataManager.GameType.BeachVolleyball:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0061");
                break;
            case GameDataManager.GameType.Fishing:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0062");
                break;
            case GameDataManager.GameType.FlyingDisc:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0063");
                break;
            case GameDataManager.GameType.Cycling:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0064");
                break;
            case GameDataManager.GameType.Swimming:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0065");
                break;
            case GameDataManager.GameType.ClayShooting:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0070");
                break;
            case GameDataManager.GameType.WaterPolo:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0071");
                break;
            case GameDataManager.GameType.PistolShooting:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0077");
                break;
            case GameDataManager.GameType.Scuba:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0078");
                break;
            case GameDataManager.GameType.WaterBasketball:
                text_gameType.text = GameSettingCtrl.GetLocalizationText("0079");
                break;
        }
    }

    public void SetViewMatchText()
    {
        text_time.text = GameSettingCtrl.GetLocalizationText("0170");
    }


    // 0:로드 1:컬러 -1:잠김.
    public void SetMatchBackUI(int index ,int stateNum)
    {
        Debug.Log(index + " 들어옴");
        switch (stateNum)
        {
            case 0:
                {
                    multiUserSlots[index].image_back.sprite = sprites_match_slot[0];
                    multiUserSlots[index].go_wait.SetActive(true);
                    multiUserSlots[index].go_lock.SetActive(false);
                    multiUserSlots[index].text_userNick.text = GameSettingCtrl.GetLocalizationText("0035");
                    multiUserSlots[index].text_userInfo.text = "";
                    multiUserSlots[index].image_grade.gameObject.SetActive(false);
                }
                break;
            case 1:
                {
                    multiUserSlots[index].image_back.sprite = sprites_match_slot[1];
                    multiUserSlots[index].go_wait.SetActive(false);
                    multiUserSlots[index].go_lock.SetActive(false);
                    multiUserSlots[index].image_grade.gameObject.SetActive(true);
                }
                break;
            default:
                {
                    multiUserSlots[index].image_back.sprite = sprites_match_slot[0];
                    multiUserSlots[index].go_wait.SetActive(false);
                    multiUserSlots[index].go_lock.SetActive(true);
                    multiUserSlots[index].text_userNick.text = "";
                    multiUserSlots[index].text_userInfo.text = "";
                    multiUserSlots[index].image_grade.gameObject.SetActive(false);
                }
                break;
        }
    }

    public void StartGame()
    {
        if (isStartGame)
        {
            return;
        }

        isStartGame = true;

        LobbySoundManager.GetInstance.Play_Voice(21);

        if (gameDataManager.gameType == GameDataManager.GameType.Fishing)
        {
            MeshFadeCtrl.instance.LoadScene("Scene_Game_Fishing");
            return;
        }

        if (gameDataManager.playType == GameDataManager.PlayType.Multi && !PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            MeshFadeCtrl.instance.StartFade(false);
            return;
        }


        switch (gameDataManager.gameType)
        {
            case GameDataManager.GameType.JetSki:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_JetSki");
                }
                break;
            case GameDataManager.GameType.BeachVolleyball:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_VolleyBall");
                }
                break;
            case GameDataManager.GameType.Fishing:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_Fishing");
                }
                break;
            case GameDataManager.GameType.FlyingDisc:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_FlyingDisc");
                }
                break;
            case GameDataManager.GameType.Cycling:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_Cycling");
                }
                break;
            case GameDataManager.GameType.Swimming:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_Swimming");
                }
                break;
            case GameDataManager.GameType.ClayShooting:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_ClayShooting");
                }
                break;
            case GameDataManager.GameType.WaterPolo:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_WaterPolo");
                }
                break;
            case GameDataManager.GameType.PistolShooting:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_PistolShooting");
                }
                break;
            case GameDataManager.GameType.Scuba:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_Scuba");
                }
                break;
            case GameDataManager.GameType.WaterBasketball:
                {
                    MeshFadeCtrl.instance.LoadScene("Scene_Game_WaterBasketBall");
                }
                break;
        }

        if (gameDataManager.gameType != GameDataManager.GameType.Fishing && PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

}

using Photon.Pun;
using SingletonPunBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Linq;
using Photon.Voice.PUN;

public class LobbyPhotonManager : Singleton<LobbyPhotonManager>
{
#if TEST_ACCOUNT && SERVER_GLOBAL
    const string PHOTON_REALTIME_APP_ID = "1461c7e4-ff3c-42ac-b727-7c1fa3b5a130";
    const string PHOTON_VOICE_APP_ID = "88e446b6-5bfd-4a1e-bec7-a31049308f60";
#elif TEST_ACCOUNT && SERVER_CHINA
    const string PHOTON_REALTIME_APP_ID = "9d293725-ce9f-4fbf-89ae-d297758592c7";//"e6360d31-7e59-4a4c-9b3c-d23a36148367";
    const string PHOTON_VOICE_APP_ID = "9d293725-ce9f-4fbf-89ae-d297758592c7";//"82adc176-1123-4cda-98e8-5895f79492ee";
#elif SERVER_CHINA
    const string PHOTON_REALTIME_APP_ID = "9d293725-ce9f-4fbf-89ae-d297758592c7";
    const string PHOTON_VOICE_APP_ID = "9d293725-ce9f-4fbf-89ae-d297758592c7";//"bc67eaa0-eded-418c-ba53-c528744c5481";
#else
    const string PHOTON_REALTIME_APP_ID = "1461c7e4-ff3c-42ac-b727-7c1fa3b5a130";
    const string PHOTON_VOICE_APP_ID = "88e446b6-5bfd-4a1e-bec7-a31049308f60";
#endif
    public bool isReady = false;

    private List<RoomInfo> keep_roomInfos;

    private GameDataManager.GameType find_gameType = GameDataManager.GameType.None;
    
    public class ServerInfo
    {
        public string name;
        public string code;
        public string ping;
        public int grade;
    }

    public ServerInfo[] serverInfos;
    public int index_server = 0;

    public int[] match_player_count;
    public int[] set_player_count;

    public int startTime = 10;
    public int countTime;

    public PhotonSubClient subClient;

    // Start is called before the first frame update
    void Start()
    {
        match_player_count = new int[11];
        match_player_count[0] = 5; //JetSki.
        match_player_count[1] = 2; //BeachVolleyBall.
        match_player_count[2] = 3; //Fishing.
        match_player_count[3] = 3; //FlyingDisc.
        match_player_count[4] = 5; //Cycling.
        match_player_count[5] = 5; //Swimming.
        match_player_count[6] = 3; //ClayShooting.
        match_player_count[7] = 2; //WaterPolo.
        match_player_count[8] = 3; //GunShooting.
        match_player_count[9] = 4; //Scuba.
        match_player_count[10] = 2; //WaterBasketBall.

        set_player_count = new int[11];
        set_player_count[0] = 5; //JetSki.
        set_player_count[1] = 2; //BeachVolleyBall.
        set_player_count[2] = 3; //Fishing.
        set_player_count[3] = 3; //FlyingDisc.
        set_player_count[4] = 8; //Cycling.
        set_player_count[5] = 5; //Swimming.
        set_player_count[6] = 3; //ClayShooting.
        set_player_count[7] = 2; //WaterPolo.
        set_player_count[8] = 3; //GunShooting.
        set_player_count[9] = 4; //Scuba.
        set_player_count[10] = 2; //WaterBasketBall.

        PhotonNetwork.SerializationRate = 10;
        PhotonNetwork.SendRate = 20;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 1f;
		subClient = new PhotonSubClient();
        StartDisconnect();
    }

    public void StartConnect(bool isRandom)
    {
        GameDataManager.instance.photonMatchState = (isRandom ? GameDataManager.PhotonMatchState.Random : GameDataManager.PhotonMatchState.Friend);
        StartConnect();
    }

    public void StartConnect(string regionCode = "")
    {
        isReady = false;

        if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random)
        {
#if DB_TEST
            countTime = 15;
#else
            countTime = 30;
#endif
        }
        else
        {
            countTime = 7;
        }

        PhotonNetwork.LocalPlayer.NickName = GameDataManager.instance.userInfo_mine.nick;
        PhotonNetwork.LocalPlayer.CustomProperties = new ExitGames.Client.Photon.Hashtable();
        PhotonNetwork.LocalPlayer.CustomProperties.Add("AppnoriID", GameDataManager.instance.userInfo_mine.id);

        PhotonNetwork.LocalPlayer.CustomProperties.Add("GameScore", Newtonsoft.Json.JsonConvert.SerializeObject(GameDataManager.instance.userInfo_mine.gameScores));
        PhotonNetwork.LocalPlayer.CustomProperties.Add("SeatIndex", -1);

        PhotonNetwork.LocalPlayer.CustomProperties.Add("ModelData", JsonUtility.ToJson(GameDataManager.instance.GetMyCustomModelData()));

        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PHOTON_REALTIME_APP_ID;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = PHOTON_VOICE_APP_ID;

#if DB_TEST
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "test_aio_summer";
#else
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Application.version + "_aio_summer";
#endif
#if SERVER_CHINA
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
        PhotonNetwork.PhotonServerSettings.AppSettings.Server = "47.94.234.45";
        PhotonNetwork.PhotonServerSettings.AppSettings.Port = 5055;
        //PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        //PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
        //PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
        //PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0;
#else
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = regionCode;
        PhotonNetwork.PhotonServerSettings.AppSettings.Server = "";
        PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0;
#endif
        PhotonNetwork.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
    }

    private void Update()
    {
        subClient.ServiceClient();
    }

    public void StartDisconnect()
    {
        isReady = false;
        PhotonNetwork.IsSyncScene = false;
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            PhotonNetwork.Disconnect();
        }
    	if (subClient.IsConnect())
        {
            subClient.Disconnect();
        }
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        LobbyUIManager.GetInstance.text_time.text = "";
    }

    public void ClearServerInfos()
    {
        serverInfos = null;
    }

    public override void OnConnectedToMaster()
    {
		Debug.Log("OnConnectedToMaster");
#if SERVER_CHINA
        serverInfos = new ServerInfo[1];
        serverInfos[0] = new ServerInfo();
        serverInfos[0].code = "cn";
        serverInfos[0].name = GetServerName(serverInfos[0].code);

        serverInfos[0].ping = PhotonNetwork.GetPing() + "ms";
        if (PhotonNetwork.GetPing() >= 200)
        {
            serverInfos[0].grade = 2;
        }
        else if (PhotonNetwork.GetPing() >= 100)
        {
            serverInfos[0].grade = 1;
        }
        else
        {
            serverInfos[0].grade = 0;
        }
        LobbyUIManager.GetInstance.InitServerUI();

#else
        if (serverInfos == null && string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion))
        {
            serverInfos = new ServerInfo[PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions.Count];
            for (int i = 0; i < serverInfos.Length; i++)
            {
                serverInfos[i] = new ServerInfo();
                serverInfos[i].code = PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions[i].Code;
                serverInfos[i].name = GetServerName(serverInfos[i].code);
                serverInfos[i].ping = PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions[i].Ping + "ms";
                if (PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions[i].Ping >= 200)
                {
                    serverInfos[i].grade = 2;
                }
                else if (PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions[i].Ping >= 100)
                {
                    serverInfos[i].grade = 1;
                }
                else
                {
                    serverInfos[i].grade = 0;
                }
            }
            LobbyUIManager.GetInstance.InitServerUI();
        }
#endif

        if (LobbyUIManager.GetInstance.rootState == LobbyUIManager.RootState.Single)
        {
            isReady = true;
            return;
        }

        PhotonNetwork.JoinLobby();
        subClient.Connect();
    }

    public string GetServerName(string code)
    {
        switch (code.ToLower())
        {
            case "us":
                return "USA";
            case "eu":
                return "Europe";
            case "ru":
                return "Russia";
            case "sa":
                return "<size=40>S.America</size>";
            case "kr":
                return "Korea";
            case "asia":
                return "Asia";
            case "cn":
                return "";
            default:
                return code;
        }
    }

    public void SetFindRoom(GameDataManager.GameType gameType)
    {
        isReady = false;
        find_gameType = gameType;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("RoomListUpdate");

        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random)
        {
            for (int i = 0; i < LobbyUIManager.GetInstance.meshButtons_gameType.Length; i++)
            {
                LobbyUIManager.GetInstance.meshButtons_gameType[i].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[0]);
                LobbyUIManager.GetInstance.meshButtons_gameType[i].SetInteractable(false);
            }

            for (int i = 0; i < roomList.Count; i++)
            {
                //Debug.LogError(roomList[i].Name + " : " + roomList[i].PlayerCount + " / " + roomList[i].MaxPlayers + " / " +roomList[i].IsOpen + " / " +(roomList[i].CustomProperties.TryGetValue("GameType",out object value) ? (GameDataManager.GameType)roomList[i].CustomProperties["GameType"] : ""));


                if (roomList[i].CustomProperties == null || !roomList[i].CustomProperties.ContainsKey("GameType") || !roomList[i].CustomProperties.ContainsKey("Code"))
                {
                    continue;
                }

                if ((string)roomList[i].CustomProperties["Code"] != "" || roomList[i].PlayerCount == roomList[i].MaxPlayers || roomList[i].IsOpen == false)
                {
                    continue;
                }

                if ((GameDataManager.GameType)roomList[i].CustomProperties["GameType"] == GameDataManager.instance.gameType)
                {
                    continue;
                } 

                switch ((GameDataManager.GameType)roomList[i].CustomProperties["GameType"])
                {
                    case GameDataManager.GameType.JetSki:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[0].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[0].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.BeachVolleyball:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[1].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[1].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.Fishing:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[2].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[2].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.FlyingDisc:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[3].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[3].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.Cycling:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[4].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[4].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.Swimming:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[5].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[5].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.ClayShooting:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[6].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[6].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.WaterPolo:
                        {
                            LobbyUIManager.GetInstance.meshButtons_gameType[7].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            LobbyUIManager.GetInstance.meshButtons_gameType[7].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.PistolShooting:
                        {
                            //LobbyUIManager.GetInstance.meshButtons_gameType[8].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            //LobbyUIManager.GetInstance.meshButtons_gameType[8].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.Scuba:
                        {
                            //LobbyUIManager.GetInstance.meshButtons_gameType[9].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            //LobbyUIManager.GetInstance.meshButtons_gameType[9].SetInteractable(true);
                        }
                        break;
                    case GameDataManager.GameType.WaterBasketball:
                        {
                            //LobbyUIManager.GetInstance.meshButtons_gameType[10].SetMaterial(LobbyUIManager.GetInstance.materials_lobbyUI[1]);
                            //LobbyUIManager.GetInstance.meshButtons_gameType[10].SetInteractable(true);
                        }
                        break;
                }
            }

            if (PhotonNetwork.InRoom)
            {
                return;
            }
            GameDataManager.GameType gameType = find_gameType;
            find_gameType = GameDataManager.GameType.None;

            keep_roomInfos = roomList;

            if (gameType == GameDataManager.GameType.None)
            {
                for (int i = 0; i < roomList.Count; i++)
                {

                    //Debug.LogError(roomList[i].Name);
                    //Debug.LogError((GameData_DDOL.GameType)roomList[i].CustomProperties["GameType"]);
                    if (roomList[i].CustomProperties == null || !roomList[i].CustomProperties.ContainsKey("GameType") || !roomList[i].CustomProperties.ContainsKey("Code") || !roomList[i].IsOpen)
                    {
                        continue;
                    }

                    if ((GameDataManager.GameType)roomList[i].CustomProperties["GameType"] == GameDataManager.instance.gameType
                        && (string)roomList[i].CustomProperties["Code"] == ""
                        && roomList[i].PlayerCount != roomList[i].MaxPlayers)
                    {
                        PhotonNetwork.JoinRoom(roomList[i].Name);
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < roomList.Count; i++)
                {
                    if (roomList[i].CustomProperties == null || !roomList[i].CustomProperties.ContainsKey("GameType") || !roomList[i].CustomProperties.ContainsKey("Code"))
                    {
                        continue;
                    }

                    if ((GameDataManager.GameType)roomList[i].CustomProperties["GameType"] == gameType
                        && (string)roomList[i].CustomProperties["Code"] == ""
                        && roomList[i].PlayerCount != roomList[i].MaxPlayers)
                    {
                        GameDataManager.instance.gameType = gameType;
                        PhotonNetwork.JoinRoom(roomList[i].Name);
                        return;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].CustomProperties == null || !roomList[i].CustomProperties.ContainsKey("GameType") || !roomList[i].CustomProperties.ContainsKey("Code"))
                {
                    continue;
                }

                if ((string)roomList[i].CustomProperties["Code"] == GameDataManager.roomCode && roomList[i].PlayerCount != roomList[i].MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(roomList[i].Name);
                    return;
                }
            }
        }
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        isReady = true;

        GameObject go = PhotonNetwork.Instantiate("Networks/PhotonVoicePref", Vector3.zero, Quaternion.identity);

        if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random)
        {
            SetStartTime();
        }

        //PhotonNetwork.Instantiate("Networks/PhotonVoicePref", Vector3.zero, Quaternion.identity);
        UpdateRoomInfo();
    }

    public void SetStartTime()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties["StartTime"] = PhotonNetwork.ServerTimestamp;
            customProperties["GameReady"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            //PhotonNetwork.CurrentRoom.CustomProperties.Add("StartTime", PhotonNetwork.ServerTimestamp);
            //PhotonNetwork.CurrentRoom.SetCustomProperties(PhotonNetwork.CurrentRoom.CustomProperties);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.JoinLobby();
    }

    void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.PublishUserId = true;
        int gameNum = ((int)GameDataManager.instance.gameType) - 1;
        roomOptions.MaxPlayers = (byte)set_player_count[gameNum];
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("GameType", GameDataManager.instance.gameType);

        switch (GameDataManager.instance.gameType)
        {
            case GameDataManager.GameType.JetSki:
            case GameDataManager.GameType.Cycling:
            {
                    roomOptions.CleanupCacheOnLeave = false;
                }
                break;
        }
        if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Friend)
        {
            roomOptions.CustomRoomProperties.Add("Code", GameDataManager.roomCode);
        }
        else
        {
            roomOptions.CustomRoomProperties.Add("Code", "");
        }
        roomOptions.CustomRoomProperties.Add("GameReady", false);
        roomOptions.CustomRoomProperties.Add("StartTime", PhotonNetwork.ServerTimestamp);
        roomOptions.CustomRoomProperties.Add("RandomLevel", (int)UnityEngine.Random.Range(1f,3.9999f));

        roomOptions.CustomRoomPropertiesForLobby = new string[] { "GameType", "Code", "StartTime", "GameReady", "RandomLevel" };

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomInfo();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomInfo();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdateRoomInfo();
    }

    void UpdateRoomInfo()
    {
        if (PhotonNetwork.IsSyncScene && PhotonNetwork.isLoadLevel)
        {
            return;
        }
        Debug.Log("UpdateRoomInfo()");
        GameDataManager.instance.gameType = (GameDataManager.GameType)PhotonNetwork.CurrentRoom.CustomProperties["GameType"];
        startTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"];

        int match_max_count = match_player_count[((int)GameDataManager.instance.gameType) - 1];

        for (int i = match_max_count; i < 5; i++)
        {
            Debug.Log(i);
            LobbyUIManager.GetInstance.SetMatchBackUI(i, -1);
        }

        LobbyUIManager.GetInstance.SetViewGameType();

        int count = 0;
        GameDataManager.instance.userInfos.Clear();

        foreach (var player in PhotonNetwork.CurrentRoom.Players.OrderBy(i => i.Value.ActorNumber))
        {
            GameDataManager.UserInfo userInfo = new GameDataManager.UserInfo();

            userInfo.id = (string)player.Value.CustomProperties["AppnoriID"];
            userInfo.nick = player.Value.NickName;
            userInfo.gameScores = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppnoriWebRequest.GameScore>>((string)player.Value.CustomProperties["GameScore"]);
            userInfo.customModelData = JsonUtility.FromJson<CustomModelData>((string)player.Value.CustomProperties["ModelData"]);

            GameDataManager.instance.userInfos.Add(userInfo);


            LobbyUIManager.GetInstance.multiUserSlots[count].text_userNick.text = userInfo.nick;

            int gameNum = (int)GameDataManager.instance.gameType;
            try
            {
                LobbyUIManager.GetInstance.multiUserSlots[count].text_userInfo.text = userInfo.gameScores[gameNum].totLaddrPoint.ToString();
                PublicGameUIManager.GetInstance.SetGradeImage(userInfo.gameScores[gameNum].userGrade, LobbyUIManager.GetInstance.multiUserSlots[count].image_grade);
            }
            catch (Exception)
            {
                LobbyUIManager.GetInstance.multiUserSlots[count].text_userInfo.text = "Null";
                PublicGameUIManager.GetInstance.SetGradeImage(0, LobbyUIManager.GetInstance.multiUserSlots[count].image_grade);
            }

            LobbyUIManager.GetInstance.multiUserSlots[count].image_grade.gameObject.SetActive(true);
            //PublicGameUIManager.profileCapture.PlayImages_Match();
            LobbyUIManager.GetInstance.SetMatchBackUI(count, 1);
            count++;
        }

        //LobbyUIManager.GetInstance.SetMultiUserProfile(false, 0);
        //LobbyUIManager.GetInstance.SetMultiUserProfile(false, 1);
        

        if (count < match_max_count)
        {
            for (int i = count; i < match_max_count; i++)
            {
                LobbyUIManager.GetInstance.SetMatchBackUI(i, 0);
            }

            if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random)
            {
                //GameDataManager.instance.userInfos[1].id = "AI";
                //GameDataManager.instance.userInfos[1].nick = GameDataManager.instance.userInfos[1].id;
                //StartTimeCoroutine(false);
            }
            else
            {
                LobbyUIManager.GetInstance.text_time.text = "";
            }

            StartTimeCoroutine(false);
        }
        else
        {
            //PublicGameUIManager.profileCapture.PlayImages(new string[2] { GameDataManager.userInfos[0].id, GameDataManager.userInfos[1].id }, ProfileCaptureCtrl.ShotState.Multi);
            StartTimeCoroutine(true);
        }
    }

    public void StartTimeCoroutine(bool isMatch)
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        timeCoroutine = StartCoroutine(TimeCoroutine(isMatch));
    }

    Coroutine timeCoroutine;

    IEnumerator TimeCoroutine(bool isMatch)
    {
        if (!isMatch)
        {
            while (true)
            {
                LobbyUIManager.GetInstance.SetMatchBackButton(true);
                LobbyUIManager.GetInstance.SetOtherGames(GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Random);
                LobbyUIManager.GetInstance.SetServerUI(true);
                LobbyUIManager.GetInstance.SetVoiceChatUI(true);

                if (GameDataManager.instance.photonMatchState == GameDataManager.PhotonMatchState.Friend)
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        LobbyUIManager.GetInstance.SetMatchStartButton(GameDataManager.instance.userInfos.Count > 1);
                    }
                    else
                    {
                        LobbyUIManager.GetInstance.SetMatchStartButton(false);
                    }                
                }
                else
                { 
                    LobbyUIManager.GetInstance.SetMatchStartButton(false);                 
                }

                LobbyUIManager.GetInstance.text_time.text = "";

                yield return null;

                WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

                while (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["GameReady"])
                {
                    //Debug.LogError("GameReady");
                    yield return waitForSeconds;
                }
                LobbyUIManager.GetInstance.SetMatchStartButton(false);
                while (true)
                {
                    int timeCount = ((startTime - PhotonNetwork.ServerTimestamp) / 1000) + countTime;
                    //Debug.LogError(timeCount);
                    if (Math.Abs(timeCount) > countTime && PhotonNetwork.IsMasterClient)
                    {
                        //Debug.LogError("들어옴.");
                        //Debug.LogError(Math.Abs(timeCount));
                        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                        startTime = PhotonNetwork.ServerTimestamp;
                        customProperties["StartTime"] = startTime;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
                        yield return waitForSeconds;
                        continue;
                    }
                    else if (Math.Abs(timeCount) > countTime && !PhotonNetwork.IsMasterClient)
                    {
                        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                        startTime = (int)customProperties["StartTime"];
                        yield return waitForSeconds;
                        continue;
                    }
                    else if (timeCount <= 5 && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen)
                    {
                        PhotonNetwork.CurrentRoom.IsOpen = false;
                        
                        //if (GameDataManager.instance.gameType == GameDataManager.GameType.Fishing && GameDataManager.instance.userInfos.Count < 2)
                        //{
                        //    LobbyUIManager.GetInstance.SetMatchBackButton(false);
                        //    PhotonNetwork.LeaveRoom();
                        //    subClient.Disconnect();
                        //    yield break;
                        //}
                    }

                    if (LobbyUIManager.GetInstance.IsOtherGames() && timeCount <= 5)
                    {
                        LobbyUIManager.GetInstance.SetOtherGames(false);
                    }

                    LobbyUIManager.GetInstance.text_time.text = timeCount.ToString();
                    if (Math.Abs(timeCount) < countTime && timeCount <= 0)
                    {                       
                        break;
                    }
                    yield return null;
                }

                if (GameDataManager.instance.userInfos.Count > 0)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.CurrentRoom.IsOpen = false;
                    }
                    StartTimeCoroutine(true);
                    yield break;
                }
            }
        }
        else
        {
            PhotonNetwork.IsSyncScene = true;
            PhotonNetwork.isLoadLevel = true;
            int gameNum = ((int)GameDataManager.instance.gameType) - 1;
            LobbyUIManager.GetInstance.SetMatchBackButton(false);
            LobbyUIManager.GetInstance.SetMatchStartButton(false);
            LobbyUIManager.GetInstance.SetOtherGames(false);
            LobbyUIManager.GetInstance.SetServerUI(false);
            LobbyUIManager.GetInstance.SetVoiceChatUI(true);

            LobbyUIManager.GetInstance.text_gameType.text = "";
            //PhotonNetwork.IsMessageQueueRunning = false;
            LobbyUIManager.GetInstance.text_time.text = "";
            yield return new WaitForSecondsRealtime(1.5f);
            PublicGameUIManager.profileCapture.StopImages();
            if (GameDataManager.instance.userInfos.Count >= 2)
            {
                GameDataManager.instance.playType = GameDataManager.PlayType.Multi;
            }
            else
            {
                GameDataManager.instance.playType = GameDataManager.PlayType.Multi_Solo;
            }

            switch (GameDataManager.instance.gameType)
            {
                case GameDataManager.GameType.Fishing:
                case GameDataManager.GameType.FlyingDisc:
                case GameDataManager.GameType.ClayShooting:
                case GameDataManager.GameType.PistolShooting:
                    {
                        if (GameDataManager.instance.userInfos.Count < 2)
                        {
                            GameDataManager.instance.SetMultiPlayers(set_player_count[gameNum]);
                        }
                    }
                    break;
                default:
                    {
                        GameDataManager.instance.SetMultiPlayers(set_player_count[gameNum]);
                    }
                    break;
            }

            LobbyUIManager.GetInstance.SetLobbyUI(LobbyUIManager.LobbyUIState.GameStart);
            subClient.Disconnect();
        }
    }

    public ServerInfo GetServerInfo()
    {
        return serverInfos[index_server];
    }
}

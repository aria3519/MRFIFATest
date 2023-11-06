using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

#if (OCULUS_PLATFORM && UNITY_EDITOR)
    private string appID = "5429869110369319";
    private string access_token = "OC|5429869110369319|6501160978a1cdfd91606d6edbd14b17";
#elif (OCULUS_PLATFORM && UNITY_ANDROID)
    private string appID = "5252906291483483";
    private string access_token = "OC|5252906291483483|cf9c97dbf092d2e4bdfb4456ff5bccfd";
#elif OCULUS_PLATFORM
    private string appID = "5429869110369319";
    private string access_token = "OC|5429869110369319|6501160978a1cdfd91606d6edbd14b17";
#elif STEAMWORKS
    private uint appID = 2074750;
    private string api_key = "DA01EA6D90AC59E23FF55A62AEA21B0A";
    protected bool m_bInitialized = false;
    protected static bool s_EverInitialized = false;
    protected Steamworks.SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    protected Steamworks.Callback<Steamworks.UserStatsReceived_t> m_UserStatsReceived;
#endif

    //public static List<AppnoriWebRequest.GameScore> gameScores;

    public static string origin_nick_mine;
    public static string image_url_mine;
    private static string imageCachePath;
    private static SHA1CryptoServiceProvider s_SHA1 = new SHA1CryptoServiceProvider();

    public enum UserInfoState
    {
        Stop, Start_Platform_UserInfo, Success_Platform_UserInfo, Failed_Platform_UserInfo, Start_DB_UserInfo, Success_DB_UserInfo, Failed_DB_UserInfo, Start_DB_ModelData, Success_DB_ModelData, Failed_DB_ModelData, Completed
    }

    public UserInfoState state_userInfo;


    [System.Serializable]
    public class AchievementInfo
    {
        public string api_name;
        public bool is_archived;
        public string title;
        public string description;
        public string unlocked_description_override;
        public string image_uri;
        public int velue_current;
        public int velue_target;
    }

    public Dictionary<string, AchievementInfo> dict_achieve;

    [System.Serializable]
    public struct UserInfo
    {
        public string id;
        public string nick;
        public List<AppnoriWebRequest.GameScore> gameScores;
        //public int ladder_point;
        //public int grade;
        public CustomModelData customModelData;
    }

    public UserInfo userInfo_mine = new UserInfo();
    public List<UserInfo> userInfos = new List<UserInfo>();

    public static string game_guid = "";

    public enum UnlockAchieveState
    {
        Add, Change
    }

#if OCULUS_PLATFORM
 [System.Serializable]
    public class AchievementInfo_Oculus
    {
        public OculusData[] data;

        [System.Serializable]
        public class OculusData
        {
            public string api_name;
            public int target;
            public string title;
            public string description;
            public string unlocked_description_override;
            public string unlocked_image_uri;
        }
    }
#elif STEAMWORKS

    [System.Serializable]
    public class AchievementInfo_Steam
    {
        public SteamData game;

        [System.Serializable]
        public class SteamData
        {
            public AvailableGameStats availableGameStats;
        }

        [System.Serializable]
        public class AvailableGameStats
        {
            public Achievement[] achievements;
        }
        [System.Serializable]
        public class Achievement
        {
            public string name;
            public string displayName;
            public string description;
            public string icon;
        }
    }

    [System.Serializable]
    public class PlayerData_Steam
    {
        public Response response;

        [System.Serializable]
        public class Response
        {
            public Player[] players;
        }

        [System.Serializable]
        public class Player
        {
            public string avatar;
        }
    }
#endif
    public enum VRDevice
    {
        NONE, OCULUS, INDEX, VIVE, PICO_3, PICO_4, Other
    }

    public static VRDevice vrDevice = VRDevice.NONE;

    public enum GameType { None, JetSki, BeachVolleyball, Fishing, FlyingDisc, Cycling, Swimming, ClayShooting, WaterPolo, PistolShooting, Scuba, WaterBasketball, Customize }
    public enum SaveGameType {JetSki_1, BeachVolleyball_1, Fishing_1, FlyingDisc_1, Cycling_1, Swimming_1, ClayShooting_1, WaterPolo_1, PistolShooting_1, Scuba_1, WaterBasketball_1 }
    public enum PlayType { None, Single, Multi, Multi_Solo }

    public GameType gameType = GameType.None;
    public PlayType playType = PlayType.None;

    public enum PhotonMatchState { Random, Friend }

    public PhotonMatchState photonMatchState;

    public static string roomCode = "";

    // Level 1~5.
    public static int level { get; set; } = 1;
    // Mode 1~.
    public static int mode { get; set; } = 1;

    public static int[] clearLevelDatas;

    [System.Serializable]
    public class TestAccount
    {
        public string user_id = "";// = "TESTLEEJAEHEUNG1";
        public string user_nick = "";// = "TEST012345";
        public string user_profile;// = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06896f8cb324e3a683a229cd8a066d56075c6548.jpg";
    }

    [System.Serializable]
    public class TestOption
    {
        public bool isPicoPC = false;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
#if DB_OPERATION
            Application.logMessageReceived += HandleLog;
#endif
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        //StartCoroutine(CheckVRDevice());

        state_userInfo = UserInfoState.Stop;
        gameType = GameType.None;
        playType = PlayType.None;

        level = 1;
        mode = 1;

        userInfo_mine = new UserInfo();
        userInfo_mine.id = "Player";
        userInfo_mine.nick = userInfo_mine.id;
        origin_nick_mine = userInfo_mine.id;
        userInfo_mine.customModelData = GetLocalModelData();
        userInfos = new List<UserInfo>();

        char[] charArray = PlayerPrefs.GetString("clearLevelData", "0000000000").ToCharArray();
        clearLevelDatas = new int[System.Enum.GetNames(typeof(SaveGameType)).Length];
        for (int i = 0; i < clearLevelDatas.Length; i++)
        {
            if (i >= charArray.Length)
            {
                clearLevelDatas[i] = 0;
            }
            else
            {
                clearLevelDatas[i] = (int)char.GetNumericValue(charArray[i]);
            }
        }

        StartPlatformInfo();
    }

    IEnumerator CheckVRDevice()
    {

#if UNITY_EDITOR
        if (GetTestOption().isPicoPC)
        {
            vrDevice = VRDevice.PICO_4;
            XRController.posVec = new Vector3(0, 0.03f, -0.02f);
            XRController.rotQ = Quaternion.Euler(-70f, 0f, 0f);
            yield break;
        }
#endif
        vrDevice = VRDevice.NONE;
        XRController.posVec = Vector3.zero;
        XRController.rotQ = Quaternion.identity;

        WaitForSeconds waitForSeconds1 = new WaitForSeconds(1f);

        while (vrDevice == VRDevice.NONE)
        {
            yield return waitForSeconds1;

            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevices(devices);

            foreach (var item in devices)
            {
                string deviceName = item.name.ToLower();
                //Debug.Log(deviceName);
                //Debug.Log(item.characteristics);
                //Debug.Log(item.serialNumber);
                if (deviceName.Contains("oculus"))
                {
                    vrDevice = VRDevice.OCULUS;
#if STEAMWORKS
                    XRController.posVec = new Vector3(0, 0f, 0f);
                    XRController.rotQ = Quaternion.Euler(-15f, 0f, 0f);
#else
                    XRController.posVec = new Vector3(0, 0.03f, -0.03f);
                    XRController.rotQ = Quaternion.Euler(-80f, 0f, 0f);
#endif
                    break;
                }

                if (deviceName.Contains("index"))
                {
                    vrDevice = VRDevice.INDEX;
                    XRController.posVec = new Vector3(0, 0.0f, 0.02f);
                    XRController.rotQ = Quaternion.Euler(-25f, 0f, 0f);
                    break;
                }

                if (deviceName.Contains("vive"))
                {
                    vrDevice = VRDevice.VIVE;
                    XRController.posVec = new Vector3(0, 0.01f, 0.01f);
                    XRController.rotQ = Quaternion.Euler(-30f, 0f, 0f);
                    break;
                }

                if (deviceName.Contains("pico"))
                {
#if PICO_PLATFORM
                    switch (Unity.XR.PXR.PXR_Input.GetControllerDeviceType())
                    {
                        case Unity.XR.PXR.PXR_Input.ControllerDevice.PICO_4:
                            {
                                vrDevice = VRDevice.PICO_4;
                            }
                            break;
                        default:
                            {
                                vrDevice = VRDevice.PICO_3;
                            }
                            break;
                    }
#else
                    vrDevice = VRDevice.PICO_3;
#endif
                    XRController.posVec = new Vector3(0, 0.03f, -0.02f);
                    XRController.rotQ = Quaternion.Euler(-70f, 0f, 0f);
                    break;
                }
            }
        }

        Debug.Log("VRDevice : " + vrDevice);
    }

    public void StartPlatformInfo()
    {
        state_userInfo = UserInfoState.Start_Platform_UserInfo;
#if TEST_ACCOUNT
        Debug.Log("Start TEST.");

        TestAccount account = GetTestAccount();

        userInfo_mine.id = account.user_id;
        userInfo_mine.nick = account.user_nick;
        origin_nick_mine = account.user_nick;
        image_url_mine = account.user_profile;

        LobbyDebugCtrl.AddText(userInfo_mine.id);
        LobbyDebugCtrl.AddText(userInfo_mine.nick);

        UserLogin();
        //StartCoroutine(AppnoriWebRequest.API_Login());
#elif OCULUS_PLATFORM
        Debug.Log("Start OCULUS_PLATFORM.");

        try
        {
            Oculus.Platform.Core.AsyncInitialize(appID).OnComplete((Oculus.Platform.Message<Oculus.Platform.Models.PlatformInitialize> msg) =>
            {
                if (msg.IsError)
                {
                    Debug.LogError("Init failed.");
                }
                else
                {
                    Debug.Log("Init success.");
                }
            });


            Oculus.Platform.Users.GetLoggedInUser().OnComplete((Oculus.Platform.Message<Oculus.Platform.Models.User> msg) =>
            {            
                if (msg.IsError)
                {   
                    Debug.LogError("GetUserInfo failed." + msg.GetError().Message);
                }
                else // User passed entitlement check
                {
                    Debug.Log("GetUserInfo success.");
                    state_userInfo = UserInfoState.Success_Platform_UserInfo;
                    userInfo_mine = new UserInfo();
                    userInfo_mine.id = "OC" + msg.Data.ID.ToString();
                    userInfo_mine.nick = GetSubtractNick(msg.Data.OculusID);
                    origin_nick_mine = msg.Data.OculusID;
                    image_url_mine = msg.Data.ImageURL;
                    if (string.IsNullOrWhiteSpace(image_url_mine))
                    {
                        image_url_mine = "Null";
                    }
                    LobbyDebugCtrl.AddText(userInfo_mine.id);
                    LobbyDebugCtrl.AddText(userInfo_mine.nick);

                    UserLogin();
                    //StartCoroutine(AppnoriWebRequest.API_Login());

                    getAchieveListCoroutine = StartCoroutine(GetAchieveList());
                }

            });
            Oculus.Platform.Request.RunCallbacks();
        }
        catch (UnityException e)
        {
            Debug.LogError("OCULUS_PLATFORM failed.");
            throw;
        }
#elif STEAMWORKS
        Debug.Log("Start STEAMWORKS.");
        if (!Steamworks.Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!Steamworks.DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (Steamworks.SteamAPI.RestartAppIfNecessary((Steamworks.AppId_t)appID))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        m_bInitialized = Steamworks.SteamAPI.Init();
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            getAchieveListCoroutine = StartCoroutine(GetAchieveList());
        }

        m_UserStatsReceived = Steamworks.Callback<Steamworks.UserStatsReceived_t>.Create(OnUserStatsReceived);
        Steamworks.SteamUserStats.RequestCurrentStats();

        s_EverInitialized = true;
        state_userInfo = UserInfoState.Success_Platform_UserInfo;

        userInfo_mine = new UserInfo();
        userInfo_mine.id = "ST" + Steamworks.SteamUser.GetSteamID().m_SteamID;
        userInfo_mine.nick = GetSubtractNick(Steamworks.SteamFriends.GetPersonaName());
        origin_nick_mine = Steamworks.SteamFriends.GetPersonaName();

        StartCoroutine(SetSteamPlayerAvatar());

#elif PICO_PLATFORM
        try
        {
            Pico.Platform.CoreService.AsyncInitialize().OnComplete(m =>
            {
                if (m.IsError)
                {
                    Debug.LogError($"Async initialize failed: code={m.GetError().Code} message={m.GetError().Message}");
                    return;
                }

                if (m.Data != Pico.Platform.PlatformInitializeResult.Success && m.Data != Pico.Platform.PlatformInitializeResult.AlreadyInitialized)
                {
                    Debug.LogError($"Async initialize failed: result={m.Data}");
                    return;
                }

                Debug.Log("AsyncInitialize Successfully");
                Pico.Platform.UserService.GetLoggedInUser().OnComplete(msg =>
                {
                    if (!msg.IsError)
                    {
                        Debug.Log("Received get user success");
                        state_userInfo = UserInfoState.Success_Platform_UserInfo;

                        Pico.Platform.Models.User user = msg.Data;
                        userInfo_mine.id = "PI" + user.ID;
                        userInfo_mine.nick = GetSubtractNick(user.DisplayName);
                        origin_nick_mine = user.DisplayName;
                        image_url_mine = user.ImageUrl;

                        LobbyDebugCtrl.AddText(userInfo_mine.id);
                        LobbyDebugCtrl.AddText(userInfo_mine.nick);

                        UserLogin();
                        //StartCoroutine(AppnoriWebRequest.API_Login());
                    }
                    else
                    {
                        Debug.LogError("Received get user error");
                        state_userInfo = UserInfoState.Failed_Platform_UserInfo;
                        Pico.Platform.Models.Error error = msg.GetError();
                        Debug.LogError("Error: " + error.Message);
                    }
                });

            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Async Initialize Failed:{e}");
            return;
        }
#endif
    }

    void Update()
    {
#if STEAMWORKS
        if (!m_bInitialized)
        {
            return;
        }

        Steamworks.SteamAPI.RunCallbacks();
#endif

#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.Home))
        //{
        //    Steamworks.SteamUserStats.ResetAllStats(true);
        //}
#endif
    }

    Coroutine getAchieveListCoroutine;
    public void StartGetAchieveList()
    {
        if (getAchieveListCoroutine != null)
        {
            StopCoroutine(getAchieveListCoroutine);
        }
        getAchieveListCoroutine = StartCoroutine(GetAchieveList());
    }


#if OCULUS_PLATFORM

    IEnumerator GetAchieveList()
    {
        Debug.Log("GetAchieveList()");
        string url = "https://graph.oculus.com/" + appID + "/achievement_definitions?access_token=" + access_token + "&fields=api_name,title,description,unlocked_description_override,locked_image_uri,unlocked_image_uri,target";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                AchievementInfo_Oculus jsonAchievement = JsonUtility.FromJson<AchievementInfo_Oculus>(www.downloadHandler.text);
                System.Array.Reverse(jsonAchievement.data);
                for (int i = 0; i < jsonAchievement.data.Length; i++)
                {
                    Debug.Log(jsonAchievement.data[i].api_name);
                }
                Oculus.Platform.Achievements.GetAllProgress().OnComplete((Oculus.Platform.Message<Oculus.Platform.Models.AchievementProgressList> msg) =>
                {
                    Debug.Log("Achievements.GetAllProgress()");
                    if (msg.IsError)
                    {
                        Debug.LogError("Platform: GetLoggedInUser() failed. Reason: " + msg.GetError().Message);
                        return;
                    }
                    else
                    {

                        Oculus.Platform.Models.AchievementProgress[] arr_achievement = msg.Data.ToArray();

                        int achieveCount = jsonAchievement.data.Length;
                        dict_achieve = new Dictionary<string, AchievementInfo>();

                        for (int i = 0; i < achieveCount; i++)
                        {
                            AchievementInfo achieve = new AchievementInfo();

                            achieve.api_name = jsonAchievement.data[i].api_name;
                            achieve.title = jsonAchievement.data[i].title;

                            achieve.description = jsonAchievement.data[i].description;
                            achieve.unlocked_description_override = jsonAchievement.data[i].unlocked_description_override;
                            achieve.image_uri = jsonAchievement.data[i].unlocked_image_uri;

                            achieve.velue_target = jsonAchievement.data[i].target;

                            for (int j = 0; j < arr_achievement.Length; j++)
                            {
                                if (arr_achievement[j].Name.Contains(achieve.api_name))
                                {
                                    achieve.is_archived = arr_achievement[j].IsUnlocked;
                                    achieve.velue_current = (int)arr_achievement[j].Count;
                                    break;
                                }
                            }

                            dict_achieve.Add(achieve.api_name, achieve);
                        }

                        if (SceneManager.GetActiveScene().name == "Scene_Lobby")
                        {
                            //StartCoroutine(AchievementCtrl.instance.SetAchieve());
                        }
                    }
                }
                );

            }
        }
    }

    //IEnumerator RemoveAchieve()
    //{
    //    List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
    //    formData.Add(new MultipartFormDataSection("access_token","OC|1079856925433694|9b61de3ed698ea1843d1d3c474ab1d98"));
    //    formData.Add(new MultipartFormDataSection("user_id","1136955046380875"));
    //    UnityWebRequest www = UnityWebRequest.Post("https://graph.oculus.com/achievement_remove_all", formData);
    //    yield return www.SendWebRequest();

    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        Debug.Log(www.error);
    //    }
    //    else
    //    {
    //        Debug.Log("Form upload complete!");
    //    }
    //}
#elif STEAMWORKS
    public IEnumerator SetSteamPlayerAvatar()
    {
        string check_url = CheckCacheData("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + api_key + "&steamids=" + userInfo_mine.id.Substring(2));
        //Debug.LogError(check_url);

        UnityWebRequest www = UnityWebRequest.Get(check_url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            image_url_mine = "n";
        }
        else
        {
            //Debug.Log(www.downloadHandler.text);
            image_url_mine = JsonUtility.FromJson<PlayerData_Steam>(www.downloadHandler.text).response.players[0].avatar;
        }

        LobbyDebugCtrl.AddText(userInfo_mine.id);
        LobbyDebugCtrl.AddText(userInfo_mine.nick);

        UserLogin();
        //StartCoroutine(AppnoriWebRequest.API_Login());
    }



    private void OnUserStatsReceived(Steamworks.UserStatsReceived_t pCallback)
    {
        //if (Steamworks.EResult.k_EResultOK == pCallback.m_eResult)
        //{
        //StartCoroutine(GetAchieveList());
        //}

        StartGetAchieveList();
    }


    IEnumerator GetAchieveList()
    {
        while (GameSettingCtrl.localizationInfos == null || GameSettingCtrl.localizationInfos.Count == 0)
        {
            yield return null;
        }
        
        string url = "https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key=" + api_key + "&appid=" + appID + "&l=" + GameSettingCtrl.GetUILanguage();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            string jsonText = File.ReadAllText(Application.dataPath + "/AchieveInfo.json");
            SetSteamAchieve(JsonUtility.FromJson<AchievementInfo_Steam>(jsonText));
        }
        else
        {
            Debug.Log(www.url);
            //Debug.Log(www.downloadHandler.text);

            File.WriteAllText(Application.dataPath + "/AchieveInfo.json", www.downloadHandler.text);

            SetSteamAchieve(JsonUtility.FromJson<AchievementInfo_Steam>(www.downloadHandler.text));
        }
        }
    }

    public void SetSteamAchieve(AchievementInfo_Steam achievementInfo)
    {
        uint achieveCount = 0;
        if (achievementInfo.game.availableGameStats.achievements != null)
        {
            achieveCount = (uint)achievementInfo.game.availableGameStats.achievements.Length;
        }
        
        dict_achieve = new Dictionary<string, AchievementInfo>();

        for (uint i = 0; i < achieveCount; i++)
        {
            AchievementInfo achieve = new AchievementInfo();

            achieve.api_name = achievementInfo.game.availableGameStats.achievements[i].name;
            Steamworks.SteamUserStats.GetAchievement(achieve.api_name, out achieve.is_archived);

            achieve.title = achievementInfo.game.availableGameStats.achievements[i].displayName;

            string[] arr_string = achievementInfo.game.availableGameStats.achievements[i].description.Split('_');
            achieve.description = arr_string[0];
            if (arr_string.Length > 1)
            {
                achieve.unlocked_description_override = arr_string[1];
            }
            else
            {
                achieve.unlocked_description_override = arr_string[0];
            }

            arr_string = achieve.api_name.Split('_');

            if (arr_string.Length > 1)
            {
                Steamworks.SteamUserStats.GetStat(arr_string[1], out achieve.velue_current);
                achieve.velue_target = int.Parse(arr_string[2]);
            }

            for (int j = 0; j < achievementInfo.game.availableGameStats.achievements.Length; j++)
            {
                if (achievementInfo.game.availableGameStats.achievements[j].name.Contains(achieve.api_name))
                {
                    achieve.image_uri = achievementInfo.game.availableGameStats.achievements[j].icon;
                    break;
                }
            }

            dict_achieve.Add(arr_string[0], achieve);
        }

        if (SceneManager.GetActiveScene().name == "Scene_Lobby")
        {
            //StartCoroutine(AchievementCtrl.instance.SetAchieve());
        }
    }

    protected virtual void OnEnable()
    {
        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to receive warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
            m_SteamAPIWarningMessageHook = new Steamworks.SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            Steamworks.SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

#else
    IEnumerator GetAchieveList()
    {
        yield return null;
    }
#endif
    public void UserLogin(bool isRe = false)
    {
        StartCoroutine(AppnoriWebRequest.API_Login(isRe));
    }

    public void UserRegister()
    {
        StartCoroutine(AppnoriWebRequest.API_Register());
    }

    public void Upload_UserCustomMedel()
    {
        StartCoroutine(AppnoriWebRequest.API_SetCustomCharData());
    }

    public void Download_UserCustomMedel()
    {
        StartCoroutine(AppnoriWebRequest.API_GetCustomCharData());
    }

    public void Upload_UserData(string _nickNm = "", string _avatarUrl = "")
    {
        StartCoroutine(AppnoriWebRequest.API_SetUserInfo(_nickNm, _avatarUrl));
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            StartCoroutine(AppnoriWebRequest.API_ErrorLog(logString, stackTrace));
        }
    }

    public void Check_DisconnectLog()
    {
        string dis_check = PlayerPrefs.GetString("isDisCheck", "");
        if (dis_check == "")
        {
            state_userInfo = UserInfoState.Completed;
        }
        else
        {
            StartCoroutine(AppnoriWebRequest.API_GameResult(JsonUtility.FromJson<AppnoriWebRequest.Request_GameResult>(dis_check)));
        }      
    }

    public string GetAchieveInfo(string achieveName)
    {
        if (dict_achieve == null || dict_achieve.Count == 0)
        {
            return "";
        }
        if (!dict_achieve.ContainsKey(achieveName))
        {
            //Debug.LogError("dict_achieve - " + achieveName);
            return "";
        }
        return dict_achieve[achieveName].description;
    }

    public bool IsUnlockAchieve(string achieveName)
    {
        if (dict_achieve == null || dict_achieve.Count == 0)
        {
            return false;
        }
        if (!dict_achieve.ContainsKey(achieveName))
        {
            //Debug.LogError("dict_achieve - " + achieveName);
            return false;
        }
        return dict_achieve[achieveName].is_archived;
    }

    public void UnlockAchieve(string achieveName, int velue, UnlockAchieveState unlockAchieveState = UnlockAchieveState.Add)
    {
#if UNITY_EDITOR
        return;
#endif

#if OCULUS_PLATFORM

        switch (achieveName)
        {
            case "Ach04":
            case "Ach05":
            case "Ach06":
            case "Ach07":
                {
                    achieveName = "Ach04";
                }
                break;
        }

        if (dict_achieve.ContainsKey(achieveName))
        {
            if (dict_achieve[achieveName].is_archived)
            {
                return;
            }

            if (dict_achieve[achieveName].velue_target == 0)
            {
                Oculus.Platform.Achievements.Unlock(dict_achieve[achieveName].api_name);
                dict_achieve[achieveName].is_archived = true;
                return;
            }

            int velue_previous = dict_achieve[achieveName].velue_current;
            if (unlockAchieveState == UnlockAchieveState.Add)
            {
                dict_achieve[achieveName].velue_current = Mathf.Clamp(dict_achieve[achieveName].velue_current + velue, 0, dict_achieve[achieveName].velue_target);
            }
            else
            {
                if (dict_achieve[achieveName].velue_current >= velue)
                {
                    return;
                }
                dict_achieve[achieveName].velue_current = Mathf.Clamp(velue, 0, dict_achieve[achieveName].velue_target);
            }

            Oculus.Platform.Achievements.AddCount(achieveName, (ulong)dict_achieve[achieveName].velue_current);

            if (dict_achieve[achieveName].velue_current >= dict_achieve[achieveName].velue_target)
            {
                dict_achieve[achieveName].is_archived = true;
                Oculus.Platform.Achievements.Unlock(achieveName);
            }
        }

#elif STEAMWORKS
        if (dict_achieve.ContainsKey(achieveName))
        {
            if (dict_achieve[achieveName].is_archived)
            {
                return;
            }
            if (dict_achieve[achieveName].velue_target == 0)
            {
                Steamworks.SteamUserStats.SetAchievement(dict_achieve[achieveName].api_name);
                dict_achieve[achieveName].is_archived = true;

                if (SceneManager.GetActiveScene().name == "Scene_Lobby")
                {
                    StartCoroutine(AchievementCtrl.instance.SetAchieve());
                }

                return;
            }

            int velue_previous = dict_achieve[achieveName].velue_current;
            if (unlockAchieveState == UnlockAchieveState.Add)
            {
                dict_achieve[achieveName].velue_current = Mathf.Clamp(dict_achieve[achieveName].velue_current + velue, 0, dict_achieve[achieveName].velue_target);
            }
            else
            {
                dict_achieve[achieveName].velue_current = Mathf.Clamp(velue, 0, dict_achieve[achieveName].velue_target);
            }
            //dict_achieve[achieveName].velue_current = Mathf.Clamp(dict_achieve[achieveName].velue_current + velue, 0, dict_achieve[achieveName].velue_target);
            Steamworks.SteamUserStats.SetStat(dict_achieve[achieveName].api_name.Split('_')[1], dict_achieve[achieveName].velue_current);

            if (dict_achieve[achieveName].velue_current >= dict_achieve[achieveName].velue_target)
            {
                dict_achieve[achieveName].is_archived = true;
            }

            if (SceneManager.GetActiveScene().name == "Scene_Lobby")
            {
                if (unlockAchieveState == UnlockAchieveState.Add || (unlockAchieveState == UnlockAchieveState.Change && velue_previous != dict_achieve[achieveName].velue_current))
                {
                    StartCoroutine(AchievementCtrl.instance.SetAchieve());
                }
            }
        }
#endif
    }

    // N : �⺻, Y: ��Ŀ��Ʈ, E: ����.
    public void SetResultScore(List<AppnoriWebRequest.Request_GameResult.Infos> playerInfos, string resultState = "N")
    {
        if (setMultiResultScoreCoroutine != null)
        {
            StopCoroutine(setMultiResultScoreCoroutine);
        }
        AppnoriWebRequest.Request_GameResult gameResult = new AppnoriWebRequest.Request_GameResult();

        gameResult.gameResult = playerInfos;
        gameResult.gameTitleNum = AppnoriWebRequest.GetGameTitleNum();

        gameResult.gameId = game_guid;
        gameResult.gameLvl = level;
        gameResult.discontYn = resultState;
        setMultiResultScoreCoroutine = StartCoroutine(AppnoriWebRequest.API_GameResult(gameResult));
    }

    Coroutine setMultiResultScoreCoroutine;

    public void SetImage(string url, RawImage set_ui_image)
    {
        set_ui_image.texture = null;
        if (url == null || url.Length < 10)
        {
            return;
        }
        StartCoroutine(SetImageCoroutine(url, set_ui_image));
    }

    IEnumerator SetImageCoroutine(string url, RawImage set_ui_image)
    {
        string check_url = CheckCacheData(url);
        Texture2D texture = new Texture2D(2, 2);

        if (!check_url.Contains("http://") && !check_url.Contains("https://"))
        {
            //Debug.LogError(check_url);
            texture.LoadImage(File.ReadAllBytes(check_url));
            set_ui_image.texture = texture;
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(check_url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            //Debug.Log(www.downloadHandler.text);

            byte[] results = www.downloadHandler.data;
            if (check_url.Contains(url))
            {
                File.WriteAllBytes(GetCachePath(url), results);
                //textttttt.text = "ó��";
            }
            else
            {
                //textttttt.text = "����";
            }

            texture.LoadImage(results);
            set_ui_image.texture = texture;
        }
    }

    public static string GetCachePath(string url)
    {
        if (imageCachePath == null)
            imageCachePath = Application.temporaryCachePath + "/Cache/";

        if (!Directory.Exists(imageCachePath))
            Directory.CreateDirectory(imageCachePath);

        return imageCachePath + System.Convert.ToBase64String(s_SHA1.ComputeHash(UTF8Encoding.Default.GetBytes(url))).Replace('/', '_');
    }

    public static string CheckCacheData(string url)
    {
        string cachePath = GetCachePath(url);
        FileInfo fileInfo = new FileInfo(cachePath);
        if (fileInfo.Exists)
        {
            return cachePath;
        }
        else
        {
            return url;
        }
    }

    private void OnDestroy()
    {
#if STEAMWORKS
        if (!m_bInitialized)
        {
            return;
        }
        Steamworks.SteamAPI.Shutdown();
#endif
    }

    public string GetUserNick(string userId)
    {
        for (int i = 0; i < userInfos.Count; i++)
        {
            if (userInfos[i].id == userId)
            {
                return userInfos[i].nick;
            }
        }

        return "NULL";
    }

    public CustomModelData GetUserModelData(string userId)
    {
        for (int i = 0; i < userInfos.Count; i++)
        {
            if (userInfos[i].id == userId)
            {
                return userInfos[i].customModelData;
            }
        }
        return CustomModelSettingCtrl.GetRandomModelData();
    }

    public void SetRandomLevel()
    {
        level = (int)UnityEngine.Random.Range(1f, 5.9999f);
    }


    void CheckClearLevelDatas(int findIndex)
    {
        if (findIndex >= clearLevelDatas.Length)
        {
            int[] new_clearLevelDatas = new int[findIndex + 1];

            for (int i = 0; i < clearLevelDatas.Length; i++)
            {
                new_clearLevelDatas[i] = clearLevelDatas[i];
            }
            clearLevelDatas = new_clearLevelDatas;
        }
    }
    public void SetClearLevelData()
    {
        if (playType != PlayType.Single)
        {
            return;
        }

        string[] names_saveGameType = System.Enum.GetNames(typeof(SaveGameType));
        string find_name = System.Enum.GetName(typeof(GameType), gameType) + "_" + mode;
        int index = 0;
        for (int i = 0; i < names_saveGameType.Length; i++)
        {
            if (names_saveGameType[i] == find_name)
            {
                index = i;
            }
        }

        CheckClearLevelDatas(index);

        if (clearLevelDatas[index] < level)
        {
            clearLevelDatas[index] = level;
            PlayerPrefs.SetString("clearLevelData", string.Join("", clearLevelDatas));
            Upload_UserData();
        }
    }

    public void SetClearLevelData(int level)
    {
        if (playType != PlayType.Single)
        {
            return;
        }

        string[] names_saveGameType = System.Enum.GetNames(typeof(SaveGameType));
        string find_name = System.Enum.GetName(typeof(GameType), gameType) + "_" + mode;
        int index = 0;
        for (int i = 0; i < names_saveGameType.Length; i++)
        {
            if (names_saveGameType[i] == find_name)
            {
                index = i;
            }
        }

        CheckClearLevelDatas(index);

        if (clearLevelDatas[index] < level)
        {
            clearLevelDatas[index] = level;
            PlayerPrefs.SetString("clearLevelData", string.Join("", clearLevelDatas));
            Upload_UserData();
        }
    }

    public int GetClearLevelData()
    {
        try
        {
            string[] names_saveGameType = System.Enum.GetNames(typeof(SaveGameType));
            string find_name = System.Enum.GetName(typeof(GameType), gameType) + "_" + mode;
            int index = -1;

            for (int i = 0; i < names_saveGameType.Length; i++)
            {
                if (names_saveGameType[i] == find_name)
                {
                    index = i;
                }
            }

            if (index != -1)
            {
                return clearLevelDatas[index];
            }
            else
            {
                Debug.LogError("None");
                return -1;
            }
        }
        catch
        {
            Debug.LogError("None");
            return -1;
        }

    }

    public CustomModelData GetCustomModelData(int playerNum)
    {
        return userInfos[playerNum].customModelData;
    }

    public CustomModelData GetMyCustomModelData()
    {
        return userInfo_mine.customModelData;
    }

    public void SetMyCustomModelData(CustomModelData customModelData)
    {
        if (customModelData == null)
        {
            userInfo_mine.customModelData = GetLocalModelData();
            return;
        }

        userInfo_mine.customModelData = customModelData;
    }

    public CustomModelData GetLocalModelData()
    {
        CustomModelData modelData = new CustomModelData();
        modelData.Gender = PlayerPrefs.GetString("gender", "m");

        switch (modelData.Gender)
        {
            case "m":
                {
                    modelData.ID_Face_I = PlayerPrefs.GetString("id_item_face_m", "1000");

                    modelData.ID_Hair_I = PlayerPrefs.GetString("id_item_hair_m", "2000");

                    modelData.ID_Wear_I = PlayerPrefs.GetString("id_item_wear_m", "3000");

                    modelData.ID_Acc_I = PlayerPrefs.GetString("id_item_acc_m", "4000");

                    float b = UnityEngine.Random.Range(0.1f, 0.7f);

                    float g = UnityEngine.Random.Range(b, b + 0.15f);

                    float r = Mathf.Clamp01(UnityEngine.Random.Range(g + 0.1f, g + 0.5f));

                    modelData.Hex_Skin_C = PlayerPrefs.GetString("hex_color_skin_m", ColorUtility.ToHtmlStringRGB(new Color(r, g, b)));

                    Color hairColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    modelData.Hex_Hair_C = PlayerPrefs.GetString("hex_color_hair_m", ColorUtility.ToHtmlStringRGB(hairColor));
                    modelData.Hex_Eyebrow_C = PlayerPrefs.GetString("hex_color_eyebrow_m", ColorUtility.ToHtmlStringRGB(Color.Lerp(hairColor, Color.black, UnityEngine.Random.Range(0f, 1f))));

                    modelData.Hex_Eye_C = PlayerPrefs.GetString("hex_color_eye_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));

                    modelData.Hex_Upper_C = PlayerPrefs.GetString("hex_color_upper_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Lower_C = PlayerPrefs.GetString("hex_color_lower_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Foot_C = PlayerPrefs.GetString("hex_color_foot_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Pattern_C = PlayerPrefs.GetString("hex_color_pattern_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Acc_C = PlayerPrefs.GetString("hex_color_acc_m", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                }
                break;
            case "f":
                {
                    modelData.ID_Face_I = PlayerPrefs.GetString("id_item_face_f", "1000");

                    modelData.ID_Hair_I = PlayerPrefs.GetString("id_item_hair_f", "2000");

                    modelData.ID_Wear_I = PlayerPrefs.GetString("id_item_wear_f", "3000");

                    modelData.ID_Acc_I = PlayerPrefs.GetString("id_item_acc_f", "4000");

                    float b = UnityEngine.Random.Range(0.1f, 0.7f);

                    float g = UnityEngine.Random.Range(b, b + 0.15f);

                    float r = Mathf.Clamp01(UnityEngine.Random.Range(g + 0.1f, g + 0.5f));

                    modelData.Hex_Skin_C = PlayerPrefs.GetString("hex_color_skin_f", ColorUtility.ToHtmlStringRGB(new Color(r, g, b)));

                    Color hairColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    modelData.Hex_Hair_C = PlayerPrefs.GetString("hex_color_hair_f", ColorUtility.ToHtmlStringRGB(hairColor));
                    modelData.Hex_Eyebrow_C = PlayerPrefs.GetString("hex_color_eyebrow_f", ColorUtility.ToHtmlStringRGB(Color.Lerp(hairColor, Color.black, UnityEngine.Random.Range(0f, 1f))));

                    modelData.Hex_Eye_C = PlayerPrefs.GetString("hex_color_eye_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));

                    modelData.Hex_Upper_C = PlayerPrefs.GetString("hex_color_upper_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Lower_C = PlayerPrefs.GetString("hex_color_lower_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Foot_C = PlayerPrefs.GetString("hex_color_foot_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Pattern_C = PlayerPrefs.GetString("hex_color_pattern_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                    modelData.Hex_Acc_C = PlayerPrefs.GetString("hex_color_acc_f", ColorUtility.ToHtmlStringRGB(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))));
                }
                break;
        }
        return modelData;
    }

    public void SetSinglePlayers(int playerCount)
    {
        userInfos = new List<UserInfo>();

        {
            UserInfo userInfo = new UserInfo();
            userInfo.id = userInfo_mine.id;
            userInfo.nick = userInfo_mine.nick;
            userInfo.customModelData = userInfo_mine.customModelData;
            userInfos.Add(userInfo);
        }

        for (int i = 1; i < playerCount; i++)
        {
            UserInfo userInfo = new UserInfo();
            userInfo.id = "CPU_" + (i + 1);
            userInfo.nick = userInfo.id;
            userInfo.customModelData = CustomModelSettingCtrl.GetRandomModelData();
            userInfos.Add(userInfo);
        }
    }

    public void SetMultiPlayers(int playerCount)
    {
        for (int i = userInfos.Count; i < playerCount; i++)
        {
            UserInfo userInfo = new UserInfo();
            userInfo.id = "CPU_" + (i + 1);
            userInfo.nick = userInfo.id;
            userInfo.customModelData = CustomModelSettingCtrl.GetRandomModelData();
            userInfos.Add(userInfo);
        }
    }

    public static string SetGameGUID()
    {
        if (GameDataManager.instance.playType == GameDataManager.PlayType.Multi || GameDataManager.instance.playType == GameDataManager.PlayType.Multi_Solo || GameDataManager.instance.gameType == GameType.Fishing)
        {
            game_guid = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            game_guid = MakeGUID();
        }
        return game_guid;
    }

    private static string MakeGUID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public static int GetMyNumber()
    {
        for (int i = 0; i < instance.userInfos.Count; i++)
        {
            if (instance.userInfos[i].id == instance.userInfo_mine.id)
            {
                return i;
            }
        }
        return 0;
    }

    public static void SetTestAccount(string id)
    {
        TestAccount account = new TestAccount();
        account.user_id = id;
        account.user_nick = id;
        account.user_profile = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06896f8cb324e3a683a229cd8a066d56075c6548.jpg";

        JsonUtility.ToJson(account);

        string path = Application.dataPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string saveJson = JsonUtility.ToJson(account);

        path += "/TestAccount";
        File.WriteAllText(path, saveJson);
        Debug.Log("Save Success: " + path);
    }

    public static TestAccount GetTestAccount()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        string path = Application.persistentDataPath + "/TestAccount";
#else
        string path = Application.dataPath + "/TestAccount";
#endif
        if (!File.Exists(path))
        {
            Debug.LogError("No such File exists");
            TestAccount account_temp = new TestAccount();
            account_temp.user_id = "TEST" + System.DateTime.Now.ToString("mmssfff");
            account_temp.user_nick = account_temp.user_id;
            return account_temp;
        }
        string saveFile = System.IO.File.ReadAllText(path);
        TestAccount account = JsonUtility.FromJson<TestAccount>(saveFile);
        return account;

    }

    public static void SetTestOption(bool isPicoPC)
    {
        TestOption option = new TestOption();
        option.isPicoPC = isPicoPC;

        JsonUtility.ToJson(option);

        string path = Application.dataPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string saveJson = JsonUtility.ToJson(option);

        path += "/TestOption";
        File.WriteAllText(path, saveJson);
        Debug.Log("Save Success: " + path);
    }

    public static TestOption GetTestOption()
    {
        string path = Application.dataPath + "/TestOption";

        if (!File.Exists(path))
        {
            Debug.LogError("No such File exists");
            return new TestOption();
        }
        string saveFile = System.IO.File.ReadAllText(path);
        TestOption option = JsonUtility.FromJson<TestOption>(saveFile);
        return option;

    }

    string GetSubtractNick(string origin_nick)
    {
        if (origin_nick.Length > 16)
        {
            return origin_nick.Substring(0, 16);
        }

        return origin_nick;
    }
}

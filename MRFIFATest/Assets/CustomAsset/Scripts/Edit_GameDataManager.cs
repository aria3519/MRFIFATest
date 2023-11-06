#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

[CustomEditor(typeof(GameDataManager))]
public class Edit_GameDataManager : Editor
{
    public string state_platform;
    public string state_account;
    public string state_photon_server;
    public string state_db_server;

    private bool isSDK = true;
    private bool isInfo = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUIStyle warningC = new GUIStyle(EditorStyles.textField);
        warningC.normal.textColor = new Color(1f,0.5f,0.5f);

        GUIStyle desableC = new GUIStyle(EditorStyles.label);
        desableC.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        GUIStyle enableC = new GUIStyle(EditorStyles.label);
        enableC.normal.textColor = new Color(0.5f, 1f, 0.5f);

        GUIStyle normalC = new GUIStyle(EditorStyles.label);

        GUIStyle enableB = new GUIStyle(EditorStyles.miniButton);
        enableB.normal.textColor = new Color(0.5f, 1f, 0.5f);

        GUIStyle normalB = new GUIStyle(EditorStyles.miniButton);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[Test Account]");
        EditorGUILayout.EndHorizontal();

        GameDataManager.TestAccount account = GameDataManager.GetTestAccount();

        EditorGUILayout.BeginHorizontal();
        string get_user_id = account.user_id;
        EditorGUILayout.LabelField("TestID : " + get_user_id);
        string set_user_id = EditorGUILayout.DelayedTextField(get_user_id);
        EditorGUILayout.EndHorizontal();

        if (set_user_id != get_user_id)
        {
            if (set_user_id.Length >= 10 && set_user_id.Length <= 50)
            {
                GameDataManager.SetTestAccount(set_user_id);
            }
            else
            {
                EditorUtility.DisplayDialog("오류", "테스트 아이디는 10~50자 사이로 해주세요.", "OK");
            }
        }

        //if (GUILayout.Button("Open Android Directory"))
        //{
        //    System.Diagnostics.Process.Start(Application.dataPath);
        //}

        if (GUILayout.Button("Copy Android Directory"))
        {
            CopyProcess();
            //System.Diagnostics.Process.Start(Application.dataPath);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[Unity Info]");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Version : " + PlayerSettings.bundleVersion);
        string get_bundleVersion = EditorGUILayout.DelayedTextField(PlayerSettings.bundleVersion);
        if (PlayerSettings.bundleVersion != get_bundleVersion)
        {
            PlayerSettings.bundleVersion = get_bundleVersion;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("BundleVersionCode : " + PlayerSettings.Android.bundleVersionCode);
        int get_bundleVersionCode = EditorGUILayout.DelayedIntField(PlayerSettings.Android.bundleVersionCode);
        if (PlayerSettings.Android.bundleVersionCode != get_bundleVersionCode)
        {
            PlayerSettings.Android.bundleVersionCode = get_bundleVersionCode;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Identifier : " + Application.identifier);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GameDataManager.TestOption option = GameDataManager.GetTestOption();

        EditorGUILayout.BeginHorizontal();
        bool get_isPicoPC = option.isPicoPC;
        EditorGUILayout.LabelField("Pico Streaming Assistant : " + get_isPicoPC);
        bool set_isPicoPC = EditorGUILayout.Toggle(get_isPicoPC);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (set_isPicoPC != get_isPicoPC)
        {
            GameDataManager.SetTestOption(set_isPicoPC);
        }

        var xrSetting_pc = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
        EditorGUILayout.BeginHorizontal();

#if UNITY_STANDALONE
        EditorGUILayout.LabelField("(PC)", enableC);
#else
        EditorGUILayout.LabelField("(PC)", desableC);
#endif
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Initialize XR on Startup : " + xrSetting_pc.InitManagerOnStart, xrSetting_pc.InitManagerOnStart ? normalC : warningC);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ActiveLoaderCount : " + xrSetting_pc.Manager.activeLoaders.Count, (xrSetting_pc.Manager.activeLoaders.Count == 1) ? normalC : warningC);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < xrSetting_pc.Manager.activeLoaders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ActiveLoader : " + xrSetting_pc.Manager.activeLoaders[i].name);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        var xrSetting_and = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        EditorGUILayout.BeginHorizontal();
#if UNITY_ANDROID
        EditorGUILayout.LabelField("(Android)", enableC);
#else
        EditorGUILayout.LabelField("(Android)", desableC);
#endif
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Initialize XR on Startup : " + xrSetting_and.InitManagerOnStart, xrSetting_and.InitManagerOnStart ? normalC : warningC);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ActiveLoaderCount : " + xrSetting_and.Manager.activeLoaders.Count, (xrSetting_and.Manager.activeLoaders.Count == 1) ? normalC : warningC);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < xrSetting_and.Manager.activeLoaders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ActiveLoader : " + xrSetting_and.Manager.activeLoaders[i].name);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[PLATFORM]");
        EditorGUILayout.EndHorizontal();
#if PICO_PLATFORM
        if (state_platform != null && state_platform.Contains("PICO_PLATFORM"))
        {
            if (!isSDK)
            {
                isSDK = true;
                //SetManifest();
                //CheckPackage();
                AssetDatabase.Refresh();
            }
            else if (!isInfo)
            {
                if (EnablePlugin(BuildTargetGroup.Android) && EnablePlugin(BuildTargetGroup.Standalone))
                {
                    AssetDatabase.Refresh();
                    isInfo = true;
                }
            }
        }
#elif OCULUS_PLATFORM
        if (state_platform != null && state_platform.Contains("OCULUS_PLATFORM"))
        {
            if (!isSDK)
            {
                isSDK = true;
                //SetManifest();
                //CheckPackage();
                AssetDatabase.Refresh();
            }
            else if (!isInfo)
            {
                if (EnablePlugin(BuildTargetGroup.Android) && EnablePlugin(BuildTargetGroup.Standalone))
                {
                    AssetDatabase.Refresh();
                    isInfo = true;
                }
            }
        }
#elif STEAMWORKS
        if (state_platform != null && state_platform.Contains("STEAMWORKS"))
        {
            if (!isSDK)
            {
                isSDK = true;
                //SetManifest();
                //CheckPackage();
                AssetDatabase.Refresh();
            }
            else if (!isInfo)
            {
                if (EnablePlugin(BuildTargetGroup.Android) && EnablePlugin(BuildTargetGroup.Standalone))
                {
                    AssetDatabase.Refresh();
                    isInfo = true;
                }
            }
        }
#endif

        if (GUILayout.Button("PICO_PLATFORM", ContainName("PICO_PLATFORM") ? enableB : normalB))
        {
            state_platform = "PICO_PLATFORM";
            isSDK = false;
            isInfo = false;
            SetPlatform(BuildTargetGroup.Android);
            SetPlatform(BuildTargetGroup.Standalone);
            AssetDatabase.Refresh();
            //SetPlatform("PICO_PLATFORM", BuildTargetGroup.Android);
            //SetPlatform("PICO_PLATFORM", BuildTargetGroup.Standalone);
            //SetLoaderList(AppnoriBuildPlatform.PICO);
        }
        else if (GUILayout.Button("OCULUS_PLATFORM", ContainName("OCULUS_PLATFORM") ? enableB : normalB))
        {
            state_platform = "OCULUS_PLATFORM";
            isSDK = false;
            isInfo = false;
            SetPlatform(BuildTargetGroup.Android);
            SetPlatform(BuildTargetGroup.Standalone);
            //SetPlatform("OCULUS_PLATFORM", BuildTargetGroup.Android);
            //SetPlatform("OCULUS_PLATFORM", BuildTargetGroup.Standalone);
            AssetDatabase.Refresh();
        }
        else if (GUILayout.Button("STEAMWORKS", ContainName("STEAMWORKS") ? enableB : normalB))
        {
            state_platform = "STEAMWORKS";
            isSDK = false;
            isInfo = false;
            SetPlatform(BuildTargetGroup.Android);
            SetPlatform(BuildTargetGroup.Standalone);
            AssetDatabase.Refresh();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[ACCOUNT]");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("TEST_ACCOUNT", ContainName("TEST_ACCOUNT") ? enableB : normalB))
        {
            state_account = "TEST_ACCOUNT";
            SetAccount(BuildTargetGroup.Android);
            SetAccount(BuildTargetGroup.Standalone);
        }
        else if (GUILayout.Button("PLATFORM_ACCOUNT", ContainName("PLATFORM_ACCOUNT") ? enableB : normalB))
        {
            state_account = "PLATFORM_ACCOUNT";
            isSDK = false;
            isInfo = false;
            SetAccount(BuildTargetGroup.Android);
            SetAccount(BuildTargetGroup.Standalone);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[PHOTON_SERVER]");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("SERVER_GLOBAL", ContainName("SERVER_GLOBAL") ? enableB : normalB))
        {
            state_photon_server = "SERVER_GLOBAL";
            SetPhotonServer(BuildTargetGroup.Android);
            SetPhotonServer(BuildTargetGroup.Standalone);
            PlayerSettings.applicationIdentifier = "com.Appnori.SummerSports";
        }
        else if (GUILayout.Button("SERVER_CHINA", ContainName("SERVER_CHINA") ? enableB : normalB))
        {
            state_photon_server = "SERVER_CHINA";
            SetPhotonServer(BuildTargetGroup.Android);
            SetPhotonServer(BuildTargetGroup.Standalone);
            PlayerSettings.applicationIdentifier = "com.Appnori.SummerSports";
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[DB SERVER]");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("DB_TEST", ContainName("DB_TEST") ? enableB : normalB))
        {
            state_db_server = "DB_TEST";
            SetDBServer(BuildTargetGroup.Android);
            SetDBServer(BuildTargetGroup.Standalone);
        }
        else if (GUILayout.Button("DB_OPERATION", ContainName("DB_OPERATION") ? enableB : normalB))
        {
            state_db_server = "DB_OPERATION";
            SetDBServer(BuildTargetGroup.Android);
            SetDBServer(BuildTargetGroup.Standalone);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    bool ContainName(string findName)
    {
#if UNITY_ANDROID
        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).Split(';');
#else
        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';');
#endif
        for (int i = 0; i < arr_define.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(arr_define[i]))
            {
                continue;
            }
            else if (arr_define[i] == findName)
            {
                return true;
            }
        }

        return false;
    }

    void SetPlatform(BuildTargetGroup buildTargetGroup)
    {
        if (buildTargetGroup == BuildTargetGroup.Android)
        {
            SetManifest();
            CheckPackage();
        }
        //EnablePlugin(platform, buildTargetGroup);

        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
        string sum_define = "";
        for (int i = 0; i < arr_define.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(arr_define[i]))
            {
                continue;
            }
            else if (arr_define[i] == "PICO_PLATFORM" || arr_define[i] == "OCULUS_PLATFORM" || arr_define[i] == "STEAMWORKS")
            {
                continue;
            }
            if (sum_define.Length >= 1)
            {
                sum_define += ";";
            }
            sum_define += arr_define[i];
        }

        if (sum_define.Length >= 1)
        {
            sum_define += ";";
        }
        //USING_XR_MANAGEMENT.
        //USING_XR_SDK_OCULUS.
        sum_define += state_platform;

        Debug.Log(sum_define);
        
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sum_define);

        //AssetDatabase.Refresh();
    }

    void SetAccount(BuildTargetGroup buildTargetGroup)
    {
        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
        string sum_define = "";
        for (int i = 0; i < arr_define.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(arr_define[i]))
            {
                continue;
            }
            else if (arr_define[i] == "TEST_ACCOUNT" || arr_define[i] == "PLATFORM_ACCOUNT")
            {
                continue;
            }
            if (sum_define.Length >= 1)
            {
                sum_define += ";";
            }
            sum_define += arr_define[i];
        }

        if (sum_define.Length >= 1)
        {
            sum_define += ";";
        }

        sum_define += state_account;

        Debug.Log(sum_define);


        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sum_define);
    }

    void SetPhotonServer(BuildTargetGroup buildTargetGroup)
    {
        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
        string sum_define = "";
        for (int i = 0; i < arr_define.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(arr_define[i]))
            {
                continue;
            }
            else if (arr_define[i] == "SERVER_GLOBAL" || arr_define[i] == "SERVER_CHINA")
            {
                continue;
            }
            if (sum_define.Length >= 1)
            {
                sum_define += ";";
            }
            sum_define += arr_define[i];
        }

        if (sum_define.Length >= 1)
        {
            sum_define += ";";
        }

        sum_define += state_photon_server;

        Debug.Log(sum_define);


        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sum_define);
    }
    void SetDBServer(BuildTargetGroup buildTargetGroup)
    {
        string[] arr_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
        string sum_define = "";
        for (int i = 0; i < arr_define.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(arr_define[i]))
            {
                continue;
            }
            else if (arr_define[i] == "DB_TEST" || arr_define[i] == "DB_OPERATION")
            {
                continue;
            }
            if (sum_define.Length >= 1)
            {
                sum_define += ";";
            }
            sum_define += arr_define[i];
        }

        if (sum_define.Length >= 1)
        {
            sum_define += ";";
        }

        sum_define += state_db_server;

        Debug.Log(sum_define);


        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sum_define);
    }
    void SetManifest()
    {
        string path = Application.dataPath + "/Manifests/";
       

        switch (state_platform)
        {
            case "PICO_PLATFORM":
                {
                    path += "PicoManifest";
                }
                break;
            case "OCULUS_PLATFORM":
                {
                    path += "OculusManifest";
                }
                break;
            default:
                return;
        }
        path += ".xml";

        StreamReader streamReader = new StreamReader(path);
        string manifest_copy = streamReader.ReadToEnd();
        streamReader.Close();

        path = "Assets/Plugins/Android/AndroidManifest.xml";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(manifest_copy);
        writer.Close();

        switch (state_platform)
        {
            case "PICO_PLATFORM":
                {
                    Debug.Log("SetManifest - Pico");
                }
                break;
            case "OCULUS_PLATFORM":
                {
                    Debug.Log("SetManifest - Oculus");
                }
                break;
        }

    }

    void CheckPackage()
    {
        switch (state_platform)
        {
            case "PICO_PLATFORM":
                {
                    SetPackageData(PackageState.PicoXR);
                }
                break;
            case "OCULUS_PLATFORM":
                {
                    SetPackageData(PackageState.OculusXR);
                }
                break;
            case "STEAMWORKS":
                {
                    SetPackageData(PackageState.OpenXR);
                }
                break;
            default:
                return;

        }
    }

    public enum PackageState
    {
        OpenXR, OculusXR, PicoXR
    }

    void SetPackageData(PackageState packageState)
    {
        string path = Application.dataPath;
        for (int i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] == '/')
            {
                path = path.Substring(0, i + 1);
                break;
            }
        }
        path += "Packages/manifest.json";

        StreamReader streamReader = new StreamReader(path);
        string manifest_copy = streamReader.ReadToEnd();
        streamReader.Close();

        try
        {
            var manifest = Json.Deserialize(manifest_copy) as Dictionary<string, object>;

            foreach (var dependencies in manifest)
            {
                if (!dependencies.Key.Contains("dependencies"))
                {
                    continue;
                }

                Dictionary<string, object> d = dependencies.Value as Dictionary<string, object>;

                if (d.ContainsKey("com.unity.xr.openxr"))
                {
                    d.Remove("com.unity.xr.openxr");
                }

                if (d.ContainsKey("com.htc.upm.vive.openxr"))
                {
                    d.Remove("com.htc.upm.vive.openxr");
                }

                if (d.ContainsKey("com.unity.xr.oculus"))
                {
                    d.Remove("com.unity.xr.oculus");
                }

                if (d.ContainsKey("com.unity.xr.picoxr"))
                {
                    d.Remove("com.unity.xr.picoxr");
                }

                if (d.ContainsKey("com.unity.xr.qiyu"))
                {
                    d.Remove("com.unity.xr.qiyu");
                }

                if (d.ContainsKey("com.qiyu.sdk.platform"))
                {
                    d.Remove("com.qiyu.sdk.platform");
                }
            }

            List<string> list_add_pack = new List<string>();
            //string add_pack = "";
            switch (packageState)
            {
                case PackageState.OpenXR:
                    {
                        list_add_pack.Add("com.unity.xr.openxr@file:../LocalPackages/com.unity.xr.openxr_1.6.0");
                    }
                    break;
                case PackageState.OculusXR:
                    {
                        list_add_pack.Add("com.unity.xr.oculus@file:../LocalPackages/com.unity.xr.oculus_1.13.0");
                    }
                    break;
                case PackageState.PicoXR:
                    {
                        list_add_pack.Add("com.unity.xr.picoxr@file:../LocalPackages/PICO Unity Integration SDK-213");
                    }
                    break;
            }

            foreach (var dependencies in manifest)
            {
                if (!dependencies.Key.Contains("dependencies"))
                {
                    continue;
                }

                var d = dependencies.Value as Dictionary<string, object>;

                for (int i = 0; i < list_add_pack.Count; i++)
                {
                    if (d.ContainsKey(list_add_pack[i].Split('@')[0]))
                    {
                        Debug.Log(list_add_pack[i] + " - 이미 설치되어 있음.");
                    }
                    else
                    {
                        d.Add(list_add_pack[i].Split('@')[0], list_add_pack[i].Split('@')[1]);
                    }
                }
            }
            manifest_copy = Json.Serialize(manifest);
            Debug.Log(manifest_copy);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            Debug.Log("에러");
        }

        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(manifest_copy);
        writer.Close();

    }

    public bool EnablePlugin(BuildTargetGroup buildTargetGroup)
    {
        try
        {
            var buildTargetSettings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            var pluginsSettings = buildTargetSettings.AssignedSettings;
            var success = UnityEditor.XR.Management.Metadata.XRPackageMetadataStore.AssignLoader(pluginsSettings, GetLoaderName(), buildTargetGroup);
            if (!success)
            {
                return false;
            }
        }
        catch (System.Exception)
        {
            return false;
        }
        return true;
    }

    

    string GetLoaderName()
    {
        switch (state_platform)
        {
            case "PICO_PLATFORM":
                {
                    return "Unity.XR.PXR.PXR_Loader";
                }
            case "QIYU_PLATFORM":
                {
                    return "Unity.XR.Qiyu.QiyuLoader";
                }
            case "OCULUS_PLATFORM":
                {
                    return "Unity.XR.Oculus.OculusLoader";
                }
            case "STEAMWORKS":
                {
                    return "UnityEngine.XR.OpenXR.OpenXRLoader";
                }
            case "VIVEPORT":
                {
                    return "UnityEngine.XR.OpenXR.OpenXRLoader";
                }
            default:
                {
                    return "";
                }
        }
    }

    void CopyProcess()
    {
        using (var proc = new System.Diagnostics.Process())
        {
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.StartInfo.FileName = "cmd";
            proc.Start();

            proc.StandardInput.WriteLine($"adb push "+ Application.dataPath + "/TestAccount /sdcard/Android/data/" + Application.identifier + "/files/");
            proc.StandardInput.WriteLine($"exit");

            proc.StandardInput.Flush();

            string line = proc.StandardOutput.ReadToEnd();
            //Debug.Log(line);
            bool isError = false;
            string[] lines = line.Split('\n');
            int i = 0;
            for (i = 0; i < lines.Length; i++)
            {
                if (lines[i].ToLower().Contains("error"))
                {
                    isError = true;
                    break;
                }
            }
            if (!isError)
            {
                Debug.Log("Success - Copy Android Directory");
            }
            else
            {
                Debug.LogError(lines[i]);
            }

            line = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(line))
            {
                Debug.LogError(line);
            }

            proc.WaitForExit();
        }
    }

}
#endif
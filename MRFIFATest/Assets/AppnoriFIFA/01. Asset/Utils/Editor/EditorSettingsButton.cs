using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    using Photon.Pun;
    using UnityEditor;

    [InitializeOnLoad]
    public class EditorSettingsButton : MonoBehaviour
    {
        private static readonly GUIContent Button_Build = new(null, EditorGUIUtility.FindTexture(@"d_BuildSettings.Android.Small"));
        private static readonly GUIContent Button_Project = new(null, EditorGUIUtility.FindTexture(@"d__Popup"));
        private static readonly GUIContent Button_Animation = new(null, EditorGUIUtility.FindTexture(@"d_UnityEditor.AnimationWindow"));
        private static readonly GUIContent Button_PhotonRPCRefresh = new(null , AssetDatabase.LoadAssetAtPath("Assets/3_Private Assets/5_Jet ski/03. Script/Utils/Editor/Photon Refresh.png", typeof(Texture)) as Texture);
        private static readonly GUIContent Button_CSharpProject = new(null, AssetDatabase.LoadAssetAtPath("Assets/3_Private Assets/5_Jet ski/03. Script/Utils/Editor/VisualStudio Icon.png", typeof(Texture)) as Texture);

        private static readonly string Path_BuildSettings = "File/Build Settings...";
        private static readonly string Path_ProjectSettings = "Edit/Project Settings...";
        private static readonly string Path_Animation = "Window/Animation/Animation";
        private static readonly string Path_CSharpProject = "Assets/Open C# Project";

        static EditorSettingsButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            Button_Build.image = EditorUserBuildSettings.activeBuildTarget switch
            {
                BuildTarget.StandaloneWindows64 => EditorGUIUtility.FindTexture(@"BuildSettings.Standalone.Small"),
                BuildTarget.Android => EditorGUIUtility.FindTexture(@"d_BuildSettings.Android.Small"),
                BuildTarget.iOS => EditorGUIUtility.FindTexture(@"d_BuildSettings.iPhone.Small"),
                _ => EditorGUIUtility.FindTexture(@"BuildSettings.Editor.Small"),
            };

            if (GUILayout.Button(Button_Build, GUIStyles.ToolbarStyles.commandButtonStyle))
                EditorApplication.ExecuteMenuItem(Path_BuildSettings);

            if (GUILayout.Button(Button_Project, GUIStyles.ToolbarStyles.commandButtonStyle))
                EditorApplication.ExecuteMenuItem(Path_ProjectSettings);

            if (GUILayout.Button(Button_Animation, GUIStyles.ToolbarStyles.commandButtonStyle))
                EditorApplication.ExecuteMenuItem(Path_Animation);

            if (Button_PhotonRPCRefresh.image == null)
                Button_PhotonRPCRefresh.image = AssetDatabase.LoadAssetAtPath("Assets/3_Private Assets/5_Jet ski/03. Script/Utils/Editor/Photon Refresh.png", typeof(Texture)) as Texture;

            if (GUILayout.Button(Button_PhotonRPCRefresh, GUIStyles.ToolbarStyles.commandButtonStyle))
            {
                ServerSettings serverSettings = PhotonNetwork.PhotonServerSettings;

                Undo.RecordObject(serverSettings, "RPC-list cleared for PUN.");
                serverSettings.RpcList.Clear();
                EditorUtility.SetDirty(serverSettings);

                Debug.Log("Cleared the PhotonServerSettings.RpcList, which breaks compatibility with older builds. You should update the \"App Version\" in the PhotonServerSettings to avoid issues.");

                PhotonEditor.UpdateRpcList();

                Debug.Log("Updated the PhotonServerSettings.RpcList");
            }

            if (Button_CSharpProject.image == null)
                Button_CSharpProject.image = AssetDatabase.LoadAssetAtPath("Assets/3_Private Assets/5_Jet ski/03. Script/Utils/Editor/VisualStudio Icon.png", typeof(Texture)) as Texture;

            if (GUILayout.Button(Button_CSharpProject, GUIStyles.ToolbarStyles.commandButtonStyle))
                EditorApplication.ExecuteMenuItem(Path_CSharpProject);
        }
    }
}

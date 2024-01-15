using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace Jisu.Utils
{
    [Serializable]
    class SceneToJson
    {
        public string[] scenePath;

        public SceneToJson(int length)
        {
            scenePath = new string[length];
        }
    }

    static class SceneHelper
    {
        static SceneAsset sceneToOpen;

        public static void StartScene(SceneAsset scene)
        {
            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;

            sceneToOpen = scene;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (sceneToOpen == null || EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (AssetDatabase.OpenAsset(sceneToOpen.GetInstanceID()) == false)
                Debug.LogWarning("Couldn't Find the Scene File");

            sceneToOpen = null;

            EditorApplication.update -= OnUpdate;
        }
    }

    [InitializeOnLoad]
    class SceneSwitchLeftButton : LocalSingleton<SceneSwitchLeftButton>
    {
        public static string FolderPath { get; private set; }

        public static readonly int SceneMaxCount = 15;
        public static readonly string FileName = @"/SceneHelperData.json";
        public static readonly Texture SceneIcon = EditorGUIUtility.FindTexture(@"UnityEditor.SceneView");

        public static readonly List<SceneAsset> Scene = new(SceneMaxCount);

        static SceneSwitchLeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            EditorApplication.update += InitializeSceneList;
        }

        private static void InitializeSceneList()
        {
            GetFolderPath();

            var fromJson = AssetDatabase.LoadAssetAtPath(FolderPath + FileName, typeof(TextAsset)) as TextAsset;
            if (fromJson != null)
            {
                var loadSceneToJson = JsonUtility.FromJson<SceneToJson>(fromJson.ToString());
                if (loadSceneToJson.scenePath.Length > 0)
                {
                    for (int i = 0; i < loadSceneToJson.scenePath.Length; i++)
                        Scene.Add(AssetDatabase.LoadAssetAtPath(loadSceneToJson.scenePath[i], typeof(SceneAsset)) as SceneAsset);
                }
            }
            else
                System.IO.File.WriteAllText(FolderPath + FileName, JsonUtility.ToJson(new SceneToJson(0), true));

            EditorApplication.update -= InitializeSceneList;
        }

        private static void GetFolderPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            FolderPath = System.IO.Path.GetDirectoryName(sourceFilePath);
            var rootIndex = FolderPath.IndexOf(@"Assets\");
            if (rootIndex > -1)
                FolderPath = FolderPath[rootIndex..];
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(null, SceneIcon, "Scene Switcher Setting"), "Command"))
                EditorWindow.GetWindow(typeof(SceneSwitcherWindow));

            for (int i = 0; i < Scene.Count; i++)
            {
                if (Scene[i] != null)
                {
                    var sceneName = Scene[i].name.Split('_');
                    if (GUILayout.Button(new GUIContent($"{sceneName[^1][0]}", $"{Scene[i].name}"), GUIStyles.ToolbarStyles.sceneButtonStyle))
                    {
                        switch (Event.current.button)
                        {
                            case 0:
                                SceneHelper.StartScene(Scene[i]);
                                break;
                            case 1:
                                var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scene[i]), OpenSceneMode.Additive);
                                EditorSceneManager.SetActiveScene(scene);
                                break;
                        }
                    }
                }
                else
                    GUILayout.Button(new GUIContent($"X", $"X"), GUIStyles.ToolbarStyles.sceneButtonStyle);
            }
        }
    }

    public class SceneSwitcherWindow : EditorWindow
    {
        private List<SceneAsset> Scene => SceneSwitchLeftButton.Scene;
        private ReorderableList list;

        private void Awake()
        {
            titleContent = new GUIContent("Scene Switcher Setting", SceneSwitchLeftButton.SceneIcon);
        }

        private void OnEnable()
        {
            list = new ReorderableList(Scene, typeof(SceneAsset), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Scene List"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    Scene[index] = (SceneAsset)EditorGUI.ObjectField(new(rect.x, rect.y + 2f, rect.width * 0.9f, EditorGUIUtility.singleLineHeight + 2f), Scene[index] == null ? null : Scene[index], typeof(SceneAsset), true);

                    if (Scene[index] != null)
                    {
                        var sceneName = Scene[index].name.Split('_');
                        if (GUI.Button(new Rect(rect.x + rect.width * 0.912f, rect.y + 1f, rect.width * 0.1f, EditorGUIUtility.singleLineHeight), new GUIContent(null, SceneSwitchLeftButton.SceneIcon, "Open Scene with Single / Additive"), GUIStyles.ToolbarStyles.sceneButtonStyle))
                        {
                            switch (Event.current.button)
                            {
                                case 0:
                                    SceneHelper.StartScene(Scene[index]);
                                    break;
                                case 1:
                                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scene[index]), OpenSceneMode.Additive);
                                    EditorSceneManager.SetActiveScene(scene);
                                    break;
                            }
                        }
                    }
                },
                onAddCallback = list => Scene.Add(null),
                onCanAddCallback = list => Scene.Count < SceneSwitchLeftButton.SceneMaxCount,
                onRemoveCallback = list => ReorderableList.defaultBehaviours.DoRemoveButton(list),
                onChangedCallback = list => minSize = maxSize = new Vector2(350f, 200f + Mathf.Max(list.count - 5, 0) * 22f)
            };
        }

        private void OnDisable()
        {
            SaveSceneData();
        }

        private void SaveSceneData()
        {
            var count = Scene.Count;
            var sceneToJson = new SceneToJson(count);
            for (var i = 0; i < count; i++)
                sceneToJson.scenePath[i] = AssetDatabase.GetAssetPath(Scene[i]);

            var toJson = JsonUtility.ToJson(sceneToJson, true);
            System.IO.File.WriteAllText(SceneSwitchLeftButton.FolderPath + SceneSwitchLeftButton.FileName, toJson);

            AssetDatabase.Refresh();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Scene Switcher Settings", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Save")))
                SaveSceneData();

            GUILayout.EndHorizontal();

            list?.DoLayoutList();

            EditorGUI.EndChangeCheck();
        }
    }
}
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEditor;

namespace Jisu.Utils
{
    /// <summary> 
    /// 유니티 에디터 재생 버튼 우측에 타임스케일 조정 슬라이더 생성
    /// </summary>
    public class EditorTimescaleSlider : MonoBehaviour
    {
        public static string FolderPath { get; private set; }
        private static readonly string fileName = @"/TimeScaleData.json";

        private static readonly GUIContent label = new GUIContent("Time Scale");

        private static TimeScaleData data;

        [InitializeOnLoadMethod]
        static void Init()
        {
            InitFolderPath();

            LoadData();

            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }
        private static void InitFolderPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            FolderPath = System.IO.Path.GetDirectoryName(sourceFilePath);
            int rootIndex = FolderPath.IndexOf(@"Assets\");
            if (rootIndex > -1)
            {
                FolderPath = FolderPath.Substring(rootIndex, FolderPath.Length - rootIndex);
            }
        }

        static void OnToolbarGUI()
        {
            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(70f));

            var prev = data.isUse;
            data.isUse = EditorGUILayout.Toggle(data.isUse, GUIStyles.ToggleStyles.commandToggleStyle, GUILayout.MaxWidth(15f));
            if (data.isUse != prev)
                SaveData();

            if (data.isUse)
                Time.timeScale = EditorGUILayout.Slider(Time.timeScale, 0f, 10f, GUILayout.MaxWidth(150f));
        }

        static void LoadData()
        {
            var fromJson = AssetDatabase.LoadAssetAtPath(FolderPath + fileName, typeof(TextAsset)) as TextAsset;
            if (fromJson == null)
            {
                var newData = new TimeScaleData();
                var toJson = JsonUtility.ToJson(newData, true);
                System.IO.File.WriteAllText(FolderPath + fileName, toJson);

                data = newData;
            }
            else
                data = JsonUtility.FromJson<TimeScaleData>(fromJson.ToString());
        }

        static void SaveData()
        {
            var toJson = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(FolderPath + fileName, toJson);
        }
    }

    [Serializable]
    class TimeScaleData
    {
        public bool isUse;
    }

    class BuildPreProcessor : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Time.timeScale = 1.5f;
        }
    }
}
#endif // UNITY_EDITOR
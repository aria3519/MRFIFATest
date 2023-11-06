#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

class AppnoriBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty prop_layer = tagManager.FindProperty("layers");
        prop_layer.ClearArray();
        for (int i = 0; i < 32; i++)
        {
            prop_layer.InsertArrayElementAtIndex(i);
            
            switch (i)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                    {
                        prop_layer.GetArrayElementAtIndex(i).stringValue = LayerMask.LayerToName(i);
                    }
                    break;
                default:
                    {
                        prop_layer.GetArrayElementAtIndex(i).stringValue = "L" + i;
                    }
                    break;
            }
        }

        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }
}
#endif

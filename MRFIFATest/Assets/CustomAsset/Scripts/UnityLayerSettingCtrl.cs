/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UnityLayerSettingCtrl))]
public class Edit_UnityLayerSettingCtrl : Editor
{
    string tagName = "";

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(0f, 20f, 0f),Quaternion.identity, Vector3.one);

        GUIStyle uIStyle = new GUIStyle(EditorStyles.label);
        uIStyle.alignment = TextAnchor.MiddleRight;

        UnityLayerSettingCtrl settingCtrl = (UnityLayerSettingCtrl)target;

        if (GUILayout.Button("SetEdit"))
        {
            settingCtrl.SetSettingData();
            AssetDatabase.SaveAssets();
            Debug.Log("SetEdit");
        }

        if (string.IsNullOrWhiteSpace(settingCtrl.layerCollisionMatrix))
        {
            settingCtrl.layerCollisionMatrix = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        }
        //settingCtrl.layerCollisionMatrix = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        //settingCtrl.layerCollisionMatrix = "f8fffffffefffffffefffffffffffffffffffffffffffffffffffefffffffffffffffffffffffffdffffffffffffffffffffffffffffdfffffffffffffffffffbffffffffffff7fffffffffffffffdffffffffffffdffffbfffffffffffffffffffffffffffdffffffffdfffffffffffffffffffffffffffffffffffffffffff";
        //if (settingCtrl.list_tag == null)
        //{
        //    settingCtrl.list_tag = new List<string>();
        //}

        //List<string> list_tag = settingCtrl.list_tag;

        if (settingCtrl.layers == null)
        {
            settingCtrl.layers = new string[32];

            for (int i = 0; i < settingCtrl.layers.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 4:
                    case 5:
                        {
                            settingCtrl.layers[i] = LayerMask.LayerToName(i);
                        }
                        break;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        string[] layers = settingCtrl.layers;

        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(0f, 20f, 0f), Quaternion.identity, Vector3.one);

        ///////////////// Layer Collision Matrix /////////////////

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("[Layer Collision Matrix]");
        EditorGUILayout.EndHorizontal();


        int count = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(layers[i]))
            {
                count++;
            }
        }

        Matrix4x4 matrix = GUI.matrix;
        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(738f, 0f, 0f), Quaternion.Euler(0f, 0f, 90f), Vector3.one);

        for (int i = 0; i < layers.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(layers[i]))
            {
                EditorGUILayout.LabelField("", uIStyle, GUILayout.Width(150f), GUILayout.Height(15f));
            }
        }

        for (int i = 0; i < layers.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(layers[i]))
            {
                EditorGUILayout.LabelField(layers[i], uIStyle, GUILayout.Width(150f), GUILayout.Height(15f));
            }
        }

        GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(0f, -420f, 0f), Quaternion.identity, Vector3.one);

        for (int y = 0; y < layers.Length; y++)
        {
            if (string.IsNullOrWhiteSpace(layers[y]))
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(100f));
            EditorGUILayout.LabelField(layers[y], uIStyle, GUILayout.Width(130f), GUILayout.Height(13f));

            for (int x = layers.Length - 1; x >= y; x--)
            {
                if (string.IsNullOrWhiteSpace(layers[x]))
                {
                    continue;
                }
                bool isCheck = settingCtrl.IsLayerCollision(x, y);

                bool isToggle = EditorGUILayout.Toggle(isCheck, GUILayout.Height(13f));

                if (isCheck != isToggle)
                {
                    settingCtrl.SetLayerCollision(x, y);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(0f, 30f, 0f), Quaternion.identity, Vector3.one);

        ////////////////// Tag /////////////////

        //EditorGUILayout.BeginHorizontal();

        //int leng = list_tag.Count;

        //EditorGUILayout.LabelField("[Tags]");
        //tagName = EditorGUILayout.TextField(tagName);

        //if (GUILayout.Button("ADD"))
        //{
        //    if (IsAddTag(list_tag, tagName))
        //    {
        //        leng++;
        //        list_tag.Add(tagName);
        //        settingCtrl.list_tag = list_tag;
        //    }
        //}

        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
        //for (int i = 0; i < list_tag.Count; i++)
        //{
        //    EditorGUILayout.BeginHorizontal();

        //    EditorGUILayout.LabelField("Tag " + i);
        //    string temp = EditorGUILayout.DelayedTextField(list_tag[i]);
        //    if (temp != list_tag[i] && IsAddTag(list_tag, temp))
        //    {
        //        list_tag[i] = temp;
        //    }


        //    if (GUILayout.Button("Remove") && leng >= 1)
        //    {
        //        leng--;
        //        list_tag.RemoveAt(i);
        //        settingCtrl.list_tag = list_tag;
        //    }

        //    EditorGUILayout.EndHorizontal();
        //}
        //EditorGUILayout.EndVertical();

        //GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(0f, 20f, 0f), Quaternion.identity, Vector3.one);

        ///////////////// Layer /////////////////

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("[Layers]");

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
        for (int i = 0; i < layers.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            switch (i)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("Builtin Layer " + i);
                        EditorGUILayout.TextField(layers[i]);
                        GUI.enabled = true;                        
                    }
                    break;
                default:
                    {
                        EditorGUILayout.LabelField("User Layer " + i);
                        layers[i] = EditorGUILayout.DelayedTextField(layers[i]);
                    }
                    break;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        ///////////////////////////////////////////

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    public bool IsAddTag(List<string> list_tag, string add_name)
    {
        if (list_tag.Count >= 50 || string.IsNullOrWhiteSpace(add_name))
        {
            return false;
        }

        for (int i = 0; i < list_tag.Count; i++)
        {
            if (list_tag[i] == add_name)
            {
                return false;
            }
        }

        return true;
    }


}
#endif


public class UnityLayerSettingCtrl : MonoBehaviour
{
    public List<string> list_tag;
    public string[] layers;
    public string layerCollisionMatrix;

    void Awake()
    {
        AppnoriLayer.InitData(layers);
        SetSettingData();
    }

    public void SetSettingData()
    {
        for (int y = 0; y < layers.Length; y++)
        {
            if (string.IsNullOrWhiteSpace(layers[y]))
            {
                continue;
            }

            for (int x = layers.Length - 1; x >= y; x--)
            {
                if (string.IsNullOrWhiteSpace(layers[x]))
                {
                    continue;
                }

                Physics.IgnoreLayerCollision(x, y, !IsLayerCollision(x, y));
            }
        }
#if UNITY_EDITOR
        List<GameObject[]> list_tempGOs = new List<GameObject[]>();

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty prop_tag = tagManager.FindProperty("tags");

        //for (int i = 0; i < list_tag.Count; i++)
        //{
        //    try
        //    {
        //        GameObject[] findGOs = GameObject.FindGameObjectsWithTag(list_tag[i]);
        //        if (findGOs == null || findGOs.Length == 0)
        //        {
        //            list_tempGOs.Add(null);
        //            continue;
        //        }
        //        list_tempGOs.Add(findGOs);
        //    }
        //    catch (Exception)
        //    {
        //        list_tempGOs.Add(null);
        //        continue;
        //    }
        //}

        //prop_tag.ClearArray();
        //for (int i = 0; i < list_tag.Count; i++)
        //{
        //    prop_tag.InsertArrayElementAtIndex(i);
        //    prop_tag.GetArrayElementAtIndex(i).stringValue = list_tag[i];
        //}

        SerializedProperty prop_layer = tagManager.FindProperty("layers");
        prop_layer.ClearArray();
        for (int i = 0; i < layers.Length; i++)
        {
            prop_layer.InsertArrayElementAtIndex(i);
            prop_layer.GetArrayElementAtIndex(i).stringValue = layers[i];
        }

        tagManager.ApplyModifiedProperties();


        //for (int i = 0; i < list_tag.Count; i++)
        //{
        //    if (list_tempGOs[i] == null)
        //    {
        //        continue;
        //    }
        //    for (int j = 0; j < list_tempGOs[i].Length; j++)
        //    {
        //        list_tempGOs[i][j].tag = list_tag[i];
        //    }
        //}
#endif
    }

    public bool IsLayerCollision(int x,int y)
    {
        int i = ((x / 8)) * 2 + (y * 8);
        string str16 = layerCollisionMatrix[i].ToString() + layerCollisionMatrix[i + 1].ToString();
        int convertNum = Convert.ToInt32(str16, 16);

        return (convertNum & (1 << (x % 8))) != 0;
    }
    public void SetLayerCollision(int x, int y)
    {
        char[] charArr = layerCollisionMatrix.ToCharArray();

        int i = ((x / 8)) * 2 + (y * 8);
        string str16 = charArr[i].ToString() + charArr[i + 1].ToString();
        int convertNum = Convert.ToInt32(str16, 16);
        string change16 = (convertNum ^ (1 << (x % 8))).ToString("x2");

        charArr[i] = change16[0];
        charArr[i + 1] = change16[1];

        if (x != y)
        {
            i = ((y / 8)) * 2 + (x * 8);
            str16 = charArr[i].ToString() + charArr[i + 1].ToString();
            convertNum = Convert.ToInt32(str16, 16);
            change16 = (convertNum ^ (1 << (y % 8))).ToString("x2");

            charArr[i] = change16[0];
            charArr[i + 1] = change16[1];
        }

        layerCollisionMatrix = new string(charArr);
    }

    //public string GetTagName(int i)
    //{
    //    if (list_tag == null || list_tag.Count <= i)
    //    {
    //        return "";
    //    }
    //    return list_tag[i];
    //}

}

public static class AppnoriLayer
{
    private static string[] layers;

    public static void InitData(string[] _layers)
    {
        layers = _layers;
    }

    public static string GetLayer(string layer_origin)
    {
#if UNITY_EDITOR
        return layer_origin;
#endif
        switch (layer_origin)
        {
            case "Default":
            case "TransparentFX":
            case "Ignore Raycast":
            case "Water":
            case "UI":
                {
                    return layer_origin;
                }
        }

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == layer_origin)
            {
                return "L" + i;
            }
        }

        return "";
    }
}*/
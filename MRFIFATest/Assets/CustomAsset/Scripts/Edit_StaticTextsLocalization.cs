#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[CustomEditor(typeof(StaticTextsLocalization))]
public class Edit_StaticTextsLocalization : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        StaticTextsLocalization staticTextsLocaliztion = (StaticTextsLocalization)target;

        StaticTextsLocalization.DataSlot[] dataSlots = staticTextsLocaliztion.dataSlots;

        if (dataSlots == null)
        {
            dataSlots = new StaticTextsLocalization.DataSlot[0];
        }

        EditorGUILayout.BeginHorizontal();

        int leng = dataSlots.Length;
        EditorGUILayout.LabelField("[ Langth - " + leng + " ]");

        if (GUILayout.Button("+") && leng < 70)
        {
            leng++;
            Array.Resize(ref dataSlots, leng);
            staticTextsLocaliztion.dataSlots = dataSlots;
        }
        if (GUILayout.Button("-") && leng >= 1)
        {
            leng--;
            Array.Resize(ref dataSlots, leng);
            staticTextsLocaliztion.dataSlots = dataSlots;
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < dataSlots.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (dataSlots[i] == null)
            {
                dataSlots[i] = new StaticTextsLocalization.DataSlot();
            }

            EditorGUILayout.LabelField("Slot " + i + (dataSlots[i].ui != null ? (" - " + dataSlots[i].ui.text) : ""));

            dataSlots[i].id = EditorGUILayout.TextField(dataSlots[i].id);
            dataSlots[i].ui = (Text)EditorGUILayout.ObjectField(dataSlots[i] == null ? null : dataSlots[i].ui, typeof(Text), true);

            //object ob = EditorGUILayout.ObjectField(dataSlots[i].ui == null ? null : dataSlots[i].ui, typeof(Text), true);

            //dataSlots[i].ui = (Text)EditorGUILayout.ObjectField(dataSlots[i].ui == null ? null : dataSlots[i].ui, typeof(Text), true);
            //dataSlots[i].id = EditorGUILayout.TextField(dataSlots[i].id);
            EditorGUILayout.EndHorizontal();
        }

        //////////////////////////////////

        StaticTextsLocalization.DataSlot_M[] dataSlots_m = staticTextsLocaliztion.dataSlots_m;

        if (dataSlots_m == null)
        {
            dataSlots_m = new StaticTextsLocalization.DataSlot_M[0];
        }

        EditorGUILayout.BeginHorizontal();

        leng = dataSlots_m.Length;
        EditorGUILayout.LabelField("[ TextMesh - " + leng + " ]");

        if (GUILayout.Button("+") && leng < 50)
        {
            leng++;
            Array.Resize(ref dataSlots_m, leng);
            staticTextsLocaliztion.dataSlots_m = dataSlots_m;
        }
        if (GUILayout.Button("-") && leng >= 1)
        {
            leng--;
            Array.Resize(ref dataSlots_m, leng);
            staticTextsLocaliztion.dataSlots_m = dataSlots_m;
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < dataSlots_m.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (dataSlots_m[i] == null)
            {
                dataSlots_m[i] = new StaticTextsLocalization.DataSlot_M();
            }

            EditorGUILayout.LabelField("Slot " + i + (dataSlots_m[i].ui != null ? (" - " + dataSlots_m[i].ui.text) : ""));

            dataSlots_m[i].id = EditorGUILayout.TextField(dataSlots_m[i].id);
            dataSlots_m[i].ui = (TextMesh)EditorGUILayout.ObjectField(dataSlots_m[i] == null ? null : dataSlots_m[i].ui, typeof(TextMesh), true);

            EditorGUILayout.EndHorizontal();
        }

        //////////////////////

        StaticTextsLocalization.DataSlot_MP[] dataSlots_mp = staticTextsLocaliztion.dataSlots_mp;
        if (dataSlots_mp == null)
        {
            dataSlots_mp = new StaticTextsLocalization.DataSlot_MP[0];
        }

        EditorGUILayout.BeginHorizontal();

        leng = dataSlots_mp.Length;
        EditorGUILayout.LabelField("[ TextMeshPro - " + leng + " ]");

        if (GUILayout.Button("+") && leng < 50)
        {
            leng++;
            Array.Resize(ref dataSlots_mp, leng);
            staticTextsLocaliztion.dataSlots_mp = dataSlots_mp;
        }
        if (GUILayout.Button("-") && leng >= 1)
        {
            leng--;
            Array.Resize(ref dataSlots_mp, leng);
            staticTextsLocaliztion.dataSlots_mp = dataSlots_mp;
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < dataSlots_mp.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (dataSlots_mp[i] == null)
            {
                dataSlots_mp[i] = new StaticTextsLocalization.DataSlot_MP();
            }

            EditorGUILayout.LabelField("Slot " + i + (dataSlots_mp[i].ui != null ? (" - " + dataSlots_mp[i].ui.text) : ""));

            dataSlots_mp[i].id = EditorGUILayout.TextField(dataSlots_mp[i].id);
            dataSlots_mp[i].ui = (TextMeshPro)EditorGUILayout.ObjectField(dataSlots_mp[i] == null ? null : dataSlots_mp[i].ui, typeof(TextMeshPro), true);

            EditorGUILayout.EndHorizontal();
        }

        //////////////////////

        StaticTextsLocalization.DataSlot_MPU[] dataSlots_mpu = staticTextsLocaliztion.dataSlots_mpu;
        if (dataSlots_mpu == null)
        {
            dataSlots_mpu = new StaticTextsLocalization.DataSlot_MPU[0];
        }

        EditorGUILayout.BeginHorizontal();

        leng = dataSlots_mpu.Length;
        EditorGUILayout.LabelField("[ TextMeshProUGUI - " + leng + " ]");

        if (GUILayout.Button("+") && leng < 50)
        {
            leng++;
            Array.Resize(ref dataSlots_mpu, leng);
            staticTextsLocaliztion.dataSlots_mpu = dataSlots_mpu;
        }
        if (GUILayout.Button("-") && leng >= 1)
        {
            leng--;
            Array.Resize(ref dataSlots_mpu, leng);
            staticTextsLocaliztion.dataSlots_mpu = dataSlots_mpu;
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < dataSlots_mpu.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (dataSlots_mpu[i] == null)
            {
                dataSlots_mpu[i] = new StaticTextsLocalization.DataSlot_MPU();
            }

            EditorGUILayout.LabelField("Slot " + i + (dataSlots_mpu[i].ui != null ? (" - " + dataSlots_mpu[i].ui.text) : ""));

            dataSlots_mpu[i].id = EditorGUILayout.TextField(dataSlots_mpu[i].id);
            dataSlots_mpu[i].ui = (TextMeshProUGUI)EditorGUILayout.ObjectField(dataSlots_mpu[i] == null ? null : dataSlots_mpu[i].ui, typeof(TextMeshProUGUI), true);

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("DataReduction"))
        {
            staticTextsLocaliztion.SetDataReduction();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
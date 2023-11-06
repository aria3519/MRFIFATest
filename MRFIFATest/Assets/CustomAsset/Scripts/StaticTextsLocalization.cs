using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SingletonBase;
using UnityEngine.UI;
using TMPro;

public class StaticTextsLocalization : Singleton<StaticTextsLocalization>
{
    [System.Serializable]
    public class DataSlot
    {
        public string id;
        public Text ui;
    }

    public DataSlot[] dataSlots;

    [System.Serializable]
    public class DataSlot_M
    {
        public string id;
        public TextMesh ui;
    }

    public DataSlot_M[] dataSlots_m;

    [System.Serializable]
    public class DataSlot_MP
    {
        public string id;
        public TextMeshPro ui;
    }

    public DataSlot_MP[] dataSlots_mp;

    [System.Serializable]
    public class DataSlot_MPU
    {
        public string id;
        public TextMeshProUGUI ui;
    }

    public DataSlot_MPU[] dataSlots_mpu;


    private bool isInit = false;

    private void Start()
    {
        if (GameSettingCtrl.localizationInfos == null || GameSettingCtrl.localizationInfos.Count == 0)
        {
            return;
        }
        SetLocaliztion();
    }

    public void AddData(string _id, Text ui_text)
    {
        Array.Resize(ref dataSlots, dataSlots.Length + 1);
        int index = dataSlots.Length - 1;
        dataSlots[index] = new DataSlot();
        dataSlots[index].id = _id;
        dataSlots[index].ui = ui_text;

        if (isInit)
        {
            dataSlots[index].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots[index].id);
        }
    }
    public void AddData(string _id, TextMesh ui_text)
    {
        Array.Resize(ref dataSlots_m, dataSlots_m.Length + 1);
        int index = dataSlots_m.Length - 1;
        dataSlots_m[index] = new DataSlot_M();
        dataSlots_m[index].id = _id;
        dataSlots_m[index].ui = ui_text;

        if (isInit)
        {
            dataSlots_m[index].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_m[index].id);
        }
    }
    public void AddData(string _id, TextMeshPro ui_text)
    {
        Array.Resize(ref dataSlots_mp, dataSlots_mp.Length + 1);
        int index = dataSlots_mp.Length - 1;
        dataSlots_mp[index] = new DataSlot_MP();
        dataSlots_mp[index].id = _id;
        dataSlots_mp[index].ui = ui_text;

        if (isInit)
        {
            dataSlots_mp[index].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_mp[index].id);
        }
    }
    public void AddData(string _id, TextMeshProUGUI ui_text)
    {
        Array.Resize(ref dataSlots_mpu, dataSlots_mpu.Length + 1);
        int index = dataSlots_mpu.Length - 1;
        dataSlots_mpu[index] = new DataSlot_MPU();
        dataSlots_mpu[index].id = _id;
        dataSlots_mpu[index].ui = ui_text;

        if (isInit)
        {
            dataSlots_mpu[index].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_mpu[index].id);
        }
    }

    public void SetLocaliztion()
    {
        isInit = true;
        SetDataReduction();

        if (dataSlots != null)
        {
            for (int i = 0; i < dataSlots.Length; i++)
            {
                dataSlots[i].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots[i].id);
            }
        }

        if (dataSlots_m != null)
        {
            for (int i = 0; i < dataSlots_m.Length; i++)
            {
                dataSlots_m[i].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_m[i].id);
            }
        }

        if (dataSlots_mp != null)
        {
            for (int i = 0; i < dataSlots_mp.Length; i++)
            {
                dataSlots_mp[i].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_mp[i].id);
            }
        }

        if (dataSlots_mpu != null)
        {
            for (int i = 0; i < dataSlots_mpu.Length; i++)
            {
                dataSlots_mpu[i].ui.text = GameSettingCtrl.GetLocalizationText(dataSlots_mpu[i].id);
            }
        }
    }

    public void SetDataReduction()
    {
        if (dataSlots != null)
        {
            List<DataSlot> list_dataSlot = new List<DataSlot>();

            for (int i = 0; i < dataSlots.Length; i++)
            {
                if (dataSlots[i].ui == null || string.IsNullOrWhiteSpace(dataSlots[i].id))
                {
                    continue;
                }
                list_dataSlot.Add(dataSlots[i]);
            }

            dataSlots = list_dataSlot.ToArray();
        }

        if (dataSlots_m != null)
        {
            List<DataSlot_M> list_dataSlot_m = new List<DataSlot_M>();
            for (int i = 0; i < dataSlots_m.Length; i++)
            {
                if (dataSlots_m[i].ui == null || string.IsNullOrWhiteSpace(dataSlots_m[i].id))
                {
                    continue;
                }
                list_dataSlot_m.Add(dataSlots_m[i]);
            }

            dataSlots_m = list_dataSlot_m.ToArray();
        }

        if (dataSlots_mp != null)
        {
            List<DataSlot_MP> list_dataSlot_mp = new List<DataSlot_MP>();
            for (int i = 0; i < dataSlots_mp.Length; i++)
            {
                if (dataSlots_mp[i].ui == null || string.IsNullOrWhiteSpace(dataSlots_mp[i].id))
                {
                    continue;
                }
                list_dataSlot_mp.Add(dataSlots_mp[i]);
            }

            dataSlots_mp = list_dataSlot_mp.ToArray();
        }

        if (dataSlots_mpu != null)
        {
            List<DataSlot_MPU> list_dataSlot_mpu = new List<DataSlot_MPU>();
            for (int i = 0; i < dataSlots_mpu.Length; i++)
            {
                if (dataSlots_mpu[i].ui == null || string.IsNullOrWhiteSpace(dataSlots_mpu[i].id))
                {
                    continue;
                }
                list_dataSlot_mpu.Add(dataSlots_mpu[i]);
            }

            dataSlots_mpu = list_dataSlot_mpu.ToArray();
        }
    }
}

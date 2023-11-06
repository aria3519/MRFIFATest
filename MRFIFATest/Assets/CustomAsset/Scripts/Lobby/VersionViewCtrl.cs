using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionViewCtrl : MonoBehaviour
{
    void Start()
    {
        GameSettingCtrl.AddLocalizationChangedEvent(SetText);

    }

    public void SetText(LanguageState languageState)
    {
        transform.GetComponent<TextMesh>().text = GameSettingCtrl.GetLocalizationText("0068") + "\n" + Application.version;
        Debug.Log(transform.GetComponent<TextMesh>().text);
    }
}

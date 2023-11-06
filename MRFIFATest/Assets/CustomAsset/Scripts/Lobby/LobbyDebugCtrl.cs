using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDebugCtrl : MonoBehaviour
{
    private static Text text_debug;

    private void Awake()
    {
        text_debug = transform.GetComponent<Text>();
        text_debug.text = "";
        gameObject.SetActive(false);
    }
    
    public static void AddText(string add_text)
    {
#if DB_TEST
        if (text_debug == null)
        {
            return;
        }
        text_debug.text += add_text + "\n";
        text_debug.gameObject.SetActive(true);
#endif
    }

    public static void ClearText()
    {
#if DB_TEST
        if (text_debug == null)
        {
            return;
        }
        text_debug.text = "";
        text_debug.gameObject.SetActive(false);
#endif
    }
}


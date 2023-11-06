using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HowToPlayCtrl))]
public class Edit_HowToPlayCtrl : Editor
{
    [System.Serializable]
    public enum SlotState
    {
        Slot_1_1, Slot_1_2, Slot_1_3, Slot_2_1, Slot_2_2, Slot_2_3
    }

    [System.Serializable]
    public enum IconState
    {
        None, Num_1, Num_2, Num_3, Num_4, Num_5, Num_6, Num_7, Num_8, Num_9, Warning
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        HowToPlayCtrl howToPlay = (HowToPlayCtrl)target;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("[ Video Setting ]");
        EditorGUILayout.EndHorizontal();

        if (howToPlay.list_videoSlot == null || howToPlay.list_videoSlot.Count == 0)
        {
            howToPlay.list_videoSlot = new List<HowToPlayCtrl.VideoSlot>();
            HowToPlayCtrl.VideoSlot videoSlot = new HowToPlayCtrl.VideoSlot();
            videoSlot.device = HowToPlayCtrl.VRDevice.DEFAULT;
            switch (howToPlay.slotState)
            {
                case SlotState.Slot_1_1:
                case SlotState.Slot_1_2:
                case SlotState.Slot_1_3:
                    {
                        videoSlot.videoClips = new VideoClip[1];
                    }
                    break;
                case SlotState.Slot_2_1:
                case SlotState.Slot_2_2:
                case SlotState.Slot_2_3:
                    {
                        videoSlot.videoClips = new VideoClip[2];
                    }
                    break;
            }
            howToPlay.list_videoSlot.Add(videoSlot);
        }

        for (int i = 0; i < howToPlay.list_videoSlot.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Separator();
            HowToPlayCtrl.VRDevice device = (HowToPlayCtrl.VRDevice)EditorGUILayout.EnumPopup(howToPlay.list_videoSlot[i].device);
            if (howToPlay.list_videoSlot[i].device != device)
            {
                HowToPlayCtrl.VRDevice device_previous = howToPlay.list_videoSlot[i].device;
                howToPlay.list_videoSlot[i].device = device;
                if (device == HowToPlayCtrl.VRDevice.DEFAULT)
                {
                    if (howToPlay.list_videoSlot.Count > 1)
                    {
                        howToPlay.list_videoSlot.RemoveAt(i);
                        break;
                    }
                }
                else if (device_previous == HowToPlayCtrl.VRDevice.DEFAULT)
                {
                    HowToPlayCtrl.VideoSlot videoSlot = new HowToPlayCtrl.VideoSlot();
                    videoSlot.device = HowToPlayCtrl.VRDevice.DEFAULT;
                    switch (howToPlay.slotState)
                    {
                        case SlotState.Slot_1_1:
                        case SlotState.Slot_1_2:
                        case SlotState.Slot_1_3:
                            {
                                videoSlot.videoClips = new VideoClip[1];
                            }
                            break;
                        case SlotState.Slot_2_1:
                        case SlotState.Slot_2_2:
                        case SlotState.Slot_2_3:
                            {
                                videoSlot.videoClips = new VideoClip[2];
                            }
                            break;
                    }
                    howToPlay.list_videoSlot.Add(videoSlot);
                    howToPlay.list_videoSlot[i].device = device;
                    break;
                }
            }

            for (int j = 0; j < howToPlay.list_videoSlot[i].videoClips.Length; j++)
            {
                howToPlay.list_videoSlot[i].videoClips[j] = (VideoClip)EditorGUILayout.ObjectField(howToPlay.list_videoSlot[i].videoClips[j] == null ? null : howToPlay.list_videoSlot[i].videoClips[j], typeof(VideoClip), true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3f);
        }

        //////////////////////////////////////
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("============================================");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("[ Canvas Setting ]");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Slot");
        SlotState temp_slotState = (SlotState)EditorGUILayout.EnumPopup(howToPlay.slotState);
        if (howToPlay.slotState != temp_slotState)
        {
            SetSlotSize(temp_slotState);
            for (int i = 0; i < howToPlay.list_videoSlot.Count; i++)
            {
                switch (temp_slotState)
                {
                    case SlotState.Slot_1_1:
                    case SlotState.Slot_1_2:
                    case SlotState.Slot_1_3:
                        {
                            howToPlay.list_videoSlot[i].videoClips = new VideoClip[1];
                        }
                        break;
                    case SlotState.Slot_2_1:
                    case SlotState.Slot_2_2:
                    case SlotState.Slot_2_3:
                        {
                            howToPlay.list_videoSlot[i].videoClips = new VideoClip[2];
                        }
                        break;
                }
            }

            howToPlay.slotState = temp_slotState;
        }
        EditorGUILayout.EndHorizontal();

        //////////////////////////////////////////

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Icons");
        EditorGUILayout.EndHorizontal();

        int iconLength = GetIconLength(howToPlay.slotState);
        if (howToPlay.iconStates == null || howToPlay.iconStates.Length != iconLength)
        {
            howToPlay.iconStates = new IconState[iconLength];
        }


        if (howToPlay.slotState == SlotState.Slot_1_1 || howToPlay.slotState == SlotState.Slot_1_2 || howToPlay.slotState == SlotState.Slot_1_3)
        {
            for (int i = 0; i < howToPlay.iconStates.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                IconState temp_iconState = (IconState)EditorGUILayout.EnumPopup(howToPlay.iconStates[i]);
                if (howToPlay.iconStates[i] != temp_iconState)
                {
                    SetIcon(i, temp_iconState);
                    howToPlay.iconStates[i] = temp_iconState;
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            int index_sub = howToPlay.iconStates.Length / 2;
            for (int i = 0; i < index_sub; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                IconState temp_iconState = (IconState)EditorGUILayout.EnumPopup(howToPlay.iconStates[i]);
                if (howToPlay.iconStates[i] != temp_iconState)
                {
                    SetIcon(i, temp_iconState);
                    howToPlay.iconStates[i] = temp_iconState;
                }

                temp_iconState = (IconState)EditorGUILayout.EnumPopup(howToPlay.iconStates[i + index_sub]);
                if (howToPlay.iconStates[i + index_sub] != temp_iconState)
                {
                    SetIcon(i + index_sub, temp_iconState);
                    howToPlay.iconStates[i + index_sub] = temp_iconState;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    public void SetSlotSize(SlotState slotState)
    {
        Transform transform = ((HowToPlayCtrl)target).transform;

        switch (slotState)
        {
            case SlotState.Slot_1_1:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 10);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 10);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(3200, 1800);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 900, 0);
                    tempTr.localScale = new Vector3(40, 37.5f, 37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_1.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 900, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_1.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 512);
                    tempTr.localPosition = new Vector3(0, -0.3f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr != null)
                    {
                        DestroyImmediate(tempTr.gameObject);
                    }
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 10);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-7, 2.42f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case SlotState.Slot_1_2:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 18);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 18);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(3200, 3250);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 1625, 0);
                    tempTr.localScale = new Vector3(40, 37.5f, 37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_1.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 1625, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_2.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1024);
                    tempTr.localPosition = new Vector3(0, -0.35f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr != null)
                    {
                        DestroyImmediate(tempTr.gameObject);
                    }
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 18);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-7, 6.12f, 0);
                    tempTr.GetChild(1).localPosition = new Vector3(-7, -1.05f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case SlotState.Slot_1_3:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 25);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 25);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(3200, 4650);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 2325, 0);
                    tempTr.localScale = new Vector3(40, 37.5f, 37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_1.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 2325, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_3.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1536);
                    tempTr.localPosition = new Vector3(0, -0.55f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr != null)
                    {
                        DestroyImmediate(tempTr.gameObject);
                    }
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 25);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-7, 9.72f, 0);
                    tempTr.GetChild(1).localPosition = new Vector3(-7, 2.57f, 0);
                    tempTr.GetChild(2).localPosition = new Vector3(-7, -4.63f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case SlotState.Slot_2_1:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 10);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 10);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(6300, 1800);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 900, 0);
                    tempTr.localScale = new Vector3(38.65f, 37.5f, 37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_2.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 900, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_1.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 512);
                    tempTr.localPosition = new Vector3(-7.75f, -0.3f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr == null)
                    {
                        Transform tr_origin = transform.Find("Board/Image_Mask");
                        tempTr = Instantiate(tr_origin, tr_origin.parent);
                        tempTr.name = "Image_Mask2";
                    }
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 512);
                    tempTr.localPosition = new Vector3(7.75f, -0.3f, 0);
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.SetAsLastSibling();
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 10);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-14.75f, 2.42f, 0);
                    tempTr.GetChild(1).localPosition = new Vector3(0.75f, 2.42f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case SlotState.Slot_2_2:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 18);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 18);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(6300, 3250);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 1625, 0);
                    tempTr.localScale = new Vector3(38.65f, 37.5f,37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_2.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 1625, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_2.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1024);
                    tempTr.localPosition = new Vector3(-7.75f, -0.35f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr == null)
                    {
                        Transform tr_origin = transform.Find("Board/Image_Mask");
                        tempTr = Instantiate(tr_origin, tr_origin.parent);
                        tempTr.name = "Image_Mask2";
                    }
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1024);
                    tempTr.localPosition = new Vector3(7.75f, -0.35f, 0);
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.SetAsLastSibling();
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 18);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-14.75f, 6.12f, 0);
                    tempTr.GetChild(1).localPosition = new Vector3(-14.75f, -1.05f, 0);
                    tempTr.GetChild(2).localPosition = new Vector3(0.75f, 6.12f, 0);
                    tempTr.GetChild(3).localPosition = new Vector3(0.75f, -1.05f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case SlotState.Slot_2_3:
                {
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 25);
                    Transform tempTr = transform.Find("Board");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 25);
                    //Window.
                    tempTr = transform.Find("Board/Image_Window");
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(6300, 4650);
                    tempTr = transform.Find("Board/Image_Window/Top");
                    tempTr.localPosition = new Vector3(0, 2325, 0);
                    tempTr.localScale = new Vector3(38.65f, 37.5f, 37.5f);
                    tempTr.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/Mesh_TopBar_2.mesh")[0];
                    tempTr = transform.Find("Board/Image_Window/Text_Title");
                    tempTr.localPosition = new Vector3(-44, 2325, -80);
                    //Mask.
                    tempTr = transform.Find("Board/Image_Mask");
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Mask_3.png");
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1536);
                    tempTr.localPosition = new Vector3(-7.75f, -0.55f, 0);

                    tempTr = transform.Find("Board/Image_Mask2");
                    if (tempTr == null)
                    {
                        Transform tr_origin = transform.Find("Board/Image_Mask");
                        tempTr = Instantiate(tr_origin, tr_origin.parent);
                        tempTr.name = "Image_Mask2";
                    }
                    foreach (Object o in objects)
                    {
                        if (o.GetType() == typeof(Sprite))
                        {
                            tempTr.GetComponent<Image>().sprite = (Sprite)o;
                            break;
                        }
                    }
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1536);
                    tempTr.localPosition = new Vector3(7.75f, -0.55f, 0);
                    //Icons.
                    tempTr = transform.Find("Board/Icons");
                    tempTr.SetAsLastSibling();
                    tempTr.GetComponent<RectTransform>().sizeDelta = new Vector2(30.5f, 25);

                    int targetCount = GetIconLength(slotState);
                    for (int i = tempTr.childCount; i > targetCount; i--)
                    {
                        DestroyImmediate(tempTr.GetChild(i - 1).gameObject);
                    }
                    for (int i = targetCount - tempTr.childCount; i > 0; i--)
                    {
                        Instantiate(tempTr.GetChild(0).gameObject, tempTr.GetChild(0).parent);
                    }

                    tempTr.GetChild(0).localPosition = new Vector3(-14.75f, 9.72f, 0);
                    tempTr.GetChild(1).localPosition = new Vector3(-14.75f, 2.57f, 0);
                    tempTr.GetChild(2).localPosition = new Vector3(-14.75f, -4.63f, 0);
                    tempTr.GetChild(3).localPosition = new Vector3(0.75f, 9.72f, 0);
                    tempTr.GetChild(4).localPosition = new Vector3(0.75f, 2.57f, 0);
                    tempTr.GetChild(5).localPosition = new Vector3(0.75f, -4.63f, 0);

                    for (int i = 0; i < targetCount; i++)
                    {
                        tempTr.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
        }
    }
    public void SetIcon(int index, IconState iconState)
    {
        Image image_icon = ((HowToPlayCtrl)target).transform.Find("Board/Icons").GetChild(index).GetComponent<Image>();

        switch (iconState)
        {
            case IconState.None:
                {
                    image_icon.sprite = null;
                }
                break;
            case IconState.Num_1:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_1");
                }
                break;
            case IconState.Num_2:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_2");
                }
                break;
            case IconState.Num_3:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_3");
                }
                break;
            case IconState.Num_4:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_4");
                }
                break;
            case IconState.Num_5:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_5");
                }
                break;
            case IconState.Num_6:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_6");
                }
                break;
            case IconState.Num_7:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_7");
                }
                break;
            case IconState.Num_8:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_8");
                }
                break;
            case IconState.Num_9:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_9");
                }
                break;
            case IconState.Warning:
                {
                    image_icon.sprite = GetIcon("HowToPlay_Icons_10");
                }
                break;
        }

        image_icon.gameObject.SetActive(iconState != IconState.None);
    }

    public Sprite GetIcon(string name_sprite)
    {
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Assets/4_Graphic Assets/UI/HowToPlay_New/HowToPlay_Icons.png");
        foreach (Object o in objects)
        {
            if (o.GetType() == typeof(Sprite) && o.name == name_sprite)
            {
                return (Sprite)o;
            }
        }
        return null;
    }

    public int GetIconLength(SlotState slotState)
    {
        switch (slotState)
        {
            case SlotState.Slot_1_1:
                return 1;
            case SlotState.Slot_1_2:
                return 2;
            case SlotState.Slot_1_3:
                return 3;
            case SlotState.Slot_2_1:
                return 2;
            case SlotState.Slot_2_2:
                return 4;
            case SlotState.Slot_2_3:
                return 6;
            default:
                return 1;
        }
    }
}
#endif

public class HowToPlayCtrl : MonoBehaviour
{
    public enum VRDevice
    {
        DEFAULT, OCULUS, INDEX, VIVE, PICO_3, PICO_4
    }

    [System.Serializable]
    public class VideoSlot
    {
        public VRDevice device;
        public VideoClip[] videoClips;
    }

    public List<VideoSlot> list_videoSlot;

    private Text text_title;

#if UNITY_EDITOR
    public Edit_HowToPlayCtrl.SlotState slotState;
    public Edit_HowToPlayCtrl.IconState[] iconStates;
#endif
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetInitData());
    }

    IEnumerator SetInitData()
    {
        yield return null;

        if (GameDataManager.instance != null)
        {
            text_title = transform.Find("Board/Image_Window/Text_Title").GetComponent<Text>();
            GameSettingCtrl.AddLocalizationChangedEvent(() => text_title.text = GameSettingCtrl.GetLocalizationText("0037"));
        }

        string vrDevice = GameDataManager.vrDevice.ToString();

        for (int i = 0; i < list_videoSlot.Count; i++)
        {
            if (list_videoSlot[i].device == VRDevice.DEFAULT || list_videoSlot[i].device.ToString() == vrDevice)
            {
                for (int j = 0; j < list_videoSlot[i].videoClips.Length; j++)
                {
                    VideoPlayer videoPlayer;
                    if (j == 0)
                    {
                        videoPlayer = transform.Find("Board/Image_Mask/Image_Video").GetComponent<VideoPlayer>();
                    }
                    else
                    {
                        videoPlayer = transform.Find("Board/Image_Mask2/Image_Video").GetComponent<VideoPlayer>();
                    }

                    videoPlayer.clip = list_videoSlot[i].videoClips[j];

                    RenderTexture rt = new RenderTexture((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, 16, RenderTextureFormat.Default);
                    rt.useMipMap = true;
                    videoPlayer.targetTexture = rt;

                    videoPlayer.gameObject.AddComponent<RawImage>().texture = rt;
                }
                break;
            }
        }
    }
}

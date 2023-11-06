﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum LobbyState
{
    Lobby, Customize
}

public class LobbyPlayerCtrl : MonoBehaviour
{
    public LobbyState lobbyState = LobbyState.Lobby;

    public LobbyPropCtrl grab_prop_lobby;
    public CustomizeItemCtrl grab_prop_custom;

    private int handActiveState = -1;

    public UIHandInfo[] handInfos = new UIHandInfo[2];

    public LayerMask layerMask;

    public enum HandActionState
    {
        Idle, GrabProp, UIDirection
    }
    public HandActionState[] handActionStates = new HandActionState[2];

    private bool isRotation = false;
    private bool isPalette_h = false;
    private bool isPalette_sv = false;

    private Vector2 previousDir_hand;
    private Vector2 previousDir_arm;
    private float rotForce;
    private float drag = 2f;

    private void Awake()
    {
        handInfos = new UIHandInfo[2];
        Transform tempTr;
        for (int i = 0; i < handInfos.Length; i++)
        {
            handInfos[i] = new UIHandInfo();
            if (i == 0)
            {
                tempTr = transform.Find("Camera Offset/LeftHand Controller");
            }
            else
            {
                tempTr = transform.Find("Camera Offset/RightHand Controller");
            }
            handInfos[i].controller = tempTr.GetComponent<XRController>();
            handInfos[i].anim = tempTr.GetChild(0).GetComponent<Animator>();
            handInfos[i].rayDir = handInfos[i].anim.transform.Find("RayDir");
            if (i == 0)
            {
                tempTr = transform.Find("Camera Offset/RayL");
            }
            else
            {
                tempTr = transform.Find("Camera Offset/RayR");
            }
            handInfos[i].line_transform = tempTr.Find("Line");
            handInfos[i].ball_transform = tempTr.Find("Sphere");
        }

        handActionStates = new HandActionState[2];
        for (int i = 0; i < handActionStates.Length; i++)
        {
            handActionStates[i] = HandActionState.Idle;
        }

        InputDevices_Update(new InputDevice());
    }

    void OnEnable()
    {
        InputDevices_Update(new InputDevice());

        InputDevices.deviceConnected += InputDevices_Update;
        InputDevices.deviceDisconnected += InputDevices_Update;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_Update;
        InputDevices.deviceDisconnected -= InputDevices_Update;
    }

    private void InputDevices_Update(InputDevice _device)
    {
        List<InputDevice> inputDevices = new List<InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, inputDevices);
        handInfos[0].isControllerOn = false;
        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float velue))
            {
                handInfos[0].isControllerOn = true;
            }
        }

        if (!handInfos[0].isControllerOn)
        {
            handInfos[0].InitData();
        }

        inputDevices.Clear();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        handInfos[1].isControllerOn = false;
        foreach (var device in inputDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float velue))
            {
                handInfos[1].isControllerOn = true;
            }
        }
        if (!handInfos[1].isControllerOn)
        {
            handInfos[1].InitData();
        }
    }

    void Update()
    {
#if OCULUS_PLATFORM && !UNITY_EDITOR
        if (!OVRManager.hasInputFocus)
        {
            for (int i = 0; i < 2; i++)
            {
                handInfos[i].SetPointState(UIHandInfo.PointState.Idle);
            }
            return;
        }
#endif
        for (int i = 0; i < 2; i++)
        {
            if (handInfos[i].controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))  // 통합 트리거 체크.
            {
                handInfos[i].anim.SetFloat("Value", triggerValue);

                if (triggerValue >= 0.6f && !handInfos[i].isTriggerOn)
                {
                    handInfos[i].SetButtonEventState(UIHandInfo.TriggerEventState.On);
                }
                else if (triggerValue <= 0.4f && handInfos[i].isTriggerOn)
                {
                    handInfos[i].SetButtonEventState(UIHandInfo.TriggerEventState.Off);
                }
                else
                {
                    handInfos[i].SetButtonEventState(UIHandInfo.TriggerEventState.None);
                }
            }
        }

        if (PublicGameUIManager.GetInstance.GetCurrentState() != PublicGameUIManager.ViewState.None) // 퍼블릭 UI가 보여지고 있을때.
        {
            return;
        }

        for (int i = 0; i < 2; i++) // 통합 레이 계산.
        {
            if (Physics.Raycast(handInfos[i].rayDir.position, handInfos[i].rayDir.forward, out RaycastHit hit, 3f, layerMask))
            {
                handInfos[i].hitCollider = hit.collider;
                handInfos[i].hitPoint = hit.point;
            }
            else
            {
                handInfos[i].hitCollider = null;
            }
        }

        if (lobbyState == LobbyState.Lobby)
        {
            if (LobbyUIManager.GetInstance.isStartGame) // 게임이 시작되려고 할때.
            {
                for (int i = 0; i < 2; i++)
                {
                    handInfos[i].SetPointState(UIHandInfo.PointState.Idle);
                }
                return;
            }

            if (LobbyUIManager.GetInstance.lobbyUIState != LobbyUIManager.LobbyUIState.Close) // 로비 중앙 UI 켜져있을때.
            {
                for (int i = 0; i < 2; i++)
                {
                    if (handInfos[i].hitCollider != null)
                    {
                        Vector3 dir = handInfos[i].hitPoint - handInfos[i].rayDir.position;

                        handInfos[i].SetPointState(UIHandInfo.PointState.UI);
                        handInfos[i].SetUITransform(handInfos[i].hitPoint, handInfos[i].rayDir.position, Quaternion.LookRotation(dir), dir.magnitude);

                        if (handInfos[i].buttonState == UIHandInfo.TriggerEventState.On)
                        {
                            MeshButtonCtrl button = handInfos[i].hitCollider.GetComponent<MeshButtonCtrl>();
                            if (button != null && button.IsInteractable())
                            {
                                button.event_click.Invoke();
                            }
                            else if (handInfos[i].hitCollider.gameObject.layer == 5)
                            {
                                handActiveState = i;
                            }
                        }
                    }
                    else
                    {
                        handInfos[i].SetPointState(UIHandInfo.PointState.Point);
                    }
                }
                return;
            }

            if (grab_prop_lobby == null || (grab_prop_lobby != null && grab_prop_lobby.GetPropState() == PropState.Idle)) // 로비 중앙 UI 꺼져있을때.
            {
                for (int i = 0; i < 2; i++)
                {
                    if (handActionStates[i] != HandActionState.UIDirection)
                    {
                        LobbyPropCtrl lobbyPropCtrl = LobbyPropManager.GetInstance.FindNearProp(handInfos[i], i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    LobbyPropManager.GetInstance.SetSelectPointActive(false, i);
                }
            }

            for (int i = 0; i < 2; i++) // 본 계산.
            {
                if (i == 0)
                {
                    if (handActiveState == 1 || !handInfos[0].isControllerOn)
                    {
                        continue;
                    }
                }
                else
                {
                    if (handActiveState == 0 || !handInfos[1].isControllerOn)
                    {
                        continue;
                    }
                }

                if (handInfos[i].hitCollider != null) // UI가 닿았는지.
                {
                    if (handInfos[i].hitCollider.gameObject.layer == 5 && handActionStates[i] != HandActionState.GrabProp)
                    {
                        handActionStates[i] = HandActionState.UIDirection;
                        handInfos[i].SetPointState(UIHandInfo.PointState.UI);
                        Vector3 dir = handInfos[i].hitPoint - handInfos[i].rayDir.position;
                        handInfos[i].SetUITransform(handInfos[i].hitPoint, handInfos[i].rayDir.position, handInfos[i].rayDir.rotation, dir.magnitude);
                    }
                    else if (handActionStates[i] != HandActionState.GrabProp)
                    {
                        handActionStates[i] = HandActionState.Idle;
                    }
                }
                else
                {
                    if (handActionStates[i] != HandActionState.GrabProp)
                    {
                        handActionStates[i] = HandActionState.Idle;
                    }
                }

                switch (handInfos[i].buttonState)
                {
                    case UIHandInfo.TriggerEventState.On:
                        {
                            handActiveState = i;

                            if (grab_prop_lobby != null)
                            {
                                grab_prop_lobby.SetPropState(PropState.Idle);
                            }

                            if (handActionStates[i] == HandActionState.UIDirection)
                            {
                                grab_prop_lobby = null;
                            }
                            else
                            {
                                grab_prop_lobby = LobbyPropManager.GetInstance.FindNearProp(handInfos[i], i);
                            }

                            if (grab_prop_lobby != null)
                            {
                                handInfos[0].SetPointState(UIHandInfo.PointState.Idle);
                                handInfos[1].SetPointState(UIHandInfo.PointState.Idle);

                                LobbyPropManager.GetInstance.StopSelectGame();
                                if (LobbyUIManager.GetInstance.lobbyUIState != LobbyUIManager.LobbyUIState.Close)
                                {
                                    LobbyUIManager.GetInstance.SetLobbyUI(LobbyUIManager.LobbyUIState.Close);
                                }

                                if (LobbyPropManager.GetInstance.keep_prop != null && LobbyPropManager.GetInstance.keep_prop != grab_prop_lobby)
                                {
                                    LobbyPropManager.GetInstance.keep_prop.SetIdlePos();
                                    LobbyPropManager.GetInstance.keep_prop.SetPropState(PropState.Idle);
                                    LobbyPropManager.GetInstance.keep_prop = null;
                                    GameDataManager.instance.gameType = GameDataManager.GameType.None;
                                }
                                grab_prop_lobby.SetPropState(PropState.Grab, handInfos[i].rayDir, i);
                                handActionStates[i] = HandActionState.GrabProp;
                            }
                        }
                        break;
                    case UIHandInfo.TriggerEventState.Off:
                        {
                            handActiveState = -1;

                            if (grab_prop_lobby != null)
                            {
                                Vector3 handVec = handInfos[i].rayDir.forward;
                                handVec.y = 0f;
                                handVec = handVec.normalized;
                                Vector3 targetVec = LobbyPropManager.GetInstance.gameStartPos.position - handInfos[i].rayDir.position;
                                targetVec.y = 0f;
                                targetVec = targetVec.normalized;

                                float dist = Mathf.Clamp01((handInfos[1].rayDir.position - LobbyPropManager.GetInstance.gameStartPos.position).sqrMagnitude);

                                if (dist <= 0.1f || Vector3.Dot(handVec, targetVec) >= Mathf.Lerp(0f, 0.75f, dist))
                                {
                                    grab_prop_lobby.SetPropState(PropState.Set);
                                    LobbyPropManager.GetInstance.SetSelectPointActive(false, 0);
                                    LobbyPropManager.GetInstance.SetSelectPointActive(false, 1);
                                }
                                else
                                {
                                    grab_prop_lobby.SetPropState(PropState.Idle);
                                }

                                handActionStates[i] = HandActionState.Idle;
                            }
                        }
                        break;
                }
            }
        }
        else if (lobbyState == LobbyState.Customize)
        {
            if (isRotation)
            {
                Vector2 currentDir_hand = new Vector2(handInfos[handActiveState].rayDir.forward.x, handInfos[handActiveState].rayDir.forward.z).normalized;
                Vector3 arm_dir = handInfos[handActiveState].rayDir.position - Camera.main.transform.position;

                Vector2 currentDir_arm = new Vector2(arm_dir.x, arm_dir.z).normalized;

                rotForce = Vector2.SignedAngle(previousDir_hand, currentDir_hand) * 0.5f + Vector2.SignedAngle(previousDir_arm, currentDir_arm) * 3f;
                CustomizeManager.GetInstance.SetRotationCharacter(rotForce);
                rotForce *= 5000f * Time.deltaTime;

                previousDir_hand = currentDir_hand;
                previousDir_arm = currentDir_arm;
            }
            else if (rotForce != 0f)
            {
                rotForce *= 1 - Time.deltaTime * drag;

                CustomizeManager.GetInstance.SetRotationCharacter(rotForce * Time.deltaTime);

                if (Mathf.Abs(rotForce) <= 5f)
                {
                    rotForce = 0f;
                }
            }

            CustomizeItemCtrl itemCtrl = null;

            for (int i = 1; i >= 0; i--)
            {
                if (handActiveState == -1 && handInfos[i].isControllerOn)
                {
                    if (handInfos[i].hitCollider != null)
                    {
                        Vector3 dir = handInfos[i].hitPoint - handInfos[i].rayDir.position;

                        handInfos[i].SetUITransform(handInfos[i].hitPoint, handInfos[i].rayDir.position, Quaternion.LookRotation(dir), dir.magnitude);

                        if (handInfos[i].buttonState == UIHandInfo.TriggerEventState.On)
                        {
                            MeshButtonCtrl button = handInfos[i].hitCollider.GetComponent<MeshButtonCtrl>();
                            if (button != null && button.IsInteractable())
                            {
                                button.event_click.Invoke();
                                continue;
                            }
                            else
                            {
                                ColorPaletteCtrl colorPalette = handInfos[i].hitCollider.GetComponent<ColorPaletteCtrl>();
                                if (colorPalette != null)
                                {
                                    int state = colorPalette.GetPaletteState(handInfos[i].hitPoint);

                                    switch (state)
                                    {
                                        case 1:
                                            {
                                                isPalette_h = true;
                                                handActiveState = i;
                                                colorPalette.Click_Palette(true, handInfos[i].hitPoint);
                                                handInfos[i].SetPointState(UIHandInfo.PointState.Point);
                                            }
                                            continue;
                                        case 2:
                                            {
                                                isPalette_sv = true;
                                                handActiveState = i;
                                                colorPalette.Click_Palette(false, handInfos[i].hitPoint);
                                                handInfos[i].SetPointState(UIHandInfo.PointState.Point);
                                            }
                                            continue;
                                    }
                                }
                            }
                        }

                        handInfos[i].SetPointState(UIHandInfo.PointState.UI);
                        continue;
                    }
                    else
                    {
                        handInfos[i].SetPointState(UIHandInfo.PointState.PropLine);
                    }
                }
                else if ((isPalette_h || isPalette_sv) && handActiveState == i && handInfos[i].isControllerOn)
                {
                    int state = isPalette_h ? 1 : 2;
                    if (handInfos[i].hitCollider != null)
                    {
                        switch (state)
                        {
                            case 1:
                                {
                                    isPalette_h = true;
                                    handActiveState = i;
                                    CustomizeManager.GetInstance.colorPalette.Click_Palette(true, handInfos[i].hitPoint);
                                }
                                break;
                            case 2:
                                {
                                    isPalette_sv = true;
                                    handActiveState = i;
                                    CustomizeManager.GetInstance.colorPalette.Click_Palette(false, handInfos[i].hitPoint);
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (state)
                        {
                            case 1:
                                {
                                    isPalette_h = true;
                                    handActiveState = i;
                                    CustomizeManager.GetInstance.colorPalette.Click_Palette(true, handInfos[i].rayDir.position + handInfos[i].rayDir.forward);
                                }
                                break;
                            case 2:
                                {
                                    isPalette_sv = true;
                                    handActiveState = i;
                                    CustomizeManager.GetInstance.colorPalette.Click_Palette(false, handInfos[i].rayDir.position + handInfos[i].rayDir.forward);
                                }
                                break;
                        }
                    }
                }
                else if (handActiveState != i && handActiveState != -1 && handInfos[i].isControllerOn)
                {
                    handInfos[i].SetPointState(UIHandInfo.PointState.Idle);
                    continue;
                }

                if (grab_prop_custom == null && !isRotation && !isPalette_h && !isPalette_sv)
                {
                    CustomizeProp customizeProp = CustomizeManager.GetInstance.FindNearProp(handInfos[i], i);
                    if (customizeProp is CustomizeItemCtrl)
                    {
                        if (itemCtrl == null)
                        {
                            itemCtrl = (CustomizeItemCtrl)customizeProp;
                        }
                        else if (!itemCtrl.isLock)
                        {
                            itemCtrl = (CustomizeItemCtrl)customizeProp;
                        }
                    }
                }

                if (handInfos[i].buttonState == UIHandInfo.TriggerEventState.On)
                {
                    handActiveState = i;

                    CustomizeProp uIProp = CustomizeManager.GetInstance.FindNearProp(handInfos[i], i);

                    Vector3 handVec = handInfos[i].rayDir.forward;
                    handVec.y = 0f;
                    Vector3 targetVec = CustomizeManager.GetInstance.customSetPos_body.position - handInfos[i].rayDir.position;
                    targetVec.y = 0f;
                    targetVec = targetVec.normalized;

                    if (uIProp != null)
                    {
                        if (uIProp is CustomizeItemCtrl)
                        {
                            grab_prop_custom = (CustomizeItemCtrl)uIProp;

                            if (grab_prop_custom.isLock)
                            {
                                grab_prop_custom = null;
                            }
                            if (grab_prop_custom != null)
                            {
                                grab_prop_custom.SetPropState(PropState.Grab, handInfos[i].rayDir, i);
                            }
                        }
                        else if ((uIProp is MeshButtonCtrl) && ((MeshButtonCtrl)uIProp).IsInteractable())
                        {
                            ((MeshButtonCtrl)uIProp).event_click.Invoke();
                        }
                    }
                    else if ((handInfos[i].rayDir.position - CustomizeManager.GetInstance.customSetPos_body.position).sqrMagnitude <= 0.15f || Vector3.Dot(handVec, targetVec) >= 0.75f)
                    {
                        isRotation = true;
                        previousDir_hand.x = handInfos[i].rayDir.forward.x;
                        previousDir_hand.y = handInfos[i].rayDir.forward.z;
                        previousDir_hand = previousDir_hand.normalized;

                        Vector3 arm_dir = handInfos[i].rayDir.position - Camera.main.transform.position;
                        previousDir_arm.x = arm_dir.x;
                        previousDir_arm.y = arm_dir.z;
                        previousDir_arm = previousDir_arm.normalized;
                    }

                    handInfos[i].SetPointState(UIHandInfo.PointState.Idle);
                }
                else if (handInfos[i].buttonState == UIHandInfo.TriggerEventState.Off)
                {
                    handActiveState = -1;

                    if (grab_prop_custom != null)
                    {
                        Vector3 handVec = handInfos[i].rayDir.forward;
                        handVec.y = 0f;
                        handVec = handVec.normalized;
                        Vector3 targetVec = CustomizeManager.GetInstance.customSetPos_body.position - handInfos[i].rayDir.position;
                        targetVec.y = 0f;
                        targetVec = targetVec.normalized;

                        if ((handInfos[i].rayDir.position - CustomizeManager.GetInstance.customSetPos_body.position).sqrMagnitude <= 0.15f || Vector3.Dot(handVec, targetVec) >= 0.9f)
                        {
                            grab_prop_custom.SetPropState(PropState.Set);
                            grab_prop_custom = null;
                        }
                        else
                        {
                            grab_prop_custom.SetPropState(PropState.Idle);
                            grab_prop_custom = null;
                        }
                    }

                    handInfos[i].SetPointState(UIHandInfo.PointState.PropLine);

                    isRotation = false;
                    isPalette_h = false;
                    isPalette_sv = false;
                }
            }

            if (grab_prop_custom == null)
            {
                if (itemCtrl != null)
                {
                    CustomizeManager.GetInstance.CheckSlotLock(itemCtrl);
                }
                else
                {
                    CustomizeManager.GetInstance.CheckSlotLock(null);
                }
            }
        }
    }
}

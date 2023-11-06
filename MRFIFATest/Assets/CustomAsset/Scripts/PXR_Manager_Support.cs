using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PXR_Manager_Support : MonoBehaviour
{
#if PICO_PLATFORM
    private Camera[] eyeCamera;
    private int[] eyeCameraOriginCullingMask = new int[3];
    private CameraClearFlags[] eyeCameraOriginClearFlag = new CameraClearFlags[3];

    void Awake()
    {
        eyeCamera = new Camera[3];
        Camera[] cam = gameObject.GetComponentsInChildren<Camera>();
        for (int i = 0; i < cam.Length; i++)
        {
            if (cam[i].stereoTargetEye == StereoTargetEyeMask.Both)
            {
                eyeCamera[0] = cam[i];
            }
            else if (cam[i].stereoTargetEye == StereoTargetEyeMask.Left)
            {
                eyeCamera[1] = cam[i];
            }
            else if (cam[i].stereoTargetEye == StereoTargetEyeMask.Right)
            {
                eyeCamera[2] = cam[i];
            }
        }
    }

    void OnEnable()
    {
        Unity.XR.PXR.PXR_Plugin.System.SeethroughStateChangedChanged = null;
        Unity.XR.PXR.PXR_Plugin.System.SeethroughStateChangedChanged += SeeThroughStateChangedCallback;
    }

    void OnDisable()
    {
        Unity.XR.PXR.PXR_Plugin.System.SeethroughStateChangedChanged -= SeeThroughStateChangedCallback;
    }

    void SeeThroughStateChangedCallback(int value)
    {
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (eyeCamera[i] != null)
                {
                    switch (value)
                    {
                        case 0:
                        case 1:
                            if (eyeCamera[i].clearFlags == CameraClearFlags.Nothing)
                            {
                                eyeCamera[i].clearFlags = eyeCameraOriginClearFlag[i];
                                eyeCamera[i].cullingMask = eyeCameraOriginCullingMask[i];
                            }
                            break;
                        case 2:
                            eyeCameraOriginClearFlag[i] = eyeCamera[i].clearFlags;
                            eyeCameraOriginCullingMask[i] = eyeCamera[i].cullingMask;

                            eyeCamera[i].cullingMask = 0;
                            eyeCamera[i].clearFlags = CameraClearFlags.Nothing;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("PXRLog SeeThroughStateChangedCallback Error_Support" + e.ToString());
        }

    }
#endif
}

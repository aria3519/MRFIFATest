﻿/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.  

NOTICE：All information contained herein is, and remains the property of 
PICO Technology Co., Ltd. The intellectual and technical concepts 
contained hererin are proprietary to PICO Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd. 
*******************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Unity.XR.PXR
{
    public class PXR_Manager : MonoBehaviour
    {
        private const string TAG = "[PXR_Manager]";
        private static PXR_Manager instance = null;
        private static bool bindVerifyServiceSuccess = false;
        public static PXR_Manager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PXR_Manager>();
                    if (instance == null)
                    {
                        Debug.LogError("PXRLog instance is not initialized!");
                    }
                }
                return instance;
            }
        }

        private int lastBoundaryState = 0;
        private int currentBoundaryState;
        private float refreshRate = -1.0f;

        private Camera[] eyeCamera;
        private int[] eyeCameraOriginCullingMask = new int[3];
        private CameraClearFlags[] eyeCameraOriginClearFlag = new CameraClearFlags[3];
        private Color[] eyeCameraOriginBackgroundColor = new Color[3];

        [HideInInspector]
        public bool showFps;
        [HideInInspector]
        public bool useDefaultFps = true;
        [HideInInspector]
        public int customFps;
        [HideInInspector]
        public bool screenFade;
        [HideInInspector]
        public bool eyeTracking;
        [HideInInspector]
        public FaceTrackingMode trackingMode = FaceTrackingMode.None;
        [HideInInspector]
        public bool faceTracking;
        [HideInInspector]
        public bool lipsyncTracking;
        [HideInInspector]
        public FoveationLevel foveationLevel = FoveationLevel.None;

        //MRC
        #region MRCData
        [HideInInspector]
        public bool openMRC = true;
        [HideInInspector]
        public LayerMask foregroundLayerMask = -1;
        [HideInInspector]
        public LayerMask backLayerMask = -1;
        private static bool MRCInitSucceed = false;

        private Texture[] swapChain = new Texture[2];
        private struct LayerTexture
        {
            public Texture[] swapChain;
        };
        private LayerTexture[] layerTexturesInfo;
        private bool createMRCOverlaySucceed = false;
        private int imageIndex;
        private UInt32 imageCounts = 0;

        private CameraData xmlCameraData;
        private bool mrcPlay = false;
        private float[] cameraAttribute;
        private PxrLayerParam layerParam = new PxrLayerParam();
        private float yFov = 53f;
        [HideInInspector]
        public GameObject backCameraObj = null;
        [HideInInspector]
        public GameObject foregroundCameraObj = null;
        [HideInInspector]
        public RenderTexture mrcRenderTexture = null;
        [HideInInspector]
        public RenderTexture foregroundMrcRenderTexture = null;
        private Color foregroundColor = new Color(0, 1, 0, 1);
        private float height;


        #endregion

        private bool isNeedResume = false;

        //Entitlement Check Result
        [HideInInspector]
        public int appCheckResult = 100;
        public delegate void EntitlementCheckResult(int ReturnValue);
        public static event EntitlementCheckResult EntitlementCheckResultEvent;

        public Action<float> DisplayRefreshRateChanged;

        [HideInInspector]
        public bool useRecommendedAntiAliasingLevel = true;

        private List<PxrEventDataBuffer> eventList = new List<PxrEventDataBuffer>();

        //MR Event
        public static event Action<PxrEventSpatialAnchorSaveResult> SpatialAnchorSaveResult;
        public static event Action<PxrEventSpatialAnchorDeleteResult> SpatialAnchorDeleteResult;
        public static event Action<PxrEventSpatialAnchorLoadResults> SpatialAnchorLoadResults;
        public static event Action<PxrEventSpatialAnchorLoadResultsAvailable> SpatialAnchorLoadResultsAvailable;
        public static event Action<PxrEventSpatialAnchorLoadResultsComplete> SpatialAnchorLoadResultsComplete;
        public static event Action<PxrEventDataSpatialModelSaveResult> SpatialModelSaveResult;
        public static event Action<PxrEventDataSpatialModelDeleteResult> SpatialModelDeleteResult;
        public static event Action<PxrEventDataSpatialModelLoadResults> SpatialModelLoadResults;
        public static event Action<PxrEventDataSpatialInstancePersistenceExportComplete> SpatialInstancePersistenceExportComplete;
        public static event Action<PxrEventDataSpatialInstancePersistenceImportComplete> SpatialInstancePersistenceImportComplete;
        public static event Action<PxrEventNewSpaceReady> NewSpaceReady;
        public static event Action<PxrEventRoomSceneLoadResultsComplete> RoomSceneLoadResultsComplete;
        public static event Action<PxrEventRoomSceneDataSaveResult> RoomSceneDataSaveResult;
        public static event Action<PxrEventRoomSceneDataDeleteResult> RoomSceneDataDeleteResult;


        void Awake()
        {
            //version log
            Debug.Log("PXRLog XR Platform----SDK Version:" + PXR_Plugin.System.UPxr_GetSDKVersion());

            //log level
            int logLevel = PXR_Plugin.System.UPxr_GetConfigInt(ConfigType.UnityLogLevel);
            Debug.Log("PXRLog XR Platform----SDK logLevel:" + logLevel);
            PLog.logLevel = (PLog.LogLevel)logLevel;
            if (!bindVerifyServiceSuccess)
            {
                PXR_Plugin.PlatformSetting.UPxr_BindVerifyService(gameObject.name);
            }
            eyeCamera = new Camera[3];
            Camera[] cam = gameObject.GetComponentsInChildren<Camera>();
            for (int i = 0; i < cam.Length; i++) {
                if (cam[i].stereoTargetEye == StereoTargetEyeMask.Both) {
                    eyeCamera[0] = cam[i];
                }else if (cam[i].stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    eyeCamera[1] = cam[i];
                }
                else if(cam[i].stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    eyeCamera[2] = cam[i];
                }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            SetFrameRate();
#endif
            PXR_Plugin.Render.UPxr_SetFoveationLevel(foveationLevel);
            PXR_Plugin.System.UPxr_EnableEyeTracking(eyeTracking);
            PXR_Plugin.System.UPxr_EnableFaceTracking(faceTracking);
            PXR_Plugin.System.UPxr_EnableLipSync(lipsyncTracking);

            int recommendedAntiAliasingLevel = 0;
            recommendedAntiAliasingLevel = PXR_Plugin.System.UPxr_GetConfigInt(ConfigType.AntiAliasingLevelRecommended);
            if (useRecommendedAntiAliasingLevel && QualitySettings.antiAliasing != recommendedAntiAliasingLevel)
            {
                QualitySettings.antiAliasing = recommendedAntiAliasingLevel;
            }

            if (openMRC && MRCInitSucceed == false)
            {
                UPxr_MRCPoseInitialize();
            }
        }

        void OnEnable()
        {
            if (PXR_OverLay.Instances.Count > 0)
            {
                if (Camera.main.gameObject.GetComponent<PXR_OverlayManager>() == null)
                {
                    Camera.main.gameObject.AddComponent<PXR_OverlayManager>();
                }

                foreach (var layer in PXR_OverLay.Instances)
                {
                    if (eyeCamera[0] != null && eyeCamera[0].enabled) {
                        layer.RefreshCamera(eyeCamera[0],eyeCamera[0]);
                    }
                    else if (eyeCamera[1] != null && eyeCamera[1].enabled)
                    {
                        layer.RefreshCamera(eyeCamera[1], eyeCamera[2]);
                    }
                }
            }

            if (openMRC) {
                UPxr_MRCDataBinding();
            }

        }

        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                if (isNeedResume)
                {
                    StartCoroutine("StartXR");
                    isNeedResume = false;
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (openMRC && MRCInitSucceed)
            {
                PXR_Plugin.Render.UPxr_DestroyLayer(99999);
            }
        }

        public IEnumerator StartXR()
        {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("PXRLog Initializing XR Failed. Check log for details.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }

        void StopXR()
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        void Start()
        {
            bool systemFps = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            PXR_Plugin.System.UPxr_GetTextSize("");//load res & get permission of external storage
            systemFps = Convert.ToBoolean(Convert.ToInt16(PXR_Plugin.System.UPxr_GetConfigInt(ConfigType.ShowFps)));
#endif
            if (systemFps || showFps)
            {
                Camera.main.transform.Find("FPS").gameObject.SetActive(true);
            }

            if (PXR_PlatformSetting.Instance.startTimeEntitlementCheck)
            {
                if (PXR_Plugin.PlatformSetting.UPxr_IsCurrentDeviceValid() != PXR_PlatformSetting.simulationType.Valid)
                {
                    Debug.Log("PXRLog Entitlement Check Simulation DO NOT PASS");
                    string appID = PXR_PlatformSetting.Instance.appID;
                    Debug.Log("PXRLog Entitlement Check Enable");
                    // 0:success -1:invalid params -2:service not exist -3:time out
                    PXR_Plugin.PlatformSetting.UPxr_AppEntitlementCheckExtra(appID);
                }
                else
                {
                    Debug.Log("PXRLog Entitlement Check Simulation PASS");
                }
            }
#if UNITY_EDITOR
            Application.targetFrameRate = 72;
#endif
            PXR_Plugin.Controller.UPxr_SetControllerDelay();
        }
        
        void Update()
        {
            currentBoundaryState = PXR_Plugin.Boundary.UPxr_GetSeeThroughState();
            // boundary
            for (int i = 0; i < 3; i++)
            {
                if (eyeCamera[i] != null && eyeCamera[i].enabled)
                {
                    if (currentBoundaryState != lastBoundaryState)
                    {
                        if (currentBoundaryState == 2) // close camera render
                        {
                            // record
                            eyeCameraOriginCullingMask[i] = eyeCamera[i].cullingMask;
                            eyeCameraOriginClearFlag[i] = eyeCamera[i].clearFlags;
                            eyeCameraOriginBackgroundColor[i] = eyeCamera[i].backgroundColor;

                            // close render
                            eyeCamera[i].cullingMask = 0;
                            eyeCamera[i].clearFlags = CameraClearFlags.SolidColor;
                            eyeCamera[i].backgroundColor = Color.black;
                        }
                        else if (currentBoundaryState == 1) // open camera render
                        {
                            if (lastBoundaryState == 2)
                            {
                                if (eyeCamera[i].cullingMask == 0)
                                {
                                    eyeCamera[i].cullingMask = eyeCameraOriginCullingMask[i];
                                }
                                if (eyeCamera[i].clearFlags == CameraClearFlags.SolidColor)
                                {
                                    eyeCamera[i].clearFlags = eyeCameraOriginClearFlag[i];
                                }
                                if (eyeCamera[i].backgroundColor == Color.black)
                                {
                                    eyeCamera[i].backgroundColor = eyeCameraOriginBackgroundColor[i];
                                }
                            }
                        }
                        else // open camera render(recover)
                        {
                            if ((lastBoundaryState == 2 || lastBoundaryState == 1))
                            {
                                if (eyeCamera[i].cullingMask == 0)
                                {
                                    eyeCamera[i].cullingMask = eyeCameraOriginCullingMask[i];
                                }
                                if (eyeCamera[i].clearFlags == CameraClearFlags.SolidColor)
                                {
                                    eyeCamera[i].clearFlags = eyeCameraOriginClearFlag[i];
                                }
                                if (eyeCamera[i].backgroundColor == Color.black)
                                {
                                    eyeCamera[i].backgroundColor = eyeCameraOriginBackgroundColor[i];
                                }
                            }
                        }
                    }
                }
            }
            lastBoundaryState = currentBoundaryState;
            if (Math.Abs(refreshRate - PXR_Plugin.System.UPxr_RefreshRateChanged()) > 0.1f)
            {
                refreshRate = PXR_Plugin.System.UPxr_RefreshRateChanged();
                if (DisplayRefreshRateChanged != null)
                    DisplayRefreshRateChanged(refreshRate);
            }
            //recenter callback
            if (PXR_Plugin.System.UPxr_GetHomeKey())
            {
                if (PXR_Plugin.System.RecenterSuccess != null)
                {
                    PXR_Plugin.System.RecenterSuccess();
                }
                PXR_Plugin.System.UPxr_InitHomeKey();
            }

            //MRC
            if (openMRC && MRCInitSucceed) {
                UPxr_GetLayerImage();
                if (createMRCOverlaySucceed)
                {
                    MRCUpdata();
                }
            }
            //pollEvent
            PollEventOfMixedReality();
        }

        private void PollEventOfMixedReality()
        {
            eventList.Clear();
            bool ret = PXR_Plugin.MixedReality.UPxr_PollEventQueue(ref eventList);
            if (ret)
            {
                for (int i = 0; i < eventList.Count; i++)
                {
                    Debug.Log("PXRLog PollEventOfMixedReality" + eventList[i].type);
                    switch (eventList[i].type)
                    {
                        case PxrStructureType.SpatialAnchorSaveResult:
                            {
                                if (SpatialAnchorSaveResult != null)
                                {
                                    PxrEventSpatialAnchorSaveResult result = new PxrEventSpatialAnchorSaveResult()
                                    {
                                        type = PxrStructureType.SpatialAnchorSaveResult,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                        uuid = new PxrSpatialInstanceUuid()
                                        {
                                            value0 = BitConverter.ToUInt64(eventList[i].data, 16),
                                            value1 = BitConverter.ToUInt64(eventList[i].data, 24),
                                        },
                                        handle = BitConverter.ToUInt64(eventList[i].data, 32),
                                        location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 40)
                                    };
                                    SpatialAnchorSaveResult(result);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialAnchorDeleteResult:
                            {
                                if (SpatialAnchorDeleteResult != null)
                                {
                                    PxrEventSpatialAnchorDeleteResult result = new PxrEventSpatialAnchorDeleteResult()
                                    {
                                        type = PxrStructureType.SpatialAnchorDeleteResult,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                        uuid = new PxrSpatialInstanceUuid()
                                        {
                                            value0 = BitConverter.ToUInt64(eventList[i].data, 16),
                                            value1 = BitConverter.ToUInt64(eventList[i].data, 24),
                                        },
                                        location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 32)
                                    };
                                    SpatialAnchorDeleteResult(result);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialAnchorLoadResults:
                            {
                                if (SpatialAnchorLoadResults != null)
                                {
                                    PxrEventSpatialAnchorLoadResults results = new PxrEventSpatialAnchorLoadResults()
                                    {
                                        type = PxrStructureType.SpatialAnchorLoadResults,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        hasNext = BitConverter.ToBoolean(eventList[i].data, 4),
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                        numResults = BitConverter.ToUInt32(eventList[i].data, 16),
                                        loadResults = new PxrSpatialAnchorLoadResult[BitConverter.ToUInt32(eventList[i].data, 16)],
                                    };
                                    int offset = 24;
                                    for (int j = 0; j < results.numResults; j++)
                                    {
                                        results.loadResults[j] = new PxrSpatialAnchorLoadResult();
                                        results.loadResults[j].anchorHandle = BitConverter.ToUInt64(eventList[i].data, offset);
                                        offset += 8;
                                        results.loadResults[j].uuid = new PxrSpatialInstanceUuid();
                                        results.loadResults[j].uuid.value0 = BitConverter.ToUInt64(eventList[i].data, offset);
                                        offset += 8;
                                        results.loadResults[j].uuid.value1 = BitConverter.ToUInt64(eventList[i].data, offset);
                                        offset += 8;
                                    }
                                    SpatialAnchorLoadResults(results);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialModelSaveResult:
                            {
                                if (SpatialModelSaveResult != null)
                                {
                                    PxrEventDataSpatialModelSaveResult result = new PxrEventDataSpatialModelSaveResult()
                                    {
                                        type = PxrStructureType.SpatialModelSaveResult,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                        uuid = new PxrSpatialInstanceUuid()
                                        {
                                            value0 = BitConverter.ToUInt64(eventList[i].data, 16),
                                            value1 = BitConverter.ToUInt64(eventList[i].data, 24),
                                        },
                                        handle = BitConverter.ToUInt64(eventList[i].data, 32),
                                        location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 40),
                                    };
                                    SpatialModelSaveResult(result);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialModelDeleteResult:
                            {
                                if (SpatialModelDeleteResult != null)
                                {
                                    PxrEventDataSpatialModelDeleteResult result = new PxrEventDataSpatialModelDeleteResult()
                                    {
                                        type = PxrStructureType.SpatialModelDeleteResult,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                        uuid = new PxrSpatialInstanceUuid()
                                        {
                                            value0 = BitConverter.ToUInt64(eventList[i].data, 16),
                                            value1 = BitConverter.ToUInt64(eventList[i].data, 24),
                                        },
                                        location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 32)
                                    };
                                    SpatialModelDeleteResult(result);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialInstancePersistenceExportComplete:
                            {
                                if (SpatialInstancePersistenceExportComplete != null)
                                {
                                    PxrEventDataSpatialInstancePersistenceExportComplete data = new PxrEventDataSpatialInstancePersistenceExportComplete()
                                    {
                                        type = PxrStructureType.SpatialInstancePersistenceExportComplete,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        request = BitConverter.ToUInt64(eventList[i].data, 8),
                                    };
                                    SpatialInstancePersistenceExportComplete(data);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialInstancePersistenceImportComplete:
                            {
                                if (SpatialInstancePersistenceImportComplete != null)
                                {
                                    PxrEventDataSpatialInstancePersistenceImportComplete data = new PxrEventDataSpatialInstancePersistenceImportComplete()
                                    {
                                        type = PxrStructureType.SpatialInstancePersistenceImportComplete,
                                        eventLevel = eventList[i].eventLevel,
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                        request = BitConverter.ToUInt64(eventList[i].data, 8),
                                    };
                                    SpatialInstancePersistenceImportComplete(data);
                                }
                            }
                            break;
                        case PxrStructureType.NewSpaceReady:
                            {
                                if (NewSpaceReady != null)
                                {
                                    PxrEventNewSpaceReady data = new PxrEventNewSpaceReady()
                                    {
                                        type = PxrStructureType.NewSpaceReady,
                                        eventLevel = eventList[i].eventLevel,
                                        state = (PxrNewSpaceType)BitConverter.ToInt32(eventList[i].data, 0)
                                    };
                                    NewSpaceReady(data);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialAnchorLoadResultsAvailable:
                            {
                                if (SpatialAnchorLoadResultsAvailable != null)
                                {
                                    PxrEventSpatialAnchorLoadResultsAvailable data = new PxrEventSpatialAnchorLoadResultsAvailable()
                                    {
                                        type = PxrStructureType.SpatialAnchorLoadResultsAvailable,
                                        eventLevel = eventList[i].eventLevel,
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 0),
                                    };
                                    SpatialAnchorLoadResultsAvailable(data);
                                }
                            }
                            break;
                        case PxrStructureType.SpatialAnchorLoadResultsComplete:
                            {
                                if (SpatialAnchorLoadResultsComplete != null)
                                {
                                    PxrEventSpatialAnchorLoadResultsComplete data = new PxrEventSpatialAnchorLoadResultsComplete()
                                    {
                                        type = PxrStructureType.SpatialAnchorLoadResultsComplete,
                                        eventLevel = eventList[i].eventLevel,
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 0),
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 8),
                                    };
                                    SpatialAnchorLoadResultsComplete(data);
                                }
                            }
                            break;
                        case PxrStructureType.RoomSceneDataSaveResult:
                        {
                            if (RoomSceneDataSaveResult != null)
                            {
                                PxrEventRoomSceneDataSaveResult data = new PxrEventRoomSceneDataSaveResult()
                                {
                                    type = PxrStructureType.RoomSceneDataSaveResult,
                                    eventLevel = eventList[i].eventLevel,
                                    result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                    asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                    handle = BitConverter.ToUInt64(eventList[i].data, 16),
                                    location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 24),
                                };
                                RoomSceneDataSaveResult(data);
                            }
                        }
                            break;
                        case PxrStructureType.RoomSceneDataDeleteResult:
                        {
                            if (RoomSceneDataDeleteResult != null)
                            {
                                PxrEventRoomSceneDataDeleteResult data = new PxrEventRoomSceneDataDeleteResult()
                                {
                                    type = PxrStructureType.RoomSceneDataDeleteResult,
                                    eventLevel = eventList[i].eventLevel,
                                    result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 0),
                                    asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 8),
                                    handle = BitConverter.ToUInt64(eventList[i].data, 16),
                                    location = (PxrSpatialPersistenceLocation)BitConverter.ToInt32(eventList[i].data, 24),
                                };
                                RoomSceneDataDeleteResult(data);
                            }
                        }
                            break;
                        case PxrStructureType.RoomSceneLoadResultsComplete:
                            {
                                if (RoomSceneLoadResultsComplete != null)
                                {
                                    PxrEventRoomSceneLoadResultsComplete data = new PxrEventRoomSceneLoadResultsComplete()
                                    {
                                        type = PxrStructureType.RoomSceneLoadResultsComplete,
                                        eventLevel = eventList[i].eventLevel,
                                        asyncRequestId = BitConverter.ToUInt64(eventList[i].data, 0),
                                        result = (PxrSpatialPersistenceResult)BitConverter.ToInt32(eventList[i].data, 8),
                                    };
                                    RoomSceneLoadResultsComplete(data);
                                }
                            }
                            break;
                    }
                }
            }
        }
        void OnDisable()
        {
            StopAllCoroutines();
            if (openMRC) {
                UPxr_UnMRCDataBinding();
            }

        }

        private void SetFrameRate()
        {
            int targetFrameRate = (int)PXR_Plugin.System.UPxr_GetConfigInt(ConfigType.TargetFrameRate);
            int displayRefreshRate = (int)PXR_Plugin.System.UPxr_GetConfigFloat(ConfigType.DisplayRefreshRate);
            Application.targetFrameRate = targetFrameRate > 0 ? targetFrameRate : displayRefreshRate;
            if (!useDefaultFps)
            {
                if (customFps <= displayRefreshRate)
                {
                    Application.targetFrameRate = customFps;
                }
                else
                {
                    Application.targetFrameRate = displayRefreshRate;
                }
            }
            PLog.i(TAG, string.Format("Customize FPS : {0}", Application.targetFrameRate));
        }
        
        //bind verify service success call back
        void BindVerifyServiceCallback()
        {
            bindVerifyServiceSuccess = true;
        }

        private void verifyAPPCallback(string callback)
        {
            Debug.Log("PXRLog verifyAPPCallback callback = " + callback);
            appCheckResult = Convert.ToInt32(callback);
            if (EntitlementCheckResultEvent != null)
            {
                EntitlementCheckResultEvent(appCheckResult);
            }
        }

        public Camera[] GetEyeCamera()
        {
            return eyeCamera;
        }


        #region MRC FUNC
        public void UPxr_MRCPoseInitialize()
        {
            if (layerTexturesInfo == null)
            {
                layerTexturesInfo = new LayerTexture[2];
            }
            xmlCameraData = new CameraData();
            xmlCameraData.translation = new float[3];
            xmlCameraData.rotation = new float[4];
            UPxr_ReadXML(out xmlCameraData);
            Invoke("Pxr_GetHeight", 0.5f);
            PXR_Plugin.System.UPxr_SetIsSupportMovingMrc(true);
            PxrPosef pose = new PxrPosef();
            pose.orientation.x = xmlCameraData.rotation[0];
            pose.orientation.y = xmlCameraData.rotation[1];
            pose.orientation.z = xmlCameraData.rotation[2];
            pose.orientation.w = xmlCameraData.rotation[3];
            pose.position.x = xmlCameraData.translation[0];
            pose.position.y = xmlCameraData.translation[1];
            pose.position.z = xmlCameraData.translation[2];
            PXR_Plugin.System.UPxr_SetMrcPose(ref pose);

            PXR_Plugin.System.UPxr_GetMrcPose(ref pose);
            xmlCameraData.rotation[0] = pose.orientation.x;
            xmlCameraData.rotation[1] = pose.orientation.y;
            xmlCameraData.rotation[2] = pose.orientation.z;
            xmlCameraData.rotation[3] = pose.orientation.w;
            xmlCameraData.translation[0] = pose.position.x;
            xmlCameraData.translation[1] = pose.position.y;
            xmlCameraData.translation[2] = pose.position.z;
            mrcPlay = false;
            UInt64 textureWidth = (UInt64)xmlCameraData.imageWidth;
            UInt64 textureHeight = (UInt64)xmlCameraData.imageHeight;
            PXR_Plugin.System.UPxr_SetMrcTextutrWidth(textureWidth);
            PXR_Plugin.System.UPxr_SetMrcTextutrHeight(textureHeight);
            UPxr_CreateMRCOverlay((uint)xmlCameraData.imageWidth, (uint)xmlCameraData.imageHeight);
            MRCInitSucceed = true;
            Debug.Log("PXR_MRCInit Succeed");
        }

        public void UPxr_CreateMRCOverlay(uint width, uint height)
        {
            if (width <= 0 || height <= 0) {
                Debug.Log("PXR MRC Abnormal calibration data");
                return;
            }
            layerParam.layerId = 99999;
            layerParam.layerShape = PXR_OverLay.OverlayShape.Quad;
            layerParam.layerType = PXR_OverLay.OverlayType.Overlay;
            layerParam.layerLayout = PXR_OverLay.LayerLayout.Stereo;
            layerParam.format = (UInt64)RenderTextureFormat.Default;
            layerParam.width = width;
            layerParam.height = height;
            layerParam.sampleCount = 1;
            layerParam.faceCount = 1;
            layerParam.arraySize = 1;
            layerParam.mipmapCount = 1;
            layerParam.layerFlags = 0;
            PXR_Plugin.Render.UPxr_CreateLayerParam(layerParam);
        }

        public void UPxr_MRCDataBinding() {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering += BeginRendering;
            }
            else
            {
                Camera.onPostRender += UPxr_CopyMRCTexture;
            }
            PXR_Plugin.System.RecenterSuccess += UPxr_Calibration;
        }

        public void UPxr_UnMRCDataBinding()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering -= BeginRendering;
            }
            else
            {
                Camera.onPostRender -= UPxr_CopyMRCTexture;
            }
            PXR_Plugin.System.RecenterSuccess -= UPxr_Calibration;
        }

        private void BeginRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            UPxr_CopyMRCTexture(arg2[0]);
        }

        public void UPxr_CopyMRCTexture(Camera cam)
        {
            if (cam == null || cam.tag != Camera.main.tag || cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return;
            if (createMRCOverlaySucceed && PXR_Plugin.System.UPxr_GetMRCEnable())
            {
                PXR_Plugin.Render.UPxr_GetLayerNextImageIndex(99999, ref imageIndex);

                for (int eyeId = 0; eyeId < 2; ++eyeId)
                {
                    Texture dstT = layerTexturesInfo[eyeId].swapChain[imageIndex];

                    if (dstT == null)
                        continue;
                    RenderTexture rt;
                    if (eyeId == 0)
                    {
                        rt = mrcRenderTexture as RenderTexture;
                    }
                    else
                    {
                        rt = foregroundMrcRenderTexture as RenderTexture;
                    }
                    RenderTexture tempRT = null;

                    if (!(QualitySettings.activeColorSpace == ColorSpace.Gamma && rt != null && rt.format == RenderTextureFormat.ARGB32))
                    {
                        RenderTextureDescriptor descriptor = new RenderTextureDescriptor((int)layerParam.width, (int)layerParam.height, RenderTextureFormat.ARGB32, 0);
                        descriptor.msaaSamples = (int)layerParam.sampleCount;
                        descriptor.useMipMap = true;
                        descriptor.autoGenerateMips = false;
                        descriptor.sRGB = false;
                        tempRT = RenderTexture.GetTemporary(descriptor);

                        if (!tempRT.IsCreated())
                        {
                            tempRT.Create();
                        }
                        if (eyeId == 0)
                        {
                            mrcRenderTexture.DiscardContents();
                        }
                        else
                        {
                            foregroundMrcRenderTexture.DiscardContents();
                        }
                        tempRT.DiscardContents();

                        if (eyeId == 0)
                        {
                            Graphics.Blit(mrcRenderTexture, tempRT);
                            Graphics.CopyTexture(tempRT, 0, 0, dstT, 0, 0);
                        }
                        else
                        {
                            Graphics.Blit(foregroundMrcRenderTexture, tempRT);
                            Graphics.CopyTexture(tempRT, 0, 0, dstT, 0, 0);
                        }
                    }
                    else
                    {
                        if (eyeId == 0)
                        {
                            Graphics.CopyTexture(mrcRenderTexture, 0, 0, dstT, 0, 0);
                        }
                        else
                        {
                            Graphics.CopyTexture(foregroundMrcRenderTexture, 0, 0, dstT, 0, 0);
                        }
                    }

                    if (tempRT != null)
                    {
                        RenderTexture.ReleaseTemporary(tempRT);
                    }
                }
                PxrLayerQuad layerSubmit = new PxrLayerQuad();
                layerSubmit.header.layerId = 99999;
                layerSubmit.header.layerFlags = (UInt32)PxrLayerSubmitFlagsEXT.PxrLayerFlagMRCComposition;
                layerSubmit.width = 1.0f;
                layerSubmit.height = 1.0f;
                layerSubmit.header.colorScaleX = 1.0f;
                layerSubmit.header.colorScaleY = 1.0f;
                layerSubmit.header.colorScaleZ = 1.0f;
                layerSubmit.header.colorScaleW = 1.0f;
                layerSubmit.pose.orientation.w = 1.0f;
                layerSubmit.header.headPose.orientation.x = 0;
                layerSubmit.header.headPose.orientation.y = 0;
                layerSubmit.header.headPose.orientation.z = 0;
                layerSubmit.header.headPose.orientation.w = 1;
                PXR_Plugin.Render.UPxr_SubmitLayerQuad(layerSubmit);
            }
        }

        

        public void UPxr_ReadXML(out CameraData cameradata)
        {
            CameraData cameraDataNew = new CameraData();
            string path = Application.persistentDataPath + "/mrc.xml";
            cameraAttribute = PXR_Plugin.PlatformSetting.UPxr_MRCCalibration(path);
            Debug.Log("cameraDataLength: " + cameraAttribute.Length);
            for (int i = 0; i < cameraAttribute.Length; i++)
            {
                Debug.Log("cameraData: " + i.ToString() + ": " + cameraAttribute[i].ToString());
            }
            cameraDataNew.imageWidth = cameraAttribute[0];
            cameraDataNew.imageHeight = cameraAttribute[1];
            yFov = cameraAttribute[2];
            cameraDataNew.translation = new float[3];
            cameraDataNew.rotation = new float[4];
            for (int i = 0; i < 3; i++)
            {
                cameraDataNew.translation[i] = cameraAttribute[3 + i];
            }
            for (int i = 0; i < 4; i++)
            {
                cameraDataNew.rotation[i] = cameraAttribute[6 + i];
            }
            cameradata = cameraDataNew;
        }

        public void UPxr_CreateCamera(CameraData cameradata)
        {
            if (backCameraObj == null)
            {
                backCameraObj = new GameObject("myBackCamera");
                backCameraObj.tag = "mrc";
                backCameraObj.transform.parent = Camera.main.transform.parent;
                Camera camera = backCameraObj.AddComponent<Camera>();
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.transform.localScale = Vector3.one;
                camera.depth = 9999;
                camera.gameObject.layer = 0;
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.orthographic = false;
                camera.fieldOfView = yFov;
                camera.aspect = cameradata.imageWidth / cameradata.imageHeight;
                camera.transform.localPosition = UPxr_ToVector3(cameradata.translation);
                camera.transform.localEulerAngles = UPxr_ToRotation(cameradata.rotation);
                camera.allowMSAA = true;
                camera.cullingMask = backLayerMask;
                if (mrcRenderTexture == null)
                {
                    mrcRenderTexture = new RenderTexture((int)cameradata.imageWidth, (int)cameradata.imageHeight, 24, RenderTextureFormat.ARGB32);
                }
                mrcRenderTexture.name = "mrcRenderTexture";
                camera.targetTexture = mrcRenderTexture;
            }
            else
            {
                backCameraObj.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
                backCameraObj.GetComponent<Camera>().transform.localPosition = UPxr_ToVector3(cameradata.translation);
                backCameraObj.GetComponent<Camera>().transform.localEulerAngles = UPxr_ToRotation(cameradata.rotation);
                backCameraObj.GetComponent<Camera>().transform.localScale = Vector3.one;
                backCameraObj.GetComponent<Camera>().depth = 9999;
                backCameraObj.GetComponent<Camera>().gameObject.layer = 0;
                backCameraObj.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
                backCameraObj.GetComponent<Camera>().orthographic = false;
                backCameraObj.GetComponent<Camera>().fieldOfView = yFov;
                backCameraObj.GetComponent<Camera>().aspect = cameradata.imageWidth / cameradata.imageHeight;
                backCameraObj.GetComponent<Camera>().allowMSAA = true;
                backCameraObj.GetComponent<Camera>().cullingMask = backLayerMask;
                if (mrcRenderTexture == null)
                {
                    mrcRenderTexture = new RenderTexture((int)cameradata.imageWidth, (int)cameradata.imageHeight, 24, RenderTextureFormat.ARGB32);
                }
                backCameraObj.GetComponent<Camera>().targetTexture = mrcRenderTexture;
                backCameraObj.SetActive(true);
            }
            if (foregroundCameraObj == null)
            {
                foregroundCameraObj = new GameObject("myForegroundCamera");
                foregroundCameraObj.transform.parent = Camera.main.transform.parent;
                Camera camera = foregroundCameraObj.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = foregroundColor;
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.transform.localScale = Vector3.one;
                camera.depth = 10000;
                camera.gameObject.layer = 0;
                camera.orthographic = false;
                camera.fieldOfView = yFov;
                camera.aspect = cameradata.imageWidth / cameradata.imageHeight;
                camera.transform.localPosition = UPxr_ToVector3(cameradata.translation);
                camera.transform.localEulerAngles = UPxr_ToRotation(cameradata.rotation);
                camera.allowMSAA = true;
                camera.cullingMask = foregroundLayerMask;
                if (foregroundMrcRenderTexture == null)
                {
                    foregroundMrcRenderTexture = new RenderTexture((int)cameradata.imageWidth, (int)cameradata.imageHeight, 24, RenderTextureFormat.ARGB32);
                }
                foregroundMrcRenderTexture.name = "foregroundMrcRenderTexture";
                camera.targetTexture = foregroundMrcRenderTexture;
            }
            else
            {
                foregroundCameraObj.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                foregroundCameraObj.GetComponent<Camera>().backgroundColor = foregroundColor;
                foregroundCameraObj.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
                foregroundCameraObj.GetComponent<Camera>().transform.localPosition = UPxr_ToVector3(cameradata.translation);
                foregroundCameraObj.GetComponent<Camera>().transform.localEulerAngles = UPxr_ToRotation(cameradata.rotation);
                foregroundCameraObj.GetComponent<Camera>().transform.localScale = Vector3.one;
                foregroundCameraObj.GetComponent<Camera>().depth = 10000;
                foregroundCameraObj.GetComponent<Camera>().gameObject.layer = 0;
                foregroundCameraObj.GetComponent<Camera>().orthographic = false;
                foregroundCameraObj.GetComponent<Camera>().fieldOfView = yFov;
                foregroundCameraObj.GetComponent<Camera>().aspect = cameradata.imageWidth / cameradata.imageHeight;
                foregroundCameraObj.GetComponent<Camera>().allowMSAA = true;
                foregroundCameraObj.GetComponent<Camera>().cullingMask = foregroundLayerMask;
                if (foregroundMrcRenderTexture == null)
                {
                    foregroundMrcRenderTexture = new RenderTexture((int)cameradata.imageWidth, (int)cameradata.imageHeight, 24, RenderTextureFormat.ARGB32);
                }
                foregroundCameraObj.GetComponent<Camera>().targetTexture = foregroundMrcRenderTexture;
                foregroundCameraObj.SetActive(true);
            }
            mrcPlay = true;

            Debug.Log("PxrMRC Camera create");
        }

        public Vector3 UPxr_ToVector3(float[] translation)
        {
            Debug.Log("translation:" + new Vector3(translation[0], translation[1], -translation[2]).ToString());
            return new Vector3(translation[0], translation[1] + height, (-translation[2]) * 1f);
        }

        public Vector3 UPxr_ToRotation(float[] rotation)
        {
            Quaternion quaternion = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            Vector3 vector3 = quaternion.eulerAngles;
            Debug.Log("rotation:" + vector3.ToString());
            return new Vector3(-vector3.x, -vector3.y, -vector3.z);
        }

        public void Pxr_GetHeight()
        {
            height = Camera.main.transform.localPosition.y - PXR_Plugin.System.UPxr_GetMrcY();
            Debug.Log("Pxr_GetMrcY+:" + PXR_Plugin.System.UPxr_GetMrcY().ToString());
        }

        private void MRCUpdata() {
            if (PXR_Plugin.System.UPxr_GetMRCEnable())
            {
                if (backCameraObj == null)
                {
                    if (Camera.main.transform != null)
                    {
                        UPxr_CreateCamera(xmlCameraData);
                        UPxr_Calibration();
                    }
                }                     
                else
                {
                    if (!mrcPlay)
                    {
                        if (Camera.main.transform != null)
                        {
                            UPxr_CreateCamera(xmlCameraData);
                            UPxr_Calibration();
                        }
                    }
                }
                if (foregroundCameraObj != null)
                {
                    Vector3 cameraLookAt = Camera.main.transform.position - foregroundCameraObj.transform.position;
                    float distance = Vector3.Dot(cameraLookAt, foregroundCameraObj.transform.forward);
                    foregroundCameraObj.GetComponent<Camera>().farClipPlane = Mathf.Max(foregroundCameraObj.GetComponent<Camera>().nearClipPlane + 0.001f, distance);
                }
                if (backCameraObj != null && foregroundCameraObj != null)
                {
                    UPxr_Calibration();
                }
            }
            else
            {
                if (mrcPlay == true)
                {
                    mrcPlay = false;
                    backCameraObj.SetActive(false);
                    foregroundCameraObj.SetActive(false);
                }
            }
        }

        public void UPxr_GetLayerImage()
        {
            if (createMRCOverlaySucceed == false)
            {
                if (PXR_Plugin.Render.UPxr_GetLayerImageCount(99999, EyeType.EyeLeft, ref imageCounts) == 0 && imageCounts > 0)
                {
                    if (layerTexturesInfo[0].swapChain == null)
                    {
                        layerTexturesInfo[0].swapChain = new Texture[imageCounts];
                    }
                    for (int j = 0; j < imageCounts; j++)
                    {
                        IntPtr ptr = IntPtr.Zero;
                        PXR_Plugin.Render.UPxr_GetLayerImagePtr(99999, EyeType.EyeLeft, j, ref ptr);
                        if (ptr == IntPtr.Zero)
                        {
                            continue;
                        }
                        Texture sc = Texture2D.CreateExternalTexture((int)layerParam.width, (int)layerParam.height, TextureFormat.RGBA32, false, true, ptr);

                        if (sc == null)
                        {
                            continue;
                        }

                        layerTexturesInfo[0].swapChain[j] = sc;
                    }

                }
                if (PXR_Plugin.Render.UPxr_GetLayerImageCount(99999, EyeType.EyeRight, ref imageCounts) == 0 && imageCounts > 0)
                {
                    if (layerTexturesInfo[1].swapChain == null)
                    {
                        layerTexturesInfo[1].swapChain = new Texture[imageCounts];
                    }

                    for (int j = 0; j < imageCounts; j++)
                    {
                        IntPtr ptr = IntPtr.Zero;
                        PXR_Plugin.Render.UPxr_GetLayerImagePtr(99999, EyeType.EyeRight, j, ref ptr);
                        if (ptr == IntPtr.Zero)
                        {
                            continue;
                        }

                        Texture sc = Texture2D.CreateExternalTexture((int)layerParam.width, (int)layerParam.height, TextureFormat.RGBA32, false, true, ptr);

                        if (sc == null)
                        {
                            continue;
                        }

                        layerTexturesInfo[1].swapChain[j] = sc;
                    }

                    createMRCOverlaySucceed = true;
                    Debug.Log("Pxr_GetMrcLayerImage : true");
                }
            }
        }

        public void UPxr_Calibration()
        {
            if (PXR_Plugin.System.UPxr_GetMRCEnable())
            {
                PxrPosef pose = new PxrPosef();
                pose.orientation.x = 0;
                pose.orientation.y = 0;
                pose.orientation.z = 0;
                pose.orientation.w = 0;
                pose.position.x = 0;
                pose.position.y = 0;
                pose.position.z = 0;
                PXR_Plugin.System.UPxr_GetMrcPose(ref pose);
                backCameraObj.transform.localPosition = new Vector3(pose.position.x, pose.position.y + height, (-pose.position.z) * 1f);
                foregroundCameraObj.transform.localPosition = new Vector3(pose.position.x, pose.position.y + height, (-pose.position.z) * 1f);
                Vector3 rototion = new Quaternion(pose.orientation.x, pose.orientation.y, pose.orientation.z, pose.orientation.w).eulerAngles;
                backCameraObj.transform.localEulerAngles = new Vector3(-rototion.x, -rototion.y, -rototion.z);
                foregroundCameraObj.transform.localEulerAngles = new Vector3(-rototion.x, -rototion.y, -rototion.z);
            }
        }

        #endregion
    }
    public struct CameraData
    {
        public string id;
        public string cameraName;
        public float imageWidth;
        public float imageHeight;
        public float[] translation;
        public float[] rotation;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using System;

namespace Appnori.Util
{
    public class XRPlayerManager : MonoBehaviour
    {
        public static XRPlayerManager instance;

        [SerializeField]
        public bool UseCreateXRPlayer;
        [SerializeField]
        public bool UseOptionButton;
        [SerializeField]
        public bool UseTrigger;
        [SerializeField]
        public bool UseGrip;
        [SerializeField]
        public bool FadeOnAwake = false;
        [SerializeField]
        public bool UseHaptic;
        /// <summary>
        /// 멀티플레이어 Photonview
        /// </summary>
        private PhotonView pv;
        delegate void E();
        private E e;

        /// <summary>
        /// 플레이어의 컨트롤러
        /// </summary>

        public Transform XRHead { get; set; }
        public XRController XrController_L { get; set; }
        public XRController XrController_R { get; set; }

        /// <summary>
        /// 컨트롤러 메뉴
        /// </summary>
        private Notifier<bool> Menu_L = new Notifier<bool>();
        private Notifier<bool> Menu_R = new Notifier<bool>();
        protected virtual void OnMenu_L(bool isOn) { }
        protected virtual void OnMenu_R(bool isOn) { }

        private Notifier<bool> Home_L = new Notifier<bool>();
        private Notifier<bool> Home_R = new Notifier<bool>();
        protected virtual void OnHome_L(bool isOn) { }
        protected virtual void OnHome_R(bool isOn) { }

        /// <summary>
        /// 컨트롤러 트리거
        /// </summary>
        private Notifier<bool> Trigger_L = new Notifier<bool>();
        private Notifier<bool> Trigger_R = new Notifier<bool>();
        protected virtual void OnTrigger_L(bool isOn) { }
        protected virtual void OnTrigger_R(bool isOn) { }

        /// <summary>
        /// 컨트롤러 그랩
        /// </summary>
        private Notifier<bool> Grip_L = new Notifier<bool>();
        private Notifier<bool> Grip_R = new Notifier<bool>();
        protected virtual void OnGrip_L(bool isOn) { }
        protected virtual void OnGrip_R(bool isOn) { }


        /// <summary>
        /// Fade In/Out
        /// </summary>
        private MeshRenderer renderer_fade;
        private Coroutine coroutine_fade;

        private Color color_black = new Color(0, 0, 0, 1);
        private Color color_clear = new Color(0, 0, 0, 0);


        protected virtual void Awake()
        {
            instance = this;

            if (UseCreateXRPlayer)
                CreateXRPlayer();

            e += Connect_PhotonView;
            e += Connect_XrController;

            e();
        }

        protected virtual void Update()
        {
            if (pv != null)
                if (!pv.IsMine)
                    return;

            //홈&메뉴 버튼
            if (UseOptionButton)
            {
                if (XrController_L != null)
                {
                    if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out var isMenuL))
                    {
                        Menu_L.CurrentData = isMenuL;
                    }
                    if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var isHomeL))
                    {
                        Home_L.CurrentData = isHomeL;
                    }
                }
                if (XrController_R != null)
                {
                    if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out var isMenuR))
                    {
                        Menu_R.CurrentData = isMenuR;
                    }
                    if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var isHomeR))
                    {
                        Home_R.CurrentData = isHomeR;
                    }
                }
            }

            //트리거
            if (UseTrigger)
            {
                if (XrController_L != null)
                {
                    if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out var isTriggerL))
                    {
                        Trigger_L.CurrentData = isTriggerL;
                    }
                }
                if (XrController_R != null)
                {
                    if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out var isTriggerR))
                    {
                        Trigger_R.CurrentData = isTriggerR;
                    }
                }
            }

            //그립
            if (UseGrip)
            {
                if (XrController_L != null)
                {
                    if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out var isGripL))
                    {
                        Grip_L.CurrentData = isGripL;
                    }
                }
                if (XrController_R != null)
                {
                    if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out var isGripR))
                    {
                        Grip_R.CurrentData = isGripR;
                    }
                }
            }
        }

        protected void CreateXRPlayer()
        {
            if (pv != null)
                if (!pv.IsMine)
                    return;

            gameObject.name = "Player";
            var xrOrigin = gameObject.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            var cameraOffset = transform.Find("Trackables");
            cameraOffset.name = "Camera OffSet";

            var mainCamera = new GameObject("Main Camera");
            List<GameObject> Hands = new List<GameObject>();
            Hands.Add(new GameObject("LeftHand Controller"));
            Hands.Add(new GameObject("RightHand Controller"));
            mainCamera.transform.parent = cameraOffset.transform;
            mainCamera.tag = "MainCamera";

            var camera = mainCamera.AddComponent<Camera>();
            camera.nearClipPlane = 0.01f;
            mainCamera.AddComponent<AudioListener>();
            mainCamera.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            var meshFadeCtrl = mainCamera.AddComponent<MeshFadeCtrl>();
            meshFadeCtrl.FadeOnAwake = true;
            var cameraData = mainCamera.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;

            cameraData.requiresColorTexture = false;
            cameraData.requiresDepthTexture = false;
            cameraData.renderShadows = false;

            mainCamera.GetComponent<Camera>().allowHDR = false;
            mainCamera.GetComponent<Camera>().allowMSAA = false;
            mainCamera.GetComponent<Camera>().farClipPlane = 1200f;

            xrOrigin.CameraFloorOffsetObject = cameraOffset.gameObject;
            xrOrigin.Camera = camera;
            xrOrigin.RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;
            xrOrigin.CameraYOffset = 0;



            foreach (GameObject i in Hands)
            {
                i.transform.parent = cameraOffset.transform;
                var xrController = i.AddComponent<XRController>();
                i.AddComponent<XRRayInteractor>();
                var lineRenderer = i.AddComponent<LineRenderer>();
                i.AddComponent<XRInteractorLineVisual>();

                lineRenderer.material = Instantiate(Resources.Load<Material>("LineMaterial"));
                lineRenderer.numCornerVertices = 4;
                lineRenderer.numCapVertices = 4;
                lineRenderer.sortingOrder = 5;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (i.name.Contains("Left"))
                {
                    xrController.controllerNode = XRNode.LeftHand;
                }
                else if (i.name.Contains("Right"))
                {
                    xrController.controllerNode = XRNode.RightHand;
                }
            }
        }


        private void Connect_PhotonView()
        {
            try
            {
                pv = GetComponent<PhotonView>();
            }
            catch
            {
                Debug.Log("PhotonView가 없습니다.");
            }
        }

        private void Connect_XrController()
        {
            try
            {
                XRHead = GetComponentInChildren<Camera>().transform;

                //if (UseFade)
                {
                    renderer_fade = Instantiate(Resources.Load<GameObject>("FadeSphere"), XRHead).GetComponent<MeshRenderer>();

                    if (FadeOnAwake)
                    {
                        Color color = new Color();
                        color = Color.black;
                        color.a = 0;

                        renderer_fade.sharedMaterial.SetColor("_BaseColor", color);
                    }
                    else
                    {
                        renderer_fade.enabled = false;
                    }
                }


                XRController[] controllers = GetComponentsInChildren<XRController>();
                foreach (XRController i in controllers)
                {
                    if (i.controllerNode == UnityEngine.XR.XRNode.LeftHand)
                        XrController_L = i;
                    else if (i.controllerNode == UnityEngine.XR.XRNode.RightHand)
                        XrController_R = i;
                }

                ///키버튼 맵핑
                if (XrController_L != null)
                {
                    Trigger_L.OnDataChanged += OnTrigger_L;
                    Grip_L.OnDataChanged += OnGrip_L;
                    Menu_L.OnDataChanged += OnMenu_L;
                    Home_L.OnDataChanged += OnHome_L;
                }
                if (XrController_R != null)
                {
                    Trigger_R.OnDataChanged += OnTrigger_R;
                    Grip_R.OnDataChanged += OnGrip_R;
                    Menu_R.OnDataChanged += OnMenu_R;
                    Home_R.OnDataChanged += OnHome_R;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
        }

        //컨트롤러 Value Get
        protected float GetTriggerValue(string type)
        {
            if (type == "Left" && XrController_L != null)
            {
                if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out var L))
                {
                    return L;
                }
            }
            else if (type == "Right" && XrController_R != null)
            {
                if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out var R))
                {
                    return R;
                }
            }
            return -1;
        }

        protected float GetGripValue(string type)
        {
            if (type == "Left" && XrController_L != null)
            {
                if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.grip, out var L))
                {
                    return L;
                }
            }
            else if (type == "Right" && XrController_R != null)
            {
                if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.grip, out var R))
                {
                    return R;
                }
            }
            return -1;
        }

        protected Vector2 GetTouchpadValue(string type)
        {
            if (type == "Left" && XrController_L != null)
            {
                if (XrController_L.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var L))
                {
                    return L;
                }
            }
            else if (type == "Right" && XrController_R != null)
            {
                if (XrController_R.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var R))
                {
                    return R;
                }
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Fade In/Out
        /// </summary>
        /// <param name="fadeSpeed"></param>
        /// <param name="AfterFunc"></param>

        //밝아짐(파라미터 = 대리자)
        public void FadeIn(float fadeTime = 1f, Action AfterFunc = null)
        {
            //if (!UseFade)
            //    return;

            if (pv != null)
                if (!pv.IsMine)
                    return;

            if (coroutine_fade != null)
                StopCoroutine(coroutine_fade);

            renderer_fade.enabled = true;
            renderer_fade.sharedMaterial.SetColor("_BaseColor", color_black);

            coroutine_fade = StartCoroutine(StartFadeIn_C(fadeTime, AfterFunc));
        }


        //어두워짐(파라미터 = 대리자)
        public void FadeOut(float fadeTime = 1f, Action AfterFunc = null)
        {
            //if (!UseFade)
            //    return;

            if (pv != null)
                if (!pv.IsMine)
                    return;

            if (coroutine_fade != null)
                StopCoroutine(coroutine_fade);

            renderer_fade.enabled = true;
            renderer_fade.sharedMaterial.SetColor("_BaseColor", color_clear);

            coroutine_fade = StartCoroutine(StartFadeOut_C(fadeTime, AfterFunc));
        }


        IEnumerator StartFadeIn_C(float fadeTime, Action AfterFunc = null)
        {
            float valueP = 0f;
            float speed = 1f;
            if (fadeTime > 0f)
            {
                speed = 1 / fadeTime;
            }

            yield return null;

            while (valueP < 1f)
            {
                valueP = Mathf.Clamp01(valueP + Time.deltaTime * speed);
                renderer_fade.sharedMaterial.SetColor("_BaseColor", Color.Lerp(color_black, color_clear, valueP));
                yield return null;
            }

            renderer_fade.enabled = false;

            if (AfterFunc != null)
            {
                AfterFunc();
            }              
        }


        IEnumerator StartFadeOut_C(float fadeTime, Action AfterFunc = null)
        {
            float valueP = 0f;
            float speed = 1f;
            if (fadeTime > 0f)
            {
                speed = 1 / fadeTime;
            }

            yield return null;

            while (valueP < 1f)
            {
                valueP = Mathf.Clamp01(valueP + Time.deltaTime * speed);
                renderer_fade.sharedMaterial.SetColor("_BaseColor", Color.Lerp(color_clear, color_black, valueP));
                yield return null;
            }

            if (AfterFunc != null)
            {
                AfterFunc();
            }
        }


        ///컨트롤러 Haptic
        protected void Haptic(string type, uint amplitude = 5, int duration = 5)
        {
            if (!UseHaptic)
                return;

            if (pv != null)
                if (!pv.IsMine)
                    return;

            var tempAmplitude = amplitude * 0.1f;
            var tempDuration = duration * 0.04f;
            switch (type)
            {
                case "Left":
                    if (XrController_L != null)
                    {
                        Debug.Log("Left Haptic");
                        XrController_L.SendHapticImpulse(tempAmplitude, tempDuration);
                    }
                    break;

                case "Right":
                    if (XrController_R != null)
                    {
                        Debug.Log("Right Haptic");
                        XrController_R.SendHapticImpulse(tempAmplitude, tempDuration);
                    }
                    break;

                default:
                    Debug.LogError("손 진동 형식을 정해주세요. Left, Right");
                    break;
            }
        }


    }

}

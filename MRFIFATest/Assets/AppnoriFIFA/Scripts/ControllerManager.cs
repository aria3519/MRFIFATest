using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Appnori.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using FStudio.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace FStudio.MatchEngine
{
    public struct ButtonState
    {
        public bool GripR;
        public bool GripL;
        public bool TriL;
        public bool TriR;
        public bool MenuL;
        public bool MenuR;
    }


    public class ControllerManager : SceneObjectSingleton<ControllerManager>
    {

        // 플랫폼 구분
        // input Action event 등록함수
        // 빼는 함수
        // 현재 컨트롤 상태 구분 함수

        [SerializeField] private InputActionAsset inputSystem;

        public static ButtonState _ButtonSt = new ButtonState();

        /* AsyncOperationHandle<GameObject> opHandle;
         public string key;*/

        public List<ActionBasedController> _XRcontros = new List<ActionBasedController>();


        private void Awake()
        {


            /*Addressables.LoadAssetAsync<InputActionAsset>("Assets/AppnoriFIFA/Scripts/playerInputSetting.inputactions").Completed += (obj) =>
            {
                if (obj.Status != AsyncOperationStatus.Succeeded)
                    return;
            };*/

           /* AssetBundle bun = AssetBundle.LoadFromMemory(File.ReadAllBytes("Assets/AppnoriFIFA/Scripts/playerInputSetting.inputactions"));
            inputSystem = bun.LoadAsset<InputActionAsset>("playerInputSetting.inputactions");*/
                



            StartCoroutine(FindXRContr());

        }


        /*public IEnumerator OnLoadObject()
        {
            opHandle = Addressables.LoadAssetAsync<GameObject>(key);
            yield return opHandle;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject obj = opHandle.Result;
                Instantiate(obj, transform);
            }
        }*/
        /*private void OnLoadDone(UnityEngine.ResourceManagement.IAsyncOperation<Sprite> obj)
        {

        }*/


       

        private IEnumerator FindXRContr()
        {

            while (true)
            {

                if (GameObject.Find("XR Origin/Camera Offset/LeftHand Controller") != null
                    && GameObject.Find("XR Origin/Camera Offset/RightHand Controller") != null)

                {
                    var contrL = GameObject.Find("XR Origin/Camera Offset/LeftHand Controller").GetComponent<ActionBasedController>();
                    var contrR = GameObject.Find("XR Origin/Camera Offset/RightHand Controller").GetComponent<ActionBasedController>();

                    _XRcontros.Add(contrL);
                    _XRcontros.Add(contrR);
                    yield break;
                }


                yield return null;
            }
        }

        public void OnInputAction()
        {
            inputSystem.Enable();
            InputActionMap Map = inputSystem.actionMaps[0];

            InputAction OnTriggerActL = Map.FindAction("OnTriggerL");
            InputAction OnTriggerActR = Map.FindAction("OnTriggerR");
            InputAction OnGripActL = Map.FindAction("OnGripL");
            InputAction OnGripActR = Map.FindAction("OnGripR");
            InputAction OnMenuActL = Map.FindAction("OnMenuL");
            InputAction OnMenuActR = Map.FindAction("OnMenuR");

            OnTriggerActL.performed += OnTriL;
            OnTriggerActL.canceled += OnTriL;
            OnTriggerActR.performed += OnTriR;
            OnTriggerActR.canceled += OnTriR;
            OnGripActL.performed += OnGripL;
            OnGripActL.canceled += OnGripL;
            OnGripActR.performed += OnGripR;
            OnGripActR.canceled += OnGripR;
            OnMenuActL.performed += OnMenuL;
            OnMenuActL.canceled += OnMenuL;
            OnMenuActR.performed += OnMenuR;
            OnMenuActR.canceled += OnMenuR;



        }
        public void RemoveInputAction()
        {
            inputSystem.Disable();
            InputActionMap Map = inputSystem.actionMaps[0];
            InputAction OnTriggerActL = Map.FindAction("OnTriggerL");
            InputAction OnTriggerActR = Map.FindAction("OnTriggerR");
            InputAction OnGripActL = Map.FindAction("OnGripL");
            InputAction OnGripActR = Map.FindAction("OnGripR");
            InputAction OnMenuActL = Map.FindAction("OnMenuL");
            InputAction OnMenuActR = Map.FindAction("OnMenuR");

            OnTriggerActL.performed -= OnTriL;
            OnTriggerActL.canceled -= OnTriL;
            OnTriggerActR.performed -= OnTriR;
            OnTriggerActR.canceled -= OnTriR;
            OnGripActL.performed -= OnGripL;
            OnGripActL.canceled -= OnGripL;
            OnGripActR.performed -= OnGripR;
            OnGripActR.canceled -= OnGripR;
            OnMenuActL.performed -= OnMenuL;
            OnMenuActL.canceled -= OnMenuL;
            OnMenuActR.performed -= OnMenuR;
            OnMenuActR.canceled -= OnMenuR;


        }


        // 이벤트 등록 하고
        public void OnTriL(InputAction.CallbackContext ctx)
        {
            //Debug.LogError("Moving");


            // Vector2 temp = ctx.action.ReadValue<Vector2>();
            _ButtonSt.TriL = ctx.action.IsPressed();
           





        }
        public void OnTriR(InputAction.CallbackContext ctx)
        {
            //Debug.LogError("Moving");


            // Vector2 temp = ctx.action.ReadValue<Vector2>();
            _ButtonSt.TriR = ctx.action.IsPressed();






        }
        public void OnGripL(InputAction.CallbackContext ctx)
        {
            _ButtonSt.GripL = ctx.action.IsPressed();


           // Debug.LogError("OnGripL" + _ButtonSt.GripL);

        }
        public void OnGripR(InputAction.CallbackContext ctx)
        {

            _ButtonSt.GripR = ctx.action.IsPressed();
            //Debug.LogError("OnGripL" + _ButtonSt.GripR);


        }

        public void OnMenuL(InputAction.CallbackContext ctx)
        {

            _ButtonSt.MenuR = ctx.action.IsPressed();

        }
        public void OnMenuR(InputAction.CallbackContext ctx)
        {

            _ButtonSt.MenuL = ctx.action.IsPressed();

        }




#if STEAMWORKS && UNITY_STANDALONE


#else


#endif
    }
}

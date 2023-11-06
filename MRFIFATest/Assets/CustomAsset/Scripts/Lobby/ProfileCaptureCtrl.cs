using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class ProfileCaptureCtrl : MonoBehaviour
{
    public class ModelData
    {
        public CustomModelSettingCtrl customModel;
        public Transform tr;
        public Animator anim;
        public Transform center;
        //public Transform pivot;
    }
    private ModelData[] modelDatas = new ModelData[5];

    private Camera cam;

    //public Light[] lights;
    public Light captureLight;

    private GameObject parentGO;

    private RenderTexture[] renderTextures_result = new RenderTexture[8];
    private RenderTexture[] renderTextures_multi = new RenderTexture[5];

    public MeshRenderer renderer_cam_view;

    private void Awake()
    {
        for (int i = 0; i < modelDatas.Length; i++)
        {
            modelDatas[i] = new ModelData();
            modelDatas[i].tr = transform.Find("Parent/Model0" + (i + 1));
            modelDatas[i].customModel = modelDatas[i].tr.GetComponent<CustomModelSettingCtrl>();
            modelDatas[i].anim = modelDatas[i].tr.GetComponent<Animator>();
            modelDatas[i].center = modelDatas[i].tr.Find("ITEM_HEAD");
            //modelDatas[i].pivot = modelDatas[i].tr.Find("ITEM_HEAD");
        }

        for (int i = 0; i < renderTextures_result.Length; i++)
        {
            renderTextures_result[i] = Resources.Load<RenderTexture>("UI/RenderTexture_Result0" + (i + 1));
        }

        for (int i = 0; i < renderTextures_multi.Length; i++)
        {
            renderTextures_multi[i] = Resources.Load<RenderTexture>("UI/RenderTexture_MultiAnim0" + (i + 1));
        }

        cam = transform.GetComponentInChildren<Camera>();

        int layerIndex = 31;

        cam.cullingMask = 1 << layerIndex;

        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == "Post-process Volume")
            {
                continue;
            }
            transforms[i].gameObject.layer = layerIndex;
        }

        parentGO = transform.GetChild(0).gameObject;
        parentGO.SetActive(false);
    }

    public void ShotResultImages(ResultUserData userData, bool isResultBoard = false)
    {
        if (isResultBoard && userData.slot.Count == 2)
        {
            PlayImages_Result(userData);
            return;
        }

        Light[] lights_temp = Light.GetLights(LightType.Directional, 0);
        //lights[0].gameObject.SetActive(true);
        //lights[1].gameObject.SetActive(true);
        captureLight.gameObject.SetActive(true);
        for (int i = 0; i < lights_temp.Length; i++)
        {
            //if (lights_temp[i] == lights[0])
            if (lights_temp[i] == captureLight)
            {
                lights_temp[i] = null;
            }
            else if (lights_temp[i].enabled)
            {
                lights_temp[i].enabled = false;
            }
            else
            {
                lights_temp[i] = null;
            }
        }

        parentGO.SetActive(true);
        renderer_cam_view.enabled = false;

        cam.fieldOfView = 30f;

        RenderTexture currentRT = RenderTexture.active;
        modelDatas[0].tr.gameObject.SetActive(true);
        for (int i = 1; i < modelDatas.Length; i++)
        {
            modelDatas[i].tr.gameObject.SetActive(false);
        }
        modelDatas[0].tr.localPosition = Vector3.zero;

        cam.transform.localRotation = Quaternion.Euler(0f, 15f, 0f);


        for (int i = 0; i < userData.slot.Count; i++)
        {
            modelDatas[0].customModel.enabled = true;
            modelDatas[0].customModel.Init(GameDataManager.instance.GetUserModelData(userData.slot[i].userId), CustomModelViewState.Normal, null, 0.2f);
            modelDatas[0].tr.gameObject.SetActive(false);
            modelDatas[0].tr.gameObject.SetActive(true);

            string stateName = "WarmUp01";

            float normalTime = 0f;

            //switch (i)
            //{
            //    case 1:
            //        {
            //            normalTime = 0.25f;
            //        }
            //        break;
            //    case 2:
            //        {
            //            normalTime = 0.5f;
            //        }
            //        break;
            //    case 3:
            //        {
            //            normalTime = 0.75f;
            //        }
            //        break;
            //    case 4:
            //        {
            //            normalTime = 1f;
            //        }
            //        break;
            //}


            modelDatas[0].anim.Play(stateName, -1, normalTime);
            modelDatas[0].anim.Update(1f);

            //cam.transform.position = customModels_center[0].position + new Vector3(0f, 0.1f, -2f);

            Debug.Log(cam);
            Debug.Log(modelDatas[0].center);
            cam.transform.position = modelDatas[0].center.position + new Vector3(-0.45f, 0.1f, -1.8f);
            cam.transform.position = modelDatas[0].center.position + new Vector3(-0.45f, 0.1f, -1.8f);

            cam.targetTexture = renderTextures_result[i];

            RenderTexture.active = renderTextures_result[i];

            cam.Render();
        }

        cam.targetTexture = null;

        RenderTexture.active = currentRT;
        renderer_cam_view.enabled = true;
        parentGO.SetActive(false);

        for (int i = 0; i < lights_temp.Length; i++)
        {
            if (lights_temp[i] != null)
            {
                lights_temp[i].enabled = true;
            }
        }
    }

    public void PlayImages_Result(ResultUserData userData)
    {
        if (shotImagesCoroutine != null)
        {
            StopCoroutine(shotImagesCoroutine);
        }

        shotImagesCoroutine = StartCoroutine(PlayImagesCoroutine_Result(userData));
    }

    IEnumerator PlayImagesCoroutine_Result(ResultUserData userData)
    {
        cam.fieldOfView = 40f;
        cam.backgroundColor = Color.clear;

        RenderTexture rt = RenderTexture.active;

        for (int i = 0; i < modelDatas.Length; i++)
        {
            modelDatas[i].tr.gameObject.SetActive(false);
            RenderTexture.active = renderTextures_result[i];
            GL.Clear(true, true, Color.clear);
        }
        RenderTexture.active = rt;


        yield return new WaitForSecondsRealtime(0.5f);
        parentGO.SetActive(true);

        Vector3[] poss = new Vector3[2];

        string[] keep_userIds = new string[2];

        for (int i = 0; i < keep_userIds.Length; i++)
        {
            keep_userIds[i] = "";
        }

        ////////////////////

        while (true)
        {
            Light[] lights_temp = Light.GetLights(LightType.Directional, 0);

            for (int j = 0; j < lights_temp.Length; j++)
            {
                if (lights_temp[j] == captureLight)
                {
                    lights_temp[j] = null;
                }
                else if (lights_temp[j].enabled)
                {
                    lights_temp[j].enabled = false;
                }
                else
                {
                    lights_temp[j] = null;
                }
            }

            captureLight.gameObject.SetActive(true);
            renderer_cam_view.enabled = false;
            RenderTexture currentRT = RenderTexture.active;
            int count = userData.slot.Count;
            for (int i = 0; i < keep_userIds.Length; i++)
            {
                if (count > i && userData.slot[i].userId != keep_userIds[i])
                {
                    keep_userIds[i] = userData.slot[i].userId;

                    modelDatas[i].tr.gameObject.SetActive(true);
                    modelDatas[i].customModel.enabled = true;
                    modelDatas[i].customModel.Init(GameDataManager.instance.GetUserModelData(userData.slot[i].userId), CustomModelViewState.Normal, null, 0.2f);

                    string stateName = "";

                    if (i == 0)
                    {
                        stateName = "Result_Win";
                    }
                    else
                    {
                        stateName = "Result_Lose";
                    }

                    cam.transform.localRotation = Quaternion.identity;

                    modelDatas[i].anim.Play(stateName, -1, 0f);

                    modelDatas[i].tr.localPosition = Vector3.zero;

                    poss[i] = modelDatas[i].center.position + new Vector3(0f, 0.1f, -4f);
                }
                else if (count <= i && keep_userIds[i] != "")
                {
                    keep_userIds[i] = "";
                    modelDatas[i].tr.gameObject.SetActive(false);
                }

                modelDatas[i].tr.localPosition = Vector3.zero;

                if (i == 0)
                {
                    poss[i] = Vector3.Lerp(poss[i], modelDatas[i].center.position + new Vector3(0f, 0.1f, -1.3f), Time.unscaledDeltaTime * 5f);
                    cam.transform.position = poss[i];
                    PublicGameUIManager.GetInstance.SyncProfileLight(modelDatas[i].anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
                }
                else
                {
                    poss[i] = Vector3.Lerp(poss[i], modelDatas[i].center.position + new Vector3(0f, 0.05f, -1.8f), Time.unscaledDeltaTime * 5f);
                    cam.transform.position = poss[i];
                }

                cam.targetTexture = renderTextures_result[i];
                RenderTexture.active = renderTextures_result[i];
                cam.Render();

                modelDatas[i].tr.localPosition = new Vector3(0f, -100f, 0f);
            }

            RenderTexture.active = currentRT;
            cam.targetTexture = null;
            renderer_cam_view.enabled = true;

            captureLight.gameObject.SetActive(false);
            for (int j = 0; j < lights_temp.Length; j++)
            {
                if (lights_temp[j] != null)
                {
                    lights_temp[j].enabled = true;
                }
            }
            yield return null;
        }
    }


    public void PlayImages_Match()
    {
        if (shotImagesCoroutine != null)
        {
            StopCoroutine(shotImagesCoroutine);
        }

        shotImagesCoroutine = StartCoroutine(PlayImagesCoroutine_Match());
    }

    Coroutine shotImagesCoroutine;

    public void StopImages()
    {
        if (shotImagesCoroutine != null)
        {
            StopCoroutine(shotImagesCoroutine);
        }

        parentGO.SetActive(false);
    }

    IEnumerator PlayImagesCoroutine_Match()
    {
        cam.fieldOfView = 50f;
        cam.backgroundColor = Color.clear;

        RenderTexture rt = RenderTexture.active;

        for (int i = 0; i < modelDatas.Length; i++)
        {
            modelDatas[i].tr.gameObject.SetActive(false);
            RenderTexture.active = renderTextures_multi[i];
            GL.Clear(true, true, Color.clear);
        }
        RenderTexture.active = rt;


        yield return new WaitForSecondsRealtime(0.5f);
        parentGO.SetActive(true);

        string[] keep_userIds = new string[5];

        for (int i = 0; i < keep_userIds.Length; i++)
        {
            keep_userIds[i] = "";
        }

        while (true)
        {
            Light[] lights_temp = Light.GetLights(LightType.Directional, 0);

            for (int j = 0; j < lights_temp.Length; j++)
            {
                //if (lights_temp[j] == lights[0])
                if (lights_temp[j] == captureLight)
                {
                    lights_temp[j] = null;
                }
                else if (lights_temp[j].enabled)
                {
                    lights_temp[j].enabled = false;
                }
                else
                {
                    lights_temp[j] = null;
                }
            }
            //lights[0].gameObject.SetActive(true);
            //lights[1].gameObject.SetActive(true);
            captureLight.gameObject.SetActive(true);
            renderer_cam_view.enabled = false;
            RenderTexture currentRT = RenderTexture.active;
            int count = GameDataManager.instance.userInfos.Count;
            for (int i = 0; i < keep_userIds.Length; i++)
            {      
                if (count > i && GameDataManager.instance.userInfos[i].id != keep_userIds[i])
                {
                    keep_userIds[i] = GameDataManager.instance.userInfos[i].id;

                    modelDatas[i].tr.gameObject.SetActive(true);
                    modelDatas[i].customModel.enabled = true;
                    modelDatas[i].customModel.Init(GameDataManager.instance.userInfos[i].customModelData, CustomModelViewState.Normal, null, 0.2f);

                    string stateName = "WarmUp0" + ((int)Random.Range(1f,3.9999f));

                    cam.transform.localRotation = Quaternion.identity;

                    modelDatas[i].anim.Play(stateName, -1, 0f);

                    modelDatas[i].tr.localPosition = Vector3.zero;
                }
                else if(count <= i && keep_userIds[i] != "")
                {
                    keep_userIds[i] = "";
                    modelDatas[i].tr.gameObject.SetActive(false);
                }

                modelDatas[i].tr.localPosition = Vector3.zero;

                cam.transform.position = modelDatas[i].tr.position + new Vector3(-1.05f, 0.6f, -3f);
                cam.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);

                cam.targetTexture = renderTextures_multi[i];
                RenderTexture.active = renderTextures_multi[i];
                cam.Render();

                modelDatas[i].tr.localPosition = new Vector3(0f, -100f, 0f);
            }

            RenderTexture.active = currentRT;
            cam.targetTexture = null;
            renderer_cam_view.enabled = true;

            //lights[0].gameObject.SetActive(false);
            //lights[1].gameObject.SetActive(false);
            captureLight.gameObject.SetActive(false);
            for (int j = 0; j < lights_temp.Length; j++)
            {
                if (lights_temp[j] != null)
                {
                    lights_temp[j].enabled = true;
                }
            }
            yield return null;
        }
    }
}

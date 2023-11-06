#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

namespace Appnori.Util
{
    public class Edit_SetRenderQueue : MonoBehaviour
    {
        [MenuItem("Tools/Set Render Queue")]
        private static void SetRenderQueue()
        {
            Object[] objects = Selection.objects;

            for (int i = 0; i < objects.Length; i++)
            {
                try
                {
                    Material mat = (Material)objects[i];
                    mat.SetFloat("_QueueOffset", 550);
                    AssetDatabase.SaveAssetIfDirty(Selection.activeObject);
                }
                catch (System.Exception)
                {

                }
            }
            //Material mat = (Material)Selection.activeObject;
            //mat.SetFloat("_QueueOffset", 550);
            //AssetDatabase.SaveAssetIfDirty(Selection.activeObject);
        }

        [MenuItem("Tools/Get Render Queue")]
        private static void GetRenderQueue()
        {
            Material mat = (Material)Selection.activeObject;
            Debug.Log(Selection.activeObject.name + " - " + mat.renderQueue);
        }
    }
}
#endif
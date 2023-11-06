using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Appnori.Util
{
    public class LobbyFishingLineCtrl : MonoBehaviour
    {
        public Transform camTr;
        public Material mat;

        // Update is called once per frame
        void Update()
        {
            mat.SetFloat("_Thickness", (camTr.position - transform.position).magnitude * 0.002f + 0.0001f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Appnori.Util
{
    public class NetAlphaOffsetCtrl : MonoBehaviour
    {
        public Transform camTr;
        public float start_cut_off = 0.5f;
        public float multiply_cut_off = 0.1f;
        private Renderer renderer;
        private readonly string name_cut_off = "_Cutoff";

        // Start is called before the first frame update
        void Start()
        {
            renderer = transform.GetComponent<Renderer>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (camTr == null)
            {
                return;
            }

            renderer.material.SetFloat(name_cut_off, Mathf.Clamp(start_cut_off - Mathf.Abs(camTr.position.x - transform.position.x) * multiply_cut_off, 0.01f, 0.99f));
        }
    }
}

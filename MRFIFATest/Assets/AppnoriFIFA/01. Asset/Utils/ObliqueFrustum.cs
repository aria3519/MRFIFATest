using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public class ObliqueFrustum : MonoBehaviour
    {
        [SerializeField] private float horizenOlique;
        [SerializeField] private float verticalOlique;

        private Camera targetCM;

        private void Awake()
        {
            targetCM = Camera.main;

            SetOblique(horizenOlique, verticalOlique);
        }

        // If values close to 1 or -1, one side of the frustum is flat from other reference line.
        private void SetOblique(float horizObli, float vertObli)
        {
            Matrix4x4 mat = Camera.main.projectionMatrix;
            mat[0, 2] = horizObli;
            mat[1, 2] = vertObli;
            targetCM.projectionMatrix = mat;
        }
    }
}
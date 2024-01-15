using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using static Jisu.Utils.MotionTrailRenderer;

namespace Jisu.Utils
{
    public class MotionTrail : MonoBehaviour
    {
        private Transform myTF;
        private MeshFilter myMesh;
        private Renderer myRenderer;

        private MeshRendererType meshRendererType;
        private MeshFilter meshFilter;
        private SkinnedMeshRenderer skinnedMesh;

        private Transform targetTr;
        private Mesh bakedMesh;

        private string shaderProperty;
        private float originAlpha;

        //private Sequence alphaSequence;

        public bool IsActive => myRenderer.enabled;

        private void Awake()
        {
            myTF = transform;
            myMesh = GetComponent<MeshFilter>();
            myRenderer = GetComponent<Renderer>();

            shaderProperty = "_Alpha";
            originAlpha = myRenderer.material.GetFloat(shaderProperty);

            myRenderer.enabled = false;
        }

        public MotionTrail Initialize(in MeshRendererType meshRendererType, in MeshFilter meshFilter, in SkinnedMeshRenderer skinnedMesh, in float runTime)
        {
            this.meshRendererType = meshRendererType;
            this.meshFilter = meshFilter;
            this.skinnedMesh = skinnedMesh;

            if (this.meshRendererType == MeshRendererType.Mesh)
            {
                targetTr = this.meshFilter.transform;
                bakedMesh = this.meshFilter.sharedMesh;
            }
            else
            {
                targetTr = this.skinnedMesh.transform;
                bakedMesh = new();
            }

            var seq1Time = runTime * 0.3f;
            var seq2Time = runTime - seq1Time;

            /*alphaSequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, originAlpha, seq1Time, (value) => myRenderer.material.SetFloat(shaderProperty, value)).SetEase(Ease.InSine))
                .Append(DOVirtual.Float(originAlpha, 0f, seq2Time, (value) => myRenderer.material.SetFloat(shaderProperty, value)).SetEase(Ease.OutSine)
                    .OnComplete(() => myRenderer.enabled = false))
                .SetAutoKill(false)
                .Pause();*/

            return this;
        }

        public void Render()
        {
            myRenderer.enabled = true;

            if (meshRendererType == MeshRendererType.SkinnedMesh)
                skinnedMesh.BakeMesh(bakedMesh);

            myMesh.mesh = bakedMesh;
            myTF.SetPositionAndRotation(targetTr.position, targetTr.rotation);

            //alphaSequence.Restart();
        }

        private void OnDestroy()
        {
            //alphaSequence.Kill();
        }
    }
}
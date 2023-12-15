using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace Jisu.Utils
{
    public class MotionTrailRenderer : MonoBehaviour
    {
        public enum MeshRendererType { Mesh, SkinnedMesh }

        [Serializable]
        public class MotionTrailPool
        {
            [SerializeField] private string poolName;
            [SerializeField] private MeshRendererType meshRendererType;
            [SerializeField] private MeshFilter meshFilter;
            [SerializeField] private SkinnedMeshRenderer skinnedMesh;
            [SerializeField] private GameObject origin;
            [SerializeField] private int countInPool = 5;
            [SerializeField] private float runTime = 0.5f;

            private Transform Root;

            private readonly List<MotionTrail> pool = new();

            public void WarmPool(in Transform root)
            {
                Root = new GameObject($"MotionTrail Pool {poolName}").transform;
                Root.parent = root;
                Root.localPosition = Vector3.zero;

                for (int i = 0; i < countInPool; i++)
                    pool.Add(Instantiate(origin, Root).GetComponent<MotionTrail>().Initialize(meshRendererType, meshFilter, skinnedMesh, runTime));
            }

            public void Render()
            {
                var instance = pool.FirstOrDefault(obj => obj.IsActive == false);
                if (instance == null)
                {
                    instance = Instantiate(origin, Root).GetComponent<MotionTrail>();
                    pool.Add(instance);
                }

                instance.Render();
            }
        }

        [SerializeField] private List<MotionTrailPool> motionTrailPools;
        [SerializeField] private float intvlTime = 0.15f;
        [SerializeField] private bool playOnAwake;

        private CoroutineWrapper motionWrapper;

        private void Start()
        {
            var root = transform;

            for (int i = 0; i < motionTrailPools.Count; i++)
                motionTrailPools[i].WarmPool(root);

            motionWrapper = CoroutineWrapper.Generate(this);

            if (playOnAwake)
                motionWrapper.Start(SetMotionTrail_Coroutine());
        }

        public void SetMotionTrail(in bool isPlay)
        {
            if (motionTrailPools.Count == 0)
            {
                DebugForEditor.LogWarning($"motionTrailPools is Empty !!!");
                return;
            }

            if (isPlay)
                motionWrapper.Start(SetMotionTrail_Coroutine());
            else
                motionWrapper.Stop();
        }

        private IEnumerator SetMotionTrail_Coroutine()
        {
            while (enabled)
            {
                for (int i = 0; i < motionTrailPools.Count; i++)
                    motionTrailPools[i].Render();

                yield return YieldInstructionCache.WaitForSeconds(intvlTime);
            }
        }

        public void PlayMotionTrail(in float duration)
        {
            if (motionTrailPools.Count == 0)
            {
                DebugForEditor.LogWarning($"motionTrailPools is Empty !!!");
                return;
            }

            if (duration <= intvlTime)
            {
                DebugForEditor.LogWarning($"MotionTrail Duration is too Short via intvlTime !!! : {duration} / {intvlTime}");
                return;
            }

            if (motionWrapper.IsPlaying)
                motionWrapper.Stop();

            motionWrapper.Start(PlayMotionTrail_Coroutine(duration));
        }
        
        private IEnumerator PlayMotionTrail_Coroutine(float runTime)
        {
            var time = 0f;
            while(time < runTime)
            {
                for (int i = 0; i < motionTrailPools.Count; i++)
                    motionTrailPools[i].Render();

                time += Time.deltaTime;

                yield return YieldInstructionCache.WaitForSeconds(intvlTime);
            }
        }
    }
}
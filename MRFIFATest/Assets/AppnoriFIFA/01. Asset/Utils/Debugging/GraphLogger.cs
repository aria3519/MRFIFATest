using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Jisu.Utils
{
    public class GraphLogger : LocalSingleton<GraphLogger>
    {
        [SerializeField] private Transform Root;
        [SerializeField] private GameObject barOrigin;
        [SerializeField] private int MaxCount;
        [SerializeField] private Text maxText;

        [Readonly][SerializeField] private List<float> floatDataList = new List<float>();
        [Readonly][SerializeField] private List<float> prevData;
        private readonly List<Transform> graphObject = new List<Transform>();

        private Transform PoolRoot;
        private readonly List<GameObject> graphObjectPool = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            PoolRoot = Instantiate(new GameObject("PoolRoot"), transform).transform;
            PoolRoot.SetParent(transform);
            for (int i = 0; i < MaxCount + 1; i++)
            {
                var instance = Instantiate(barOrigin, PoolRoot);
                instance.SetActive(false);
                graphObjectPool.Add(instance);
            }
        }

        public static void Reset()
        {
            if (Instance == null)
                return;

            Instance.ResetInternal();
        }

        private void ResetInternal()
        {
            prevData = new List<float>(floatDataList.ToArray());

            floatDataList.Clear();

            for(int i = 0; i < graphObject.Count; i++)
            {
                graphObject[i].SetParent(PoolRoot);
                graphObject[i].gameObject.SetActive(false);
            }
            graphObject.Clear();
        }

        public static void OnDataUpdate(in float data)
        {
            if (Instance == null)
                return;

            Instance.OnDataUpdateInternal(data);
        }

        private void OnDataUpdateInternal(float data)
        {
            floatDataList.Add(data);

            if (floatDataList.Count > MaxCount)
            {
                floatDataList.RemoveAt(0);

                var deleteTarget = graphObject.First();
                graphObject.Remove(deleteTarget);

                deleteTarget.SetParent(PoolRoot);
                deleteTarget.gameObject.SetActive(false);
            }

            Render();
        }

        private void Render()
        {
            var avg = floatDataList.Average();
            var max = Mathf.Max(floatDataList.Max(), 0.01f);
            maxText.text = $"Max : {max:F4}, avg : {avg:F4}";

            var count = floatDataList.Count;
            for (int i = 0; i < count - 1; ++i)
            {
                var value = floatDataList[i] / max;
                graphObject[i].localScale = new Vector3(1, Mathf.Max(0.001f, value), 1);
            }

            var currentValue = floatDataList[count - 1] / max;
            var instance = graphObjectPool.FirstOrDefault((obj) => obj.activeInHierarchy == false);
            instance.transform.SetParent(Root);
            instance.SetActive(true);
            instance.transform.localScale = new Vector3(1, Mathf.Max(0.001f, currentValue), 1);
            instance.transform.SetAsLastSibling();

            graphObject.Add(instance.transform);
        }
    }
}
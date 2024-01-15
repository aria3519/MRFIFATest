using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jisu.Utils
{
    public class DebugRayVisualizer : LocalSingleton<DebugRayVisualizer>
    {
        public class LineVisualizer
        {
            public bool isUse;

            private Transform myTF;
            private LineRenderer line;

            private readonly Gradient lineColor = new Gradient();

            public void Initialize(in Transform transform, in LineRenderer renderer)
            {
                isUse = false;

                myTF = transform;
                line = renderer;
            }

            public void Draw(in Vector3 start, in Vector3 direction, in Color color)
            {
                isUse = true;

                myTF.position = start;
                myTF.rotation = Quaternion.LookRotation(direction);

                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, start + direction.magnitude * myTF.forward);

                var colorKey = new GradientColorKey[1];
                colorKey[0] = new GradientColorKey(color, 0f);
                lineColor.colorKeys = colorKey;

                line.colorGradient = lineColor;
            }

            public void Erase()
            {
                isUse = false;

                line.positionCount = 0;
            }
        }

        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Material lineMtrl;
        private readonly List<LineVisualizer> lineList = new List<LineVisualizer>();

        private bool IsInitialize = false;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < 3; i++)
            {
                var newInstance = new GameObject($"LineRenderer {i + 1:D2}");
                newInstance.transform.SetParent(transform);

                var newLineRenderer = newInstance.AddComponent<LineRenderer>();
                newLineRenderer.positionCount = 0;
                newLineRenderer.startColor = Color.red;
                newLineRenderer.endColor = Color.red;
                newLineRenderer.startWidth = lineWidth;
                newLineRenderer.endWidth = lineWidth;
                newLineRenderer.material = lineMtrl;
                newLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var newVisualizer = new LineVisualizer();
                newVisualizer.Initialize(newInstance.transform, newLineRenderer);
                lineList.Add(newVisualizer);
            }

            IsInitialize = true;
        }

        public static void DrawRay(in Vector3 start, in Vector3 direction, in Color color, in float duration)
        {
            if (Instance == null)
                return;

            if (Instance.IsInitialize == false)
                return;

            Instance.DrawRayInternal(start, direction, color, duration);
        }

        private void DrawRayInternal(in Vector3 start, in Vector3 direction, in Color color, in float duration)
        {
            var line = lineList.FirstOrDefault((line) => line.isUse == false);
            if (line == null)
            {
                var newInstance = new GameObject($"LineRenderer {lineList.Count:D2}");
                newInstance.transform.SetParent(transform);

                var newLineRenderer = newInstance.AddComponent<LineRenderer>();
                newLineRenderer.positionCount = 0;
                newLineRenderer.startColor = Color.red;
                newLineRenderer.endColor = Color.red;
                newLineRenderer.startWidth = lineWidth;
                newLineRenderer.endWidth = lineWidth;
                newLineRenderer.material = lineMtrl;
                newLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                line = new LineVisualizer();
                line.Initialize(newInstance.transform, newLineRenderer);
                lineList.Add(line);
            }

            line.Draw(start, direction, color);

            StartCoroutine(Erase_Coroutine(lineList.IndexOf(line), duration));
        }

        private IEnumerator Erase_Coroutine(int index, float delay)
        {
            yield return YieldInstructionCache.WaitForSeconds(delay);

            lineList[index].Erase();
        }
    }
}

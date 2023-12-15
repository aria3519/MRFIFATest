using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    using TMPro;
    using UnityEngine.UI;

    public class FrameTimeChecker : MonoBehaviour
    {
        [SerializeField] private int targetFPS;
        [SerializeField] private Text logText;
        [SerializeField] private TextMeshPro logTextMesh;
        [SerializeField] private bool isUseRuntimeLog;
        [SerializeField] private int logIndex;

        private float targetFrameTime;

        private float frames = 0f;
        private float timeElapes = 0f;
        private float frameTime = 0f;

        private void Awake()
        {
            targetFrameTime = 1000.0f / targetFPS;
        }

        private void Update()
        {
            frames++;

            timeElapes += Time.unscaledDeltaTime;
            if(timeElapes > 1f)
            {
                frameTime = timeElapes / frames;
                timeElapes -= 1f;

                frames = Mathf.Min(frames, targetFPS);
                frameTime = Mathf.Max(frameTime * 1000.0f, targetFrameTime);

                UpdateFrameTime();

                frames = 0f;
            }
        }

        private void UpdateFrameTime()
        {
            var log = $"FPS : {(int)frames} / {targetFPS}\nFrameTime : {frameTime:F1} / {targetFrameTime:F1} ms";
            if (isUseRuntimeLog)
            {
                RuntimeLogManager.Notify(logIndex, log);
            }
            else if(logTextMesh != null)
            {
                logTextMesh.text = log;
            }
            else if(logText != null)
            {
                logText.text = log;
            }
        }
    }
}

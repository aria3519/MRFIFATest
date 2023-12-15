using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System.Text;

namespace Jisu.Utils
{
    public class RuntimeLogManager : LocalSingleton<RuntimeLogManager>
    {
        [System.Serializable]
        public class RuntimeLog
        {
            public TextMeshPro text;
            public float stringLength;
            public List<string> logs = new();

            private int count = 0;

            public void SetLog(in string str, in bool isDebugLog)
            {
                if (isDebugLog)
                    Debug.Log($"[{count}] {str}");

                if (logs.Count >= stringLength)
                    logs.RemoveAt(0);

                logs.Add($"[{count++}] {str}\n");

                StringBuilder output = new StringBuilder();
                foreach (var noti in logs)
                    output.Append(noti);

                text.text = output.ToString();
            }
        }

        [SerializeField] private RuntimeLog[] runtimeLogs;

        public static void Notify(in int index, in string str, in bool isDebugLog = false)
        {
            if (Instance == null)
                return;

            Instance.runtimeLogs[index].SetLog(str, isDebugLog);
        }
    }
}
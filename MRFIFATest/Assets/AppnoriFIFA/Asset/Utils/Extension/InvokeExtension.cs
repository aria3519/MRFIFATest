using System;
using System.Collections;
using UnityEngine;

namespace Jisu.Utils
{
    public static class InvokeExtension
    {
        /// <summary>Invokes an action after a given time.</summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="time">The time in seconds.</param>
        public static Coroutine Invoke(this MonoBehaviour monoBehaviour, Action action, float time)
        {
            return monoBehaviour.StartCoroutine(InvokeImplementation(action, time));
        }

        private static IEnumerator InvokeImplementation(Action action, float time)
        {
            yield return YieldInstructionCache.WaitForSeconds(time);
            action();
        }

        /// <summary>Invokes an action after a given time, then repeatedly every repeateRate seconds.</summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="time">The time in seconds.</param>
        /// <param name="repeatRate">The repeat rate in seconds.</param>
        public static Coroutine InvokeRepeating(this MonoBehaviour monoBehaviour, Action action, float time, float repeatRate)
        {
            return monoBehaviour.StartCoroutine(InvokeRepeatingImplementation(action, time, repeatRate));
        }

        private static IEnumerator InvokeRepeatingImplementation(Action action, float time, float repeatRate)
        {
            yield return YieldInstructionCache.WaitForSeconds(time);
            while (true)
            {
                action();
                yield return YieldInstructionCache.WaitForSeconds(repeatRate);
            }
        }
    }
}
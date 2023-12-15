using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public interface INotifiable<T>
    {
        T CurrentData { get; set; }
        event Action<T> OnDataChanged;
        event Action<T> OnDataChangedOnce;
        event Action<T, T> OnDataChangedDelta;
    }

    public static class NotifierExtension
    {
        public static void AddClampedValue(this Notifier<int> notifier, int value, int min, int max)
        {
            notifier.CurrentData = Mathf.Clamp(notifier.CurrentData + value, min, max);
        }

        public static void AddClampedValue(this Notifier<float> notifier, float value, float min, float max)
        {
            notifier.CurrentData = Mathf.Clamp(notifier.CurrentData + value, min, max);
        }
    }


    public class NotifierClass<T> : INotifiable<T> where T : class
    {
        public bool isEnabled;
        private T currentData;
        public T CurrentData
        {
            get => currentData;
            set
            {
                if (currentData == null || !currentData.Equals(value))
                {
                    if (isEnabled)
                    {
                        var lastData = currentData;
                        currentData = value;

                        OnDataChanged?.Invoke(currentData);
                        OnDataChangedOnce?.Invoke(currentData);
                        OnDataChangedDelta?.Invoke(lastData, currentData);
                        OnDataChangedOnce = null;
                    }
                    else
                    {
                        currentData = value;
                    }
                }

            }
        }
        public event Action<T> OnDataChanged;
        public event Action<T> OnDataChangedOnce;
        public event Action<T, T> OnDataChangedDelta;

        public bool IsSubscribed => (OnDataChanged != null && OnDataChanged.GetInvocationList().Length > 0) || (OnDataChangedOnce != null && OnDataChangedOnce.GetInvocationList().Length > 0);

        public NotifierClass()
        {
            isEnabled = true;
        }
    }

    public class Notifier<T> : INotifiable<T> where T : struct
    {
        public bool isEnabled;
        private T currentData;
        public T CurrentData
        {
            get => currentData;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(currentData, value))
                {
                    if (isEnabled)
                    {
                        var lastData = currentData;
                        currentData = value;

                        OnDataChanged?.Invoke(currentData);
                        OnDataChangedOnce?.Invoke(currentData);
                        OnDataChangedDelta?.Invoke(lastData, currentData);
                        OnDataChangedOnce = null;
                    }
                    else
                    {
                        currentData = value;
                    }
                }
            }
        }
        public event Action<T> OnDataChanged;
        public event Action<T> OnDataChangedOnce;
        public event Action<T, T> OnDataChangedDelta;

        public bool IsSubscribed => (OnDataChanged != null && OnDataChanged.GetInvocationList().Length > 0) || (OnDataChangedOnce != null && OnDataChangedOnce.GetInvocationList().Length > 0);

        public Notifier()
        {
            isEnabled = true;
        }
    }
}
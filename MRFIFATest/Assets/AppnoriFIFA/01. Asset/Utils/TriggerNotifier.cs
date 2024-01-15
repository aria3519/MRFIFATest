using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    [RequireComponent(typeof(Collider))]
    public class TriggerNotifier : MonoBehaviour
    {
        [SerializeField] List<string> targetTag;

        public readonly Notifier<bool> IsCollision = new();

        private bool isTargetSizeOne;

        private void Awake()
        {
            isTargetSizeOne = targetTag.Count == 1;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTargetSizeOne)
            {
                if(other.CompareTag(targetTag[0]))
                {
                    IsCollision.CurrentData = true;
                }
            }
            else
            {
                if (targetTag.Contains(other.tag))
                {
                    IsCollision.CurrentData = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isTargetSizeOne)
            {
                if (other.CompareTag(targetTag[0]))
                    IsCollision.CurrentData = false;
            }
            else
            {
                if (targetTag.Contains(other.tag))
                {
                    IsCollision.CurrentData = false;
                }
            }
        }

        public void AddActionOnDataChanged(Action<bool> newAction)
        {
            IsCollision.OnDataChanged += newAction;
        }

        public void AddActionOnDataChangedDelta(Action<bool, bool> newAction)
        {
            IsCollision.OnDataChangedDelta += newAction;
        }
    }
}

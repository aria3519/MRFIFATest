using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public static class AnimationCurveExtension
    {
        public static void EditKey(this AnimationCurve curve, in int index, in float time, in float value)
        {
            var targetKey = curve.keys[index];
            targetKey.time = time;
            targetKey.value = value;
            curve.MoveKey(index, targetKey);
        }

        public static void EditTime(this AnimationCurve curve, in int index, in float time)
        {
            var targetKey = curve.keys[index];
            targetKey.time = time;
            curve.MoveKey(index, targetKey);
        }

        public static void EditValue(this AnimationCurve curve, in int index, in float value)
        {
            var targetKey = curve.keys[index];
            targetKey.value = value;
            curve.MoveKey(index, targetKey);
        }
    }
}

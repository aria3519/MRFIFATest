using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if XR_INTERACTION_TOOLKIT

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Jisu.Utils
{
    public static class XRExtension
    {
        public static bool TryHapticImpulse(this XRController controller, float amp, float duration)
        {
            if (controller.inputDevice.TryGetHapticCapabilities(out var capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    return controller.inputDevice.SendHapticImpulse(0, amp, duration);
                }
            }
            return false;
        }
    }
}
#endif
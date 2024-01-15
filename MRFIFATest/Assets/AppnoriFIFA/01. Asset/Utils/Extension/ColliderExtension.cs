using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public static class ColliderExtension
    {
        public static bool ComputePenetration(this Collider staticCollider, Collider dynamicCollider, in Vector3 worldOffset, out Vector3 dir, out float dis)
        {
            return Physics.ComputePenetration(
                dynamicCollider, dynamicCollider.transform.position + worldOffset, dynamicCollider.transform.rotation,
                staticCollider, staticCollider.transform.position, staticCollider.transform.rotation,
                out dir, out dis);
        }

        public static bool ComputePenetration(this Collider staticCollider, Collider dynamicCollider, out Vector3 dir, out float dis)
        {
            return Physics.ComputePenetration(
                dynamicCollider, dynamicCollider.transform.position, dynamicCollider.transform.rotation,
                staticCollider, staticCollider.transform.position, staticCollider.transform.rotation,
                out dir, out dis);
        }
    }
}
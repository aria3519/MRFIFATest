using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public static class PhysicsExtension
    {
        /// <summary>
        /// 충돌 면과 스윙 방향을 고려한 충돌 후 충돌체의 속도 계산
        /// </summary>
        /// <param name="face"> 충돌 면</param>
        /// <param name="faceV"> 면의 속도 </param>
        /// <param name="hittedV"> 충돌체의 속도 </param>
        /// <param name="Friction"> 마찰(충돌체의 면에 대한 반사된 속도 계수 & 스윙 방향 Up 벡터 계수) </param>
        /// <returns></returns>
        public static Vector3 ToTouch(in Transform face, in Vector3 faceV, in Vector3 hittedV, in float Friction = 0.3f)
        {
            var faceV_Forward = Vector3.Dot(faceV, face.forward) * face.forward;
            var faceV_Up = (1f - Friction) * Vector3.Dot(faceV, face.up) * face.up;

            var hitted_Reflected = Friction * Vector3.Reflect(hittedV, face.forward);

            return faceV_Up + (faceV_Forward + hitted_Reflected);
        }

        public static class DragMotion
        {
            public static float GetTimeAtPlace(in float vZ, in float place, in float drag)
                     => (-1f / drag) * Mathf.Log(1f - (drag * place / vZ));
        }

        public static class Projectile
        {
            public static Vector2 GetVelocityAtoB(in Vector2 A, in Vector2 B, in float t, in float G = 9.81f)
            {
                var v = (B - A) / t;
                v.y += G * t * 0.5f;

                return v;
            }

            public static Vector3 GetVelocityAtoB(in Vector3 A, in Vector3 B, in float t, in float G = 9.81f)
            {
                var v = (B - A) / t;
                v.y += G * t * 0.5f;

                return v;
            }

            public static Vector2 GetVelocityAtoBInMaxHeight(in Vector2 A, in Vector2 B, in float maxHeight, out float t, in float G = 9.81f)
            {
                if (maxHeight < A.y || maxHeight < B.y)
                {
                    t = 0f;
                    return Vector2.zero;
                }

                var vY = Mathf.Sqrt(2f * G * (maxHeight - A.y));
                t = (vY + Mathf.Sqrt(Mathf.Pow(vY, 2) - 2f * G * (B.y - A.y))) / G;

                var v = (B - A) / t;
                v.y = vY;

                return v;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"> Start Position </param>
            /// <param name="B"> End Position </param>
            /// <param name="maxHeight"> Max height </param>
            /// <param name="t"> Total motion time </param>
            /// <param name="G"> Gravity constant </param>
            /// <returns></returns>
            public static Vector3 GetVelocityAtoBInMaxHeight(in Vector3 A, in Vector3 B, in float maxHeight, out float t, in float G = 9.81f)
            {
                if (maxHeight < A.y || maxHeight < B.y)
                {
                    t = 0f;
                    return Vector3.zero;
                }

                var vY = Mathf.Sqrt(2f * G * (maxHeight - A.y));
                t = (vY + Mathf.Sqrt(Mathf.Pow(vY, 2) - 2f * G * (B.y - A.y))) / G;

                var v = (B - A) / t;
                v.y = vY;

                return v;
            }

            public static Vector2 GetPosition(in Vector2 originPos, in Vector2 initVeloicty, in float t, in float G = 9.81f) 
                => originPos + new Vector2(
                    initVeloicty.x * t, 
                    initVeloicty.y * t - 0.5f * G * t * t);

            public static Vector3 GetPosition(in Vector3 originPos, in Vector3 initVeloicty, in float t, in float G = 9.81f) 
                => originPos + new Vector3(
                    initVeloicty.x * t, 
                    initVeloicty.y * t - 0.5f * G * t * t, 
                    initVeloicty.z * t);
            
            public static float GetHeight(in float originHeight, in float vY, in float t, in float G = 9.81f) 
                => originHeight + vY * t - 0.5f * G * t * t;
            
            public static Vector2 GetPositionWithDrag(in Vector2 originPos, in Vector2 initVelocity, in float t, in float drag, in float G = 9.81f) 
                => originPos + new Vector2(
                                (initVelocity.x / drag) * (1 - Mathf.Exp(-drag * t)),
                                (-G * t / drag) + (1 / drag) * (initVelocity.y + G / drag) * (1 - Mathf.Exp(-drag * t)));

            /// <summary>
            /// x = mVx / drag * (1 - e^(-bt / m));
            /// y = -mgt / drag + (m / drag) * (mg / drag) * (1 - e^(-bt / m));
            /// z = mVz / drag * (1 - e^(-bt / m));
            /// 에서 m을 1로 계산
            /// </summary>
            public static Vector3 GetPositionWithDrag(in Vector3 originPos, in Vector3 initVelocity, in float t, in float drag, in float G = 9.81f) 
                => originPos + new Vector3(
                                (initVelocity.x / drag) * (1 - Mathf.Exp(-drag * t)),
                                (-G * t / drag) + (1 / drag) * (initVelocity.y + G / drag) * (1 - Mathf.Exp(-drag * t)),
                                (initVelocity.z / drag) * (1 - Mathf.Exp(-drag * t)));
            
            public static float GetHeightWithDrag(in float originHeight, in float vY, in float t, in float drag, in float G = 9.81f) 
                => originHeight + (-G * t / drag) + (1 / drag) * (vY + G / drag) * (1 - Mathf.Exp(-drag * t));

            public static Vector3 GetTimeAtPlace(in Vector3 initVelocity, in Vector3 place, in float drag)
                => new Vector3(DragMotion.GetTimeAtPlace(initVelocity.x, place.x, drag),
                    // 임시
                    0f,
                    DragMotion.GetTimeAtPlace(initVelocity.z, place.z, drag));
        }
    }
}
using UnityEngine;
using System.Collections;

namespace Jisu.Utils
{
	public class CatmulRom
	{
		public static float CatmullRomLerp(float pre, float start, float end, float post, float t)
		{
#if UNITY_EDITOR
            var safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

            var a = 2f * start;
            var b = end - pre;
            var c = 2f * pre - 5f * start + 4f * end - post;
            var d = -pre + 3f * (start - end) + post;
            var tsqr = t * t;

			return (a + (b * t) + (c * tsqr) + (t * tsqr * d)) * 0.5f;
		}

		public static float CatmullRomLerp(float pre, float start, float end, float t)
		{
			// extrapolate the 4th position linearly
			var post = end + (end - start);

            return CatmullRomLerp(pre, start, post, t);
		}

		// Vector 2
		public static Vector3 CatmullRomLerp(in Vector2 pre, in Vector2 start, in Vector2 end, in Vector2 post, in float t)
		{
            var a = 2f * start;
            var b = end - pre;
            var c = 2f * pre - 5f * start + 4f * end - post;
            var d = -pre + 3f * (start - end) + post;
            var tsqr = t * t;

			return (a + (b * t) + (c * tsqr) + (t * tsqr * d)) * 0.5f;
		}

		public static Vector3 CatmullRomLerp(in Vector2 pre, in Vector2 start, in Vector2 end, in float t)
		{
            // extrapolate the 4th position linearly
            var post = end + (end - start);

            return CatmullRomLerp(pre, start, end, post, t);
		}

		// Vector 3
		public static Vector3 CatmullRomLerp(in Vector3 pre, in Vector3 start, in Vector3 end, in Vector3 post, in float t)
		{
			var a = 2f * start;
			var b = end - pre;
			var c = 2f * pre - 5f * start + 4f * end - post;
			var d = -pre + 3f * (start - end) + post;
			var tsqr = t * t;

			return (a + (b * t) + (c * tsqr) + (t * tsqr * d)) * 0.5f;
		}

		public static Vector3 CatmullRomLerp(in Vector3 pre, in Vector3 start, in Vector3 end, in float t)
		{
			// extrapolate the 4th position linearly
			var post = end + (end - start);

            return CatmullRomLerp(pre, start, end, post, t);
		}
	}

	public static class VectorExtension
    {
        public static bool IsNaN(this in Vector3 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);

        public static float Remap(this float value, in float x1, in float y1, in float x2, in float y2) => x2 + (y2 - x2) * ((value - x1) / (y1 - x1));
        public static float Remap(this float value, in (float, float) input, in (float, float) output) => output.Item1 + (output.Item2 - output.Item1) * ((value - input.Item1) / (input.Item2 - input.Item1));
        public static float Remap01(this float value, in float x1, in float y1) => value.Remap((x1, y1), (0f, 1f));

        public static Vector2 ToXZ(this in Vector3 vector) => new(vector.x, vector.z);
        public static Vector2 ToVector2(this in Vector3 vector) => vector;
        public static Vector3 ToVector3(this in Vector2 vector) => vector;
        public static Vector3 ToVector3(this in Vector2 vector, in float z) => new(vector.x, vector.y, z);
        public static Vector3 ToVector3FromXZ(this in Vector2 xzVector) => new(xzVector.x, 0, xzVector.y);
        public static Vector3 ToVector3FromXZ(this in Vector2 xzVector, in float y) => new(xzVector.x, y, xzVector.y);

        public static Vector2 AdaptX(this in Vector2 yVector, in float x) => new(x, yVector.y);
        public static Vector2 AdaptY(this in Vector2 xVector, in float y) => new(xVector.x, y);

        public static Vector3 AdaptX(this in Vector3 yzVector, in float x) => new(x, yzVector.y, yzVector.z);
        public static Vector3 AdaptY(this in Vector3 xzVector, in float y) => new(xzVector.x, y, xzVector.z);
        public static Vector3 AdaptZ(this in Vector3 xyVector, in float z) => new(xyVector.x, xyVector.y, z);

        public static float RandomRange(this in Vector2 vector) => Random.Range(vector.x, vector.y);

        public static Vector3 Round(this in Vector3 vector, in float scale) => new(
            Mathf.Round(vector.x / scale) * scale,
            Mathf.Round(vector.y / scale) * scale,
            Mathf.Round(vector.z / scale) * scale);

        public static Vector2 Decrease(this in Vector2 vector, in float amount) => new(
            Mathf.Sign(vector.x) * Mathf.Max(Mathf.Abs(vector.x) - amount, 0),
            Mathf.Sign(vector.y) * Mathf.Max(Mathf.Abs(vector.y) - amount, 0));
        public static Vector3 Decrease(this in Vector3 vector, in float amount) => new(
            Mathf.Sign(vector.x) * Mathf.Max(Mathf.Abs(vector.x) - amount, 0),
            Mathf.Sign(vector.y) * Mathf.Max(Mathf.Abs(vector.y) - amount, 0),
            Mathf.Sign(vector.z) * Mathf.Max(Mathf.Abs(vector.z) - amount, 0));

        //projection2D
        public static Vector2 ProjectionToXAxis(this in Vector2 vector, in Vector2 start, in float xAxisValue)
        {
            return new Vector2 (
                xAxisValue,
                (vector.y - start.y) * (xAxisValue - start.x) / (vector.x - start.x) + start.y);
        }
        public static Vector2 ProjectionToYAxis(this in Vector2 vector, in Vector2 start, in float yAxisValue)
        {
            return new Vector2 (
                (vector.x - start.x) * (yAxisValue - start.y) / (vector.y - start.y) + start.x,
                yAxisValue);
        }

        //projection3D
        public static Vector3 ProjectionToZAxis(this in Vector3 vector, in Vector3 start, in float zAxisValue)
        {
            return new Vector3 (
                (vector.x - start.x) * (zAxisValue - start.z) / (vector.z - start.z) + start.x,
                (vector.y - start.y) * (zAxisValue - start.z) / (vector.z - start.z) + start.y,
                zAxisValue);
        }
        public static Vector3 ProjectionToXAxis(this in Vector3 vector, in Vector3 start, in float xAxisValue)
        {
            return new Vector3 (
                xAxisValue,
                (vector.y - start.y) * (xAxisValue - start.x) / (vector.x - start.x) + start.y,
                (vector.z - start.z) * (xAxisValue - start.x) / (vector.x - start.x) + start.z);
        }
        public static Vector3 ProjectionToYAxis(this in Vector3 vector, in Vector3 start, in float yAxisValue)
        {
            return new Vector3 (
                (vector.x - start.x) * (yAxisValue - start.y) / (vector.y - start.y) + start.x,
                yAxisValue,
                (vector.z - start.z) * (yAxisValue - start.y) / (vector.y - start.y) + start.z);
        }

        public static Vector3 ToAbs(this in Vector3 vector)
        {
            return new Vector3 (Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        public static Vector3 IntersectionPoint(in Vector3 origin, in Vector3 target, in Vector3 center, in float radius)
        {
            var centerDir = center - origin;
            var forwardLength = Vector3.Project(centerDir, (target - origin).normalized);
            var orthogonal = Vector3.Distance(origin + forwardLength, center);

            if (orthogonal > radius)
                orthogonal = radius;

            var innerForward = Mathf.Sqrt((radius * radius) - (orthogonal * orthogonal));
            var dist = forwardLength.magnitude - innerForward;

            return origin + (target - origin).normalized * dist;
        }
	}
}

using UnityEngine;

namespace Util
{
    public struct Vector3Extension
    {
        // 3차 베지어 곡선 계산
        public static Vector3 Cubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;

            return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
        }
        
        public static bool CompareVector3(Vector3 a, Vector3 b, float tolerance = 0.001f)
        {
            return
                Mathf.Abs(a.x - b.x) < tolerance &&
                Mathf.Abs(a.y - b.y) < tolerance &&
                Mathf.Abs(a.z - b.z) < tolerance;
        }
    }
}


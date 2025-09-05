using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class TransformExtension
    {
        public static void LookAt2D(this Transform transform, Transform target) => LookAt2D(transform, target.position);
        public static void LookAt2D(this Transform transform, Vector3 targetPosition)
        {
            Vector2 dir = (targetPosition - transform.position).normalized;

            // 오른쪽을 기준으로 한 signed angle
            float angle = Vector2.SignedAngle(Vector2.right, dir);

            // flip 여부 판단
            bool flip = (angle > 90f) || (angle < -90f);

            // Y축 회전
            float yRot = flip ? 180f : 0f;

            // Z축 회전 (flip일 때는 180 기준으로 보정)
            float zRot = flip ? Mathf.DeltaAngle(angle, 180f) : angle;

            // 오일러 회전에 의해 z값 회전이 안먹히고 x값이 변화하는 것을 방지하기 위해 y를 먼저 회전후 z 회전
            transform.localRotation = Quaternion.AngleAxis(yRot, Vector3.up) * 
                                      Quaternion.AngleAxis(zRot, Vector3.forward);
        }
    }
}
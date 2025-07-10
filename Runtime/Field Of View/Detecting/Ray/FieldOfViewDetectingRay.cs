using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Weariness.Noesis.FieldOfView
{
    public class FieldOfViewDetectingRay : MonoBehaviour
    {
        public FieldOfViewDetectingRayData data = new();

        public bool IsInView(Vector3 targetPos)
        {
            Vector3 dirToTarget = (targetPos - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(dirToTarget);

            float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            bool inHorizontal = (horizontalAngle >= data.xAngle.Min) && (horizontalAngle <= data.xAngle.Max);
            bool inVertical = (verticalAngle >= data.yAngle.Min) && (verticalAngle <= data.yAngle.Max);

            return inHorizontal && inVertical;
        }

        public bool RayDetecting(Vector3 originPosition, Transform targetTransform, Mesh targetMesh)
        {
            // 감지 범위보다 멀면
            if (Vector3.Distance(originPosition, targetTransform.position) > data.range) return false;
            // 감지 시야 내에 있지 않다면
            if (IsInView(targetTransform.position) == false) return false;

            // 1. Mesh의 local bounds 얻기
            var bounds = targetMesh.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 center = bounds.center;

            // 2. 네 꼭짓점 (XY평면 기준, Z는 max/min)
            //   (min.x, max.y, center.z) - 왼쪽 위
            //   (max.x, max.y, center.z) - 오른쪽 위
            //   (min.x, min.y, center.z) - 왼쪽 아래
            //   (max.x, min.y, center.z) - 오른쪽 아래
            List<Vector3> worldPointList = new();
            // --- 월드 변환 ---
            worldPointList.Add(targetTransform.TransformPoint(center));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(min.x, max.y, center.z)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(max.x, max.y, center.z)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(min.x, min.y, center.z)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(max.x, min.y, center.z)));

            worldPointList.Add(targetTransform.TransformPoint(new Vector3(center.x, max.y, min.x)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(center.x, max.y, max.x)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(center.x, min.y, min.x)));
            worldPointList.Add(targetTransform.TransformPoint(new Vector3(center.x, min.y, max.x)));

            bool LineCast(Vector3 start, Vector3 end)
            {
                var value = Physics.Linecast(start, end, out var hit);
                if (value)
                {
                    var hitTransform = hit.collider.transform;
                    var isSameTransform = targetTransform == hitTransform || hitTransform.IsChildOf(targetTransform) || targetTransform.IsChildOf(hitTransform);
                    value = isSameTransform;
                }

#if UNITY_EDITOR
                if (data.isRayDebug)
                    Debug.DrawLine(start, end, value ? Color.green : Color.red, 1f);
#endif

                return value;
            }
            foreach (var endPoint in worldPointList)
            {
                if (LineCast(originPosition, endPoint))
                    return true;
            }

            return false;
        }
    }
}

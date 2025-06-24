using System.Collections.Generic;
using UnityEngine;

namespace Weariness.AI.Detecting.Field_Of_View
{
    public partial class FieldOfViewDetectingController : MonoBehaviour
    {
        public FieldOfViewDetecting_CommonData commonData;

        public bool IsInView(Vector3 targetPos)
        {
            Vector3 dirToTarget = (targetPos - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(dirToTarget);

            float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            bool inHorizontal = (horizontalAngle >= commonData.xAngle.Min) && (horizontalAngle <= commonData.xAngle.Max);
            bool inVertical   = (verticalAngle   >= commonData.yAngle.Min) && (verticalAngle   <= commonData.yAngle.Max);

            return inHorizontal && inVertical;
        }
    }
    public partial class FieldOfViewDetectingController
    {
        public FieldOfViewDetecting_RayData rayDetectingData;

        public bool RayDetecting(GameObject targetObject)
        {
            var meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return RayDetecting(transform.position, targetObject.transform, meshFilter.sharedMesh);
            }

            Debug.LogWarning("Mesh, Collider 등 시야로 감지할 수 있는 어떤 방법도 없습니다.");
            return false;
        }

        public bool RayDetecting(Vector3 originPosition, Transform targetTransform, Mesh targetMesh)
        {
            // 감지 범위보다 멀면
            if (Vector3.Distance(originPosition, targetTransform.position) > commonData.range) return false;
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
                if(commonData.isRayDebug)
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
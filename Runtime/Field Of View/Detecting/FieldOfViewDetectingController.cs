using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Noesis.FieldOfView
{
    public partial class FieldOfViewDetectingController : MonoBehaviour 
    {
        [SerializeField] private FieldOfViewDetectingType detectingType = FieldOfViewDetectingType.MeshRay;
        public FieldOfViewDetectingData data;
        public FieldOfViewDetectingRay detectingRay;
        public FieldOfViewDetectingCamera detectingCamera;

        public bool RayDetecting(GameObject targetObject)
        {
            switch (detectingType)
            {
                case FieldOfViewDetectingType.MeshRay:
                    var meshFilter = targetObject.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                        return detectingRay.RayDetecting(transform.position, targetObject.transform, meshFilter.sharedMesh);
                    break;
                case FieldOfViewDetectingType.ColliderRay:
                    break;
                case FieldOfViewDetectingType.BoxColliderRay:
                    break;
                case FieldOfViewDetectingType.SphereColliderRay:
                    break;
                case FieldOfViewDetectingType.CapsuleColliderRay:
                    break;

                case FieldOfViewDetectingType.Camera:
                    var value = detectingCamera.DetectingTargetThresHold(targetObject);
                    // 탐지 성공
                    if(1f - value < data.detectingThreshold)
                    {
                        return true;
                    }
                    break;
                default:
                    Debug.LogWarning("Mesh, Collider 등 시야로 감지할 수 있는 어떤 방법도 없습니다.");
                    break;
            }
            return false;
        }
    }
}
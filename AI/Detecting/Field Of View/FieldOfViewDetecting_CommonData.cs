using System;
using UnityEngine;
using Weariness.Util;

namespace Weariness.AI.Detecting.Field_Of_View
{
    [Serializable]
    public class FieldOfViewDetecting_CommonData
    {
#if UNITY_EDITOR
        public bool isRayDebug = true;
#endif
        [Space]
        public float range = 100f; // 탐지 범위
        public MinMax<float> xAngle = new(-60,60); // 탐지 각도 (x축)
        public MinMax<float> yAngle = new(-60,60); // 탐지 각도 (y축)
        [Tooltip("탐지 대상 레이어 지정")] public LayerMask targetLayer = int.MaxValue;
    }
}
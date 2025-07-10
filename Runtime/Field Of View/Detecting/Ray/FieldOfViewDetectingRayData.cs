using System;
using UnityEngine;
using Weariness.Util;

namespace Weariness.Noesis.FieldOfView
{
    public class FieldOfViewDetectingRayData
    {
#if UNITY_EDITOR
        public bool isRayDebug = true;
#endif
        [Space]
        public float range = 100f; // 탐지 범위
        public MinMax<float> xAngle = new(-60, 60); // 탐지 각도 (x축)
        public MinMax<float> yAngle = new(-60, 60); // 탐지 각도 (y축)
    }
}
using System;
using UnityEngine;
using Weariness.Util;

namespace Weariness.Noesis.FieldOfView
{
    [Serializable]
    public class FieldOfViewDetectingData
    {
        [Range(0f, 1f)] public float detectingThreshold = 1.0f; // 1 : 무조건 탐지, 0 : 탐지 불가

        [Tooltip("탐지 대상 레이어 지정")] public LayerMask targetLayer = int.MaxValue;
    }
}
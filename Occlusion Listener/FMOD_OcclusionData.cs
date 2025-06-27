using System;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    [Serializable]
    public class FMOD_OcclusionData
    {
        [Range(0f, 10f)]
        public float SoundOcclusionWidening = 1f;
        [Range(0f, 10f)]
        public float PlayerOcclusionWidening = 1f;
        public LayerMask OcclusionLayer = int.MaxValue;
    }
}
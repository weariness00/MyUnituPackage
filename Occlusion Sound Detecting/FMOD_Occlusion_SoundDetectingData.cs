using System;
using UnityEngine;

namespace Weariness.FMOD.Detecting
{
    [AddComponentMenu("FMOD Studio/Occlusion/Sound Detecting Data")]
    public class FMOD_Occlusion_SoundDetectingData : MonoBehaviour
    {
        public float range = 10f; // 탐지 범위
        public float threshold = 0.5f; // 소리 감지 임계값

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
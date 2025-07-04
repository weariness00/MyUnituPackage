using System;
using FMOD;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Weariness.FMOD.Occlusion;

namespace Weariness.FMOD.Occlusion.Detecting
{
    [RequireComponent(typeof(FMOD_Occlusion_SoundDetectingData))]
    [AddComponentMenu("FMOD Studio/Occlusion/Sound Detecting Controller")]
    public class FMOD_Occlusion_SoundDetectingController : MonoBehaviour
    {
        public FMOD_Occlusion_SoundDetectingMode detectingMode = FMOD_Occlusion_SoundDetectingMode.Ray; // 탐지 모드
        [SerializeField] private FMOD_Occlusion_SoundDetectingData detectingData;
        [FormerlySerializedAs("rayOcclusionData")] [SerializeField] private FMOD_OcclusionRayData occlusionRayData;
        private float min, max;

        private void Reset()
        {
            if (detectingData == null)
                detectingData = GetComponent<FMOD_Occlusion_SoundDetectingData>();

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void VirtualListenerUpdate()
        {
            // 가상 리스너의 위치를 매우 먼 곳으로 보낸다.
            var attributes3D = transform.To3DAttributes();
            attributes3D.position = new VECTOR()
            {
                x = 9999999f,
                y = 9999999f,
                z = 9999999f
            };
            RuntimeManager.StudioSystem.setListenerAttributes(1, attributes3D);
        }
        
        public bool TryAnyDetecting(out Transform emitterTransform)
        {
            if (detectingMode == FMOD_Occlusion_SoundDetectingMode.Ray) return TryAnyDetectingToRay(out emitterTransform);

            emitterTransform = null;
            return false;
        }

        private bool TryAnyDetectingToRay(out Transform emitterTransform)
        {
            emitterTransform = null;
#if WEARINESS_FMOD_OCCLUSION
            var playingEmitters = FMOD_Occlusion_System.Instance.GetPlayingEmitters();
            var attributes3D = transform.To3DAttributes();

            foreach (var emitter in playingEmitters)
            {
                // 3d 음향이 아니면 검사 안함
                emitter.EventDescription.is3D(out var is3D);
                if (is3D == false) continue;

                var instance = emitter.EventInstance;
                instance.getMinMaxDistance(out min, out max);
                var dis = Vector3.Distance(emitter.transform.position, transform.position);
                // 실재 거리는 들릴 수 있는 최대 거리를 생각한다.
                if (dis - max > detectingData.range) continue;

                // 감지 거리는 사운드가 들릴 수 있는 범위임으로 실제 음악이 퍼져나가는 범위가 감지 범위 내에 있으면 들리는 것으로 판단하게 해야한다.
                // 그래서 감지 범위 내에서 소리가 들릴 경우 어느정도 가까운지를 임의적으로 위치를 이동시켜 계산한다.
                var realPosition = Mathf.Abs(dis) < 0.001f ? emitter.transform.position : Vector3.Lerp(emitter.transform.position, transform.position, Mathf.Abs(dis - detectingData.range) / dis);
                attributes3D.position = new VECTOR
                {
                    x = realPosition.x,
                    y = realPosition.y,
                    z = realPosition.z
                };
                FMODUnity.RuntimeManager.StudioSystem.setListenerAttributes(1, attributes3D);
                instance.setListenerMask((uint)(1 << StudioListener.ListenerCount)); // 리스트너 갯수 + 1 번째가 가상 리스너
                instance.getVolume(out var volume, out var finalvolume);
                instance.setListenerMask(uint.MaxValue);

                // 거리에 따라 감쇠된 소리에 오쿨루젼 계산을 포함
                finalvolume *= 1f - occlusionRayData.OccludeBetween(emitter.transform.position, transform.position);

                // 실제 감쇠된 소리가 감지가능한 소리크기보다 작은지 검사
                if (finalvolume < (1f - detectingData.threshold)) continue;
                
                VirtualListenerUpdate();
                emitterTransform = emitter.transform;
                return true;
            }

            VirtualListenerUpdate();
#endif
            return false;
        }
    }
}
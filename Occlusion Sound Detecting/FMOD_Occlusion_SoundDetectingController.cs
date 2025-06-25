using System;
using FMOD;
using FMODUnity;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD.Detecting
{
    [RequireComponent(typeof(FMOD_Occlusion_SoundDetectingData))]
    public class FMOD_Occlusion_SoundDetectingController : MonoBehaviour
    {
        private FMOD_Occlusion_SoundDetectingData detectingData;

        private float min, max;

        private void Reset()
        {
            if (detectingData == null)
                detectingData = GetComponent<FMOD_Occlusion_SoundDetectingData>();
            
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            if (detectingData == null)
                detectingData = GetComponent<FMOD_Occlusion_SoundDetectingData>();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void FixedUpdate()
        {
            GetTryAnyDetecting(out var emitterTransform);
        }

        public bool GetTryAnyDetecting(out Transform emitterTransform)
        {
            var playingEmitters = FMOD_Occlusion_System.Instance.GetPlayingEmitters();
            var attributes3D = transform.To3DAttributes();

            foreach (var emitter in playingEmitters)
            {
                // 3d 음향이 아니면 검사 안함
                emitter.EventDescription.is3D(out var is3D);
                if(is3D == false) continue;
                
                var instance = emitter.EventInstance;
                instance.getMinMaxDistance(out min, out max);
                var dis = Vector3.Distance(emitter.transform.position, transform.position);
                // 실재 거리는 들릴 수 있는 최대 거리를 생각한다.
                if(dis - max > detectingData.range) continue;

                // 감지 거리는 사운드가 들릴 수 있는 범위임으로 실제 음악이 퍼져나가는 범위가 감지 범위 내에 있으면 들리는 것으로 판단하게 해야한다.
                // 그래서 감지 범위 내에서 소리가 들릴 경우 어느정도 가까운지를 임의적으로 위치를 이동시켜 계산한다.
                var realPosition = Vector3.Lerp(emitter.transform.position, transform.position, (dis - detectingData.range) / dis);
                attributes3D.position = new VECTOR{
                    x = realPosition.x,
                    y = realPosition.y,
                    z = realPosition.z
                };
                FMODUnity.RuntimeManager.StudioSystem.setListenerAttributes(StudioListener.ListenerCount - 1, attributes3D);
                instance.setListenerMask(1 << 1);
                instance.getVolume(out var volume);
                
                // 실제 감쇠된 소리가 감지가능한 소리크기보다 작은지 검사
                if (volume < (1f - detectingData.threshold)) continue;
                
                emitterTransform = emitter.transform;
                return true;
            }
        
            emitterTransform = null;
            return false;
        }
    }
}
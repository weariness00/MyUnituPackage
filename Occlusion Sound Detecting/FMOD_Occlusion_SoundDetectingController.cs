using System;
using FMODUnity;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD.Detecting
{
    [RequireComponent(typeof(FMOD_Occlusion_SoundDetectingData))]
    public class FMOD_Occlusion_SoundDetectingController : MonoBehaviour
    {
        private FMOD_Occlusion_SoundDetectingData detectingData;

        private void Reset()
        {
            if (detectingData == null)
                detectingData = GetComponent<FMOD_Occlusion_SoundDetectingData>();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnValidate()
        {
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
            var playingEmitters = FMOD_Occlusion_SoundDetectingSystem.Instance.GetPlayingEmitters();
            foreach (var emitter in playingEmitters)
            {
                // 3d 음향이 아니면 검사 안함
                emitter.EventDescription.is3D(out var is3D);
                if(is3D == false) continue;
                
                var dis = Vector3.Distance(emitter.transform.position, transform.position);
                if(dis > detectingData.range) continue;
                FMODUnity.RuntimeManager.StudioSystem.setListenerAttributes(StudioListener.ListenerCount - 1, transform.To3DAttributes());
        
                var instance = emitter.EventInstance;
                instance.setListenerMask(1 << 1);
                instance.getVolume(out var volume);
                if (volume < (1f - detectingData.threshold)) continue;
                
                emitterTransform = emitter.transform;
                return true;
            }
        
            emitterTransform = null;
            return false;
        }
    }
}
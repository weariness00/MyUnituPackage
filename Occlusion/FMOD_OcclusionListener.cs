using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Weariness.FMOD.Occlusion
{
    
    public class FMOD_OcclusionListener : StudioListener
    {
        public FMOD_OcclusionData occlusionData;
        private float min, max;

        public void FixedUpdate()
        {
            SoundOcclusion();
        }

        private void SoundOcclusion()
        {
            var playingEmitters = FMOD_Occlusion_System.Instance.GetPlayingEmitters();
            foreach (var emitter in playingEmitters)
            {
                var desc = emitter.EventDescription;
                desc.getMinMaxDistance(out min, out max);

                // 사운드가 들리는 범위 내라면
                if (Vector3.Distance(transform.position, emitter.transform.position) <= max)
                {
                    var occlusionValue = FMOD_OcclusionUtil.OccludeBetween(emitter.transform.position, transform.position, occlusionData);
                    SetOcclusionParameter(emitter.EventInstance, 1f - occlusionValue);
                }
            }
        }
        
        private void SetOcclusionParameter(EventInstance instance, float value)
        {
            if(instance.getParameterByName("Occlusion", out var param) == RESULT.OK)
            {
                if (Math.Abs(param - value) < 0.01f) return; // 값이 거의 같으면 업데이트하지 않음
                instance.setParameterByName("Occlusion", value);
            }
            else
            {
                // Occlusion이라는 파라미터가 없으면 단순히 볼륨 조절
                instance.setVolume(value);
            }
        }
    }
}
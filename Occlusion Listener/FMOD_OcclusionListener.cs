using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Weariness.FMOD.Occlusion
{
    
    public class FMOD_OcclusionListener : StudioListener
    {
        [SerializeField] private FMOD_OcclusionMode occlusionMode = FMOD_OcclusionMode.Ray;
        
        public FMOD_OcclusionCameraData occlusionCameraData;
        public FMOD_OcclusionRayData occlusionRayData;

        public void FixedUpdate()
        {
            UpdateSoundOcclusion();
        }

        private void UpdateSoundOcclusion()
        {
            if(occlusionMode == FMOD_OcclusionMode.Camera)
            {
                CameraOcclusion();
            }
            else if(occlusionMode == FMOD_OcclusionMode.Ray)
            {
                RayOcclusion();
            }
        }

        private void RayOcclusion()
        {
            float min, max;
#if WEARINESS_FMOD_OCCLUSION
            var playingEmitters = FMOD_Occlusion_System.Instance.GetPlayingEmitters();
            foreach (var emitter in playingEmitters)
            {
                var desc = emitter.EventDescription;
                desc.getMinMaxDistance(out min, out max);

                // 사운드가 들리는 범위 내라면
                if (Vector3.Distance(transform.position, emitter.transform.position) <= max)
                {
                    var occlusionValue = occlusionRayData.OccludeBetween(emitter.transform.position, transform.position);
                    FMOD_OcclusionUtil.SetOcclusionParameter(emitter.EventInstance, occlusionValue);
                }
            }
#endif
        }

        private void CameraOcclusion()
        {
            float min, max;
#if WEARINESS_FMOD_OCCLUSION
            var playingEmitters = FMOD_Occlusion_System.Instance.GetPlayingEmitters();
            foreach (var emitter in playingEmitters)
            {
                var desc = emitter.EventDescription;
                desc.getMinMaxDistance(out min, out max);

                // 사운드가 들리는 범위 내라면
                if (Vector3.Distance(transform.position, emitter.transform.position) <= max)
                {
                    var occlusionValue = occlusionCameraData.OcclusionCamera.GetOcclusionValue(emitter);
                    FMOD_OcclusionUtil.SetOcclusionParameter(emitter.EventInstance, occlusionValue);
                }
            }
#endif
        }
    }
}
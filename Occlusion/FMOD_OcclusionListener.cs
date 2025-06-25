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
                    var occlusionValue = OccludeBetween(emitter.transform.position, transform.position);
                    SetOcclusionParameter(emitter.EventInstance, 1f - occlusionValue);
                }
            }
        }

        private float OccludeBetween(Vector3 sound, Vector3 listener)
        {
            var soundOcclusionWidening = occlusionData.SoundOcclusionWidening;
            var playerOcclusionWidening = occlusionData.PlayerOcclusionWidening;
            
            Vector3 soundLeft = CalculatePoint(sound, listener, soundOcclusionWidening, true);
            Vector3 soundRight = CalculatePoint(sound, listener, soundOcclusionWidening, false);

            Vector3 soundAbove = new Vector3(sound.x, sound.y + soundOcclusionWidening, sound.z);
            Vector3 soundBelow = new Vector3(sound.x, sound.y - soundOcclusionWidening, sound.z);

            Vector3 listenerLeft = CalculatePoint(listener, sound, playerOcclusionWidening, true);
            Vector3 listenerRight = CalculatePoint(listener, sound, playerOcclusionWidening, false);

            Vector3 listenerAbove = new Vector3(listener.x, listener.y + playerOcclusionWidening * 0.5f, listener.z);
            Vector3 listenerBelow = new Vector3(listener.x, listener.y - playerOcclusionWidening * 0.5f, listener.z);

            int occlusionCount = 0;
            void CastLine(Vector3 start, Vector3 end)
            {
                RaycastHit hit;
                Physics.Linecast(start, end, out hit, occlusionData.OcclusionLayer);
                if (hit.collider != null) occlusionCount++;
#if UNITY_EDITOR
                if (occlusionData.PlayerOcclusionWidening == 0f || occlusionData.SoundOcclusionWidening == 0f)
                    Debug.DrawLine(start, end, Color.blue);
                else
                    Debug.DrawLine(start, end, hit.collider ? Color.red : Color.green);
#endif
            }
            CastLine(soundLeft, listenerLeft);
            CastLine(soundLeft, listener);
            CastLine(soundLeft, listenerRight);

            CastLine(sound, listenerLeft);
            CastLine(sound, listener);
            CastLine(sound, listenerRight);

            CastLine(soundRight, listenerLeft);
            CastLine(soundRight, listener);
            CastLine(soundRight, listenerRight);

            CastLine(soundAbove, listenerAbove);
            CastLine(soundBelow, listenerBelow);

            return occlusionCount / 11f;
        }

        private Vector3 CalculatePoint(Vector3 a, Vector3 b, float m, bool posOrneg)
        {
            float x;
            float z;
            float n = Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
            float mn = (m / n);
            if (posOrneg)
            {
                x = a.x + (mn * (a.z - b.z));
                z = a.z - (mn * (a.x - b.x));
            }
            else
            {
                x = a.x - (mn * (a.z - b.z));
                z = a.z + (mn * (a.x - b.x));
            }

            return new Vector3(x, a.y, z);
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
#if WEARINESS_FMOD_OCCLUSION
using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Weariness.Util;   

namespace Weariness.FMOD
{
    public class FMOD_Occlusion_System : Singleton<FMOD_Occlusion_System>
    {
        [NonSerialized] private FMOD_Occlusion_SystemData data = new();
        private int prevListenerCount = 0;

        public void Start()
        {
            prevListenerCount = StudioListener.ListenerCount + 1;
            RuntimeManager.StudioSystem.setNumListeners(prevListenerCount); // 예: N = 2, 3 등
        }

        public void Update()
        {
            if (prevListenerCount != StudioListener.ListenerCount + 1)
            {
                prevListenerCount = StudioListener.ListenerCount + 1;
                RuntimeManager.StudioSystem.setNumListeners(prevListenerCount); // 예: N = 2, 3 등
            }

            InitVirtualListenerDAttributes();
        }
        public void InitVirtualListenerDAttributes()
        {
            // 가상 리스너의 위치를 매우 먼 곳으로 보낸다.
            var attributes3D = transform.To3DAttributes();
            attributes3D.position = new VECTOR()
            {
                x = 9999999f,
                y = 9999999f,
                z = 9999999f
            };
            RuntimeManager.StudioSystem.setListenerAttributes(prevListenerCount - 1, attributes3D);
        }
        
        public void FixedUpdate()
        {
            foreach (var emitter in data.emitterHashSet)
            {
                emitter.EventInstance.getPlaybackState(out var state);
                if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.SUSTAINING)
                {
                    data.playEmiiterHashSet.Add(emitter);
                }
                else
                {
                    data.playEmiiterHashSet.Remove(emitter);
                }
            }
        }

        public void AddEmitter(StudioEventEmitter emitter)
        {
            if (emitter != null)
            {
                data.emitterHashSet.Add(emitter);
            }
        }
        
        public void RemoveEmitter(StudioEventEmitter emitter)
        {
            if (emitter != null && data.emitterHashSet.Contains(emitter))
            {
                data.emitterHashSet.Remove(emitter);
                data.playEmiiterHashSet.Remove(emitter);
            }
        }
        
        public void CleanUp()
        {
            data.emitterHashSet.Clear();
            data.playEmiiterHashSet.Clear();
        }

        public HashSet<StudioEventEmitter> GetPlayingEmitters() => data.playEmiiterHashSet;
    }
}
#endif
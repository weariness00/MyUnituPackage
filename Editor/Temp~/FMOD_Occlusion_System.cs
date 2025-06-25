using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Weariness.Util;   

namespace Weariness.FMOD
{
    public class FMOD_Occlusion_System : Singleton<FMOD_Occlusion_System>
    {
        [NonSerialized] private FMOD_Occlusion_SystemData data = new();

        public void Start()
        {
            FMODUnity.RuntimeManager.StudioSystem.setNumListeners(2); // 예: N = 2, 3 등
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
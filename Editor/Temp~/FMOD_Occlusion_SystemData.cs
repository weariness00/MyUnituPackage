using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace Weariness.FMOD
{
    public class FMOD_Occlusion_SystemData
    {
        public HashSet<StudioEventEmitter> emitterHashSet = new HashSet<StudioEventEmitter>();
        public HashSet<StudioEventEmitter> playEmiiterHashSet = new HashSet<StudioEventEmitter>();

        public void FixedUpdate()
        {
            foreach (var emitter in emitterHashSet)
            {
                emitter.EventInstance.getPlaybackState(out var state);
                if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.SUSTAINING)
                {
                    playEmiiterHashSet.Add(emitter);
                }
            }
        }
    }
}
#if WEARINESS_FMOD_OCCLUSION

using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace Weariness.FMOD
{
    public class FMOD_Occlusion_SystemData
    {
        public HashSet<StudioEventEmitter> emitterHashSet = new HashSet<StudioEventEmitter>();
        public HashSet<StudioEventEmitter> playEmiiterHashSet = new HashSet<StudioEventEmitter>();
    }
}

#endif
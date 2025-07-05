using System;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    [Serializable]
    public class FMOD_OcclusionCameraData
    {
        public Camera occlusionCamera;
        public RenderTexture occlusionTexture;
    }
}
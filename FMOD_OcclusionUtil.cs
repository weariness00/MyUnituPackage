using System;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Weariness.FMOD.Occlusion
{
    public static class FMOD_OcclusionUtil
    {
        public static void SetOcclusionParameter(EventInstance instance, float value)
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

        public static float GetOcclusionParameter(EventInstance instance)
        {
            if (instance.getParameterByName("Occlusion", out var param) == RESULT.OK)
            {
                return (float) param;
            }

            return 0f;
        }
    }
}
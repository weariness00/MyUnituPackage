using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    [CreateAssetMenu(fileName = "FMOD Occlusion Setting", menuName = "FMOD/Occlusion/Setting SO", order = 0)]
    public class FMOD_OcclusionSO : ScriptableObject
    {
        public static bool IsLoad => SettingProviderHelper.isLoad;
        public static FMOD_OcclusionSO Instance => SettingProviderHelper.setting;
        
        public Shader occlusionShader;
        public Material occlusionMaterialInstance;
        public ComputeShader occlusionTextureSampling;
        
        [Space]
        public Material volumeChannelMaterial;
    }
}
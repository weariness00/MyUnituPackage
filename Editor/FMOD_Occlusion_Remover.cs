using UnityEditor;

namespace Weariness.FMOD.Occlusion
{
    public class FMOD_Occlusion_Remover
    {
        [MenuItem("FMOD/Occlusion/Init Occlusion System Data")]
        public static void InitOcclusionSystemData()
        {
            FMOD_ScriptGenerator.Init();
        }
        
        [MenuItem("FMOD/Occlusion/Remove Occlusion System Data")]
        public static void RemoveOcclusionSystemData()
        {
            FMOD_ScriptGenerator.RemoveSymbol();
        }
    }
}
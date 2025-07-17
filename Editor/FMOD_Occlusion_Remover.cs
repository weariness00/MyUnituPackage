using UnityEditor;
using Weariness.FMOD.Occlusion.Editor;

namespace Weariness.FMOD.Occlusion
{
    public class FMOD_Occlusion_Remover
    {
        [MenuItem("FMOD/Occlusion/Init Occlusion System Data")]
        public static void InitOcclusionSystemData()
        {
            FMOD_ScriptGenerator.Init();
        }
    }
}
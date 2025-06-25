using UnityEditor;

namespace Weariness.FMOD
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
            var targetGroup = BuildTargetGroup.Standalone;
            var symbol = "WEARINESS_FMOD_OCCLUSION";
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));
            if (defineList.Contains(symbol))
            {
                defineList.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
            }
        }
    }
}
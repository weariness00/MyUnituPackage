using UnityEditor;
using Weariness.Util;

namespace Weariness.FMOD.Occlusion.Editor
{
    [InitializeOnLoad]
    public static class SymbolInitializer
    {
        public static readonly string SymbolName = "WEARINESS_FMOD_OCCLUSION";
        static SymbolInitializer() => Init();
        public static void Init()
        {
            const string key = "WEARINESS_FMOD_OCCLUSION_INIT";
            if (DataPrefs.GetBool(key, false) == false)
            {
                AddSymbol();
                DataPrefs.SetBool(key, true);
            }
        }
        
        [MenuItem("FMOD/Occlusion/Add Occlusion System", false, 3)]
        public static void AddOcclusionSystemData()
        {
            AddSymbol();
        }
        
        [MenuItem("FMOD/Occlusion/Remove Occlusion System", false, 4)]
        public static void RemoveOcclusionSystemData()
        {
            RemoveSymbol();
        }
        
        private static void AddSymbol()
        {
            var targetGroup = BuildTargetGroup.Standalone;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));
            if (!defineList.Contains(SymbolName))
            {
                defineList.Add(SymbolName);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
            }
        }

        public static void RemoveSymbol()
        {
            var targetGroup = BuildTargetGroup.Standalone;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));
            if (defineList.Contains(SymbolName))
            {
                defineList.Remove(SymbolName);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
            }
        }
    }
}
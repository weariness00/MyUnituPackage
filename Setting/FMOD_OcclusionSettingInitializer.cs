using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Weariness.FMOD.Occlusion
{
    [InitializeOnLoad]
    public class FMOD_OcclusionSettingInitializer
    {
        static FMOD_OcclusionSettingInitializer() => EditorApplication.delayCall += Init;
        
        private static void Init()
        {
            InitOcclusionSO();
        }

        private static void InitOcclusionSO()
        {
            if (SettingProviderHelper.setting == null)
            {
                var so = AssetDatabase.LoadAssetAtPath<FMOD_OcclusionSO>("Packages/com.weariness.fmod.occlusion/Setting/FMOD Occlusion Setting.asset");
                if (so == null) so = AssetDatabase.LoadAssetAtPath<FMOD_OcclusionSO>("Assets/Scripts/Setting/FMOD Occlusion Setting.asset");
                SettingProviderHelper.setting = so;
                SettingProviderHelper.Save();
            }
            
            if(SettingProviderHelper.setting == null)
                Debug.LogError("FMOD Occlusion Setting SO가 존재하지 않습니다. 'Packages/com.weariness.fmod.occlusion/Setting/FMOD Occlusion Setting.asset' 경로에 파일이 있는지 확인하세요.");
            else
                Debug.Log("FMOD Occlusion Setting SO 초기화 완료: " + SettingProviderHelper.setting.name);
        }
        
    }
}

#endif

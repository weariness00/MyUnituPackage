using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Weariness.Util;

namespace Weariness.FMOD.Occlusion
{
#if UNITY_EDITOR
    
    public static class Provider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            var provider = new SettingsProvider(
                "Project/FMOD/Occlusion",
                SettingsScope.Project,
                new []{"FMOD", "Occlusion"})
            {
                guiHandler = searchContext =>
                {
                    EditorGUILayout.LabelField("FMOD Occlusion", EditorStyles.boldLabel);
                    var setting = SettingProviderHelper.setting = (FMOD_OcclusionSO)EditorGUILayout.ObjectField(
                        $"FMOD Occlusion Setting",
                        SettingProviderHelper.setting,
                        typeof(FMOD_OcclusionSO),
                        false
                    );

                    if (setting != null)
                    {
                        UnityEditor.Editor.CreateEditor(setting).OnInspectorGUI();
                    }
                    
                    // setting이 변경되었을 경우 Save() 호출
                    if (GUI.changed)
                    {
                        SettingProviderHelper.Save();
                    }
                }
            };

            return provider;
        }
    }
    
#endif

    public static class SettingProviderHelper
    {
        public static FMOD_OcclusionSO setting;

        private static readonly string SettingKey = nameof(FMOD_OcclusionSO);

#if UNITY_EDITOR
        static SettingProviderHelper()
        {
            Load();
        }

        public static void Save()
        {
            if (setting != null)
            {
                string path = AssetDatabase.GetAssetPath(setting);
                DataPrefs.SetString(SettingKey, path);
            }
        }

        public static void Load()
        {
            if (DataPrefs.HasKey(SettingKey))
            {
                string settingPath = DataPrefs.GetString(SettingKey, string.Empty);
                setting = AssetDatabase.LoadAssetAtPath<FMOD_OcclusionSO>(settingPath);
                Debug.Assert(setting != null, $"{settingPath} 경로에 {SettingKey} 데이터가 존재하지 않습니다.");
            }
        }

#else
        public static void Load()
        {
            var path = DataPrefs.GetString(SettingKey, string.Empty);
            path = GetDataPath(path);
            setting = Resources.Load<FMOD_OcclusionSO>(path);
            Debug.Assert(setting != null, $"{path}해당 경로에 {SettingKey} 데이터가 존재하지 않습니다.");

            if (setting == null)
            {
                Addressables.LoadAssetAsync<FMOD_OcclusionSO>(SettingKey).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        setting = handle.Result;
                    }
                };
            }
            
        }
#endif
        public static string GetDataPath(string path)
        {
            path = path.Replace("Assets/", "");
            path = path.Replace("Resources/", "");
            path = path.Replace(".asset", "");
            return path;
        }
    }
}
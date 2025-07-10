using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Weariness.Util;

namespace Weariness.Noesis.FieldOfView
{
#if UNITY_EDITOR

    public static class Provider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            var provider = new SettingsProvider(
                "Project/Noesis/Field Of View",
                SettingsScope.Project,
                new[] { "Noesis", "Field Of View" })
            {
                guiHandler = searchContext =>
                {
                    EditorGUILayout.LabelField("Field Of View", EditorStyles.boldLabel);
                    var setting = SettingProviderHelper.setting = (FieldOfViewSO)EditorGUILayout.ObjectField(
                        $"Field Of View",
                        SettingProviderHelper.setting,
                        typeof(FieldOfViewSO),
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
        public static bool isLoad = false;
        public static FieldOfViewSO setting;

        public static readonly string SettingKey = nameof(FieldOfViewSO);

#if UNITY_EDITOR
        static SettingProviderHelper()
        {
            isLoad = false;
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
                setting = AssetDatabase.LoadAssetAtPath<FieldOfViewSO>(settingPath);
                Debug.Assert(setting != null, $"{settingPath} 경로에 {SettingKey} 데이터가 존재하지 않습니다.");
                isLoad = true;
            }
        }

#else
        public static void Load()
        {
            var path = DataPrefs.GetString(SettingKey, string.Empty);
            path = GetDataPath(path);
            setting = Resources.Load<FieldOfViewSO>(path);
            Debug.Assert(setting != null, $"{path}해당 경로에 {SettingKey} 데이터가 존재하지 않습니다.");

            if (setting == null)
            {
                Addressables.LoadAssetAsync<FieldOfViewSO>(SettingKey).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        setting = handle.Result;
                        isLoad = true;
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
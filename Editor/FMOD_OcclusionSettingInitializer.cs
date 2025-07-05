using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
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
            EnsureAddressableSetup();
            InitOcclusionSO();
            InitAssembly();
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

        private static void InitAssembly()
        {
            if (SettingProviderHelper.setting != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(SettingProviderHelper.setting);
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogError("Addressable 설정을 찾을 수 없습니다.");
                    return;
                }

                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                var entry = settings.FindAssetEntry(guid);

                if (entry == null)
                {
                    Debug.Log($"[Addressable] {SettingProviderHelper.setting.name} 을 Addressable에 등록합니다.");
                    entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                }

                if (entry.address != SettingProviderHelper.SettingKey)
                {
                    Debug.Log($"[Addressable] 주소를 '{entry.address}' → '{SettingProviderHelper.SettingKey}'로 변경합니다.\n경로 : {assetPath}");
                    entry.SetAddress(SettingProviderHelper.SettingKey);
                }

                EditorUtility.SetDirty(settings);
            }
        }
        
        private static void EnsureAddressableSetup()
        {
            if (AddressableAssetSettingsDefaultObject.Settings != null)
                return;
            
            AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
                
            var bundleList = AssetDatabase.GetAllAssetBundleNames();
            if (AddressableAssetSettingsDefaultObject.Settings != null && bundleList.Length > 0)
            {
                var displayChoice = EditorUtility.DisplayDialog("Legacy Bundles Detected",
                    "We have detected the use of legacy bundles in this project.  Would you like to auto-convert those into Addressable? \nThis will take each asset bundle you have defined (we have detected " +
                    bundleList.Length +
                    " bundles), create an Addressable group with a matching name, then move all assets from those bundles into corresponding groups.  This will remove the asset bundle assignment from all assets, and remove all asset bundle definitions from this project.  This cannot be undone.",
                    "Convert", "Ignore");
                if (displayChoice)
                {
                    var asm = typeof(AddressableAssetSettings).Assembly;
                    var utilType = asm.GetType("UnityEditor.AddressableAssets.AddressableAssetUtility");
                    var method = utilType?.GetMethod(
                        "ConvertAssetBundlesToAddressables",
                        BindingFlags.Static | BindingFlags.NonPublic);

                    if (method != null)
                        method.Invoke(null, null);
                    else
                        Debug.LogError("AddressableAssetUtility.Convert… 메서드를 찾을 수 없습니다.");
                }
            }
                
            Debug.Log("[Addressable Auto Setup] Addressable 환경이 자동으로 구성되었습니다.");
        }
    }
}

#endif

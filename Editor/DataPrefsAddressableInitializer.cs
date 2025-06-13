using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Weariness.Util.Editor
{
    [InitializeOnLoad]
    public static class DataPrefsAddressableInitializer
    {
        static DataPrefsAddressableInitializer() => EditorApplication.delayCall += Init;

        private static void Init()
        {
            // 1) Settings asset을 패키지 내부에 만들거나 로드
            EnsureAddressablesSetup();

            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            // 2) Group 생성 (이미 있으면 건너뜀)
            const string groupName = "Weariness.Util";
            var group = settings.FindGroup(groupName);

            if (group == null)
            {
                group = settings.CreateGroup(groupName,
                    false /*isDefault*/,
                    false /*readOnly*/,
                    false /*singleSchema*/,
                    null,
                    new System.Type[] { typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema) });
            }

            // 3) BundledAssetGroupSchema 설정 (로컬/원격 경로 등)
            var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            // 예: 로컬(StreamingAssets)로 빌드하고 로드하도록 설정
            bundleSchema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
            bundleSchema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
            bundleSchema.AssetBundledCacheClearBehavior = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenWhenNewVersionLoaded;

            // 4) 패키지 내 특정 폴더에서 에셋 검색 → Group에 추가
            const string targetFilePath = "Packages/com.weariness.dataprefs/Setting/DataPrefs.json";
            const string fallbackFilePath = "Assets/Scripts/Setting/DataPrefs.json";
            AssetDatabase.ImportAsset(targetFilePath);
            var guid = AssetDatabase.AssetPathToGUID(fallbackFilePath);
            if (guid == "") guid = AssetDatabase.AssetPathToGUID(targetFilePath);
            // 4) 아직 등록되지 않았다면 엔트리 생성
            if (!string.IsNullOrEmpty(guid) && settings.FindAssetEntry(guid) == null)
            {
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = DataPrefs.AddressableKey;
            }

            // 5) 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!string.IsNullOrEmpty(guid) && settings.FindAssetEntry(guid) == null)
            {
                Init();
                return;
            }

            Debug.Log("[DataPrefs] Addressable 설정이 완료되었습니다.");
        }
        
        private static void EnsureAddressablesSetup()
        {
            if (AddressableAssetSettingsDefaultObject.Settings != null)
                return;
            
            AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
                
            var bundleList = AssetDatabase.GetAllAssetBundleNames();
            if (AddressableAssetSettingsDefaultObject.Settings != null && bundleList.Length > 0)
            {
                var displayChoice = EditorUtility.DisplayDialog("Legacy Bundles Detected",
                    "We have detected the use of legacy bundles in this project.  Would you like to auto-convert those into Addressables? \nThis will take each asset bundle you have defined (we have detected " +
                    bundleList.Length +
                    " bundles), create an Addressables group with a matching name, then move all assets from those bundles into corresponding groups.  This will remove the asset bundle assignment from all assets, and remove all asset bundle definitions from this project.  This cannot be undone.",
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
                        Debug.LogError("[DataPrefs] AddressableAssetUtility.Convert… 메서드를 찾을 수 없습니다.");
                }
            }
                
            UnityEngine.Debug.Log("[AddressablesAutoSetup] Addressables 환경이 자동으로 구성되었습니다.");
        }
    }
}
using System.IO;
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
        static DataPrefsAddressableInitializer() => Init();

        private static void Init()
        {
            // 1) Settings asset을 패키지 내부에 만들거나 로드
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            // 2) Group 생성 (이미 있으면 건너뜀)
            const string groupName = "Weariness.Util";
            var group = settings.FindGroup(groupName);

            if (group == null)
            {
                settings.CreateGroup(groupName,
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
                // 원하는 주소 지정
                entry.address = Path.GetFileNameWithoutExtension(targetFilePath);
            }

            // 5) 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[DataPrefs] Addressable 설정이 완료되었습니다.");
        }
    }
}
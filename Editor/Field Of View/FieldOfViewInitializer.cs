using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Weariness.Noesis.FieldOfView.Editor
{
    [InitializeOnLoad]
    public static class FieldOfViewInitializer
    {
        static readonly string DetectingTargetLayerName = "Field Of View Detecting Target";

        static FieldOfViewInitializer()
        {
            Init();
            EditorApplication.delayCall += InitAddresable;
        }
        public static void Init()
        {
            EnsureLayerExists(DetectingTargetLayerName);
        }

        public static void InitAddresable()
        {
            InitAssembly();
            EnsureAddressableSetup();
        }

        private static void EnsureLayerExists(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            bool layerExists = false;

            for (int i = 8; i < layersProp.arraySize; i++) // 0~7은 Unity reserved
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (layerProp != null && layerProp.stringValue == layerName)
                {
                    layerExists = true;
                    break;
                }
            }

            if (!layerExists)
            {
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(layerProp.stringValue))
                    {
                        layerProp.stringValue = layerName;
                        tagManager.ApplyModifiedProperties();
                        Debug.Log($"[LayerUtil] Layer \"{layerName}\" created at index {i}");
                        return;
                    }
                }

                Debug.LogWarning($"[LayerUtil] No empty user layer slot available to add \"{layerName}\".");
            }
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
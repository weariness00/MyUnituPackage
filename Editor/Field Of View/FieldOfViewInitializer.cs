using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Weariness.Noesis.FieldOfView.Editor
{
    [InitializeOnLoad]
    public static class FieldOfViewInitializer
    {
        static readonly string DetectingTargetLayerName = "Field Of View Detecting Target";

        static FieldOfViewInitializer() => Init();
        public static void Init()
        {
            EnsureLayerExists(DetectingTargetLayerName);

        }

        private static void EnsureLayerExists(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            bool layerExists = false;

            for (int i = 8; i < layersProp.arraySize; i++) // 0~7Àº Unity reserved
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
    }
}
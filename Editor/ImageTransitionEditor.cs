using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Weariness.Transition.Editor
{
    [CustomEditor(typeof(ImageTransition))]
    public class ImageTransitionEditor : UnityEditor.Editor
    {
        UnityEditor.Editor baseEditor;

        private SerializedProperty blockType;
        private SerializedProperty childAlignment;
        private SerializedProperty grid;
        private SerializedProperty gridGroupModeType;
        private SerializedProperty easeType;

        private void OnEnable()
        {
            var baseTarget = (Image)target;
            baseEditor = CreateEditor(baseTarget, typeof(ImageEditor));

            blockType = serializedObject.FindProperty("blockType");
            childAlignment = serializedObject.FindProperty("childAlignment");
            grid = serializedObject.FindProperty("grid");
            gridGroupModeType = serializedObject.FindProperty("gridGroupMode");
            easeType = serializedObject.FindProperty("ease");
        }

        public override void OnInspectorGUI()
        {
            // 부모 에디터 그리기
            if (baseEditor != null)
            {
                baseEditor.OnInspectorGUI();
            }

            // 자식 고유의 속성 그리기
            serializedObject.Update();
            GUILayout.Space(10);
            GUILayout.Label("Transition Info", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(blockType);
            EditorGUILayout.PropertyField(childAlignment);
            EditorGUILayout.PropertyField(grid);
            EditorGUILayout.PropertyField(gridGroupModeType);
            EditorGUILayout.PropertyField(easeType);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
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

        private SerializedProperty normalizedTime;
        private SerializedProperty blockType;
        private SerializedProperty childAlignment;
        private SerializedProperty grid;

        private void OnEnable()
        {
            var baseTarget = (Image)target;
            baseEditor = CreateEditor(baseTarget, typeof(ImageEditor));

            blockType = serializedObject.FindProperty("blockType");
            childAlignment = serializedObject.FindProperty("childAlignment");
            normalizedTime = serializedObject.FindProperty("normalizedTime");
            grid = serializedObject.FindProperty("grid");
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
            EditorGUILayout.PropertyField(normalizedTime);
            EditorGUILayout.PropertyField(grid);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
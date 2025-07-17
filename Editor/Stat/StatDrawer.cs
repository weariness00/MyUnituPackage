using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    [CustomPropertyDrawer(typeof(Stat))]
    public class StatDrawer : PropertyDrawer
    {
        private static readonly string ValueKey = "baseValue";
        private static readonly string ModifierListKey = "modifierList";

        private const float FoldoutWidth = 15f;
        private bool showModifiers = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var baseValueProp = property.FindPropertyRelative("baseValue");
            var modifiersProp = property.FindPropertyRelative("modifierList");

            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 6f;

            float totalWidth = position.width;


            Rect foldoutRect = new Rect(position.x, y, FoldoutWidth, lineHeight);
            // 폴드아웃 토글 출력
            showModifiers = EditorGUI.Foldout(foldoutRect, showModifiers, GUIContent.none);
            
            // 🔹 라벨 출력
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            Rect baseLabelRect = new Rect(position.x + spacing, y, 40f, lineHeight);
            Rect baseValueRect = new Rect(baseLabelRect.xMax + spacing, y, 60f, lineHeight);
            Rect realValueRect = new Rect(baseValueRect.xMax + spacing, y, totalWidth - baseValueRect.xMax - spacing, lineHeight);

            EditorGUI.LabelField(baseLabelRect, "Value");
            EditorGUI.PropertyField(baseValueRect, baseValueProp, GUIContent.none);

            if (property.GetTargetObjectOfProperty() is Stat statObj)
            {
                float realValue = statObj.Value;
                EditorGUI.LabelField(realValueRect, $"Real Value: {realValue}");
            }

            y += lineHeight + 2;

            if (showModifiers)
            {
                EditorGUI.PropertyField(new Rect(foldoutRect.xMax + 10f, y, totalWidth - foldoutRect.xMax - 10f, EditorGUI.GetPropertyHeight(modifiersProp, true)), modifiersProp, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 2;
            if (showModifiers)
            {
                SerializedProperty modifiersProp = property.FindPropertyRelative("modifierList");
                height += EditorGUI.GetPropertyHeight(modifiersProp, true);
            }
            return height;
        }
    }
}
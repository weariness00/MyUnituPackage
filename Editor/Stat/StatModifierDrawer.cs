using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor

{
    [CustomPropertyDrawer(typeof(StatModifier))]
    public class StatModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 🔹 라벨 출력
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 6f;

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            var modifierType = (StatModifier.ModifierType)typeProp.enumValueIndex;

            // 🔹 텍스트 내용 & 크기 계산
            string extraText = modifierType == StatModifier.ModifierType.Flat ? "" : $"{valueProp.floatValue * 100}%";
            GUIStyle textStyle = EditorStyles.label;
            Vector2 textSize = textStyle.CalcSize(new GUIContent(extraText));

            // 🔹 Rect 계산
            float remainWidth = position.width - textSize.x - spacing;
            float halfWidth = (remainWidth - spacing) / 2f;

            Rect valueRect = new Rect(position.x, position.y, halfWidth, lineHeight);
            Rect textRect  = new Rect(valueRect.xMax + spacing, position.y, textSize.x, lineHeight);
            Rect typeRect  = new Rect(textRect.xMax + spacing, position.y, halfWidth, lineHeight);

            // 🔹 실제 그리기
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            EditorGUI.LabelField(textRect, extraText);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
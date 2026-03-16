using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor

{
    [CustomPropertyDrawer(typeof(StatModifier))]
    public class StatModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 6f;

            SerializedProperty typeProp  = property.FindPropertyRelative("type");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            var modifierType = (StatModifier.ModifierType)typeProp.enumValueIndex;
            string extraText = modifierType == StatModifier.ModifierType.Flat ? "" : $"{valueProp.floatValue * 100}%";
            float extraTextW = EditorStyles.label.CalcSize(new GUIContent(extraText)).x;

            float halfW = (position.width - extraTextW - spacing * 2f) / 2f;

            Rect valueRect = new Rect(position.x, position.y, halfW, lineHeight);
            Rect extraRect = new Rect(valueRect.xMax + spacing, position.y, extraTextW, lineHeight);
            Rect typeRect  = new Rect(extraRect.xMax + spacing, position.y, halfW, lineHeight);

            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            EditorGUI.LabelField(extraRect, extraText);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
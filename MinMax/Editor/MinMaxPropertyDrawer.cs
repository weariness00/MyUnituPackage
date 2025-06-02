using UnityEditor;
using UnityEngine;
namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(MinMax<int>))]
    public class MinMaxIntPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;

        private float currentValueInterval = 30;
        private float minValueInterval = 30;
        private float maxValueInterval = 30;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");

            Rect labelPosition = new Rect(position.x, position.y, position.width, position.height);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float sumInterval = 0;

            // Min Value 필드
            int minDigitCount = min.intValue.GetDigitCount();
            minValueInterval = minDigitCount <= 2 ? 25 : minDigitCount * 11;
            var minPos = new Rect(position.x + sumInterval, position.y, minValueInterval, position.height);
            min.intValue = EditorGUI.IntField(minPos, min.intValue);
            sumInterval += minValueInterval;

            int textInterval = 20;
            var rangeTextPos = new Rect(position.x + sumInterval, position.y, textInterval, position.height);
            EditorGUI.LabelField(rangeTextPos, $" ~ ");
            sumInterval += textInterval;

            int maxDigitCount = max.intValue.GetDigitCount();
            maxValueInterval = maxDigitCount <= 2 ? 25 : maxDigitCount * 11;
            var maxPos = new Rect(position.x + sumInterval, position.y, maxValueInterval, position.height);
            max.intValue = EditorGUI.IntField(maxPos, max.intValue);
            sumInterval += maxValueInterval;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(MinMax<float>))]
    public class MinMaxFloatPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;

        private float currentValueInterval = 30;
        private float minValueInterval = 30;
        private float maxValueInterval = 30;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");
            
            // Label 필드
            Rect labelPosition = new Rect(position.x, position.y, label.text.Length * 1, position.height);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float sumInterval = 0; // 전체 간격 길이

            // min value 필드 값
            int minDigitCount = min.floatValue.GetDigitCount();
            minValueInterval = minDigitCount <= 2 ? 25 : minDigitCount * 9;
            var minPos = new Rect(position.x + sumInterval, position.y, minValueInterval, position.height);
            min.floatValue = EditorGUI.FloatField(minPos, min.floatValue);
            sumInterval += minValueInterval;

            // "~" 문자열 필드
            int textInterval = 20;
            var rangeTextPos = new Rect(position.x + sumInterval, position.y, textInterval, position.height);
            EditorGUI.LabelField(rangeTextPos, $" ~ ");
            sumInterval += textInterval;

            // Max Value 필드 값
            int maxDigitCount = max.floatValue.GetDigitCount();
            maxValueInterval = maxDigitCount <= 2 ? 25 : maxDigitCount * 9;
            var maxPos = new Rect(position.x + sumInterval, position.y, maxValueInterval, position.height);
            max.floatValue = EditorGUI.FloatField(maxPos, max.floatValue);
            sumInterval += maxValueInterval;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
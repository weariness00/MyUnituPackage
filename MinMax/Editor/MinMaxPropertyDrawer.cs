using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
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
            float charWidth = 9f; // 에디터 폰트 기준 대략 문자 1자당 폭

            // Min Value 필드
            int minDigitCount = min.intValue.GetDigitCount();
            minValueInterval = Mathf.Clamp(minDigitCount * charWidth + 5f, 25f, 100f);
            var minPos = new Rect(position.x + sumInterval, position.y, minValueInterval, position.height);
            min.intValue = EditorGUI.IntField(minPos, min.intValue);
            sumInterval += minValueInterval;

            int textInterval = 20;
            var rangeTextPos = new Rect(position.x + sumInterval, position.y, textInterval, position.height);
            EditorGUI.LabelField(rangeTextPos, $" ~ ");
            sumInterval += textInterval;

            int maxDigitCount = max.intValue.GetDigitCount();
            maxValueInterval = Mathf.Clamp(maxDigitCount * charWidth + 5f, 25f, 100f);
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
            float charWidth = 9f; // 에디터 폰트 기준 대략 문자 1자당 폭

            // min value 필드 값
            int minDigitCount = min.floatValue.GetDigitCount();
            minValueInterval = Mathf.Clamp(minDigitCount * charWidth + 5f, 25f, 100f);
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
            maxValueInterval = Mathf.Clamp(maxDigitCount * charWidth + 5f, 25f, 100f);
            var maxPos = new Rect(position.x + sumInterval, position.y, maxValueInterval, position.height);
            max.floatValue = EditorGUI.FloatField(maxPos, max.floatValue);
            sumInterval += maxValueInterval;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }    
    
    [CustomPropertyDrawer(typeof(MinMax<Vector2>))]
    public class MinMaxVector2PropertyDrawer : PropertyDrawer
    {
        float commaWidth = 8f;
        float charWidth = 9f;
        float minTotalWidth = 0;
        float maxTotalWidth = 0;

        private float maxWidthLength = 150f;

        int size = 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            Vector3 min = minProp.vector2Value;
            Vector3 max = maxProp.vector2Value;

            position = EditorGUI.PrefixLabel(position, label);


            // min/max 칸 크기 계산
            float[] minWidths = new float[3];
            float[] maxWidths = new float[3];

            minTotalWidth = 16;
            maxTotalWidth = 16;
            for (int i = 0; i < size; ++i)
            {
                minWidths[i] = Mathf.Clamp(min[i].GetDigitCount() * charWidth + 5f, 25f, 100f);
                maxWidths[i] = Mathf.Clamp(max[i].GetDigitCount() * charWidth + 5f, 25f, 100f);
                if (i != 0)
                {
                    minTotalWidth += commaWidth;
                    maxTotalWidth += commaWidth;
                }

                minTotalWidth += minWidths[i];
                maxTotalWidth += maxWidths[i];
            }

            float xMin = position.x;
            float xMax = position.x;
            float yMin = position.y;
            float yMax = position.y;

            float lineHeight = EditorGUIUtility.singleLineHeight;

            // 한 줄에 다 못들어가면 2줄로 분리
            bool splitLine = minTotalWidth > maxWidthLength || maxTotalWidth > maxWidthLength;

            // min 필드
            for (int i = 0; i < size; ++i)
            {
                min[i] = EditorGUI.FloatField(new Rect(xMin, yMin, minWidths[i], lineHeight), min[i]);
                xMin += minWidths[i];
                if (i != size - 1)
                {
                    xMin += commaWidth;
                }
            }

            // ~
            EditorGUI.LabelField(new Rect(xMin, yMin, 18, lineHeight), " ~ ");

            if (splitLine)
            {
                // max를 다음 줄에
                yMax += lineHeight + EditorGUIUtility.standardVerticalSpacing;
                xMax = position.x;
            }
            else
            {
                // max는 같은 줄에
                xMax = xMin + 22;
            }


            // max 필드
            for (int i = 0; i < size; ++i)
            {
                max[i] = EditorGUI.FloatField(new Rect(xMax, yMax, maxWidths[i], lineHeight), max[i]);
                xMax += maxWidths[i];
                if (i != size - 1)
                {
                    xMax += commaWidth;
                }
            }

            minProp.vector2Value = min;
            maxProp.vector2Value = max;

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (minTotalWidth > maxWidthLength || maxTotalWidth > maxWidthLength)
            {
                // 두 줄
                return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }

            // 한 줄
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomPropertyDrawer(typeof(MinMax<Vector3>))]
    public class MinMaxVector3PropertyDrawer : PropertyDrawer
    {
        float commaWidth = 8f;
        float charWidth = 9f;
        float minTotalWidth = 0;
        float maxTotalWidth = 0;

        private float maxWidthLength = 150f;

        int size = 3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            Vector3 min = minProp.vector3Value;
            Vector3 max = maxProp.vector3Value;

            position = EditorGUI.PrefixLabel(position, label);


            // min/max 칸 크기 계산
            float[] minWidths = new float[3];
            float[] maxWidths = new float[3];

            minTotalWidth = 16;
            maxTotalWidth = 16;
            for (int i = 0; i < size; ++i)
            {
                minWidths[i] = Mathf.Clamp(min[i].GetDigitCount() * charWidth + 5f, 25f, 100f);
                maxWidths[i] = Mathf.Clamp(max[i].GetDigitCount() * charWidth + 5f, 25f, 100f);
                if (i != 0)
                {
                    minTotalWidth += commaWidth;
                    maxTotalWidth += commaWidth;
                }

                minTotalWidth += minWidths[i];
                maxTotalWidth += maxWidths[i];
            }

            float xMin = position.x;
            float xMax = position.x;
            float yMin = position.y;
            float yMax = position.y;

            float lineHeight = EditorGUIUtility.singleLineHeight;

            // 한 줄에 다 못들어가면 2줄로 분리
            bool splitLine = minTotalWidth > maxWidthLength || maxTotalWidth > maxWidthLength;

            // min 필드
            for (int i = 0; i < size; ++i)
            {
                min[i] = EditorGUI.FloatField(new Rect(xMin, yMin, minWidths[i], lineHeight), min[i]);
                xMin += minWidths[i];
                if (i != size - 1)
                {
                    xMin += commaWidth;
                }
            }

            // ~
            EditorGUI.LabelField(new Rect(xMin, yMin, 18, lineHeight), " ~ ");

            if (splitLine)
            {
                // max를 다음 줄에
                yMax += lineHeight + EditorGUIUtility.standardVerticalSpacing;
                xMax = position.x;
            }
            else
            {
                // max는 같은 줄에
                xMax = xMin + 22;
            }


            // max 필드
            for (int i = 0; i < size; ++i)
            {
                max[i] = EditorGUI.FloatField(new Rect(xMax, yMax, maxWidths[i], lineHeight), max[i]);
                xMax += maxWidths[i];
                if (i != size - 1)
                {
                    xMax += commaWidth;
                }
            }

            minProp.vector3Value = min;
            maxProp.vector3Value = max;

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (minTotalWidth > maxWidthLength || maxTotalWidth > maxWidthLength)
            {
                // 두 줄
                return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }

            // 한 줄
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
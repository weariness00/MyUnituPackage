using System;
using UnityEditor;
using UnityEngine;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxValue<float>))]
    public class MinMaxValueFloatPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;
        private SerializedProperty current;

        private SerializedProperty isMin;
        private SerializedProperty isMax;

        private SerializedProperty isOverMin;
        private SerializedProperty isOverMax;

        private bool isAdjustingSlider = false;
        private float currentValueWidth = 10;
        private float minValueWidth = 10;
        private float maxValueWidth = 10;
        private bool showOverToggle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 슬라이더를 조작중인지 이벤트 확인
            if (Event.current.type == EventType.MouseDown)
                isAdjustingSlider = true; // 슬라이더 조작 시작
            else if (Event.current.type == EventType.MouseUp)
                isAdjustingSlider = false; // 슬라이더 조작 종료

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");
            current = property.FindPropertyRelative("_current");

            isMin = property.FindPropertyRelative("_isMin");
            isMax = property.FindPropertyRelative("_isMax");

            isOverMin = property.FindPropertyRelative("isOverMin");
            isOverMax = property.FindPropertyRelative("isOverMax");

            var prevMin = min.floatValue;
            var prevMax = max.floatValue;
            var prevCurrent = current.floatValue;

            Rect prefixLabelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position = EditorGUI.PrefixLabel(prefixLabelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            float totalWidth = position.width;
            float spacing = 5f;
            float height = EditorGUIUtility.singleLineHeight;
            float tildeWidth = 15f;
            float toggleWidth = 20f;

            // 자릿수 기반 width 계산
            int currentDigits = current.floatValue.GetDigitCount();
            int minDigits = min.floatValue.GetDigitCount();
            int maxDigits = max.floatValue.GetDigitCount();

            float charWidth = 9f; // 에디터 폰트 기준 대략 문자 1자당 폭
            if (isAdjustingSlider == false)
            {
                currentValueWidth = Mathf.Clamp(currentDigits * charWidth + 5f, 25f, 100f);
                minValueWidth = Mathf.Clamp(minDigits * charWidth + 5f, 25f, 100f);
                maxValueWidth = Mathf.Clamp(maxDigits * charWidth + 5f, 25f, 100f);
            }

            // 남은 영역 = 슬라이더 폭
            float usedWidth = currentValueWidth + minValueWidth + maxValueWidth + tildeWidth + toggleWidth + spacing * 5;
            float sliderWidth = Mathf.Max(30f, totalWidth - usedWidth);

            // 위치 계산
            Rect currentValueRect = new Rect(position.x, position.y, currentValueWidth, height);
            Rect sliderRect = new Rect(currentValueRect.xMax + spacing, position.y, sliderWidth, height);
            Rect minValueRect = new Rect(sliderRect.xMax + spacing, position.y, minValueWidth, height);
            Rect tildeRect = new Rect(minValueRect.xMax, position.y, tildeWidth, height);
            Rect maxValueRect = new Rect(tildeRect.xMax + spacing, position.y, maxValueWidth, height);
            Rect toggleRect = new Rect(maxValueRect.xMax + spacing, position.y, toggleWidth, height);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // current Value 필드
            current.floatValue = EditorGUI.FloatField(currentValueRect, current.floatValue);

            // Slider 필드
            current.floatValue = GUI.HorizontalSlider(sliderRect, current.floatValue, min.floatValue, max.floatValue);

            // Min Max Value 필드
            min.floatValue = EditorGUI.FloatField(minValueRect, min.floatValue);
            EditorGUI.LabelField(tildeRect, $" ~ ");
            max.floatValue = EditorGUI.FloatField(maxValueRect, max.floatValue);

            // IsOver 관련 보여주는 Layout
            // 토글 박스 스타일 설정
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.button);
            toggleStyle.normal.textColor = Color.white;
            toggleStyle.active.textColor = Color.white;
            toggleStyle.alignment = TextAnchor.MiddleCenter;

            // 토글 할당
            showOverToggle = EditorGUI.Toggle(toggleRect, showOverToggle, toggleStyle);

            if (showOverToggle)
            {
                position.y += EditorGUIUtility.singleLineHeight + 5; // 다음 줄로 이동

                // Is Over Min 인스펙터에 표시
                // Over Min의 라벨 길이
                var toggleBoxWidth = 20f;
                GUIContent minLabelContent = new GUIContent(isOverMin.name);
                float minLabelWidth = GUI.skin.label.CalcSize(minLabelContent).x;
                Rect rect;

                // "Over Min" 라벨 그리기
                rect = new Rect(position.x, position.y, minLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMin.name);

                // Is Over Min 필드
                rect = new Rect(rect.xMax, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMin.boolValue = EditorGUI.Toggle(rect, isOverMin.boolValue);

                // Is Over Max 인스펙터에 표시
                // Over Min의 라벨 길이
                GUIContent maxLabelContent = new GUIContent(isOverMax.name);
                float maxLabelWidth = GUI.skin.label.CalcSize(maxLabelContent).x;

                // "Over Min" 라벨 그리기
                rect = new Rect(rect.xMax, position.y, maxLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMax.name);

                // Is Over Min 필드
                rect = new Rect(rect.xMax, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMax.boolValue = EditorGUI.Toggle(rect, isOverMax.boolValue);
            }

            if (Math.Abs(prevCurrent - current.floatValue) > 0.0001f ||
                Math.Abs(prevMin - min.floatValue) > 0.0001f ||
                Math.Abs(prevMax - max.floatValue) > 0.0001f)
            {
                // 현재 값이 변경되면 Min, Max 체크
                CheckMinMax();
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        // ✅ 높이 설정 (showOverToggle에 따라 높이 증가)
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var yValue = 1;
            yValue += showOverToggle ? 1 : 0;
            return EditorGUIUtility.singleLineHeight * yValue + 5;
        }

        private void CheckMinMax()
        {
            isMin.boolValue = isMax.boolValue = false;
            if (min.floatValue.CompareTo(max.floatValue) == 0)
            {
                isMin.boolValue = isMax.boolValue = true;
            }

            if (current.floatValue.CompareTo(min.floatValue) <= 0)
            {
                if (isOverMin.boolValue == false)
                {
                    current.floatValue = min.floatValue;
                }

                isMin.boolValue = true;
            }

            if (current.floatValue.CompareTo(max.floatValue) >= 0)
            {
                if (isOverMax.boolValue == false)
                {
                    current.floatValue = max.floatValue;
                }

                isMax.boolValue = true;
            }
        }
    }

    [CustomPropertyDrawer(typeof(MinMaxValue<int>))]
    public class MinMaxValueIntegerPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;
        private SerializedProperty current;

        private SerializedProperty isMin;
        private SerializedProperty isMax;

        private SerializedProperty isOverMin;
        private SerializedProperty isOverMax;

        private bool isAdjustingSlider = false;
        private float currentWidth = 30;
        private float minWidth = 30;
        private float maxWidth = 30;
        private bool showOverToggle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 슬라이더 조작 상태 확인
            if (Event.current.type == EventType.MouseDown) isAdjustingSlider = true;
            else if (Event.current.type == EventType.MouseUp) isAdjustingSlider = false;

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");
            current = property.FindPropertyRelative("_current");

            isMin = property.FindPropertyRelative("_isMin");
            isMax = property.FindPropertyRelative("_isMax");

            isOverMin = property.FindPropertyRelative("isOverMin");
            isOverMax = property.FindPropertyRelative("isOverMax");

            var prevMin = min.intValue;
            var prevMax = max.intValue;
            var prevCurrent = current.intValue;

            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);
            float totalWidth = position.width;
            float height = EditorGUIUtility.singleLineHeight;
            float spacing = 5f;

            int currentDigits = current.intValue.ToString().Length;
            int minDigits = min.intValue.ToString().Length;
            int maxDigits = max.intValue.ToString().Length;

            float charWidth = 8f;
            if (isAdjustingSlider == false)
            {
                currentWidth = Mathf.Clamp(currentDigits * charWidth + 5f, 20f, 100f);
                minWidth = Mathf.Clamp(minDigits * charWidth + 5f, 20f, 100f);
                maxWidth = Mathf.Clamp(maxDigits * charWidth + 5f, 20f, 100f);
            }

            float tildeWidth = 10f;
            float toggleWidth = 20f;

            float usedWidth = currentWidth + minWidth + maxWidth + tildeWidth + toggleWidth + spacing * 5;
            float sliderWidth = Mathf.Max(30f, totalWidth - usedWidth);

            Rect currentRect = new Rect(position.x, position.y, currentWidth, height);
            Rect sliderRect = new Rect(currentRect.xMax + spacing, position.y, sliderWidth, height);
            Rect minRect = new Rect(sliderRect.xMax + spacing, position.y, minWidth, height);
            Rect tildeRect = new Rect(minRect.xMax, position.y, tildeWidth, height);
            Rect maxRect = new Rect(tildeRect.xMax + spacing, position.y, maxWidth, height);
            Rect toggleRect = new Rect(maxRect.xMax + spacing, position.y, toggleWidth, height);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            current.intValue = EditorGUI.IntField(currentRect, current.intValue);
            current.intValue = Mathf.Clamp(current.intValue, isOverMin.boolValue ? int.MinValue : min.intValue,
                isOverMax.boolValue ? int.MaxValue : max.intValue);

            current.intValue = Mathf.RoundToInt(GUI.HorizontalSlider(sliderRect, current.intValue, min.intValue, max.intValue));
            min.intValue = EditorGUI.IntField(minRect, min.intValue);
            EditorGUI.LabelField(tildeRect, " ~ ");
            max.intValue = EditorGUI.IntField(maxRect, max.intValue);

            showOverToggle = EditorGUI.Toggle(toggleRect, showOverToggle, new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                active = { textColor = Color.white }
            });

            if (showOverToggle)
            {
                position.y += height + 5;

                GUIContent minLabelContent = new GUIContent(isOverMin.name);
                float minLabelWidth = GUI.skin.label.CalcSize(minLabelContent).x;
                float toggleBoxWidth = 20f;
                Rect rect;

                rect = new Rect(position.x, position.y, minLabelWidth, height);
                EditorGUI.LabelField(rect, isOverMin.name);
                rect = new Rect(rect.xMax, position.y, toggleBoxWidth, height);
                isOverMin.boolValue = EditorGUI.Toggle(rect, isOverMin.boolValue);

                GUIContent maxLabelContent = new GUIContent(isOverMax.name);
                float maxLabelWidth = GUI.skin.label.CalcSize(maxLabelContent).x;
                rect = new Rect(rect.xMax, position.y, maxLabelWidth, height);
                EditorGUI.LabelField(rect, isOverMax.name);
                rect = new Rect(rect.xMax, position.y, toggleBoxWidth, height);
                isOverMax.boolValue = EditorGUI.Toggle(rect, isOverMax.boolValue);
            }

            if (prevCurrent != current.intValue ||
                prevMin != min.intValue ||
                prevMax != max.intValue)
                CheckMinMax();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        // ✅ 높이 설정 (showOverToggle에 따라 높이 증가)
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = showOverToggle ? 2 : 1;
            return EditorGUIUtility.singleLineHeight * lineCount + 5;
        }

        private void CheckMinMax()
        {
            isMin.boolValue = isMax.boolValue = false;
            if (min.intValue.CompareTo(max.intValue) == 0)
            {
                isMin.boolValue = isMax.boolValue = true;
            }

            if (current.intValue.CompareTo(min.intValue) <= 0)
            {
                if (isOverMin.boolValue == false)
                {
                    current.intValue = min.intValue;
                }

                isMin.boolValue = true;
            }

            if (current.intValue.CompareTo(max.intValue) >= 0)
            {
                if (isOverMax.boolValue == false)
                {
                    current.intValue = max.intValue;
                }

                isMax.boolValue = true;
            }
        }
    }
}
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
        private SerializedProperty isOverMin;
        private SerializedProperty isOverMax;

        private bool isAdjustingSlider = false;
        private float currentValueInterval = 30;
        private float minValueInterval = 30;
        private float maxValueInterval = 30;
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
            isOverMin = property.FindPropertyRelative("isOverMin");
            isOverMax = property.FindPropertyRelative("isOverMax");

            Rect labelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float sumInterval = 0;
            Rect rect;

            // current Value 필드
            int digitCount = current.floatValue.GetDigitCount();
            if (!isAdjustingSlider) currentValueInterval = digitCount < 3 ? 25 : digitCount * 11;
            rect = new Rect(position.x + sumInterval, position.y, currentValueInterval, EditorGUIUtility.singleLineHeight);
            current.floatValue = EditorGUI.FloatField(rect, current.floatValue);
            if (!isOverMin.boolValue && current.floatValue < min.floatValue)
                current.floatValue = min.floatValue;
            else if (!isOverMax.boolValue && current.floatValue > max.floatValue)
                current.floatValue = max.floatValue;
            sumInterval += currentValueInterval + 10;

            // Slider 필드
            float sliderInterval = EditorGUIUtility.currentViewWidth - 250 - minValueInterval - maxValueInterval;
            if (sliderInterval < 50) sliderInterval = 50;
            else if (sliderInterval > 200) sliderInterval = 200;
            rect = new Rect(position.x + sumInterval, position.y, sliderInterval, EditorGUIUtility.singleLineHeight);
            current.floatValue = GUI.HorizontalSlider(rect, current.floatValue, min.floatValue, max.floatValue);
            sumInterval += sliderInterval + 10;

            // Min Value 필드
            int minDigitCount = min.floatValue.GetDigitCount();
            minValueInterval = minDigitCount <= 2 ? 25 : minDigitCount * 11;
            rect = new Rect(position.x + sumInterval, position.y, minValueInterval, EditorGUIUtility.singleLineHeight);
            min.floatValue = EditorGUI.FloatField(rect, min.floatValue);
            sumInterval += minValueInterval;

            int textInterval = 20;
            rect = new Rect(position.x + sumInterval, position.y, textInterval, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, $" ~ ");
            sumInterval += textInterval;

            int maxDigitCount = max.floatValue.GetDigitCount();
            maxValueInterval = maxDigitCount <= 2 ? 25 : maxDigitCount * 11;
            rect = new Rect(position.x + sumInterval, position.y, maxValueInterval, EditorGUIUtility.singleLineHeight);
            max.floatValue = EditorGUI.FloatField(rect, max.floatValue);
            sumInterval += maxValueInterval + 10;

            // IsOver 관련 보여주는 Layout
            // 토글 박스 스타일 설정
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.button);
            toggleStyle.normal.textColor = Color.white;
            toggleStyle.active.textColor = Color.white;
            toggleStyle.alignment = TextAnchor.MiddleCenter;

            // 토글 할당
            var toggleBoxWidth = 20f;
            rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
            showOverToggle = EditorGUI.Toggle(rect, showOverToggle, toggleStyle);
            sumInterval += toggleBoxWidth;

            if (showOverToggle)
            {
                position.y += EditorGUIUtility.singleLineHeight + 5; // 다음 줄로 이동
                sumInterval = 0; // 새 줄이므로 위치 초기화
                
                // Is Over Min 인스펙터에 표시
                // Over Min의 라벨 길이
                GUIContent minLabelContent = new GUIContent(isOverMin.name);
                float minLabelWidth = GUI.skin.label.CalcSize(minLabelContent).x;
                
                // "Over Min" 라벨 그리기
                rect = new Rect(position.x + sumInterval, position.y, minLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMin.name);
                sumInterval += minLabelWidth; // 라벨 너비만큼 간격 추가
                
                // Is Over Min 필드
                rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMin.boolValue = EditorGUI.Toggle(rect, isOverMin.boolValue);
                sumInterval += toggleBoxWidth;
                
                // Is Over Max 인스펙터에 표시
                // Over Min의 라벨 길이
                GUIContent maxLabelContent = new GUIContent(isOverMax.name);
                float maxLabelWidth = GUI.skin.label.CalcSize(maxLabelContent).x;
                
                // "Over Min" 라벨 그리기
                rect = new Rect(position.x + sumInterval, position.y, maxLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMax.name);
                sumInterval += maxLabelWidth; // 라벨 너비만큼 간격 추가
                
                // Is Over Min 필드
                rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMax.boolValue = EditorGUI.Toggle(rect, isOverMax.boolValue);
                sumInterval += toggleBoxWidth;
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
    }

    [CustomPropertyDrawer(typeof(MinMaxValue<int>))]
    public class MinMaxValueIntegerPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;
        private SerializedProperty current;
        private SerializedProperty isOverMin;
        private SerializedProperty isOverMax;

        private bool isAdjustingSlider = false;
        private float currentValueInterval = 30;
        private float minValueInterval = 30;
        private float maxValueInterval = 30;
        private bool showOverToggle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");
            current = property.FindPropertyRelative("_current");
            isOverMin = property.FindPropertyRelative("isOverMin");
            isOverMax = property.FindPropertyRelative("isOverMax");

            // 슬라이더를 조작중인지 이벤트 확인
            if (Event.current.type == EventType.MouseDown)
                isAdjustingSlider = true; // 슬라이더 조작 시작
            else if (Event.current.type == EventType.MouseUp)
                isAdjustingSlider = false; // 슬라이더 조작 종료

            // Label 필드
            Rect labelPosition = new Rect(position.x, position.y, label.text.Length * 1, EditorGUIUtility.singleLineHeight);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Rect rect;

            float sumInterval = 0; // 전체 간격 길이

            // current int 필드 값
            int digitCount = current.intValue.GetDigitCount();
            if (!isAdjustingSlider) currentValueInterval = digitCount <= 3 ? 25 : digitCount * 10;
            rect = new Rect(position.x + sumInterval, position.y, currentValueInterval, EditorGUIUtility.singleLineHeight);
            current.intValue = EditorGUI.IntField(rect, current.intValue);
            if (!isOverMin.boolValue && current.intValue < min.intValue)
                current.intValue = min.intValue;
            else if (!isOverMax.boolValue && current.intValue > max.intValue)
                current.intValue = max.intValue;
            sumInterval += currentValueInterval + 10;

            // slider 필드 값
            float sliderInterval = EditorGUIUtility.currentViewWidth - 250 - minValueInterval - maxValueInterval;
            if (sliderInterval < 50) sliderInterval = 50;
            else if (sliderInterval > 200) sliderInterval = 200;
            rect = new Rect(position.x + sumInterval, position.y, sliderInterval, position.height);
            current.intValue = (int)GUI.HorizontalSlider(rect, current.intValue, min.intValue, max.intValue);
            sumInterval += sliderInterval + 10;

            // min value 필드 값
            int minDigitCount = min.intValue.GetDigitCount();
            minValueInterval = minDigitCount <= 2 ? 25 : minDigitCount * 9;
            rect = new Rect(position.x + sumInterval, position.y, minValueInterval, EditorGUIUtility.singleLineHeight);
            min.intValue = EditorGUI.IntField(rect, min.intValue);
            sumInterval += minValueInterval;

            // "~" 문자열 필드
            int textInterval = 20;
            rect = new Rect(position.x + sumInterval, position.y, textInterval, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, $" ~ ");
            sumInterval += textInterval;

            // Max Value 필드 값
            int maxDigitCount = max.intValue.GetDigitCount();
            maxValueInterval = maxDigitCount <= 2 ? 25 : maxDigitCount * 9;
            rect = new Rect(position.x + sumInterval, position.y, maxValueInterval, EditorGUIUtility.singleLineHeight);
            max.intValue = EditorGUI.IntField(rect, max.intValue);
            sumInterval += maxValueInterval;
            
            // IsOver 관련 보여주는 Layout
            // 토글 박스 스타일 설정
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.button);
            toggleStyle.normal.textColor = Color.white;
            toggleStyle.active.textColor = Color.white;
            toggleStyle.alignment = TextAnchor.MiddleCenter;

            // 토글 할당
            var toggleBoxWidth = 20f;
            rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
            showOverToggle = EditorGUI.Toggle(rect, showOverToggle, toggleStyle);
            sumInterval += toggleBoxWidth;

            if (showOverToggle)
            {
                position.y += EditorGUIUtility.singleLineHeight + 5; // 다음 줄로 이동
                sumInterval = 0; // 새 줄이므로 위치 초기화
                
                // Is Over Min 인스펙터에 표시
                // Over Min의 라벨 길이
                GUIContent minLabelContent = new GUIContent(isOverMin.name);
                float minLabelWidth = GUI.skin.label.CalcSize(minLabelContent).x;
                
                // "Over Min" 라벨 그리기
                rect = new Rect(position.x + sumInterval, position.y, minLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMin.name);
                sumInterval += minLabelWidth; // 라벨 너비만큼 간격 추가
                
                // Is Over Min 필드
                rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMin.boolValue = EditorGUI.Toggle(rect, isOverMin.boolValue);
                sumInterval += toggleBoxWidth;
                
                // Is Over Max 인스펙터에 표시
                // Over Min의 라벨 길이
                GUIContent maxLabelContent = new GUIContent(isOverMax.name);
                float maxLabelWidth = GUI.skin.label.CalcSize(maxLabelContent).x;
                
                // "Over Min" 라벨 그리기
                rect = new Rect(position.x + sumInterval, position.y, maxLabelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, isOverMax.name);
                sumInterval += maxLabelWidth; // 라벨 너비만큼 간격 추가
                
                // Is Over Min 필드
                rect = new Rect(position.x + sumInterval, position.y, toggleBoxWidth, EditorGUIUtility.singleLineHeight);
                isOverMax.boolValue = EditorGUI.Toggle(rect, isOverMax.boolValue);
                sumInterval += toggleBoxWidth;
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
    }
}
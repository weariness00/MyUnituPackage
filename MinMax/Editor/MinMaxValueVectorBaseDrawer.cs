using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    // ============================================
    // 공통 상수/레이아웃 유틸
    // ============================================
    internal static class MMVLayout
    {
        // 고정 폭들 (슬라이더 조절 중에도 변하지 않음)
        public const float AxisLabelWidth = 18f; // "X","Y","Z"
        public const float CurFieldWidth  = 60f;
        public const float MinFieldWidth  = 60f;
        public const float MaxFieldWidth  = 60f;
        public const float TildeWidth     = 15f;
        public const float SliderMinWidth = 100f;

        public const float Spacing = 5f;      // 요소 간 가로 간격
        public const float TopGap  = 2f;      // 줄과 줄 사이 여백

        // [Axis][Cur][Slider][Min][~][Max] 레이아웃 계산
        public static void CalcAxisRowRects(
            Rect row,
            out Rect axisRect,
            out Rect curRect,
            out Rect sliderRect,
            out Rect minRect,
            out Rect tildeRect,
            out Rect maxRect)
        {
            float x = row.x;
            float h = row.height;

            axisRect = new Rect(x, row.y, AxisLabelWidth, h);
            x = axisRect.xMax + Spacing;

            curRect = new Rect(x, row.y, CurFieldWidth, h);
            x = curRect.xMax + Spacing;

            // 남은 폭에서 [Min][~][Max]와 간격을 제외한 값을 Slider로
            float remain = row.xMax - x;
            float rightNeeded = MinFieldWidth + Spacing + TildeWidth + Spacing + MaxFieldWidth;
            float sliderW = Mathf.Max(SliderMinWidth, remain - rightNeeded);
            sliderRect = new Rect(x, row.y, sliderW, h);
            x = sliderRect.xMax + Spacing;

            minRect = new Rect(x, row.y, MinFieldWidth, h);
            x = minRect.xMax;

            tildeRect = new Rect(x, row.y, TildeWidth, h);
            x = tildeRect.xMax + Spacing;

            maxRect = new Rect(x, row.y, MaxFieldWidth, h);
        }
    }

    // ============================================
    // 확장 상태 기억 (프로퍼티별)
    // ============================================
    internal static class MMVExpandState
    {
        private static readonly Dictionary<string, bool> _expanded = new Dictionary<string, bool>();

        public static bool Get(string key)
        {
            bool v;
            return _expanded.TryGetValue(key, out v) && v;
        }

        public static void Set(string key, bool value)
        {
            _expanded[key] = value;
        }
    }

    // ============================================
    // 실수 벡터(Vector2/Vector3)용 베이스
    // ============================================
    public abstract class MinMaxValueFloatVectorBaseDrawer : PropertyDrawer
    {
        // 라벨 옆 토글 버튼 스타일
        static GUIStyle ToggleButtonStyle => new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            active = { textColor = Color.white }
        };

        protected abstract int   GetComponentCount();
        protected abstract float Get(SerializedProperty vecProp, int index);
        protected abstract void  Set(SerializedProperty vecProp, int index, float value);
        protected virtual string AxisName(int index) => index == 0 ? "X" : (index == 1 ? "Y" : "Z");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");
            var curProp = property.FindPropertyRelative("_current");

            var isMinProp = property.FindPropertyRelative("_isMin");
            var isMaxProp = property.FindPropertyRelative("_isMax");

            var overMin   = property.FindPropertyRelative("isOverMin");
            var overMax   = property.FindPropertyRelative("isOverMax");

            int comps = GetComponentCount();
            float lineH = EditorGUIUtility.singleLineHeight;

            // 1) 첫 줄: [라벨][토글]
            // PrefixLabel로 라벨을 그리고, contentRect 시작에 토글 버튼을 배치하면 "라벨 옆"에 보입니다.
            var labelRect = new Rect(position.x, position.y, position.width, lineH);
            Rect contentRect = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            // 라벨 옆 토글 버튼
            bool expanded = MMVExpandState.Get(property.propertyPath);
            var toggleBtnRect = new Rect(contentRect.x, labelRect.y, 20f, lineH);
            expanded = EditorGUI.Toggle(toggleBtnRect, expanded, ToggleButtonStyle);
            MMVExpandState.Set(property.propertyPath, expanded);

            // 2) 토글 펼치기면: [IsOverMin][IsOverMax] 라인
            if (expanded)
            {
                float x = contentRect.x + 25f;
                float wBox = 20f;

                // IsOverMin
                var minLabelSize = GUI.skin.label.CalcSize(new GUIContent(overMin.name));
                var minLabelRect = new Rect(x, labelRect.y, minLabelSize.x, lineH);
                EditorGUI.LabelField(minLabelRect, overMin.name);
                var minToggleRect = new Rect(minLabelRect.xMax, labelRect.y, wBox, lineH);
                overMin.boolValue = EditorGUI.Toggle(minToggleRect, overMin.boolValue);

                // Space
                x = minToggleRect.xMax + MMVLayout.Spacing;

                // IsOverMax
                var maxLabelSize = GUI.skin.label.CalcSize(new GUIContent(overMax.name));
                var maxLabelRect = new Rect(x, labelRect.y, maxLabelSize.x, lineH);
                EditorGUI.LabelField(maxLabelRect, overMax.name);
                var maxToggleRect = new Rect(maxLabelRect.xMax, labelRect.y, wBox, lineH);
                overMax.boolValue = EditorGUI.Toggle(maxToggleRect, overMax.boolValue);
            }
            float y = labelRect.yMax + MMVLayout.TopGap;
            // 3) 컴포넌트별 줄: [Axis][Cur][Slider][Min][~][Max]
            bool anyAtMin = false;
            bool anyAtMax = false;

            for (int i = 0; i < comps; ++i)
            {
                var row = new Rect(position.x, y, position.width, lineH);
                MMVLayout.CalcAxisRowRects(row, out var axisR, out var curR, out var sldR, out var minR, out var tilR, out var maxR);

                // 값 읽기
                float cur = Get(curProp, i);
                float mn  = Get(minProp, i);
                float mx  = Get(maxProp, i);

                // [Axis]
                EditorGUI.LabelField(axisR, AxisName(i));

                // [Cur] (float)
                cur = EditorGUI.FloatField(curR, cur);

                // [Slider]
                cur = GUI.HorizontalSlider(sldR, cur, mn, mx);

                // [Min]~[Max]
                mn = EditorGUI.FloatField(minR, mn);
                EditorGUI.LabelField(tilR, " ~ ");
                mx = EditorGUI.FloatField(maxR, mx);

                // Over 미허용이면 즉시 클램프
                bool touchMin = false, touchMax = false;
                if (!overMin.boolValue && cur < mn) { cur = mn; touchMin = true; }
                if (!overMax.boolValue && cur > mx) { cur = mx; touchMax = true; }

                anyAtMin |= touchMin || (!overMin.boolValue && cur <= mn);
                anyAtMax |= touchMax || (!overMax.boolValue && cur >= mx);

                // 반영
                Set(curProp, i, cur);
                Set(minProp, i, mn);
                Set(maxProp, i, mx);

                y += lineH + MMVLayout.TopGap;
            }

            // 4) 상태 플래그 (_isMin/_isMax) 갱신: 어느 축이라도 경계면 true
            isMinProp.boolValue = anyAtMin;
            isMaxProp.boolValue = anyAtMax;

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int comps = GetComponentCount();

            // 라벨 줄(1) + (펼침 줄:1 or 0) + 컴포넌트 줄(comps)
            int lines = 1 + comps;
            // 줄 사이 여백은 각 줄 뒤에 TopGap이 있으니, 총 (lines-1)회 발생
            return EditorGUIUtility.singleLineHeight * lines + MMVLayout.TopGap * (lines - 1);
        }
    }

    // Vector2
    [CustomPropertyDrawer(typeof(MinMaxValue<Vector2>))]
    public class MinMaxValueVector2Drawer : MinMaxValueFloatVectorBaseDrawer
    {
        protected override int GetComponentCount() => 2;
        protected override float Get(SerializedProperty vecProp, int index)
        {
            var v = vecProp.vector2Value;
            return index == 0 ? v.x : v.y;
        }
        protected override void Set(SerializedProperty vecProp, int index, float value)
        {
            var v = vecProp.vector2Value;
            if (index == 0) v.x = value; else v.y = value;
            vecProp.vector2Value = v;
        }
    }

    // Vector3
    [CustomPropertyDrawer(typeof(MinMaxValue<Vector3>))]
    public class MinMaxValueVector3Drawer : MinMaxValueFloatVectorBaseDrawer
    {
        protected override int GetComponentCount() => 3;
        protected override float Get(SerializedProperty vecProp, int index)
        {
            var v = vecProp.vector3Value;
            return index == 0 ? v.x : index == 1 ? v.y : v.z;
        }
        protected override void Set(SerializedProperty vecProp, int index, float value)
        {
            var v = vecProp.vector3Value;
            if (index == 0) v.x = value; else if (index == 1) v.y = value; else v.z = value;
            vecProp.vector3Value = v;
        }
    }

    // ============================================
    // 정수 벡터(Vector2Int/Vector3Int)용 베이스
    // ============================================
    public abstract class MinMaxValueIntVectorBaseDrawer : PropertyDrawer
    {
        static GUIStyle ToggleButtonStyle => new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            active = { textColor = Color.white }
        };

        protected abstract int GetComponentCount();
        protected abstract int Get(SerializedProperty vecProp, int index);
        protected abstract void Set(SerializedProperty vecProp, int index, int value);
        protected virtual string AxisName(int index) => index == 0 ? "X" : (index == 1 ? "Y" : "Z");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");
            var curProp = property.FindPropertyRelative("_current");

            var isMinProp = property.FindPropertyRelative("_isMin");
            var isMaxProp = property.FindPropertyRelative("_isMax");

            var overMin   = property.FindPropertyRelative("isOverMin");
            var overMax   = property.FindPropertyRelative("isOverMax");

            int comps = GetComponentCount();
            float lineH = EditorGUIUtility.singleLineHeight;

            // 1) [라벨][토글]
            var labelRect = new Rect(position.x, position.y, position.width, lineH);
            Rect contentRect = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            bool expanded = MMVExpandState.Get(property.propertyPath);
            var toggleBtnRect = new Rect(contentRect.x, labelRect.y, 20f, lineH);
            expanded = EditorGUI.Toggle(toggleBtnRect, expanded, ToggleButtonStyle);
            MMVExpandState.Set(property.propertyPath, expanded);


            // 2) 펼침 라인: [IsOverMin][IsOverMax]
            if (expanded)
            {
                float x = contentRect.x + 25f;
                float wBox = 20f;

                var minLabelSize = GUI.skin.label.CalcSize(new GUIContent(overMin.name));
                var minLabelRect = new Rect(x, labelRect.y, minLabelSize.x, lineH);
                EditorGUI.LabelField(minLabelRect, overMin.name);
                var minToggleRect = new Rect(minLabelRect.xMax, labelRect.y, wBox, lineH);
                overMin.boolValue = EditorGUI.Toggle(minToggleRect, overMin.boolValue);

                x = minToggleRect.xMax + MMVLayout.Spacing;

                var maxLabelSize = GUI.skin.label.CalcSize(new GUIContent(overMax.name));
                var maxLabelRect = new Rect(x, labelRect.y, maxLabelSize.x, lineH);
                EditorGUI.LabelField(maxLabelRect, overMax.name);
                var maxToggleRect = new Rect(maxLabelRect.xMax, labelRect.y, wBox, lineH);
                overMax.boolValue = EditorGUI.Toggle(maxToggleRect, overMax.boolValue);
            }
            float y = labelRect.yMax + MMVLayout.TopGap;

            // 3) 컴포넌트 줄
            bool anyAtMin = false;
            bool anyAtMax = false;

            for (int i = 0; i < comps; ++i)
            {
                var row = new Rect(position.x, y, position.width, lineH);
                MMVLayout.CalcAxisRowRects(row, out var axisR, out var curR, out var sldR, out var minR, out var tilR, out var maxR);

                int cur = Get(curProp, i);
                int mn  = Get(minProp, i);
                int mx  = Get(maxProp, i);

                EditorGUI.LabelField(axisR, AxisName(i));

                cur = EditorGUI.IntField(curR, cur);

                // Slider는 float 기반이므로 반올림
                float slid = GUI.HorizontalSlider(sldR, cur, mn, mx);
                cur = Mathf.RoundToInt(slid);

                mn = EditorGUI.IntField(minR, mn);
                EditorGUI.LabelField(tilR, " ~ ");
                mx = EditorGUI.IntField(maxR, mx);

                bool touchMin = false, touchMax = false;
                if (!overMin.boolValue && cur < mn) { cur = mn; touchMin = true; }
                if (!overMax.boolValue && cur > mx) { cur = mx; touchMax = true; }

                anyAtMin |= touchMin || (!overMin.boolValue && cur <= mn);
                anyAtMax |= touchMax || (!overMax.boolValue && cur >= mx);

                Set(curProp, i, cur);
                Set(minProp, i, mn);
                Set(maxProp, i, mx);

                y += lineH + MMVLayout.TopGap;
            }

            isMinProp.boolValue = anyAtMin;
            isMaxProp.boolValue = anyAtMax;

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int comps = GetComponentCount();

            int lines = 1 + comps;
            return EditorGUIUtility.singleLineHeight * lines + MMVLayout.TopGap * (lines - 1);
        }
    }

    // Vector2Int
    [CustomPropertyDrawer(typeof(MinMaxValue<Vector2Int>))]
    public class MinMaxValueVector2IntDrawer : MinMaxValueIntVectorBaseDrawer
    {
        protected override int GetComponentCount() => 2;
        protected override int Get(SerializedProperty vecProp, int index)
        {
            var v = vecProp.vector2IntValue;
            return index == 0 ? v.x : v.y;
        }
        protected override void Set(SerializedProperty vecProp, int index, int value)
        {
            var v = vecProp.vector2IntValue;
            if (index == 0) v.x = value; else v.y = value;
            vecProp.vector2IntValue = v;
        }
    }

    // Vector3Int
    [CustomPropertyDrawer(typeof(MinMaxValue<Vector3Int>))]
    public class MinMaxValueVector3IntDrawer : MinMaxValueIntVectorBaseDrawer
    {
        protected override int GetComponentCount() => 3;
        protected override int Get(SerializedProperty vecProp, int index)
        {
            var v = vecProp.vector3IntValue;
            return index == 0 ? v.x : index == 1 ? v.y : v.z;
        }
        protected override void Set(SerializedProperty vecProp, int index, int value)
        {
            var v = vecProp.vector3IntValue;
            if (index == 0) v.x = value;
            else if (index == 1) v.y = value;
            else v.z = value;
            vecProp.vector3IntValue = v;
        }
    }
}

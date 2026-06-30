using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    // 1줄: 프로퍼티 라벨 + Min/Max 헤더, 2줄: [min 필드] ~ [max 필드]
    public abstract class MinMaxNumberBaseDrawer : PropertyDrawer
    {
        const float GapX = 4f;
        const float GapMid = 8f;
        const float GapRow = 2f;
        const float TildeWidth = 16f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            float line = EditorGUIUtility.singleLineHeight;

            // 1줄: 프로퍼티 라벨 + Min/Max 헤더
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, line);
            EditorGUI.LabelField(labelRect, label);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float usable = position.width - GapX * 2 - TildeWidth - GapMid * 2;
            float halfW = usable * 0.5f;

            float hx = position.x + GapX;
            Rect minHeaderRect = new Rect(hx, position.y, halfW, line);
            Rect maxHeaderRect = new Rect(minHeaderRect.xMax + GapMid + TildeWidth + GapMid, position.y, halfW, line);
            EditorGUI.LabelField(minHeaderRect, "Min", EditorStyles.centeredGreyMiniLabel);
            EditorGUI.LabelField(maxHeaderRect, "Max", EditorStyles.centeredGreyMiniLabel);

            // 2줄: 값 필드
            float y = position.y + line + EditorGUIUtility.standardVerticalSpacing;

            double minVal, maxVal;
            Read(minProp, maxProp, out minVal, out maxVal);

            float fx = position.x + GapX;
            Rect minRect = new Rect(fx, y, halfW, line);
            fx += halfW + GapMid;
            Rect tildeRect = new Rect(fx, y, TildeWidth, line);
            fx += TildeWidth + GapMid;
            Rect maxRect = new Rect(fx, y, halfW, line);

            minVal = DrawValue(minRect, minVal);
            EditorGUI.LabelField(tildeRect, "~", EditorStyles.centeredGreyMiniLabel);
            maxVal = DrawValue(maxRect, maxVal);

            Write(minProp, maxProp, minVal, maxVal);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            return line + EditorGUIUtility.standardVerticalSpacing + line;
        }

        // ------ 타입별 훅 ------
        protected abstract void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax);
        protected abstract void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax);
        protected abstract double DrawValue(Rect rect, double value);
    }

    // int
    [CustomPropertyDrawer(typeof(MinMax<int>))]
    public class MinMaxIntDrawer : MinMaxNumberBaseDrawer
    {
        protected override void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax)
        { outMin = min.intValue; outMax = max.intValue; }

        protected override void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax)
        { min.intValue = (int)inMin; max.intValue = (int)inMax; }

        protected override double DrawValue(Rect rect, double value)
        { return EditorGUI.IntField(rect, (int)value); }
    }

    // long
    [CustomPropertyDrawer(typeof(MinMax<long>))]
    public class MinMaxLongDrawer : MinMaxNumberBaseDrawer
    {
        protected override void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax)
        {
#if UNITY_2021_2_OR_NEWER
            outMin = min.longValue; outMax = max.longValue;
#else
            outMin = min.longValue; outMax = max.longValue; // 폴백 동일 처리
#endif
        }

        protected override void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax)
        {
#if UNITY_2021_2_OR_NEWER
            min.longValue = (long)inMin; max.longValue = (long)inMax;
#else
            min.longValue = (long)inMin; max.longValue = (long)inMax;
#endif
        }

        protected override double DrawValue(Rect rect, double value)
        {
#if UNITY_2021_2_OR_NEWER
            return EditorGUI.LongField(rect, (long)value);
#else
            long v = (long)value;
            int shown = v > int.MaxValue ? int.MaxValue : (v < int.MinValue ? int.MinValue : (int)v);
            int typed = EditorGUI.IntField(rect, shown);
            return (long)typed;
#endif
        }
    }

    // float
    [CustomPropertyDrawer(typeof(MinMax<float>))]
    public class MinMaxFloatDrawer : MinMaxNumberBaseDrawer
    {
        protected override void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax)
        { outMin = min.floatValue; outMax = max.floatValue; }

        protected override void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax)
        { min.floatValue = (float)inMin; max.floatValue = (float)inMax; }

        protected override double DrawValue(Rect rect, double value)
        { return EditorGUI.FloatField(rect, (float)value); }
    }

    // double
    [CustomPropertyDrawer(typeof(MinMax<double>))]
    public class MinMaxDoubleDrawer : MinMaxNumberBaseDrawer
    {
        protected override void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax)
        {
#if UNITY_2022_1_OR_NEWER
            outMin = min.doubleValue; outMax = max.doubleValue;
#else
            // 폴백: float로 근사(정밀도 손실 가능)
            outMin = min.floatValue; outMax = max.floatValue;
#endif
        }

        protected override void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax)
        {
#if UNITY_2022_1_OR_NEWER
            min.doubleValue = inMin; max.doubleValue = inMax;
#else
            min.floatValue = (float)inMin; max.floatValue = (float)inMax;
#endif
        }

        protected override double DrawValue(Rect rect, double value)
        {
#if UNITY_2022_1_OR_NEWER
            return EditorGUI.DoubleField(rect, value);
#else
            return EditorGUI.FloatField(rect, (float)value);
#endif
        }
    }
}
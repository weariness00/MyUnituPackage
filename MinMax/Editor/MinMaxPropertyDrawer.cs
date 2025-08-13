using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace Weariness.Util.Editor
{
    /// 라벨과 같은 줄에 [Min] ~ [Max] 표시 (필드 내부 라벨 없음)
    public abstract class MinMaxNumberBaseDrawer : PropertyDrawer
    {
        const float GapX = 4f;         // 좌우 여백
        const float GapMid = 8f;       // ~ 좌우 여백
        const float TildeWidth = 16f;  // "~" 폭
        const float MinFieldWidth = 40f;
        const float MaxFieldWidth = 140f;
        const float CharWidth = 9f;    // 대략적 폰트 문자폭
        const float ExtraPad = 6f;     // 여유 폭

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            // 라벨을 그리면서, 라벨 오른쪽의 가용 영역을 얻는다
            Rect afterLabel = EditorGUI.PrefixLabel(position, label);

            // 값 읽기
            double minVal, maxVal;
            Read(minProp, maxProp, out minVal, out maxVal);

            float line = EditorGUIUtility.singleLineHeight;

            // 필드 폭 추정 (문자열 길이 기반)
            float wMin = Mathf.Clamp(EstimateWidthForValue(minVal), MinFieldWidth, MaxFieldWidth);
            float wMax = Mathf.Clamp(EstimateWidthForValue(maxVal), MinFieldWidth, MaxFieldWidth);

            // 가용 폭 내에서 배치
            float usable = afterLabel.width - GapX * 2 - TildeWidth - GapMid * 2;
            if (wMin + wMax > usable)
            {
                wMin = wMax = usable * 0.5f;
            }

            Rect row = new Rect(afterLabel.x, position.y, afterLabel.width, line);
            Rect minRect   = new Rect(row.x + GapX, row.y, wMin, row.height);
            Rect tildeRect = new Rect(minRect.xMax + GapMid, row.y, TildeWidth, row.height);
            Rect maxRect   = new Rect(tildeRect.xMax + GapMid, row.y, wMax, row.height);

            // 입력 (필드 라벨 없음)
            minVal = DrawValue(minRect, minVal);
            EditorGUI.LabelField(tildeRect, "~", EditorStyles.centeredGreyMiniLabel);
            maxVal = DrawValue(maxRect, maxVal);

            // 값 되쓰기
            Write(minProp, maxProp, minVal, maxVal);

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight; // 한 줄만 사용
        }

        // ------ 타입별 훅 ------
        protected abstract void Read(SerializedProperty min, SerializedProperty max, out double outMin, out double outMax);
        protected abstract void Write(SerializedProperty min, SerializedProperty max, double inMin, double inMax);
        protected abstract double DrawValue(Rect rect, double value);

        // 폭 추정: 부동소수는 소수 5자리까지만 고려(폭 과다 방지)
        protected virtual float EstimateWidthForValue(double v)
        {
            string s = FormatForWidth(v);
            return s.Length * CharWidth + ExtraPad;
        }

        protected virtual string FormatForWidth(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
            if (IsEffectivelyInteger(v)) return ((long)v).ToString(CultureInfo.InvariantCulture);
            return v.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        static bool IsEffectivelyInteger(double v)
            => System.Math.Abs(v - System.Math.Round(v)) < 1e-9;
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

        protected override string FormatForWidth(double v)
        { return ((long)v).ToString(CultureInfo.InvariantCulture); }
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
            // 아주 구버전 폴백: IntField 사용 (범위 주의)
            long v = (long)value;
            int shown = v > int.MaxValue ? int.MaxValue : (v < int.MinValue ? int.MinValue : (int)v);
            int typed = EditorGUI.IntField(rect, shown);
            return (long)typed;
#endif
        }

        protected override string FormatForWidth(double v)
        { return ((long)v).ToString(CultureInfo.InvariantCulture); }
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

        protected override string FormatForWidth(double v)
        { return ((float)v).ToString("0.#####", CultureInfo.InvariantCulture); }
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
            return EditorGUI.FloatField(rect, (float)value); // 폴백
#endif
        }

        protected override string FormatForWidth(double v)
        { return v.ToString("0.#####", CultureInfo.InvariantCulture); }
    }
}

using System;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    // ------------------------------------------------------------
    // 공통 레이아웃 유틸
    // ------------------------------------------------------------
    internal static class MMVScalarLayout
    {
        public const float Spacing      = 5f;
        public const float TildeWidth   = 15f;
        public const float ToggleWidth  = 20f;
        public const float SliderMinW   = 100f;
        public const float CharWidth    = 9f;   // 대략치
        public const float MinFieldW    = 25f;
        public const float MaxFieldW    = 100f;

        public static float FitByDigits(string s) =>
            Mathf.Clamp((string.IsNullOrEmpty(s) ? 1 : s.Length) * CharWidth + CharWidth, MinFieldW, MaxFieldW);

        public static void CalcRow(
            Rect row, float curW, float minW, float maxW, float toggleW,
            out Rect curR, out Rect sliderR, out Rect minR, out Rect tildeR, out Rect maxR, out Rect toggleR)
        {
            float h = row.height;
            float x = row.x;

            curR = new Rect(x, row.y, curW, h);
            x = curR.xMax + Spacing;

            // 남은 공간 중 [Min][~][Max][toggle] 제외한 영역을 슬라이더로
            float remain = row.xMax - x;
            float rightNeed = minW + TildeWidth + maxW + Spacing * 3 + toggleW; // Min + ~ + Max + 간격 + Toggle
            float sliderW = Mathf.Max(SliderMinW, remain - rightNeed);
            sliderR = new Rect(x, row.y, sliderW, h);
            x = sliderR.xMax + Spacing;

            minR = new Rect(x, row.y, minW, h);
            x = minR.xMax;
            tildeR = new Rect(x, row.y, TildeWidth, h);
            x = tildeR.xMax + Spacing;

            maxR = new Rect(x, row.y, maxW, h);
            x = maxR.xMax + Spacing;

            toggleR = new Rect(x, row.y, toggleW, h);
        }
        
        public static string FormatNumberForDisplay(double value)
        {
            // NaN이나 무한대 방어
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value.ToString();

            string str = value.ToString("G17"); // 최대 17자리 정밀도
            int dotIndex = str.IndexOf('.');

            if (dotIndex >= 0)
            {
                int decimalCount = str.Length - dotIndex - 1;
                if (decimalCount > 5)
                    return Math.Round(value, 5, MidpointRounding.AwayFromZero)
                        .ToString("0.#####"); // 5자리 제한
                else
                    return value.ToString("0." + new string('#', decimalCount));
            }
            return str;
        }
    }

    // ------------------------------------------------------------
    // 숫자 타입 구분
    // ------------------------------------------------------------
    public enum MMVKind { Float, Int, Double, Long }

    // ------------------------------------------------------------
    // 공통 베이스: MinMaxValue<T> (스칼라용: float/int/double/long)
    // ------------------------------------------------------------
    public abstract class MinMaxValueScalarBaseDrawer : PropertyDrawer
    {
        // 가변 폭: 슬라이더 드래그 중에는 폭을 고정해 깜빡임 방지
        private bool  _isDragging;
        private bool  _showOverToggle;
        private float _curW  = 10f, _minW = 10f, _maxW = 10f;

        protected abstract MMVKind Kind { get; }

        // ---- SerializedProperty <-> 값 액세스 (타입별로 오버라이드 없이 여기서 처리) ----
        double ReadCurrent(SerializedProperty p) => Kind switch
        {
            MMVKind.Float  => p.floatValue,
            MMVKind.Double => p.doubleValue,
            MMVKind.Int    => p.intValue,
            MMVKind.Long   => p.longValue,
            _ => 0
        };

        double ReadMin(SerializedProperty p) => ReadCurrent(p);
        double ReadMax(SerializedProperty p) => ReadCurrent(p);

        void WriteCurrent(SerializedProperty p, double v)
        {
            switch (Kind)
            {
                case MMVKind.Float:  p.floatValue  = (float)v; break;
                case MMVKind.Double: p.doubleValue = v;        break;
                case MMVKind.Int:    p.intValue    = (int)Math.Round(v);  break;
                case MMVKind.Long:   p.longValue   = (long)Math.Round(v); break;
            }
        }
        void WriteMin(SerializedProperty p, double v)  => WriteCurrent(p, v);
        void WriteMax(SerializedProperty p, double v)  => WriteCurrent(p, v);

        // ---- 필드 그리기 (타입별) ----
        double DrawScalarField(Rect r, double value)
        {
            switch (Kind)
            {
                case MMVKind.Float:  return EditorGUI.FloatField(r, (float)value);
                case MMVKind.Double: return EditorGUI.DoubleField(r, value);
                case MMVKind.Int:    return EditorGUI.IntField(r, (int)Math.Round(value));
                case MMVKind.Long:   return EditorGUI.LongField(r, (long)Math.Round(value));
                default:             return value;
            }
        }

        // ---- 슬라이더 (GUI.HorizontalSlider는 float 기반) ----
        static float ToFloatSafe(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return 0f;
            if (v > float.MaxValue) return float.MaxValue;
            if (v < float.MinValue) return float.MinValue;
            return (float)v;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 드래그 상태 파악
            if (Event.current.type == EventType.MouseDown) _isDragging = true;
            else if (Event.current.type == EventType.MouseUp) _isDragging = false;

            // 잡아올 프로퍼티
            var minP      = property.FindPropertyRelative("_min");
            var maxP      = property.FindPropertyRelative("_max");
            var curP      = property.FindPropertyRelative("_current");

            var isOverMin = property.FindPropertyRelative("isOverMin");
            var isOverMax = property.FindPropertyRelative("isOverMax");

            // 이전 값 저장(변화 감지)
            double prevMin = ReadMin(minP);
            double prevMax = ReadMax(maxP);
            double prevCur = ReadCurrent(curP);
            bool   prevOverMin = isOverMin.boolValue;
            bool   prevOverMax = isOverMax.boolValue;

            // 라벨 & prefix
            var labelRect   = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var contentRect = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            // 한 줄 행
            float h = EditorGUIUtility.singleLineHeight;
            var row = new Rect(contentRect.x, labelRect.y, contentRect.width, h);

            // 자릿수 기반 필드 폭(드래그 중엔 고정)
            if (!_isDragging)
            {
                _curW = MMVScalarLayout.FitByDigits(MMVScalarLayout.FormatNumberForDisplay(prevCur));
                _minW = MMVScalarLayout.FitByDigits(MMVScalarLayout.FormatNumberForDisplay(prevMin));
                _maxW = MMVScalarLayout.FitByDigits(MMVScalarLayout.FormatNumberForDisplay(prevMax));
            }

            // 레이아웃 계산
            MMVScalarLayout.CalcRow(
                row, _curW, _minW, _maxW, MMVScalarLayout.ToggleWidth,
                out Rect curR, out Rect sliderR, out Rect minR, out Rect tildeR, out Rect maxR, out Rect toggleR);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Current
            double cur = DrawScalarField(curR, prevCur);

            // Slider (float 기반)
            float s = GUI.HorizontalSlider(
                sliderR,
                ToFloatSafe(cur),
                ToFloatSafe(prevMin),
                ToFloatSafe(prevMax));
            cur = (Kind == MMVKind.Float || Kind == MMVKind.Double) ? s : Math.Round(s);

            // Min/Max
            double mn = DrawScalarField(minR, prevMin);
            EditorGUI.LabelField(tildeR, " ~ ");
            double mx = DrawScalarField(maxR, prevMax);

            // Over 허용 안 하면 즉시 클램프(표시 안정화)
            if (!isOverMin.boolValue && cur < mn) cur = mn;
            if (!isOverMax.boolValue && cur > mx) cur = mx;

            // [라벨 옆] Over 옵션 펼침 토글(버튼 느낌)
            _showOverToggle = EditorGUI.Toggle(toggleR, _showOverToggle, new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                active = { textColor = Color.white }
            });

            // 펼치면 바로 아래 줄에 IsOverMin/IsOverMax
            if (_showOverToggle)
            {
                var y2 = row.yMax + 5f;
                float box = 20f;

                // IsOverMin
                var minLbl = new GUIContent(isOverMin.name);
                float minW = GUI.skin.label.CalcSize(minLbl).x;
                var r = new Rect(contentRect.x, y2, minW, h);
                EditorGUI.LabelField(r, isOverMin.name);
                r = new Rect(r.xMax, y2, box, h);
                bool overMinNew = EditorGUI.Toggle(r, isOverMin.boolValue);

                // IsOverMax
                var maxLbl = new GUIContent(isOverMax.name);
                float maxW = GUI.skin.label.CalcSize(maxLbl).x;
                r = new Rect(r.xMax + MMVScalarLayout.Spacing, y2, maxW, h);
                EditorGUI.LabelField(r, isOverMax.name);
                r = new Rect(r.xMax, y2, box, h);
                bool overMaxNew = EditorGUI.Toggle(r, isOverMax.boolValue);

                // 변경 시 실제 struct에 반영
                if (overMinNew != prevOverMin || overMaxNew != prevOverMax)
                {
                    ApplyIsOverToTargets(property, overMinNew, overMaxNew);
                    property.serializedObject.Update();
                    // 최신값 다시 읽기
                    prevOverMin = isOverMin.boolValue;
                    prevOverMax = isOverMax.boolValue;
                }
            }

            // 값 변경 감지 → 실제 struct에 반영(프로퍼티 경유)
            bool changed =
                !NearlyEqual(prevCur, cur, Kind) ||
                !NearlyEqual(prevMin, mn, Kind) ||
                !NearlyEqual(prevMax, mx, Kind);

            if (changed)
            {
                ApplyValueToTargets(property, cur, mn, mx);
                property.serializedObject.Update();
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            // SerializedProperty를 직접 수정하지 않고(위에서 struct에 반영),
            // 마지막에 동기화만 유지.
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (_showOverToggle ? 2 : 1) + 5f;
        }

        static bool NearlyEqual(double a, double b, MMVKind kind)
        {
            if (kind == MMVKind.Float || kind == MMVKind.Double)
                return Math.Abs(a - b) < 1e-6;
            return Math.Abs(a - b) < 0.5; // 정수 계열은 반올림 기준
        }

        // ----- 실제 인스턴스(struct)에 반영: Min/Max/Current (프로퍼티 경유) -----
        void ApplyValueToTargets(SerializedProperty property, double newCur, double newMin, double newMax)
        {
            foreach (var obj in property.serializedObject.targetObjects)
            {
                Undo.RecordObject(obj, "Edit MinMaxValue");

                var boxed = fieldInfo.GetValue(obj);
                var t = boxed.GetType();

                if (t == typeof(MinMaxValue<float>))
                {
                    var mmv = (MinMaxValue<float>)boxed;
                    mmv.Min = (float)newMin; mmv.Max = (float)newMax; mmv.Current = (float)newCur;
                    fieldInfo.SetValue(obj, mmv);
                }
                else if (t == typeof(MinMaxValue<int>))
                {
                    var mmv = (MinMaxValue<int>)boxed;
                    mmv.Min = (int)Math.Round(newMin); mmv.Max = (int)Math.Round(newMax); mmv.Current = (int)Math.Round(newCur);
                    fieldInfo.SetValue(obj, mmv);
                }
#if UNITY_2021_2_OR_NEWER
                else if (t == typeof(MinMaxValue<double>))
                {
                    var mmv = (MinMaxValue<double>)boxed;
                    mmv.Min = newMin; mmv.Max = newMax; mmv.Current = newCur;
                    fieldInfo.SetValue(obj, mmv);
                }
                else if (t == typeof(MinMaxValue<long>))
                {
                    var mmv = (MinMaxValue<long>)boxed;
                    mmv.Min = (long)Math.Round(newMin); mmv.Max = (long)Math.Round(newMax); mmv.Current = (long)Math.Round(newCur);
                    fieldInfo.SetValue(obj, mmv);
                }
#endif
                EditorUtility.SetDirty(obj);
            }
        }

        // ----- isOverMin / isOverMax 반영 -----
        void ApplyIsOverToTargets(SerializedProperty property, bool overMin, bool overMax)
        {
            foreach (var obj in property.serializedObject.targetObjects)
            {
                Undo.RecordObject(obj, "Edit MinMaxValue (IsOver)");

                var boxed = fieldInfo.GetValue(obj);
                var t = boxed.GetType();

                if (t == typeof(MinMaxValue<float>))
                {
                    var mmv = (MinMaxValue<float>)boxed;
                    mmv.isOverMin = overMin; mmv.isOverMax = overMax;
                    fieldInfo.SetValue(obj, mmv);
                }
                else if (t == typeof(MinMaxValue<int>))
                {
                    var mmv = (MinMaxValue<int>)boxed;
                    mmv.isOverMin = overMin; mmv.isOverMax = overMax;
                    fieldInfo.SetValue(obj, mmv);
                }
#if UNITY_2021_2_OR_NEWER
                else if (t == typeof(MinMaxValue<double>))
                {
                    var mmv = (MinMaxValue<double>)boxed;
                    mmv.isOverMin = overMin; mmv.isOverMax = overMax;
                    fieldInfo.SetValue(obj, mmv);
                }
                else if (t == typeof(MinMaxValue<long>))
                {
                    var mmv = (MinMaxValue<long>)boxed;
                    mmv.isOverMin = overMin; mmv.isOverMax = overMax;
                    fieldInfo.SetValue(obj, mmv);
                }
#endif
                EditorUtility.SetDirty(obj);
            }
        }
    }

    // ------------------------------------------------------------
    // 타입 매핑 4종
    // ------------------------------------------------------------
    [CustomPropertyDrawer(typeof(MinMaxValue<float>))]
    public class MinMaxValueFloatDrawer : MinMaxValueScalarBaseDrawer
    {
        protected override MMVKind Kind => MMVKind.Float;
    }

    [CustomPropertyDrawer(typeof(MinMaxValue<int>))]
    public class MinMaxValueIntDrawer : MinMaxValueScalarBaseDrawer
    {
        protected override MMVKind Kind => MMVKind.Int;
    }

#if UNITY_2021_2_OR_NEWER
    [CustomPropertyDrawer(typeof(MinMaxValue<double>))]
    public class MinMaxValueDoubleDrawer : MinMaxValueScalarBaseDrawer
    {
        protected override MMVKind Kind => MMVKind.Double;
    }

    [CustomPropertyDrawer(typeof(MinMaxValue<long>))]
    public class MinMaxValueLongDrawer : MinMaxValueScalarBaseDrawer
    {
        protected override MMVKind Kind => MMVKind.Long;
    }
#endif
}

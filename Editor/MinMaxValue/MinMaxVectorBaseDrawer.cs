using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    public abstract class MinMaxVectorBaseDrawer : PropertyDrawer
    {
        const float GapX = 4f;
        const float GapMid = 8f;
        const float GapRow = 2f;
        const float TildeWidth = 16f;
        const float AxisLabelWidth = 18f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            int rows = GetComponentCount(minProp);
            float line = EditorGUIUtility.singleLineHeight;

            // 첫 줄: 프로퍼티 라벨 + Min/Max 컬럼 헤더
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, line);
            EditorGUI.LabelField(labelRect, label);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 헤더 위치 계산 (SplitRow와 동일한 레이아웃)
            float headerUsable = position.width - GapX * 2 - TildeWidth - GapMid * 2;
            float headerHalf = headerUsable * 0.5f;
            float headerX = position.x + GapX;
            Rect minHeaderRect = new Rect(headerX, position.y, headerHalf, line);
            Rect maxHeaderRect = new Rect(minHeaderRect.xMax + GapMid + TildeWidth + GapMid, position.y, headerHalf, line);
            EditorGUI.LabelField(minHeaderRect, "Min", EditorStyles.centeredGreyMiniLabel);
            EditorGUI.LabelField(maxHeaderRect, "Max", EditorStyles.centeredGreyMiniLabel);

            // 값 줄 시작 위치
            float y = position.y + line + EditorGUIUtility.standardVerticalSpacing;

            switch (minProp.propertyType)
            {
                case SerializedPropertyType.Vector2:
                {
                    Vector2 min = minProp.vector2Value;
                    Vector2 max = maxProp.vector2Value;

                    (min.x, max.x) = DrawRowFloat('X', min.x, max.x, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.y, max.y) = DrawRowFloat('Y', min.y, max.y, new Rect(position.x, y, position.width, line));

                    minProp.vector2Value = min;
                    maxProp.vector2Value = max;
                    break;
                }
                case SerializedPropertyType.Vector3:
                {
                    Vector3 min = minProp.vector3Value;
                    Vector3 max = maxProp.vector3Value;

                    (min.x, max.x) = DrawRowFloat('X', min.x, max.x, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.y, max.y) = DrawRowFloat('Y', min.y, max.y, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.z, max.z) = DrawRowFloat('Z', min.z, max.z, new Rect(position.x, y, position.width, line));

                    minProp.vector3Value = min;
                    maxProp.vector3Value = max;
                    break;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2Int min = minProp.vector2IntValue;
                    Vector2Int max = maxProp.vector2IntValue;

                    (min.x, max.x) = DrawRowInt('X', min.x, max.x, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.y, max.y) = DrawRowInt('Y', min.y, max.y, new Rect(position.x, y, position.width, line));

                    minProp.vector2IntValue = min;
                    maxProp.vector2IntValue = max;
                    break;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    Vector3Int min = minProp.vector3IntValue;
                    Vector3Int max = maxProp.vector3IntValue;

                    (min.x, max.x) = DrawRowInt('X', min.x, max.x, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.y, max.y) = DrawRowInt('Y', min.y, max.y, new Rect(position.x, y, position.width, line));
                    y += line + EditorGUIUtility.standardVerticalSpacing + GapRow;

                    (min.z, max.z) = DrawRowInt('Z', min.z, max.z, new Rect(position.x, y, position.width, line));

                    minProp.vector3IntValue = min;
                    maxProp.vector3IntValue = max;
                    break;
                }
                default:
                    EditorGUI.HelpBox(new Rect(position.x, y, position.width, line), "지원되지 않는 벡터 타입입니다.", MessageType.Warning);
                    break;
            }

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        // 축 라벨을 min 필드의 GUIContent로 전달 — indentLevel 영향 없이 확실히 표시됨
        (float, float) DrawRowFloat(char axis, float min, float max, Rect row)
        {
            SplitRow(row, out Rect leftRect, out Rect tildeRect, out Rect rightRect);
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = AxisLabelWidth;
            min = EditorGUI.FloatField(leftRect, new GUIContent(axis.ToString()), min);
            EditorGUI.LabelField(tildeRect, "~", EditorStyles.centeredGreyMiniLabel);
            max = EditorGUI.FloatField(rightRect, max);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            return (min, max);
        }

        (int, int) DrawRowInt(char axis, int min, int max, Rect row)
        {
            SplitRow(row, out Rect leftRect, out Rect tildeRect, out Rect rightRect);
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = AxisLabelWidth;
            min = EditorGUI.IntField(leftRect, new GUIContent(axis.ToString()), min);
            EditorGUI.LabelField(tildeRect, "~", EditorStyles.centeredGreyMiniLabel);
            max = EditorGUI.IntField(rightRect, max);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            return (min, max);
        }

        void SplitRow(Rect row, out Rect left, out Rect tilde, out Rect right)
        {
            float usableW = row.width - GapX * 2 - TildeWidth - GapMid * 2;
            float halfW = usableW * 0.5f;
            left = new Rect(row.x + GapX, row.y, halfW, row.height);
            tilde = new Rect(left.xMax + GapMid, row.y, TildeWidth, row.height);
            right = new Rect(tilde.xMax + GapMid, row.y, halfW, row.height);
        }

        int GetComponentCount(SerializedProperty vecProp)
        {
            return vecProp.propertyType switch
            {
                SerializedPropertyType.Vector2 => 2,
                SerializedPropertyType.Vector2Int => 2,
                SerializedPropertyType.Vector3 => 3,
                SerializedPropertyType.Vector3Int => 3,
                _ => 0
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var minProp = property.FindPropertyRelative("_min");
            int rows = GetComponentCount(minProp);
            if (rows <= 0) return EditorGUIUtility.singleLineHeight;

            float line = EditorGUIUtility.singleLineHeight;
            // 라벨 한 줄 + 값 줄 n개
            return line + EditorGUIUtility.standardVerticalSpacing +
                   rows * line + (rows - 1) * (EditorGUIUtility.standardVerticalSpacing + GapRow);
        }
    }

    [CustomPropertyDrawer(typeof(MinMax<Vector2>))]   public class MinMaxVector2Drawer : MinMaxVectorBaseDrawer { }
    [CustomPropertyDrawer(typeof(MinMax<Vector3>))]   public class MinMaxVector3Drawer : MinMaxVectorBaseDrawer { }
    [CustomPropertyDrawer(typeof(MinMax<Vector2Int>))]public class MinMaxVector2IntDrawer : MinMaxVectorBaseDrawer { }
    [CustomPropertyDrawer(typeof(MinMax<Vector3Int>))]public class MinMaxVector3IntDrawer : MinMaxVectorBaseDrawer { }
}
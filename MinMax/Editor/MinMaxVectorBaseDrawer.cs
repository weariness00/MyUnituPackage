using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Editor
{
    // =========================
    // Vector2 / Vector3 / Vector2Int / Vector3Int (2줄 고정)
    // =========================

    public abstract class MinMaxVectorBaseDrawer : PropertyDrawer
    {
        const float MiniLabelWidth = 34f; // "Min", "Max" 라벨
        const float GapY = 2f;            // 줄 간격

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");

            // 전체 라벨
            position = EditorGUI.PrefixLabel(position, label);

            float line = EditorGUIUtility.singleLineHeight;

            // 첫째 줄: Min
            var minRow = new Rect(position.x, position.y, position.width, line);
            // 둘째 줄: Max
            var maxRow = new Rect(position.x, position.y + line + EditorGUIUtility.standardVerticalSpacing + GapY, position.width, line);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            DrawLabeledVectorField(minRow, "Min", minProp);
            DrawLabeledVectorField(maxRow, "Max", maxProp);

            EditorGUI.indentLevel = oldIndent;

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        void DrawLabeledVectorField(Rect row, string miniLabel, SerializedProperty prop)
        {
            // 왼쪽 작은 라벨
            var labelRect = new Rect(row.x, row.y, MiniLabelWidth, row.height);
            EditorGUI.LabelField(labelRect, miniLabel);

            // 필드 영역
            var fieldRect = new Rect(labelRect.xMax + 4f, row.y, row.width - MiniLabelWidth - 4f, row.height);

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = EditorGUI.Vector2Field(fieldRect, GUIContent.none, prop.vector2Value);
                    break;

                case SerializedPropertyType.Vector3:
                    prop.vector3Value = EditorGUI.Vector3Field(fieldRect, GUIContent.none, prop.vector3Value);
                    break;

                case SerializedPropertyType.Vector2Int:
#if UNITY_2021_2_OR_NEWER
                    prop.vector2IntValue = EditorGUI.Vector2IntField(fieldRect, GUIContent.none, prop.vector2IntValue);
#else
                    // 구버전 폴백: 요소별 입력
                    var v2i = prop.vector2IntValue;
                    float w2 = fieldRect.width * 0.5f;
                    v2i.x = EditorGUI.IntField(new Rect(fieldRect.x + 0 * w2, fieldRect.y, w2 - 2, fieldRect.height), v2i.x);
                    v2i.y = EditorGUI.IntField(new Rect(fieldRect.x + 1 * w2 + 2, fieldRect.y, w2 - 2, fieldRect.height), v2i.y);
                    prop.vector2IntValue = v2i;
#endif
                    break;

                case SerializedPropertyType.Vector3Int:
#if UNITY_2021_2_OR_NEWER
                    prop.vector3IntValue = EditorGUI.Vector3IntField(fieldRect, GUIContent.none, prop.vector3IntValue);
#else
                    // 구버전 폴백: 요소별 입력
                    var v3i = prop.vector3IntValue;
                    float w3 = fieldRect.width / 3f;
                    v3i.x = EditorGUI.IntField(new Rect(fieldRect.x + 0 * w3, fieldRect.y, w3 - 2, fieldRect.height), v3i.x);
                    v3i.y = EditorGUI.IntField(new Rect(fieldRect.x + 1 * w3, fieldRect.y, w3 - 2, fieldRect.height), v3i.y);
                    v3i.z = EditorGUI.IntField(new Rect(fieldRect.x + 2 * w3, fieldRect.y, w3 - 2, fieldRect.height), v3i.z);
                    prop.vector3IntValue = v3i;
#endif
                    break;

                default:
                    EditorGUI.HelpBox(fieldRect, "지원되지 않는 벡터 타입", MessageType.Warning);
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 2줄 + 간격 고정
            return (EditorGUIUtility.singleLineHeight * 2)
                 + EditorGUIUtility.standardVerticalSpacing
                 + GapY;
        }
    }

    // 아래 4개는 '타입 매핑'만 담당하는 얇은 래퍼
    [CustomPropertyDrawer(typeof(MinMax<Vector2>))]
    public class MinMaxVector2Drawer : MinMaxVectorBaseDrawer { }

    [CustomPropertyDrawer(typeof(MinMax<Vector3>))]
    public class MinMaxVector3Drawer : MinMaxVectorBaseDrawer { }

    [CustomPropertyDrawer(typeof(MinMax<Vector2Int>))]
    public class MinMaxVector2IntDrawer : MinMaxVectorBaseDrawer { }

    [CustomPropertyDrawer(typeof(MinMax<Vector3Int>))]
    public class MinMaxVector3IntDrawer : MinMaxVectorBaseDrawer { }
}

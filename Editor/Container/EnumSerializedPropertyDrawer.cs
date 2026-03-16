using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Container.Editor
{
    [CustomPropertyDrawer(typeof(EnumSerialized<>))]
    public class EnumSerializedPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var valueProp = property.FindPropertyRelative("value");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, valueProp, label);
            bool inspectorChanged = EditorGUI.EndChangeCheck();

            // 1) Inspector에서 변경되었을 때
            // 2) 외부 코드에서 value만 바뀌어 문자열과 불일치할 때
            if (inspectorChanged)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}
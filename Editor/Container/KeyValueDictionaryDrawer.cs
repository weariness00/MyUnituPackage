using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Weariness.Util.Container.Editor
{
    [CustomPropertyDrawer(typeof(KeyValueDictionary<,>), true)]
    public class KeyValueDictionaryDrawer : PropertyDrawer
    {
        private bool isFoldout = false; // 전체 폴드 상태
        private readonly Dictionary<string, ReorderableList> _lists = new();
        private readonly Dictionary<string, List<bool>> _elemFoldouts = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (isFoldout == false)
                return EditorGUIUtility.singleLineHeight;

            EnsureSync(property);
            var list = GetOrCreateList(property, label);
            return list.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureSync(property);
            var list = GetOrCreateList(property, label);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            isFoldout = EditorGUI.Foldout(foldoutRect, isFoldout, label, true);
            if (isFoldout == false) return;
            list.DoList(position);
        }

        // ---------------- internal ----------------

        private ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label)
        {
            if (_lists.TryGetValue(property.propertyPath, out var cached))
                return cached;

            var keysProp = property.FindPropertyRelative("Keys");
            var valuesProp = property.FindPropertyRelative("Values");
            var list = new ReorderableList(property.serializedObject, keysProp, true, true, true, true);

            list.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, label); };

            list.onAddCallback = l =>
            {
                int idx = keysProp.arraySize;
                keysProp.InsertArrayElementAtIndex(idx);
                valuesProp.InsertArrayElementAtIndex(idx);
                SetDefaultNull(keysProp.GetArrayElementAtIndex(idx));
                SetDefaultNull(valuesProp.GetArrayElementAtIndex(idx));

                var folds = EnsureElemFoldouts(property, keysProp.arraySize);
                if (idx >= 0 && idx < folds.Count) folds[idx] = true; // 새 원소는 펼침

                property.serializedObject.ApplyModifiedProperties();
            };

            list.onRemoveCallback = l =>
            {
                int i = l.index;
                if (i >= 0 && i < keysProp.arraySize)
                {
                    keysProp.DeleteArrayElementAtIndex(i);
                    valuesProp.DeleteArrayElementAtIndex(i);

                    var folds = EnsureElemFoldouts(property, keysProp.arraySize);
                    if (i >= 0 && i < folds.Count) folds.RemoveAt(i);

                    property.serializedObject.ApplyModifiedProperties();
                }
            };

            list.onReorderCallbackWithDetails = (l, oldIndex, newIndex) =>
            {
                MoveArrayElement(valuesProp, oldIndex, newIndex);

                // 폴드 상태도 같이 이동
                var folds = EnsureElemFoldouts(property, keysProp.arraySize);
                if (oldIndex >= 0 && oldIndex < folds.Count && newIndex >= 0 && newIndex < folds.Count)
                {
                    bool temp = folds[oldIndex];
                    if (oldIndex < newIndex)
                    {
                        for (int i = oldIndex; i < newIndex; i++)
                            folds[i] = folds[i + 1];
                    }
                    else
                    {
                        for (int i = oldIndex; i > newIndex; i--)
                            folds[i] = folds[i - 1];
                    }

                    folds[newIndex] = temp;
                }
            };

            list.elementHeightCallback = index =>
            {
                if (index < 0 || index >= keysProp.arraySize)
                    return EditorGUIUtility.singleLineHeight;

                var folds = EnsureElemFoldouts(property, keysProp.arraySize);
                bool open = (index >= 0 && index < folds.Count) ? folds[index] : true;

                float line = EditorGUIUtility.singleLineHeight;
                float vsp = EditorGUIUtility.standardVerticalSpacing;

                // 헤더(폴드 토글) 라인
                float height = line + vsp;

                if (!open) return height + 2f;

                // 펼쳤다면 Key / sep / Value
                var keyElem = keysProp.GetArrayElementAtIndex(index);
                var valElem = valuesProp.GetArrayElementAtIndex(index);

                float keyH = EditorGUI.GetPropertyHeight(keyElem, includeChildren: true);
                float valH = EditorGUI.GetPropertyHeight(valElem, includeChildren: true);

                height += keyH + vsp;
                height += valH + vsp + 2f;
                return height;
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index < 0 || index >= keysProp.arraySize)
                    return;

                var folds = EnsureElemFoldouts(property, keysProp.arraySize);
                var keyElem = keysProp.GetArrayElementAtIndex(index);
                var valElem = valuesProp.GetArrayElementAtIndex(index);

                float line = EditorGUIUtility.singleLineHeight;
                float vsp = EditorGUIUtility.standardVerticalSpacing;

                // 헤더(폴드 토글 + 간단한 레이블)
                var headerRect = new Rect(rect.x, rect.y + 2f, rect.width, line);

                // Key 미리보기 (가능하면 한 줄로)
                string preview = GetInlinePreview(keyElem);
                GUIContent head = string.IsNullOrEmpty(preview)
                    ? new GUIContent($"Element {index}")
                    : new GUIContent($"Element {index}  —  {preview}");

                folds[index] = EditorGUI.Foldout(headerRect, folds[index], head, true);

                if (!folds[index]) return;

                float y = headerRect.yMax + vsp;
                var labelWidthBackup = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 60f;

                // [Key]
                float keyH = EditorGUI.GetPropertyHeight(keyElem, true);
                var keyRect = new Rect(rect.x, y, rect.width, keyH);
                EditorGUI.PropertyField(keyRect, keyElem, new GUIContent("Key"), true);
                y = keyRect.yMax + vsp;

                // [Value]
                float valH = EditorGUI.GetPropertyHeight(valElem, true);
                var valRect = new Rect(rect.x, y, rect.width, valH);
                EditorGUI.PropertyField(valRect, valElem, new GUIContent("Value"), true);

                EditorGUIUtility.labelWidth = labelWidthBackup;
            };

            _lists[property.propertyPath] = list;
            return list;
        }

        private List<bool> EnsureElemFoldouts(SerializedProperty property, int size)
        {
            if (!_elemFoldouts.TryGetValue(property.propertyPath, out var folds))
            {
                folds = new List<bool>(size);
                _elemFoldouts[property.propertyPath] = folds;
            }

            while (folds.Count < size) folds.Add(true); // 기본 펼침
            while (folds.Count > size) folds.RemoveAt(folds.Count - 1);
            return folds;
        }

        private string GetInlinePreview(SerializedProperty prop)
        {
            // 간단 프리뷰: 프리미티브/문자열/객체 이름 등
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: return prop.intValue.ToString();
                case SerializedPropertyType.Float: return prop.floatValue.ToString("g5");
                case SerializedPropertyType.String: return prop.stringValue;
                case SerializedPropertyType.Boolean: return prop.boolValue ? "true" : "false";
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue ? prop.objectReferenceValue.name : "null";
                case SerializedPropertyType.Enum:
                    return prop.enumDisplayNames != null && prop.enumValueIndex >= 0 && prop.enumValueIndex < prop.enumDisplayNames.Length
                        ? prop.enumDisplayNames[prop.enumValueIndex]
                        : $"Enum {prop.enumValueIndex}";
                default:
                    return string.Empty;
            }
        }

        private void EnsureSync(SerializedProperty property)
        {
            var keys = property.FindPropertyRelative("Keys");
            var values = property.FindPropertyRelative("Values");
            if (keys == null || values == null) return;

            while (keys.arraySize < values.arraySize)
                keys.InsertArrayElementAtIndex(keys.arraySize);
            while (values.arraySize < keys.arraySize)
                values.InsertArrayElementAtIndex(values.arraySize);
        }

        private void MoveArrayElement(SerializedProperty arrayProp, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            arrayProp.serializedObject.Update();
            arrayProp.MoveArrayElement(oldIndex, newIndex);
            arrayProp.serializedObject.ApplyModifiedProperties();
        }

        private void SetDefaultNull(SerializedProperty elem)
        {
            if (elem == null) return;
            switch (elem.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    elem.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.String:
                    elem.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Integer:
                    elem.intValue = default;
                    break;
                case SerializedPropertyType.Float:
                    elem.floatValue = default;
                    break;
                case SerializedPropertyType.Boolean:
                    elem.boolValue = default;
                    break;
                // Generic/복합 타입은 Unity 기본값 유지
            }
        }
    }
}
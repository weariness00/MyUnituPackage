using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Weariness.Util.Extensions.Editor;

namespace Weariness.Util.Editor
{
    [CustomPropertyDrawer(typeof(Stat))]
    public class StatDrawer : PropertyDrawer
    {
        private static readonly string ValueKey = "baseValue";
        private static readonly string ModifierContainerKey = "modifierContainer";
        private static readonly string GetValueKey = "GetValue";

        // 그룹별 펼침 상태 저장: "<propertyPath>/group:i" -> bool
        private readonly Dictionary<string, bool> _groupExpanded = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var baseValueProp = property.FindPropertyRelative(ValueKey);
            var modifiersProp = property.FindPropertyRelative(ModifierContainerKey);

            float lineH = EditorGUIUtility.singleLineHeight;
            float spacing = 6f;

            // 라벨/컨텐츠 분리
            Rect rowRect = new Rect(position.x, position.y, position.width, lineH);
            Rect labelRect = EditorGUI.IndentedRect(new Rect(rowRect.x, rowRect.y, EditorGUIUtility.labelWidth, lineH));
            Rect contentRect = new Rect(labelRect.xMax, rowRect.y, rowRect.xMax - labelRect.xMax, lineH);

            // 라벨 클릭으로만 토글
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);

            // 오른쪽 한 줄: Value / baseValue / RealValue
            float valueLabelW = 40f;
            float baseValueW = 60f;

            Rect baseLabelRect = new Rect(contentRect.x, contentRect.y, valueLabelW, lineH);
            Rect baseValueRect = new Rect(baseLabelRect.xMax + spacing, contentRect.y, baseValueW, lineH);
            Rect realValueRect = new Rect(baseValueRect.xMax + spacing, contentRect.y,
                contentRect.xMax - (baseValueRect.xMax + spacing), lineH);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(baseLabelRect, "Value");
            EditorGUI.PropertyField(baseValueRect, baseValueProp, GUIContent.none);

            if (property.GetTargetObjectOfProperty() is Stat statObj)
            {
                var mi = typeof(Stat).GetMethod(GetValueKey, BindingFlags.Instance | BindingFlags.NonPublic);
                if (mi != null)
                {
                    float realValue = (float)mi.Invoke(statObj, Array.Empty<object>());
                    EditorGUI.LabelField(realValueRect, $"Real Value: {realValue}");
                }
            }

            EditorGUI.EndProperty();

            if (!property.isExpanded || modifiersProp == null)
                return;

            // y 진행
            float y = position.y + lineH + 2f;
            float x = position.x + 15f;
            float w = position.width - 15f;

            // enum 이름들 확보 (배열이 비어도 enumNames는 가져올 수 있음: 요소 하나 임시 접근 방지용 체크)
            string[] enumNames = null;
            int enumCount = 0;
            // enum 정보 얻기 위해 임시로 첫 요소의 type 접근
            if (modifiersProp.arraySize > 0)
            {
                var firstElem = modifiersProp.GetArrayElementAtIndex(0);
                var typeProp = firstElem.FindPropertyRelative("type");
                enumNames = typeProp.enumDisplayNames;
                enumCount = enumNames?.Length ?? 0;
            }
            else
            {
                // 💡 배열이 비어도 enum 타입에서 직접 이름을 가져와 그룹 헤더(+버튼)만 그린다
                enumNames = Enum.GetNames(typeof(StatModifier.ModifierType));
                enumCount = enumNames.Length;
            }

            // 각 enum 값별 그룹 렌더링
            for (int gi = 0; gi < enumCount; gi++)
            {
                string gKey = $"{modifiersProp.propertyPath}/group:{gi}";
                if (!_groupExpanded.ContainsKey(gKey)) _groupExpanded[gKey] = true;

                // 그룹에 속하는 요소 인덱스 수집 (비어있으면 0개)
                List<int> groupIndices = new();
                for (int i = 0; i < modifiersProp.arraySize; i++)
                {
                    var elem = modifiersProp.GetArrayElementAtIndex(i);
                    var t = elem.FindPropertyRelative("type");
                    if (t.enumValueIndex == gi)
                        groupIndices.Add(i);
                }

                // 그룹 헤더 (Foldout + 라벨 + 카운트 + [+] 버튼)
                Rect headerRect = new Rect(x, y, w, lineH);
                float arrowWidth = 16f;
                Rect arrowRect = new Rect(headerRect.x, headerRect.y, arrowWidth, lineH);
                _groupExpanded[gKey] = EditorGUI.Foldout(arrowRect, _groupExpanded[gKey], GUIContent.none, false);

                var enumName = enumNames[gi];

                // 합계 표시는 요소가 없으면 0으로
                float propSum = 0f;
                foreach (var idx in groupIndices)
                {
                    var e = modifiersProp.GetArrayElementAtIndex(idx);
                    propSum += e.FindPropertyRelative("value").floatValue;
                }

                bool isPercent = enumName == StatModifier.ModifierType.Percent.ToString();
                string headerLabel = $"{enumName} ({groupIndices.Count}), Sum({propSum * (isPercent ? 100 : 1)}{(isPercent ? "%" : "")})";

                Rect labelR = new Rect(arrowRect.xMax + 2f, headerRect.y, headerRect.width - arrowWidth - 50f, lineH);
                EditorGUI.LabelField(labelR, headerLabel, EditorStyles.boldLabel);

                // [+] 버튼: 해당 타입 요소 추가 (배열이 0개여도 동작)
                Rect addBtn = new Rect(headerRect.xMax - 22f, headerRect.y, 22f, lineH);
                if (GUI.Button(addBtn, "+"))
                {
                    int newIndex = modifiersProp.arraySize;
                    modifiersProp.arraySize++;
                    modifiersProp.serializedObject.ApplyModifiedProperties();

                    var newElem = modifiersProp.GetArrayElementAtIndex(newIndex);
                    newElem.FindPropertyRelative("type").enumValueIndex = gi;
                    modifiersProp.serializedObject.ApplyModifiedProperties();

                    _groupExpanded[gKey] = true;
                }

                y += lineH + 2f;

                // 펼침: 그룹 요소 렌더링 (없으면 스킵)
                if (_groupExpanded[gKey] && groupIndices.Count > 0)
                {
                    for (int k = 0; k < groupIndices.Count; k++)
                    {
                        int realIndex = groupIndices[k];
                        var elem = modifiersProp.GetArrayElementAtIndex(realIndex);

                        float elemH = EditorGUI.GetPropertyHeight(elem, true);
                        Rect elemRect = new Rect(x, y, w - 18f, elemH);
                        EditorGUI.PropertyField(elemRect, elem, true);

                        Rect delRect = new Rect(elemRect.xMax, elemRect.y, 18f, lineH);
                        if (GUI.Button(delRect, "x"))
                        {
                            modifiersProp.DeleteArrayElementAtIndex(realIndex);
                            modifiersProp.serializedObject.ApplyModifiedProperties();
                            break;
                        }

                        y += elemH + 2f;
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight + 2f;
            if (!property.isExpanded) return h;

            var modifiersProp = property.FindPropertyRelative(ModifierContainerKey);
            float lineH = EditorGUIUtility.singleLineHeight;

            // 💡 배열이 0개여도 enumNames를 enum 타입에서 직접 구해 헤더 높이 계산
            int enumCount;
            if (modifiersProp != null && modifiersProp.arraySize > 0)
            {
                var firstElem = modifiersProp.GetArrayElementAtIndex(0);
                var typeProp = firstElem.FindPropertyRelative("type");
                enumCount = typeProp.enumDisplayNames?.Length ?? 0;
            }
            else
            {
                enumCount = Enum.GetNames(typeof(StatModifier.ModifierType)).Length;
            }

            // 그룹 헤더 높이
            h += (lineH + 2f) * enumCount;

            // 배열이 비어있으면 요소 높이는 없음
            if (modifiersProp == null || modifiersProp.arraySize == 0)
                return h;

            // 펼친 그룹의 요소 높이 합산
            for (int gi = 0; gi < enumCount; gi++)
            {
                string gKey = $"{modifiersProp.propertyPath}/group:{gi}";
                bool expanded = _groupExpanded.TryGetValue(gKey, out var val) ? val : true;

                if (!expanded) continue;

                for (int i = 0; i < modifiersProp.arraySize; i++)
                {
                    var elem = modifiersProp.GetArrayElementAtIndex(i);
                    if (elem.FindPropertyRelative("type").enumValueIndex == gi)
                    {
                        h += EditorGUI.GetPropertyHeight(elem, true) + 2f;
                    }
                }
            }

            return h;
        }
    }
}
// WrappingDrawer.cs
// Wrapping<T>의 인스펙터 표시:
// - 바깥 필드(MyWrappingField)는 폴드아웃(▶) 유지
// - 내부 data 라벨/토글은 숨기고 자식만 인라인으로 표시

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Weariness.Util.Container.Editor
{
    [CustomPropertyDrawer(typeof(Wrapping<>))]
    public class WrappingDrawer : PropertyDrawer
    {
        const float V_SPACING = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dataProp = property.FindPropertyRelative("data");
            EditorGUI.BeginProperty(position, label, property);

            if (IsComposite(dataProp))
            {
                // 1) 바깥 필드 라벨을 폴드아웃으로 그린다 (data 토글은 그리지 않음)
                var header = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, label, true);

                if (property.isExpanded)
                {
                    int prevIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = prevIndent + 1;

                    float y = header.yMax + V_SPACING;

// 2) OnGUI의 리스트 그리기 부분 교체
                    if (dataProp.isArray)
                    {
                        var list = GetList(property.serializedObject, dataProp);

                        // 리스트가 차지할 정확한 높이 계산
                        float listH = CalcListHeight(list, dataProp);

                        // 해당 영역에 바로 그리기(레이아웃 사용 X)
                        var listRect = new Rect(position.x, y, position.width, listH);
                        list.DoList(listRect);

                        // y 진행
                        y = listRect.yMax + V_SPACING;
                    }
                    else
                    {
                        // (기존) 구조체/클래스 자식만 인라인
                        foreach (var child in EnumerateChildren(dataProp))
                        {
                            float h = EditorGUI.GetPropertyHeight(child, true);
                            var r = new Rect(position.x, y, position.width, h);
                            EditorGUI.PropertyField(r, child, true);
                            y += h + V_SPACING;
                        }
                    }

                    EditorGUI.indentLevel = prevIndent;
                }

            }
            else
            {
                // 스칼라/오브젝트 등은 한 줄로: 라벨은 바깥 필드 이름, 값은 data
                EditorGUI.PropertyField(position, dataProp, label, includeChildren: true);
            }

            EditorGUI.EndProperty();
        }

        // 3) GetPropertyHeight에서도 동일 계산 적용
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var dataProp = property.FindPropertyRelative("data");

            if (IsComposite(dataProp))
            {
                float h = EditorGUIUtility.singleLineHeight; // 폴드아웃 헤더 한 줄

                if (property.isExpanded)
                {
                    h += V_SPACING;

                    if (dataProp.isArray)
                    {
                        var list = GetList(property.serializedObject, dataProp);
                        h += CalcListHeight(list, dataProp); // 리스트 높이 그대로 반영
                        h += V_SPACING;
                    }
                    else
                    {
                        foreach (var child in EnumerateChildren(dataProp))
                            h += EditorGUI.GetPropertyHeight(child, true) + V_SPACING;
                    }
                }

                return h;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(dataProp, true);
            }
        }


        // 배열/리스트/구조체/클래스 등 자식이 있는 타입 판별
        static bool IsComposite(SerializedProperty p)
        {
            return p.isArray || (p.propertyType == SerializedPropertyType.Generic && p.hasVisibleChildren);
        }

        // parent의 직계 자식만 열거 (parent 자신은 그리지 않음)
        static IEnumerable<SerializedProperty> EnumerateChildren(SerializedProperty parent)
        {
            var it = parent.Copy();
            var end = it.GetEndProperty();

            bool enterChildren = true;
            while (it.NextVisible(enterChildren) && !SerializedProperty.EqualContents(it, end))
            {
                yield return it.Copy();
                enterChildren = false;
            }
        }
        
        // 필드에 추가: (드로어 클래스 내부)
        readonly Dictionary<string, ReorderableList> _lists = new();

        // 헬퍼: 캐시된 ReorderableList 가져오기
        ReorderableList GetList(SerializedObject so, SerializedProperty arrayProp)
        {
            var key = arrayProp.propertyPath;
            if (_lists.TryGetValue(key, out var cached)) return cached;

            var list = new ReorderableList(so, arrayProp,
                draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true);

            list.headerHeight = 0f; // 헤더(=라벨/폴드) 제거 → data 라벨/토글 없음
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = arrayProp.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUI.GetPropertyHeight(element, true);
                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };
            list.elementHeightCallback = index =>
            {
                var element = arrayProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 4f;
            };

            _lists[key] = list;
            return list;
        }

        // 1) 리스트 높이 계산 유틸 추가
        float CalcListHeight(ReorderableList list, SerializedProperty arrayProp)
        {
            // 헤더는 이미 0으로 설정했지만 혹시 몰라 포함
            float h = list.headerHeight;

            // 요소 높이 합산 (elementHeightCallback과 동일 로직을 써야 일치)
            if (arrayProp.arraySize > 0)
            {
                for (int i = 0; i < arrayProp.arraySize; i++)
                {
                    var elem = arrayProp.GetArrayElementAtIndex(i);
                    h += EditorGUI.GetPropertyHeight(elem, includeChildren: true) + 6f; // drawElementCallback과 동일 보정
                }

                h += 2f;
            }
            else
            {
                // 빈 리스트일 때 최소 한 줄은 보여주도록(유니티 기본과 유사하게)
                h += EditorGUIUtility.singleLineHeight + 6f;
            }

            // 풋터( + / – 버튼 줄 )
            h += list.footerHeight;

            // 여백 약간
            h += 2f;

            return h;
        }

    }
}
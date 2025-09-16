using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.Extensions.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static object GetTargetObjectOfProperty(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null) return null;

            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return p?.GetValue(source, null);
            }

            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;

            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
                if (!enm.MoveNext())
                    return null;

            return enm.Current;
        }

        public static void SwapValues(SerializedProperty a, SerializedProperty b)
        {
            if (a == null || b == null) return;
            if (a.propertyType != b.propertyType || a.type != b.type)
            {
                Debug.LogError($"Swap 실패: 타입 불일치 a:{a.type}, b:{b.type}");
                return;
            }

            a.serializedObject.Update();
            b.serializedObject.Update();

            // 1) boxedValue가 가능한 경우(신형 경로)
#if UNITY_2021_3_OR_NEWER
            if (CanUseBoxedValue(a) && CanUseBoxedValue(b))
            {
                (a.boxedValue, b.boxedValue) = (b.boxedValue, a.boxedValue);

                a.serializedObject.ApplyModifiedProperties();
                b.serializedObject.ApplyModifiedProperties();
                return;
            }
#endif
            // 2) 재귀 복사 경로
            var tempSO = new SerializedObject(a.serializedObject.targetObject);
            var tempProp = tempSO.FindProperty(a.propertyPath);
            AssignValue(tempProp, a); // temp = a
            AssignValue(a, b); // a   = b
            AssignValue(b, tempProp); // b   = temp

            a.serializedObject.ApplyModifiedProperties();
            b.serializedObject.ApplyModifiedProperties();
        }

        public static void AssignValue(SerializedProperty dst, SerializedProperty src)
        {
            if (dst == null || src == null) return;
            if (dst.propertyType != src.propertyType || dst.type != src.type)
            {
                Debug.LogError($"Assign 실패: 타입 불일치 dst:{dst.type}, src:{src.type}");
                return;
            }

#if UNITY_2021_3_OR_NEWER
            if (CanUseBoxedValue(dst) && CanUseBoxedValue(src))
            {
                dst.boxedValue = src.boxedValue;
                return;
            }
#endif
            if (dst.propertyType != SerializedPropertyType.Generic)
            {
                AssignLeaf(dst, src);
                return;
            }

            // Generic(Struct/클래스/배열/ManagedReference 등) → 깊은 복사
            var srcIter = src.Copy();
            var dstIter = dst.Copy();
            int depth = src.depth;

            bool enterChildren = true;
            while (srcIter.NextVisible(enterChildren))
            {
                if (srcIter.depth <= depth) break;

                // dst도 같은 구조 순서로 이동
                if (!dstIter.NextVisible(enterChildren))
                    break;

                if (srcIter.propertyType == SerializedPropertyType.Generic)
                {
                    // 배열인 경우 길이 맞추기
                    if (srcIter.isArray && dstIter.isArray)
                    {
                        dstIter.arraySize = srcIter.arraySize;
                    }
                    // 하위로 내려가며 재귀는 반복자로 커버(enterChildren=true)
                }
                else
                {
                    AssignLeaf(dstIter, srcIter);
                }

                enterChildren = true;
            }
        }

        private static void AssignLeaf(SerializedProperty dst, SerializedProperty src)
        {
            switch (dst.propertyType)
            {
                case SerializedPropertyType.Integer:
                    dst.intValue = src.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    dst.boolValue = src.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    dst.floatValue = src.floatValue;
                    break;
                case SerializedPropertyType.String:
                    dst.stringValue = src.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    dst.colorValue = src.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    dst.objectReferenceValue = src.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    dst.intValue = src.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    dst.enumValueIndex = src.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    dst.vector2Value = src.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    dst.vector3Value = src.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    dst.vector4Value = src.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    dst.rectValue = src.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    dst.intValue = src.intValue;
                    break;
                case SerializedPropertyType.Character:
                    dst.intValue = src.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    dst.animationCurveValue = src.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    dst.boundsValue = src.boundsValue;
                    break;
#if UNITY_2020_1_OR_NEWER
                case SerializedPropertyType.Quaternion:
                    dst.quaternionValue = src.quaternionValue;
                    break;
#endif
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.Vector2Int:
                    dst.vector2IntValue = src.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    dst.vector3IntValue = src.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    dst.rectIntValue = src.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    dst.boundsIntValue = src.boundsIntValue;
                    break;
#endif
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ExposedReference:
                    dst.exposedReferenceValue = src.exposedReferenceValue;
                    break;
#endif
                case SerializedPropertyType.FixedBufferSize:
                    dst.intValue = src.intValue;
                    break;
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    dst.managedReferenceValue = src.managedReferenceValue;
                    break;
#endif
                default:
                    // 나머지는 Generic 경로에서 처리됨
                    break;
            }
        }

#if UNITY_2021_3_OR_NEWER
        private static bool CanUseBoxedValue(SerializedProperty p)
        {
            // array 요소/Generic 루트는 boxedValue가 제한적이므로 안전하게 필터링
            if (p.isArray && p.propertyType != SerializedPropertyType.String) return false;
            // 대부분의 Leaf/Struct에서 boxedValue 동작(SerializeReference 포함)
            return true;
        }
#endif
    }
}
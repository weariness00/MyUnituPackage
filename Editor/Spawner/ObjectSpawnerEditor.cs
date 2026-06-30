using UnityEditor;
using UnityEngine;
using Util;

namespace Weariness.Util
{
    [CustomEditor(typeof(ObjectSpawnerBase), true)]
    [CanEditMultipleObjects]
    public class ObjectSpawnerEditor : UnityEditor.Editor
    {
        private SerializedProperty _spawnPlace;

        private void OnEnable()
        {
            if (target == null) return;
            _spawnPlace = serializedObject.FindProperty("spawnPlace");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // spawnPlace를 제외한 나머지 프로퍼티를 기본으로 그림
            DrawPropertiesExcluding(serializedObject, "spawnPlace", "m_Script");

            // spawnPlace의 현재 타입으로 드롭다운 초기값 결정
            var currentType = GetCurrentPlaceType();
            var newType = (SpawnPlaceType)EditorGUILayout.EnumPopup("Spawn Place Type", currentType);

            if (newType != currentType)
            {
                // 타입 변경 시 새 인스턴스로 교체
                ISpawnPlace newPlace = newType switch
                {
                    SpawnPlaceType.Transform => new TransformSpawnPlace(),
                    SpawnPlaceType.Line => new LineSpawnPlace(),
                    SpawnPlaceType.Circle => new CircleSpawnPlace(),
                    SpawnPlaceType.Rect => new RectSpawnPlace(),
                    _ => null
                };

                _spawnPlace.managedReferenceValue = newPlace;
            }

            // spawnPlace 내부 프로퍼티를 직접 그림
            if (_spawnPlace.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;

                var iterator = _spawnPlace.Copy();
                var end = iterator.GetEndProperty();
                iterator.NextVisible(true);
                while (!SerializedProperty.EqualContents(iterator, end))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    if (!iterator.NextVisible(false)) break;
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private SpawnPlaceType GetCurrentPlaceType()
        {
            var value = _spawnPlace.managedReferenceValue;
            return value switch
            {
                TransformSpawnPlace => SpawnPlaceType.Transform,
                LineSpawnPlace => SpawnPlaceType.Line,
                CircleSpawnPlace => SpawnPlaceType.Circle,
                RectSpawnPlace => SpawnPlaceType.Rect,
                _ => SpawnPlaceType.Transform,
            };
        }
    }
}

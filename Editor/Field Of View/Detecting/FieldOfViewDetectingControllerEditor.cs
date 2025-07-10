using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Weariness.Noesis.FieldOfView.Editor
{
    [CustomEditor(typeof(FieldOfViewDetectingController))]
    public class FieldOfViewDetectingControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty detectingType;
        private SerializedProperty data;
        private SerializedProperty detectingRay;
        private SerializedProperty detectingCamera;

        public void OnEnable()
        {
            detectingType = serializedObject.FindProperty("detectingType");
            data = serializedObject.FindProperty("data");
            detectingRay = serializedObject.FindProperty("detectingRay");
            detectingCamera = serializedObject.FindProperty("detectingCamera");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(detectingType);

            var script = target as FieldOfViewDetectingController;

            if (detectingType.enumValueIndex == (int)FieldOfViewDetectingType.MeshRay)
            {
                EditorGUILayout.PropertyField(detectingRay);
                if (script.detectingRay == null)
                {
                    EditorGUILayout.HelpBox("Field Of View Detecting Ray 컴포넌트가 할당되어있지 않습니다.", MessageType.Error);
                }
            }
            else if (detectingType.enumValueIndex == (int)FieldOfViewDetectingType.Camera)
            {
                EditorGUILayout.PropertyField(detectingCamera);
                if (script.detectingCamera == null)
                {
                    EditorGUILayout.HelpBox("Field Of View Detecting Camera 컴포넌트가 할당되어있지 않습니다.", MessageType.Error);
                }
            }

            EditorGUILayout.PropertyField(data);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

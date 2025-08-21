using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Weariness.Util.UI;

namespace Weariness.Util.UI.Editor
{
    [CustomEditor(typeof(UIScaler))]
    public class UIScalerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as UIScaler;
            if (script.rootCanvas == null)
                EditorGUILayout.HelpBox($"Root Canvas가 존재하지 않습니다.", MessageType.Error);
            else
            {
            }
            
            base.OnInspectorGUI();
        }
    }
}
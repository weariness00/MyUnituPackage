using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    public static class CSVReaderEditorExtension
    {
        public static void Read(string path, out List<Dictionary<string, object>> list)
        {
            list = null;
            TextAsset data = Resources.Load<TextAsset>(path);
            if (data == null)
                data = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (data == null) return;
            
            data.Read(out list);
        }
        
        private static string GetResourcePathToCSV(this TextAsset csvFile)
        {
            var path = AssetDatabase.GetAssetPath(csvFile);
            
            // Resource 포함 이전 경로 제거
            string[] parts = path.Split('/');
            int index = Array.IndexOf(parts, "Resources");
            if (index != -1 && index + 1 < parts.Length)
                path = string.Join("/", parts.Skip(index + 1));
            
            // 확장자 제거
            path = path.Replace(".csv", "");
            return path;
        }
    }
}

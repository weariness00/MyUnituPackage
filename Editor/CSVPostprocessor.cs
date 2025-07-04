// Assets/Editor/CSVImporter.cs

using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    public class CSVPostprocessor : AssetPostprocessor
    {
        private static readonly Dictionary<string, ICSVProcessor> ProcessorsDictionary = new();

        public static bool HasProcessor(ICSVProcessor processor) => processor != null && HasProcessor(processor.CSV_Name);
        public static bool HasProcessor(string name) => ProcessorsDictionary.ContainsKey(name);
        public static void AddProcessor(ICSVProcessor processor) => ProcessorsDictionary[processor.CSV_Name] = processor;
        public static void AddProcessor(string name, ICSVProcessor processor) => ProcessorsDictionary[name] = processor;
        public static void RemoveProcessor(string name)
        {
            if (ProcessorsDictionary.ContainsKey(name))
                ProcessorsDictionary.Remove(name);
        }
        
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (Path.GetExtension(path).ToLower() != ".csv")
                    continue;

                // 이미 TextAsset 으로 임포트된 .csv 를 불러오기만 하면 됩니다.
                var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (ta == null)
                {
                    Debug.LogWarning($"[CSVPostprocessor] TextAsset 로드 실패: {path}");
                    continue;
                }

                // 파일명/경로로 분기해서, 원하는 Processor 호출
                foreach (var (key, value) in ProcessorsDictionary)
                {
                    if(Path.GetFileNameWithoutExtension(path) == key)
                        value.Process(ta, path);
                }
            }
        }
    }
}


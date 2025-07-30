using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    public class ExcelPostProcessor : AssetPostprocessor
    {
        private static readonly Dictionary<string, IExcelProcessor> ProcessorsDictionary = new();

        public static bool HasProcessor(IExcelProcessor processor) => processor != null && HasProcessor(processor.Name);
        public static bool HasProcessor(string name) => ProcessorsDictionary.ContainsKey(name);
        public static void AddProcessor(IExcelProcessor processor) => ProcessorsDictionary[processor.Name] = processor;
        public static void RemoveProcessor(IExcelProcessor processor) => RemoveProcessor(processor.Name);
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
                if (Path.GetExtension(path).ToLower() != ".xlsx")
                    continue;
                // 파일명/경로로 분기해서, 원하는 Processor 호출
                foreach (var (key, value) in ProcessorsDictionary)
                {
                    if (Path.GetFileNameWithoutExtension(path) == key)
                    {
                        var fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
                        using var package = new ExcelPackage(new FileInfo(fullPath));
                        value.Process(package);
                    }
                }
            }
        }
    }
}
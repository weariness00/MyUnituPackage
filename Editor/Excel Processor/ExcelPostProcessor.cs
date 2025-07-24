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
        public static void AddProcessor(string name, IExcelProcessor processor) => ProcessorsDictionary[name] = processor;
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
                    if (Path.GetFileNameWithoutExtension(path) == key)
                    {
                        using var package = new ExcelPackage(new FileInfo(path));
                        value.Process(package);
                        foreach (var sheetName in value.sheetNames)
                        {
                            var sheet = package.Workbook.Worksheets[sheetName];
                            if( sheet != null)
                            {
                                value.Process(ConvertCSV.ExportSheetToCsv(sheet), sheetName);
                            }
                        }
                    }
                }
            }
        }
    }
}
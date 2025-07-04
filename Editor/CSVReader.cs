using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    static class CSVReaderExtension
    {
        public static T DynamicCast<T>(this Dictionary<string, object> dictionary, string key) => DynamicCast<T>(dictionary, key, default(T));
        public static T DynamicCast<T>(this Dictionary<string,object> dictionary, string key, T defaultValue)
        {
            if (dictionary.TryGetValue(key, out var objectValue))
            {
                if (objectValue is T value)
                    return value;
                if (objectValue is int intValue && typeof(T) == typeof(float))
                {
                    float f = intValue;
                    return (T)(object)f;
                }
                
                if (typeof(T) == typeof(string))
                {
                    if (objectValue is string[] stringArray)
                    {
                        return (T)(object)string.Join("", stringArray);
                    }
                    if (objectValue is List<string> stringList)
                    {
                        return (T)(object)string.Join("", stringList);
                    }
                }
                if (typeof(T).IsArray)
                {
                    var valueType = objectValue.GetType();
                    var genericType = typeof(T);
                    var array = Array.CreateInstance(genericType.GetElementType(), 1);
                    if (valueType == genericType.GetElementType())
                        array.SetValue(objectValue, 0);
                    else if (typeof(float) == genericType.GetElementType())
                    {
                        if (valueType == typeof(int) && objectValue is int intValue2)
                        {
                            array.SetValue((float)intValue2, 0);
                        }
                        else if (valueType.GetElementType() == typeof(int) && objectValue is int[] intArray)
                        {
                            float[] fArray = Array.ConvertAll(intArray, i => (float)i);
                            return (T)(object)fArray;
                        }
                        else
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                    if (array is T value2)
                        return value2;
                }

                // T가 List일 때
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    var valueType = objectValue.GetType();
                    var genericType = typeof(T);
                    var list = (System.Collections.IList)Activator.CreateInstance(genericType.GetElementType() ?? throw new InvalidOperationException());
                    if (valueType == genericType.GetElementType())
                        list.Add(objectValue);
                    else if (typeof(float) == genericType.GetElementType())
                    {
                        if (valueType == typeof(int) && objectValue is int intValue2)
                        {
                            list.Add((float)intValue2); // 단일 값을 리스트에 추가
                        }
                        else if (valueType.GetElementType() == typeof(int) && objectValue is int[] intArray)
                        {
                            var fList = Array.ConvertAll(intArray, i => (float)i).ToList();
                            return (T)(object)fList;
                        }
                        else
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                    if (list is T value2)
                        return value2;
                }
            }
            
            Debug.LogWarning($"{key}에 대한 Value가 존재하지 않습니다.");
            return defaultValue;
        }
        
        public static void Read(this TextAsset csvFile, out List<Dictionary<string,object>> csv)
        {
            Debug.Assert(csvFile != null, "CSV파일이 없어서 데이터를 셋팅하지 못했습니다.");
            CSVReader.Read(csvFile, out csv);
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
    
    public static class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"(?:\r\n|\n\r|\r)(?=(?:[^""]*""[^""]*"")*[^""]*$)";
        static char[] TRIM_CHARS = { '\"' };
        
        public static void Read(string path, out List<Dictionary<string, object>> list)
        {
            list = null;
            TextAsset data = Resources.Load<TextAsset>(path);
            if (data == null)
                data = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (data == null) return;
            
            Read(data, out list);
        }
        public static void Read(TextAsset data, out List<Dictionary<string, object>> list)
        {
            list = new List<Dictionary<string, object>>();
            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
            if (lines.Length <= 1) return;

            // 나중에 값이 안들어 있는 경우에 빈 값을 넣어주도록 바꾸어야한다.
            FindHeader(data, "header", out var headerData);
            var header = headerData.Item1;
            var headerIndex = headerData.Item2;
            for (var i = headerIndex; i < lines.Length - 1; i++)
            {
                var lineValues = Regex.Split(lines[i], SPLIT_RE);
                if (lineValues.Length == 0 || lineValues[0] == "//" || lineValues[0].ToLower() == "header") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < lineValues.Length; j++)
                {
                    string value = lineValues[j];
                    if (value.StartsWith(TRIM_CHARS[0]) && value.EndsWith(TRIM_CHARS[0]))
                    {
                        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                        var values = Regex.Split(value, SPLIT_RE);
                        bool isString = true;
                        bool isFloat = false;
                        foreach (string v in values)
                        {
                            if (int.TryParse(v, out var nn))
                            {
                                isString = false;
                            }
                            else if (float.TryParse(v, out var ff))
                            {
                                isFloat = true;
                                isString = false;
                                break;
                            }
                        }

                        object objectValue = null;
                        if (isString)
                        {
                            objectValue = values;
                        }
                        else if (isFloat)
                        {
                            float[] f = new float[values.Length];
                            for (var index = 0; index < values.Length; index++)
                                float.TryParse(values[index], out f[index]);
                            objectValue = f;
                        }
                        else
                        {
                            int[] n = new int[values.Length];
                            for (var index = 0; index < values.Length; index++)
                                int.TryParse(values[index], out n[index]);
                            objectValue = n;
                        }
                        entry[header[j]] = objectValue;
                    }
                    else
                    {
                        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                        object finalvalue = value;
                        int n;
                        float f;
                        if (int.TryParse(value, out n))
                        {
                            finalvalue = n;
                        }
                        else if (float.TryParse(value, out f))
                        {
                            finalvalue = f;
                        }
                        entry[header[j]] = finalvalue;
                    }
                }
                list.Add(entry);
            }
        }

        private static void FindHeader(TextAsset data, string headerName, out  Tuple<string[], int> header)
        {
            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
            for (var i = 0; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "//") continue;
                if (values[0].ToLower() == headerName)
                {
                    header = new(Regex.Split(lines[i], SPLIT_RE), i);
                    return;
                }
            }
            header = new(Array.Empty<string>(), -1);
        }
    }
}

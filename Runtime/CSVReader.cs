using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEngine;

namespace Weariness.Util.CSV
{
    public static class CSVReaderExtension
    {
        public static T DynamicCast<T>(this Dictionary<string, object> dictionary, string key) => DynamicCast<T>(dictionary, key, default(T));

        public static T DynamicCast<T>(this Dictionary<string, object> dictionary, string key, T defaultValue)
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

        public static void Read(this TextAsset csvFile, out List<Dictionary<string, object>> csv)
        {
            try
            {
                Debug.Assert(csvFile != null, "CSV파일이 없어서 데이터를 셋팅하지 못했습니다.");
                CSVReader.Read(csvFile.text, out csv);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        public static void Read(this string text, out List<Dictionary<string, object>> csv)
        {
            try
            {
                CSVReader.Read(text, out csv);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        public static TData[] Read<TData>(this string text, Func<TData, TData> onUpdateData = null) where TData : new()
        {
            try
            {
                return CSVReader.Read<TData>(text, onUpdateData);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }

    public static partial class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"(?:\r\n|\n\r|\r)(?=(?:[^""]*""[^""]*"")*[^""]*$)";
        static char[] TRIM_CHARS = { '\"' };

        public static void Read(string text, out List<Dictionary<string, object>> list)
        {
            list = new List<Dictionary<string, object>>();
            var lines = Regex.Split(text, LINE_SPLIT_RE);
            if (lines.Length <= 1) return;

            // 나중에 값이 안들어 있는 경우에 빈 값을 넣어주도록 바꾸어야한다.
            var (headers, headerIndex) = FindHeader(text, "header");
            for (var i = headerIndex; i < lines.Length - 1; i++)
            {
                var lineValues = Regex.Split(lines[i], SPLIT_RE);
                if (lineValues.Length == 0 || lineValues[0] == "//" || lineValues[0].ToLower() == "header") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < headers.Length && j < lineValues.Length; j++)
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

                        entry[headers[j]] = objectValue;
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

                        entry[headers[j]] = finalvalue;
                    }
                }

                list.Add(entry);
            }
        }

        public static TData[] Read<TData>(string text, Func<TData, TData> onUpdateData) where TData : new()
        {
            var lines = Regex.Split(text, LINE_SPLIT_RE);
            if (lines.Length <= 1) return Array.Empty<TData>();

            // 초기화
            bool isUpdate = onUpdateData != null;
            
            // '//'이 없는 첫 행이 헤더
            var (headers, headerIndex) = FindHeader(text, "");

            // TData의 Field, Property의 데이터를 매핑
            var typeSetters = ReflectionCache.TypeSetters<TData>();
            List<TData> dataList = new();
            for (var i = headerIndex + 1; i < lines.Length - 1; i++)
            {
                var lineValues = Regex.Split(lines[i], SPLIT_RE);
                if (lineValues.Length == 0 || lineValues[0] == "//") continue;
                
                var data = new TData();
                for (var j = 0; j < headers.Length && j < lineValues.Length; j++)
                {
                    if (typeSetters.TryGetValue(headers[j], out var typeSetter))
                    {
                        string value = lineValues[j];
                        var objectValue = GetObjectValue(value, typeSetter.Type);
                        if (typeSetter.IsRef)
                            typeSetter.RefSetter(ref data, objectValue);
                        else
                            typeSetter.NonRefSetter(data, objectValue);
                    }
                }
                if (isUpdate)
                    data = onUpdateData.Invoke(data);
                dataList.Add(data);
            }

            return dataList.ToArray();
        }

        private static (string[] haeders, int headerIndex) FindHeader(string text, string headerName)
        {
            var lines = Regex.Split(text, LINE_SPLIT_RE);
            for (var i = 0; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "//") continue;
                if (values[0].ToLower() == headerName || string.IsNullOrEmpty(headerName))
                {
                    return new(Regex.Split(lines[i], SPLIT_RE), i);
                }
            }

            return new(Array.Empty<string>(), -1);
        }
        
        public static string GetFieldByCSVName(FieldInfo fieldInfo)
        {
            var attr = fieldInfo.GetCustomAttribute<CSVColumnNameAttribute>();
            if (attr != null) return attr.Name;
            return fieldInfo.Name;
        }
        
        public static string GetPropertyByCSVName(PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<CSVColumnNameAttribute>();
            if (attr != null) return attr.Name;
            return propertyInfo.Name;
        } 


        public static object GetObjectValue(string value, Type type)
        {
            object objectValue = value;
            if (type.IsArray)
            {
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                var values = Regex.Split(value, SPLIT_RE);
                type = type.GetElementType();
                objectValue = values;
                object defaultValue = Array.Empty<string>();
                if(type != typeof(string))
                    defaultValue = Array.CreateInstance(type, 0);

                if (type == typeof(bool))
                {
                    var boolList = new List<bool>();
                    foreach (string v in values)
                    {
                        if (int.TryParse(v, out var n))
                            boolList.Add(n > 0);
                        else if (float.TryParse(v, out var f))
                            boolList.Add(f > 0);
                        else
                            boolList.Add(v.ToLower() == "true");
                    }

                    objectValue = boolList.ToArray();
                }
                else if (type == typeof(int))
                {
                    int[] n = new int[values.Length];
                    for (var index = 0; index < values.Length; index++)
                        int.TryParse(values[index], out n[index]);
                    objectValue = n;
                }
                else if (type == typeof(float))
                {
                    float[] f = new float[values.Length];
                    for (var index = 0; index < values.Length; index++)
                        float.TryParse(values[index], out f[index]);
                    objectValue = f;
                }
                else if (type!.IsEnum)
                {
                    Array array = Array.CreateInstance(type, values.Length);
                    for (var index = 0; index < values.Length; index++)
                    {
                        if(int.TryParse(values[index], out var i))
                            array.SetValue(Enum.ToObject(type, i), index);
                        else if (Enum.IsDefined(type, values[index]))
                            array.SetValue(Enum.Parse(type, values[index]), index);
                        else
                            array.SetValue(defaultValue, index);
                    }

                    objectValue = array;
                }
            }
            else
            {
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object defaultValue = "";
                if(type != typeof(string))
                    defaultValue = Activator.CreateInstance(type);
                
                // bool일 경우
                if (type == typeof(bool))
                {
                    if (int.TryParse(value, out var n))
                        objectValue = n > 0;
                    else if (float.TryParse(value, out var f))
                        objectValue = f >= 1f;
                    else
                        objectValue = value.ToLower() == "true";
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(value, out var n))
                        objectValue = n;
                    else
                        objectValue = default(int);
                }
                else if (type == typeof(float))
                {
                    if (float.TryParse(value, out var f))
                        objectValue = f;
                    else
                        objectValue = default(float);
                }
                else if (type.IsEnum)
                {
                    if (int.TryParse(value, out var i))
                        objectValue = Enum.ToObject(type, i);
                    else if (Enum.IsDefined(type, value))
                        objectValue = Enum.Parse(type, value);
                    else
                        objectValue = defaultValue;
                }
            }

            var objectType = objectValue.GetType();
            if (objectType.IsArray)
                objectType = objectType.GetElementType();

            if (objectType != type)
                return Activator.CreateInstance(type);
            return objectValue;
        }
    }
}
#if INCLUDE_WEARINESS_CSV_EXCEL
using System;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader; 
using UnityEngine;

namespace Weariness.Util.CSV
{
    public static partial class CSVReaderExtension
    {
        /// <summary>
        /// excelPath에서 sheetName을 찾아 TData[]로 파싱
        /// </summary>
        public static List<TData> ReadExcel<TData>(this string excelPath, string sheetName, Func<TData, TData> onUpdateData = null)
            where TData : new()
        {
            try
            {
                using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateReader(stream); // xls/xlsx 자동 판별

                return CSVReader.ReadFromExcelSheet(reader, sheetName, onUpdateData);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>
        /// 이미 열린 IExcelDataReader에서 sheetName을 찾아 TData[]로 파싱
        /// </summary>
        public static List<TData> ReadExcel<TData>(this IExcelDataReader reader, string sheetName, Func<TData, TData> onUpdateData = null)
            where TData : new()
        {
            try
            {
                return CSVReader.ReadFromExcelSheet(reader, sheetName, onUpdateData);
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
        public static List<TData> ReadFromExcelSheet<TData>(IExcelDataReader reader, string sheetName, Func<TData, TData> onUpdateData = null) where TData : new()
        {
            List<TData> datas = new();
            if (reader == null)
            {
                Debug.LogError("IExcelDataReader가 null 입니다.");
                return datas;
            }

            // 시트로 이동
            if (reader.GotoSheet(sheetName) == false) return datas; 
            
            // 첫 행이 헤더
            (var headers, int rowHeaderIndex) = FindHeaderToExcelSheet(reader);
            if (headers.Count == 0) return datas;

            // 3) 데이터 행 파싱
            bool isUpdate = onUpdateData != null;
            var typeSetters = ReflectionCache.TypeSetters<TData>();

            while (reader.Read())
            {
                // 완전 빈 행 스킵(모든 셀 null/empty)
                if (RowIsEmpty(reader))
                    continue;

                var data = new TData();

                foreach (var (header, colIndex0) in headers)
                {
                    if (!typeSetters.TryGetValue(header, out var setter))
                        continue;
                    
                    object text = reader.GetValue(colIndex0);
                    var value = GetObjectValue(SafeToString(text), setter.Type);

                    if (setter.IsRef)
                        setter.RefSetter(ref data, value);
                    else
                        setter.NonRefSetter(data, value);
                }

                if (isUpdate)
                    data = onUpdateData.Invoke(data);

                datas.Add(data);
            }

            return datas;
        }

        private static (Dictionary<string,int> haeders, int index) FindHeaderToExcelSheet(IExcelDataReader reader)
        {
            // 2) 헤더 행 찾기 (첫 번째 유효한 행을 헤더로 간주)
            Dictionary<string, int> headers = new();
            int headerRowIndex = -1;

            while (reader.Read())
            {
                headerRowIndex++;
                // 주석 행 스킵: 첫 셀에 //가 있으면 주석으로 간주
                var first = SafeToString(reader.GetValue(0));
                if (!string.IsNullOrEmpty(first) && first.Contains("//"))
                    continue;

                // 이 행을 헤더로 사용
                headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < reader.FieldCount; c++)
                {
                    var val = SafeToString(reader.GetValue(c));
                    if (!string.IsNullOrEmpty(val))
                    {
                        // 키는 소문자로 정규화(기존 코드와 동일한 매핑 가정)
                        var key = val.ToLowerInvariant();
                        if (!headers.ContainsKey(key))
                            headers.Add(key, c); // 0-based column index
                    }
                }

                break;
            }
            if (headers.Count == 0)
                Debug.LogError("헤더 행을 찾을 수 없습니다.");
            return (headers, headerRowIndex);
        }
        
        private static string SafeToString(object v)
        {
            return v?.ToString() ?? string.Empty;
        }

        private static bool RowIsEmpty(IExcelDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var v = reader.GetValue(i);
                if (v != null && !string.IsNullOrEmpty(v.ToString()))
                    return false;
            }
            return true;
        }

        private static void SkipRows(IExcelDataReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!reader.Read()) break; // EOF 도달 시 조용히 종료
            }
        }
    }
}
#endif
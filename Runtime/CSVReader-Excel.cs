using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace Weariness.Util.CSV
{
    public static partial class CSVReader
    {
        public static TData[] ReadToExcelSheet<TData>(ExcelWorksheet workSheet, Func<TData, TData> onUpdateData = null) where TData : new()
        {
            int rowCount = workSheet.Dimension.End.Row;
            int colCount = workSheet.Dimension.End.Column;
            bool isUpdate = onUpdateData != null;
            
            // 첫 행이 헤더
            (var headers, int rowHeaderIndex) = FindHeaderToExcelSheet(workSheet);
            List<TData> dataList = new();
            // TData의 Field, Property의 데이터를 매핑
            var typeSetters = ReflectionCache.TypeSetters<TData>();
            for (int row = rowHeaderIndex + 1; row <= rowCount; row++)
            {
                var data = new TData();
                foreach (var (header, colIndex) in headers)
                {
                    if (typeSetters.TryGetValue(header, out var typeSetter))
                    {
                        var cell = workSheet.Cells[row, colIndex];
                        var text = ConvertCSV.EscapeCsv(cell?.Text ?? "");
                        var value = GetObjectValue(text, typeSetter.Type); // 또는 직접 타입 매핑
                        if (typeSetter.IsRef)
                            typeSetter.RefSetter(ref data, value);
                        else
                            typeSetter.NonRefSetter(data, value);
                    }
                }

                if (isUpdate)
                    data = onUpdateData.Invoke(data);
                dataList.Add(data);
            }

            return dataList.ToArray();
        }

        private static (Dictionary<string,int> haeders, int index) FindHeaderToExcelSheet(ExcelWorksheet workSheet)
        {
            int rowCount = workSheet.Dimension.End.Row;
            int colCount = workSheet.Dimension.End.Column;
            Dictionary<string, int> dict = new();
            for (int row = 1; row <= rowCount; row++)
            {
                string firstRowValue = workSheet.Cells[row, 1]?.Text ?? "";
                if(string.IsNullOrEmpty(firstRowValue) && firstRowValue.Contains("//")) continue;
                
                for (int col = 1; col <= colCount; col++)
                {
                    var cell = workSheet.Cells[row, col];
                    string value = cell?.Text ?? "";
                    if(string.IsNullOrEmpty(value)) continue;
                    dict.TryAdd(value.ToLower(), col);
                }
                return (dict, row);
            }

            return (null, 0);
        }
    }
}
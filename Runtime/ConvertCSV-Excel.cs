#if INCLUDE_WEARINESS_CSV_EXCEL
using System;
using OfficeOpenXml;
using UnityEngine;

namespace Weariness.Util.CSV
{
    public static partial class ConvertCSV
    {
        public static string ExportSheetToCsv(string xlsxPath, string sheetName)
        {
            try
            {
                using var package = new OfficeOpenXml.ExcelPackage(new System.IO.FileInfo(xlsxPath));
                return ExportSheetToCsv(package, sheetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        public static string ExportSheetToCsv(ExcelPackage excel, string sheetName)
        {
            var worksheet = excel?.Workbook?.Worksheets[sheetName];
            if(worksheet == null)
            {
                UnityEngine.Debug.LogError($"시트 '{sheetName}' 를 찾을 수 없습니다.");
                return "";
            }
            return ExportSheetToCsv(worksheet);
        }

        public static string ExportSheetToCsv(ExcelWorksheet worksheet)
        {
            if (worksheet == null)
            {
                UnityEngine.Debug.LogError($"시트를 찾을 수 없습니다.");
                return "";
            }

            var sb = new System.Text.StringBuilder();
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column;

            for (int row = 1; row <= rowCount; row++)
            {
                for (int col = 1; col <= colCount; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    string value = cell?.Text ?? "";
                    sb.Append(EscapeCsv(value));

                    if (col < colCount) sb.Append(",");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
#endif

using System;
using System.IO;
using System.Text;
using OfficeOpenXml;
using UnityEngine;

namespace Weariness.Util.CSV
{
    public static class ConvertCSV
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

        private static string EscapeCsv(string input)
        {
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }

            return input;
        }

        // 일단 무조건 utf-8형태로 저장
        public static void WriteCsv(string path, string[] lines)
        {
            // UTF-8 without BOM
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public static void WriteCsv(string path, string csv)
        {
            // UTF-8 without BOM
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(csv);
            }
        }
    }
}
#if INCLUDE_WEARINESS_CSV_EXCEL
using System;
using System.Text;
using UnityEngine;
using ExcelDataReader; 

namespace Weariness.Util.CSV
{
    public static partial class ConvertCSV
    {
        /// <summary>
        /// 파일 경로와 시트 이름으로 CSV 문자열을 생성 (xlsx 지원)
        /// </summary>
        public static string ExportExcelSheetToCsv(string excelPath, string sheetName)
        {
            try
            {
                using var reader = excelPath.GetExcelReader();
                return ExportExcelSheetToCsv(reader, sheetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>
        /// 이미 연 열린 IExcelDataReader로부터 시트를 찾아 CSV 생성
        /// </summary>
        public static string ExportExcelSheetToCsv(IExcelDataReader reader, string sheetName)
        {
            if (reader == null)
            {
                Debug.LogError("IExcelDataReader가 null 입니다.");
                return "";
            }

            // 원하는 시트 찾기
            bool found = false;
            do
            {
                if (string.Equals(reader.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            } while (reader.NextResult()); // 다음 시트로 이동

            if (!found)
            {
                Debug.LogError($"시트 '{sheetName}' 를 찾을 수 없습니다.");
                return "";
            }

            // 현재 위치한 시트를 행 단위로 읽어 CSV 작성
            var sb = new StringBuilder();

            while (reader.Read())
            {
                int colCount = reader.FieldCount; // 이 행의 실제 컬럼 수(가변일 수 있음)
                for (int col = 0; col < colCount; col++)
                {
                    string value = reader.GetValue(col)?.ToString() ?? "";
                    sb.Append(EscapeCsv(value));    // 기존 partial에 있는 EscapeCsv 사용
                    if (col < colCount - 1) sb.Append(",");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
#endif

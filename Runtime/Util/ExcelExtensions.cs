#if INCLUDE_WEARINESS_CSV_EXCEL
using System;
using System.IO;
using ExcelDataReader;

namespace Weariness.Util.CSV
{
    public static class ExcelExtensions
    {
        public static IExcelDataReader GetExcelReader(this string path)
        {
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = ExcelReaderFactory.CreateReader(stream); // 확장자 상관없이 자동 판별
            return reader;
        }
        
        public static bool GotoSheet(this IExcelDataReader reader, string sheetName)
        {
            do
            {
                if (string.Equals(reader.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                    return true;
            } while (reader.NextResult());
            return false;
        }
    }
}
#endif

using System.Collections.Generic;
using OfficeOpenXml;

namespace Weariness.Util.CSV.Editor
{
    public interface IExcelProcessor
    {
        public string Name { get; set; }
        public List<string> sheetNames { get; set; }
        public void Process(ExcelPackage package);
        public void Process(string sheetText, string sheetName);
    }
}
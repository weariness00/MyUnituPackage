using System.Collections.Generic;
using ExcelDataReader;

namespace Weariness.Util.CSV.Editor
{
    public interface IExcelProcessor
    {
        public string Name { get; }
        
        public string[] GetSheetNames();
        public void Process(IExcelDataReader reader, string sheetName);
    }
}

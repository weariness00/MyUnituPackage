# Convert CSV
- excel의 한 시트를 csv형태의 문자열로 export해준다.
```utf-8
string sheetName = "A Sheet"
string path = "Assets/A.xlsx"
var excel = new ExcelPackage(new FileInfo(Path.Combine(Application.dataPath, path)));

string csv = ConvertCSV.ExportSheetToCsv(excel, sheetName); 
```
---

# CSV Reader
## Post processor 사용법
- CustomProcessor를 사용하기 위해서는 CSV파일 Name을 CSVPostprocessor.AddProcessor(key, processor)로 등록할때 key와 같아야합니다.
```utf-8
[InitializeOnLoad]
public static class CustomProcessorInitializer
{
    static CustomProcessorInitializer()
    {
        // 도메인 리로드 직후 딜레이 콜에 등록
        EditorApplication.delayCall += OnEditorLoaded;
    }

    private static void OnEditorLoaded()
    {
        // 여기에 한 번만 실행할 코드
        var csv = new CSVProcessorTest();
        if (!CSVPostprocessor.HasProcessor(csv))
            CSVPostprocessor.AddProcessor(csv);

        var excel = new ExcelProcessorTest();
        if (!ExcelPostProcessor.HasProcessor(excel))
        {
            excel.sheetNames.Add("A");
            excel.sheetNames.Add("B");
            ExcelPostProcessor.AddProcessor(excel);
        }
    }
}
```

- 읽고 싶은 CSV 마다 ICSVProcessor를 상속하여 제작
```utf-8
public class CSVProcessorTest : ICSVProcessor
{
    public string CSV_Name { get; set; } = "CSV";

    public void Process(TextAsset textAsset, string path)
    {
    }
}
```

- 읽고 싶은 Excel 마다 IExcelProcessor를 상속하여 제작
```utf-8
public class ExcelProcessorTest : IExcelProcessor
{
    public string Name { get; set; } = "Excel";
    public List<string> sheetNames { get; set; } = new List<string>();

    public void Process(ExcelPackage package)
    {
        // ExcelPackage 처리 로직
        Debug.Log("Processing Excel Package");
    }

    public void Process(string sheetText, string sheetName)
    {
        // CSV로 변환된 시트 처리 로직
        if (sheetName == "A")
            ASheet(sheetText);
        else if (sheetName == "B")
            BSheet(sheetText);
        else
            Debug.LogWarning($"Unknown sheet: {sheetName}");
    }

    private void ASheet(string sheet)
    {
        // A 시트 처리 로직
        Debug.Log($"Processing A Sheet: {sheet}");
    }

    private void BSheet(string sheet)
    {
        // B 시트 처리 로직
        Debug.Log($"Processing B Sheet: {sheet}");
    }
}
```
## Properties 또는 Fields로 파싱
- Field 또는 Property의 이름과 CSV의 Header 이름이 같아야합니다.
- 만일 Field 또는 Property의 이름이 CSV의 Header와 다를 경우, `CSVFieldName` 어트리뷰트를 사용하여 매핑할 수 있습니다.
```utf-8
using System;
using UnityEngine;
using Weariness.Util.CSV;

namespace Test.CSV
{
    public class CSV : MonoBehaviour
    {
        public TextAsset csv;

        public PropertyCSVData[] propertyData;
        public FieldCSVData[] fieldData;
        public void Awake()
        {
            csv.ReadToProperty(out propertyData);
            csv.ReadToFiled(out fieldData);
        }
    }

    [Serializable]
    public class PropertyCSVData
    {
        [field: SerializeField] public string STR { get; set; }
        [field: SerializeField] public int INT { get; set; }
        [field: SerializeField] public float FLOAT { get; set; }
        [field: SerializeField] public bool BOOL { get; set; }
        [field: SerializeField] public string[] STR_ARRAY { get; set; }
        [field: SerializeField] public int[] INT_ARRAY { get; set; }
        [field: SerializeField] public float[] FLOAT_ARRAY { get; set; }
        [field: SerializeField] public bool[] BOOL_ARRAY { get; set; }
    }

    [Serializable]
    public class FieldCSVData
    {
        [CSVFieldName("STR")] public string str;
        [CSVFieldName("int")] public int intValue;
        [CSVFieldName("float")] public float floatValue;
        [CSVFieldName("bool")] public bool boolValue;
        [CSVFieldName("str_array")] public string[] strArray;
        [CSVFieldName("int_array")] public int[] intArray;
        [CSVFieldName("float_array")] public float[] floatArray;
        [CSVFieldName("bool_array")] public bool[] boolArray;
    }
}
```

---
# CSV Reader - Excel
- EPPlus 를 사용해 Excel을 CSV 형태로 읽어 사용한다.
```utf-8
using System;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using Weariness.Util.CSV;

namespace Weariness.Util.Extensions.Test.CSV
{
    
    public class CSV_Excel_Test : MonoBehaviour
    {
        public string excelPath = "Test/CSV/Test.xlsx";
        public TestData[] datas;
        
        public void Awake()
        {
            var excel = new ExcelPackage(new FileInfo(Path.Combine(Application.dataPath, excelPath)));
            var worksheet = excel?.Workbook?.Worksheets["A"];
            datas = CSVReader.ReadToExcelSheet<TestData>(worksheet);
        }
    }

    public enum A
    {
        A = 0,
        B = 1,
    }

    [Serializable]
    public struct TestData
    {
        public int av;
        public A[] type;
        public string id;
    }
}
```

## CSV Reader
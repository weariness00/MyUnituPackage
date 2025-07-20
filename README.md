# Example

## CSVPostprocessor 사용법
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
            var csv = new CSVTest();
            if(!CSVPostprocessor.HasProcessor(csv))
                CSVPostprocessor.AddProcessor(csv);
        }
    }
```

- 읽고 싶은 CSV 마다 ICSVProcessor를 상속하여 제작
```utf-8

    public class CSVTest : ICSVProcessor
    {
        public string CSV_Name { get; set; } = "CSV";

        public void Process(TextAsset textAsset, string path)
        {
            // 여기에 로직 추가
        }
    }
```
---
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
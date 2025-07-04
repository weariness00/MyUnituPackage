# Example

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
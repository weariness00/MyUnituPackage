# Example

- CustomProcessor를 사용하기 위해서는 CSV파일 Name을 CSVPostprocessor.AddProcessor(key, processor)로 등록할때 key와 같아야합니다.



    [InitializeOnLoad]
    public static class CustomProcessorInitializer
    {
        private static bool hasInited = false;

        static CustomProcessorInitializer()
        {
            // 도메인 리로드 직후 딜레이 콜에 등록
            EditorApplication.delayCall += OnEditorLoaded;
        }

        private static void OnEditorLoaded()
        {
            if (hasInited) return;
            hasInited = true;

            // 여기에 한 번만 실행할 코드
            CSVPostprocessor.AddProcessor("CSV Name", new WordProcessor());

            // 등록 해제
            EditorApplication.delayCall -= OnEditorLoaded;
        }
    }

    public class CustomProcessor : ICSVProcessor
    {
        public void Process(CSVData data)
        {
            // CSV 데이터 처리 로직
            Debug.Log("Processing CSV data with custom processor.");
        }
    }
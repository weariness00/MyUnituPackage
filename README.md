# CSV

GitHub: [https://github.com/weariness00/MyUnituPackage/tree/CSV](https://github.com/weariness00/MyUnituPackage/tree/CSV)

---

## 개요 (Overview)

### 문제 정의

디자인/밸런싱 데이터가 **CSV/엑셀**로 관리되는 프로젝트에서, 컬럼-필드 매핑, 배열 파싱, 타입 변환, 시트 선택 등 반복 작업이 많아지면 실수가 잦고 코드가 지저분해집니다.

### 목표

* **속성(Property)/필드(Field)** 기반의 **타입 안전** CSV 파서 제공
* **어트리뷰트 매핑**으로 헤더 이름 불일치 해결
* **엑셀(EPPlus)** 시트를 바로 **CSV 형태로 읽기** 및 **후처리(Post Process)** 파이프라인 제공

### 결과

* 데이터 로드 코드 **단순화** 및 **중복 제거**
* 스키마 변경에 대한 **유연한 대응**
* 빌드/에디터 양쪽에서 **일관된 데이터 경로** 확보

---

## 제공 컴포넌트

### 1) CSV Reader — Property/Field 매핑

* **역할**: `TextAsset` CSV를 **강타입 구조**로 역직렬화
* **핵심 포인트**

   * **Property/Field 이름 = CSV 헤더** 일치 시 자동 매핑
   * 이름이 다르면 `CSVFieldName`(필드), 혹은 컬럼명 지정 어트리뷰트로 **명시적 매핑**
   * `string[]`, `int[]`, `float[]`, `bool[]` 등 **배열 타입**도 지원

```csharp
using System;
using UnityEngine;
using Weariness.Util.CSV;

public class CSV_Example : MonoBehaviour
{
    public TextAsset csv;
    public PropertyRow[] propertyRows;
    public FieldRow[] fieldRows;

    void Awake()
    {
        // TextAsset 확장 메서드 기반 (리포지토리 예제 기준)
        csv.ReadToProperty(out propertyRows);
        csv.ReadToFiled(out fieldRows); // README 기준 표기
    }
}

[Serializable]
public class PropertyRow
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
public class FieldRow
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
```

---

### 2) CSV Reader — Excel(EPPlus)

* **역할**: EPPlus `ExcelWorksheet`를 **CSV 행 형태로 파싱**
* **핵심 포인트**

   * 특정 시트를 선택해 **바로 타입으로 매핑**
   * **시트 → CSV 문자열 Export** 유틸 제공(아래 Convert CSV 참조)

```csharp
using System;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using Weariness.Util.CSV;

public class CSV_Excel_Example : MonoBehaviour
{
    public string excelPath = "Test/CSV/Test.xlsx";
    public ExcelRow[] rows;

    void Awake()
    {
        var pkg = new ExcelPackage(new FileInfo(Path.Combine(Application.dataPath, excelPath)));
        var sheet = pkg?.Workbook?.Worksheets["A"];
        rows = CSVReader.ReadToExcelSheet<ExcelRow>(sheet);
    }
}

public enum A { A = 0, B = 1 }

[Serializable]
public struct ExcelRow
{
    public int av;
    public A[] ty약
```

> 시트 확장 메서드로 `sheet.Read<T>(out var datas)` 형태도 사용 가능합니다.

---

### 3) Post Processor 파이프라인

* **역할**: CSV/Excel 읽은 후 **커스텀 후처리**(검증, 변환, 에셋 생성 등) 자동화
* **등록 규칙**

   * CSV: `CSVPostprocessor.AddProcessor(processor)`
   * Excel: `ExcelPostProcessor.AddProcessor(processor)`
   * 에디터 시작 시 1회 등록: `EditorApplication.delayCall` 활용 이니셜라이저
   * **키 규칙**: CSV 파일명 ↔ `ICSVProcessor.CSV_Name` 일치

```csharp
using System.Collections.Generic;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using Weariness.Util.CSV;

[InitializeOnLoad]
public static class CustomProcessorInitializer
{
    static CustomProcessorInitializer()
    {
        EditorApplication.delayCall += OnEditorLoaded;
    }

    static void OnEditorLoaded()
    {
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

public class CSVProcessorTest : ICSVProcessor
{
    public string CSV_Name { get; set; } = "CSV"; // CSV 파일명과 동일
    public void Process(TextAsset textAsset, string path)
    {
        // CSV 후처리 로직
    }
}

public class ExcelProcessorTest : IExcelProcessor
{
    public string Name { get; set; } = "Test";
    public List<string> sheetNames { get; set; } = new List<string>();

    public void Process(ExcelPackage package)
    {
        foreach (var ws in package.Workbook.Worksheets)
        {
            switch (ws.Name)
            {
                case "A": ASheet(ws); break;
                case "B": BSheet(ws); break;
            }
        }
        Debug.Log("Processing Excel Package");
    }

    void ASheet(ExcelWorksheet sheet)
    {
        sheet.Read<SheetRow>(out var datas); // 시트 → 타입 매핑
        Debug.Log(datas.Length);
    }

    void BSheet(ExcelWorksheet sheet)
    {
        // B 시트 처리
    }
}

public struct SheetRow
{
    [CSVIgnore] public int INT;
    [CSVColumnName("int")] public int value;
}
```

---

### 4) Convert CSV (Excel → CSV 문자열 Export)

* **역할**: 엑셀의 특정 시트를 **CSV 문자열**로 내보내기

```csharp
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using Weariness.Util.CSV;

public class ConvertCSV_Example : MonoBehaviour
{
    public string path = "Assets/A.xlsx";
    public string sheetName = "A Sheet";

    void Start()
    {
        var pkg = new ExcelPackage(new FileInfo(Path.Combine(Application.dataPath, path)));
        string csv = ConvertCSV.ExportSheetToCsv(pkg, sheetName);
        Debug.Log(csv.Substring(0, Mathf.Min(120, csv.Length)) + "...");
    }
}
```

---

## 설계 & 아키텍처

* **확장 메서드**로 `TextAsset`, `ExcelWorksheet`에 자연스럽게 붙는 API 제공
* **인터페이스 분리**: `ICSVProcessor` / `IExcelProcessor`로 후처리 커스터마이즈
* **어트리뷰트 세트**: `CSVFieldName`, `CSVColumnName`, `CSVIgnore` 등으로 명시적 제어

---

## 사용 패턴

* 밸런스/레벨/아이템 테이블을 **CSV/엑셀 원본**으로 관리하고, 런타임에 **강타입 구조**로 로드
* 시트별로 다른 구조체를 매핑하여 **시트-타입 1:1** 유지
* 포스트프로세서로 **검증/정규화/스크립터블오브젝트 생성**까지 자동화

---

## 라이선스 & 기여

* 라이선스/컨트리뷰션 가이드는 저장소 기준을 따릅니다. 이슈/PR 환영합니다.

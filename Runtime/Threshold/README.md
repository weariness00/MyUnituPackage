# Threshold

수치가 특정 임계값을 돌파할 때 이벤트를 발생시키는 순수 C# 유틸리티.
`[Serializable]`이 적용되어 있어 MonoBehaviour의 `[SerializeField]`로 Inspector에서 직접 설정 가능하다.

---

## 클래스 목록

| 파일 | 설명 |
|------|------|
| `ThresholdDirection.cs` | 돌파 방향 열거형 (Descending / Ascending) |
| `ThresholdEntry.cs` | 구간 하나를 나타내는 struct (Index, Value, IsCrossed) |
| `ThresholdTracker.cs` | 임계값 추적 메인 클래스 |

---

## ThresholdTracker

### Inspector 설정 필드

| 필드 | 설명 |
|------|------|
| `direction` | 돌파 방향 (Ascending / Descending) |
| `setupMode` | 임계값 구성 방식 (ByDivision / ByRatio / ByValues) |
| `maxValue` | ByDivision / ByRatio 기준 최대값 |
| `divisions` | ByDivision: 균등 분할 수 |
| `ratioList` | ByRatio: 0~1 비율 배열 |
| `valueList` | ByValues: 절댓값 배열 |

### 프로퍼티

| 이름 | 타입 | 설명 |
|------|------|------|
| `Direction` | `ThresholdDirection` | 돌파 방향 |
| `Entries` | `IReadOnlyList<ThresholdEntry>` | 전체 구간 목록 |
| `CrossedCount` | `int` | 현재까지 돌파한 구간 수 |
| `IsAllCrossed` | `bool` | 모든 구간 돌파 여부 |

### 이벤트

| 이름 | 인자 | 설명 |
|------|------|------|
| `OnThresholdCrossed` | `int index` | 구간 돌파 시 발생. 돌파한 구간의 index 전달 |
| `OnThresholdUncrossed` | `int index` | 돌파 해제 시 발생. 수치가 임계값 반대편으로 돌아올 때 |

### 메서드

| 메서드 | 설명 |
|--------|------|
| `Init()` | Inspector 설정 기반으로 구간 목록 생성. Inspector 값 변경 시 자동 호출됨. 런타임에서는 Awake에서 호출 |
| `Update(float currentValue)` | 현재 수치 전달. 돌파/해제 시 이벤트 발생. 한 번 호출에 여러 구간 동시 처리 가능 |
| `Reset()` | 모든 구간을 미돌파 상태로 초기화. 이벤트 미발생 |

---

## 사용 예시

```csharp
// Inspector에서 [SerializeField]로 설정
[SerializeField] private ThresholdTracker tracker;

void Awake()
{
    tracker.Init();
    tracker.OnThresholdCrossed.AddListener(index => Debug.Log($"돌파: {index}"));
}

void OnValueChanged(float value)
{
    tracker.Update(value);
}
```

---

## 구현 메모

- `Init()` 호출 전 `Update()`를 호출하면 NullReferenceException 발생.
- `Init()` 재호출 시 구간 목록과 CrossedCount가 초기화됨 (이벤트 구독은 유지).
- `[Serializable]` 어트리뷰트 적용.

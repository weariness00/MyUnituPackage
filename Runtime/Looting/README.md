# Looting

게임 로직에 의존하지 않는 순수 Core 추첨 프레임워크.
DataTable 연동은 외부(게임별 구체 구현)에서 담당하며, Core는 추첨 메커니즘만 책임진다.

---

## 폴더 구조

```
Looting/
├── ILootingEntry.cs          — 추첨 항목 인터페이스 (Weight / CountMin / CountMax)
├── ILootingGroup.cs          — 추첨 그룹 인터페이스 (GroupId / SelectionWeight)
├── LootingEntry.cs           — ILootingEntry 구현체. 제네릭 Payload 보유
├── LootingGroup.cs           — ILootingGroup 구현체. LootingEntry 목록 보유
├── LootingResult.cs          — 추첨 결과 데이터 (Payload / Count / SourceGroupId)
├── LootingPickMode.cs        — enum. 추첨 개수 방식 (Single / Count / All)
├── LootingDuplicateMode.cs   — enum. 중복 허용 여부 (Allow / Disallow)
└── LootingResolver.cs        — 추첨 실행기. 그룹 추첨 3종 + 항목 추첨 3종
```

---

## Enum

### LootingPickMode

| 값 | 설명 |
|----|------|
| `Single` | 1개만 추첨 |
| `Count` | N개 추첨 (호출 시 count 인자로 지정) |
| `All` | 전체 추첨 |

### LootingDuplicateMode

| 값 | 설명 |
|----|------|
| `Allow` | 중복 허용 — 같은 항목이 여러 번 뽑힐 수 있음 |
| `Disallow` | 중복 불허 — 한 번 뽑힌 항목은 풀에서 제거 |

---

## 클래스

### LootingEntry\<T\>

추첨 항목 하나. `T`는 보상 데이터 타입.

| 멤버 | 타입 | 설명 |
|------|------|------|
| `Payload` | `T` | 보상 데이터 |
| `Weight` | `float` | 추첨 가중치 |
| `CountMin` | `int` | 수량 최솟값 |
| `CountMax` | `int` | 수량 최댓값 |

### LootingGroup\<T\>

같은 group_id에 속하는 `LootingEntry` 묶음.

| 멤버 | 타입 | 설명 |
|------|------|------|
| `GroupId` | `string` | 그룹 식별자 |
| `SelectionWeight` | `float` | 그룹 선택 가중치 |
| `EntryList` | `IReadOnlyList<LootingEntry<T>>` | 항목 목록 |

### LootingResult\<T\>

추첨 결과 하나.

| 멤버 | 타입 | 설명 |
|------|------|------|
| `Payload` | `T` | 보상 데이터 |
| `Count` | `int` | 수량 (CountMin~CountMax 범위 내 결정) |
| `SourceGroupId` | `string` | 디버깅용 출처 그룹 ID |

---

## LootingResolver — 그룹 추첨 3종

| 메서드 | 설명 |
|--------|------|
| `PickGroup<T>(groups)` | SelectionWeight 기반으로 1개 선택 |
| `PickGroups<T>(groups, count, duplicateMode)` | count개 추첨. Disallow 시 count > 그룹 수이면 그룹 수만큼만 반환 |
| `PickAllGroups<T>(groups)` | 모든 그룹 전부 반환 (중복 없음 고정) |

## LootingResolver — 항목 추첨 3종

| 메서드 | 설명 |
|--------|------|
| `Resolve<T>(group)` | Weight 기반으로 1개 추첨 |
| `Resolve<T>(group, count, duplicateMode)` | count개 추첨. Disallow 시 count > 항목 수이면 항목 수만큼만 추첨 |
| `ResolveAll<T>(group)` | 모든 항목 전부 추첨 (중복 없음 고정) |

---

## 내부 구현 규칙

- **가중치 추첨**: 전체 Weight / SelectionWeight 합산 후 `Random.Range(0f, totalWeight)` 누적 비교
- **Count 결정**: `CountMin == CountMax` 이면 고정값, 다르면 `Random.Range(CountMin, CountMax + 1)`
- **Disallow 추첨**: 리스트를 복사한 뒤 뽑힌 항목을 제거하며 반복
- **비어있거나 totalWeight == 0**: `null` / 빈 리스트 반환
- **MonoBehaviour 의존 없음**: 순수 C# 클래스. `UnityEngine.Random` 사용

---

## 외부 주입 방식

`LootingResolver`는 상태를 갖지 않으므로 DI로 싱글턴 주입하거나 직접 `new` 생성 후 사용한다.
DataTable → `LootingGroup<T>` / `LootingEntry<T>` 변환은 게임별 구체 구현(예: `TowerLootingBuilder`)에서 담당한다.

```csharp
// DI 등록 예시
builder.Register<LootingResolver>(Lifetime.Singleton);
```

---

## 조합 사용 예시

```csharp
// [그룹 1개 선택 후 단일 추첨] 약탈 구간 보상
var group  = resolver.PickGroup(sectionGroups);
var result = resolver.Resolve(group);

// [그룹 N개 선택 후 각각 단일 추첨] 복수 구간 독립시행
var picked  = resolver.PickGroups(sectionGroups, count: 5, LootingDuplicateMode.Disallow);
var results = picked.Select(g => resolver.Resolve(g)).ToList();

// [N개 중복 허용 추첨]
var results = resolver.Resolve(group, count: 3, LootingDuplicateMode.Allow);

// [전체 추첨]
var results = resolver.ResolveAll(group);
```

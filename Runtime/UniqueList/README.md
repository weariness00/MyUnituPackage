# UniqueList\<T\>

중복 없이 랜덤하게 값을 뽑을 수 있는 제네릭 컬렉션.  
내부적으로 `HashSet + List`를 병행 관리하여 O(1) 중복 검사와 O(1) 랜덤 추출을 지원한다.

---

## 파일 구성

| 파일 | 설명 |
|------|------|
| `UniqueList.cs` | 핵심 컬렉션 클래스 — `Add/Remove/Get/TryGet/Count` |
| `IUniqueListMaker.cs` | 범위 초기화 인터페이스 및 내장 구현체 |
| `UniqueExtensions.cs` | 확장 메서드 — `Init`, `ToUniqueList` |

---

## 내장 지원 타입

별도 등록 없이 범위 기반 Init 바로 사용 가능.

- `int`
- `Vector2Int`
- `Vector3Int`

---

## 커스텀 타입 등록

`IUniqueListMaker<T>`를 구현하고 정적으로 등록한다.

```csharp
public struct UniqueListMakerMyTile : IUniqueListMaker<MyTile>
{
    public void UniqueMake(UniqueList<MyTile> uniqueList, MyTile start, MyTile end)
    {
        // start~end 범위의 MyTile을 uniqueList.Add()로 채운다
    }
}

// 게임 시작 시 한 번만 등록
UniqueList<MyTile>.RegisterMaker(new UniqueListMakerMyTile());
```

---

## API

### 생성 및 초기화

```csharp
// 범위 기반 (내장 타입)
var unique = new UniqueList<int>();
unique.Init(1, 10);

var unique = new UniqueList<Vector2Int>();
unique.Init(Vector2Int.zero, new Vector2Int(3, 3));

// 범위 기반 (커스텀 maker 직접 전달)
var unique = new UniqueList<MyTile>();
unique.Init(start, end, new UniqueListMakerMyTile());

// 배열/컬렉션 기반
int[] arr = { 3, 7, 12, 5 };
var unique = new UniqueList<int>();
unique.Init(arr);
```

### Get — 랜덤 추출 (중복 없음)

```csharp
int value = unique.Get(); // 꺼내면 컬렉션에서 제거됨
// Count == 0 상태에서 호출 시 InvalidOperationException
```

### TryGet — 안전한 랜덤 추출

```csharp
if (unique.TryGet(out int value))
{
    // value 사용
}
// Count == 0이면 false 반환, 예외 없음
```

### Add / Remove

```csharp
unique.Add(5);    // 이미 있으면 무시
unique.Remove(5); // 없으면 무시
```

### Count

```csharp
int remaining = unique.Count;
```

---

## 주의사항

- `Get()`은 값을 반환함과 동시에 컬렉션에서 제거된다.
- `Count == 0`일 때 `Get()`을 호출하면 `InvalidOperationException`이 발생한다.
- 커스텀 타입은 `RegisterMaker` 또는 `Init`의 `maker` 파라미터로 반드시 등록해야 한다.

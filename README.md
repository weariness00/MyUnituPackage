# MyUnituPackage

Unity 개발에서 자주 쓰이는 유틸리티, 확장 메서드, 컨테이너, 시스템 모듈을 모아둔 라이브러리입니다.

**네임스페이스:** `Weariness.Util` / `Weariness.Util.Extensions` / `Weariness.Util.Container` 등

---

## 목차

- [Extensions (확장 메서드)](#extensions)
  - [MathExtension](#mathextension)
  - [StringExtension](#stringextension)
  - [ColorExtension](#colorextension)
  - [TransformExtension](#transformextension)
  - [Vector3Extension](#vector3extension)
  - [MeshExtension](#meshextension)
- [Container (컨테이너)](#container)
  - [KeyValueDictionary](#keyvaluedictionary)
  - [Wrapping](#wrapping)
- [MinMax / MinMaxValue](#minmax--minmaxvalue)
  - [MinMax\<T\>](#minmaxt)
  - [MinMaxValue\<T\>](#minmaxvaluet)
- [Stat / StatModifier](#stat--statmodifier)
- [Unique\<T\>](#uniquet)
- [EventBusSystem](#eventbussystem)
- [PoolSystem](#poolsystem)
- [CoroutineManager](#coroutinemanager)
- [Singleton\<T\>](#singletont)
- [DataPrefs](#dataprefs)
- [ObjectGrid](#objectgrid)
- [UI](#ui)
  - [UIScaler](#uiscaler)
  - [SafeAreaApplier](#safeareaapplier)

---

## Extensions

### MathExtension

`namespace Weariness.Util.Extensions`

수학 관련 편의 확장 메서드 모음.

| 메서드 | 설명 |
|--------|------|
| `int.Random()` | 0 ~ value 범위의 랜덤 int 반환 |
| `float.GetNearValue(min, max)` | value에서 min/max 중 더 가까운 값 반환 |
| `float.GetFarValue(min, max)` | value에서 min/max 중 더 먼 값 반환 |
| `int.GetDigitCount()` | 정수의 자릿수 반환 (음수 부호 포함) |
| `float.GetDigitCount(decimalPlaces)` | 실수의 자릿수 반환 (소수점 자릿수 최대치 지정 가능) |
| `float.IsProbability(maxProbability)` | 0~maxProbability 사이 랜덤값이 probability 미만이면 true |
| `int.IsProbability(maxProbability)` | 동일, int 버전 |

```csharp
int a = 10;
int rand = a.Random(); // 0~9 랜덤

float chance = 0.7f;
bool success = chance.IsProbability(); // 70% 확률로 true

float val = 3.5f;
float near = val.GetNearValue(0f, 5f); // 5f
```

---

### StringExtension

`namespace Weariness.Util.Extensions`

TMP 텍스트 태그(Color, Sprite) 조작 및 문자열 들여쓰기 확장 메서드.

#### Color 태그

| 메서드 | 설명 |
|--------|------|
| `ApplyColorTag(color, isRemoveColor)` | `<color=#RRGGBBAA>...</color>` 태그 적용 |
| `RemoveColorTag()` | 모든 color 태그 기호 제거 (내용은 유지) |
| `RemoveColorTagAll()` | 모든 color 태그 및 내용까지 완전 제거 |
| `RemoveColorTagAt(index)` | N번째 color 태그 제거 |
| `RemoveColorTagFirst(targetStr)` | 특정 내용에 감싸인 첫 번째 color 태그 제거 |
| `RemoveColorTagWhere(condition)` | 조건에 맞는 color 태그 제거 |

#### Sprite 태그

| 메서드 | 설명 |
|--------|------|
| `ApplySpriteTag()` | `<sprite name="...">` 태그 적용 |
| `RemoveSpriteTag()` | 모든 sprite 태그 제거 |
| `RemoveSpriteTagAt(index)` | N번째 sprite 태그 제거 |
| `RemoveSpriteTagFirst(targetStr)` | 특정 sprite 태그 제거 |
| `RemoveSpriteTagWhere(condition)` | 조건에 맞는 sprite 태그 제거 |

#### 기타

| 메서드 | 설명 |
|--------|------|
| `IndentWithTabs(indentLevel, preserveEmptyLines)` | 각 줄 앞에 탭 들여쓰기 적용 |

```csharp
// Color 태그
string result = "Hello".ApplyColorTag(Color.red);
// → "<color=#FF0000FF>Hello</color>"

// 빈 줄 유지하면서 2탭 들여쓰기
string indented = "line1\nline2".IndentWithTabs(2);
```

---

### ColorExtension

`namespace Weariness.Util.Extensions`

| 메서드 | 설명 |
|--------|------|
| `ColorExtension.ParseHex(hex)` | 16진수 문자열(`"FFFFFF"` 또는 `"#FFFFFF"`)을 Color32로 변환 |

```csharp
Color32 color = ColorExtension.ParseHex("#FF5733");
```

---

### TransformExtension

`namespace Weariness.Util.Extensions`

| 메서드 | 설명 |
|--------|------|
| `Transform.LookAt2D(target)` | 2D 환경에서 대상 방향을 바라보도록 회전 (flip 처리 포함) |
| `Transform.LookAt2D(targetPosition)` | 위치 기반 오버로드 |

```csharp
transform.LookAt2D(enemyTransform);
```

---

### Vector3Extension

`namespace Weariness.Util.Extensions`

정적 유틸리티 구조체. 확장 메서드가 아닌 static 메서드.

| 메서드 | 설명 |
|--------|------|
| `Vector3Extension.Cubic(p0, p1, p2, p3, t)` | 3차 베지어 곡선 좌표 계산 |
| `Vector3Extension.CompareVector3(a, b, tolerance)` | 허용 오차 내에서 두 Vector3 비교 |

```csharp
Vector3 point = Vector3Extension.Cubic(p0, p1, p2, p3, 0.5f);
bool equal = Vector3Extension.CompareVector3(a, b, 0.001f);
```

---

### MeshExtension

`namespace Weariness.Util.Extensions`

Unity Job System을 활용한 Mesh 관련 고급 유틸리티.

| 메서드 | 설명 |
|--------|------|
| `Mesh.GetMeshGeodesicDistance(origin, direction, maxDistance, threshold)` | 메쉬 위 두 정점 사이의 지오데식 거리 계산 (Dijkstra 기반) |
| `Mesh.CreateAdjacencyGraph(allocator)` | 메쉬 삼각형으로부터 정점 인접 그래프 생성 |
| `MeshExtension.BuildAdjacencyGraph(triangles, ref adjacency)` | 인접 그래프 빌드 |

> 분리된 메쉬 컴포넌트는 자동으로 라벨링 후 유클리드 거리 기반 브릿지 연결을 시도합니다.

```csharp
float dist = mesh.GetMeshGeodesicDistance(origin, direction, 10f);
```

---

## Container

### KeyValueDictionary

`namespace Weariness.Util.Container`

Unity Inspector에서 직렬화가 안 되는 `Dictionary`의 대체제. Key/Value를 List로 저장.

```csharp
[SerializeField]
private KeyValueDictionary<string, int> scoreTable = new();

scoreTable.Add("Player", 100);
scoreTable["Player"] = 200;
scoreTable.TryGetValue("Player", out int score);
scoreTable.Remove("Player");

foreach (var (key, value) in scoreTable)
    Debug.Log($"{key}: {value}");
```

주요 API: `Add`, `TryAdd`, `Remove(key)`, `Remove(value)`, `TryGetValue`, `GetValueOrDefault`, `Clear`, `Count`

---

### Wrapping

`namespace Weariness.Util.Container`

제네릭 래퍼. Inspector에서 `new()` 생성자가 없는 타입을 리스트로 다룰 때 유용.

```csharp
[SerializeField]
private List<Wrapping<MyData>> dataList;
```

---

## MinMax / MinMaxValue

### MinMax\<T\>

`namespace Weariness.Util`

Min/Max 범위를 표현하는 제네릭 클래스. 범위 검사, 랜덤, Clamp 기능 제공.

지원 타입별 확장: `int`, `long`, `float`, `double`, `Vector2`, `Vector3`, `Vector2Int`, `Vector3Int`

```csharp
var range = new MinMax<int>(0, 10);
int clamped = range.Clamp(15);       // → 10
int rand = range.Random();           // 0~9 랜덤 (includeMax: false)
bool inRange = range.IsInRange(5);   // → true
int len = range.Length();            // → 10
```

**이벤트:** `onChangedCurrent`, `onChangedValueMin`, `onChangedValueMax`

Vector 타입은 축별 `IsInRangeX`, `IsInRangeY`, `IsInRangeZ`, `IsInRangeXY`, `IsInRangeXZ`, `IsInRangeYZ` 도 지원합니다.

---

### MinMaxValue\<T\>

`namespace Weariness.Util`

Min/Current/Max를 함께 관리하는 클래스. HP, MP 같은 게임 스탯에 적합.

지원 타입: `int`, `long`, `float`, `double`, `Vector2`, `Vector2Int`, `Vector3`, `Vector3Int`

```csharp
var hp = new MinMaxValue<int>(100, 0, 100); // current=100, min=0, max=100

hp.Current -= 30;          // 70
hp.SetMin();               // Current → 0
hp.SetMax();               // Current → 100
bool isDead = hp.IsMin;    // Current == Min

float normalized = hp.NormalizeToRange(0f, 1f); // 0.0 ~ 1.0

// implicit 변환
int value = hp; // hp.Current
```

**이벤트:** `onChangeValueMin`, `onChangeValueMax`, `onChangeValueCurrent`

**옵션:**
- `isOverMin` / `isOverMax`: Min/Max 경계를 초과 허용할지 여부
- `GetClampCurrent()`: Over 허용 시 Min~Max 내 값 반환

---

## Stat / StatModifier

`namespace Weariness.Util`

게임 스탯 시스템. BaseValue에 Modifier를 누적 적용해 최종 Value를 산출합니다.

**계산 공식:** `finalValue = (baseValue + Flat합계) * (1 + Percent합계)`

```csharp
var health = new Stat(100f);
var damage = new Stat(10f);

var flatMod = new StatModifier(StatModifier.ModifierType.Flat, 20f);
var percentMod = new StatModifier(StatModifier.ModifierType.Percent, 0.5f);

health.AddModifier(flatMod);    // 100 + 20 = 120
damage.AddModifier(percentMod); // 10 * (1 + 0.5) = 15

Debug.Log(health.Value);  // 120
Debug.Log(damage.Value);  // 15

health.RemoveModifier(flatMod);
health.Dispose(); // 정리
```

**Stat을 Modifier로 사용:**
```csharp
// damage Stat의 현재 값을 다른 Stat의 Flat 수정자로 연결
StatModifier mod = damage.AsModifier(StatModifier.ModifierType.Flat);
health.AddModifier(mod);
```

Inspector에서 `[SerializeField]`로 직렬화되며, Editor에서도 올바르게 표시됩니다.

---

## Unique\<T\>

`namespace Weariness.Util`

중복 없이 랜덤으로 원소를 뽑는 컨테이너. 한 번 뽑힌 원소는 제거됩니다.

기본 지원 타입: `int`, `Vector2Int`, `Vector3Int`

```csharp
// int: 0~5 범위 셋업
var unique = new Unique<int>(0, 5);

while (unique.Length > 0)
{
    int value = unique.Get(); // 중복 없이 랜덤 추출
}

unique.Add(3);    // 원소 추가 (중복 무시)
unique.Remove(2); // 원소 제거
```

**커스텀 타입 지원 (`IUniqueMaker<T>`):**

```csharp
[Serializable]
public partial class MyItem { public int id; }

public partial class MyItem : IUniqueMaker<MyItem>
{
    public void UniqueMake(Unique<MyItem> unique, MyItem start, MyItem end)
    {
        for (int i = 0; i < 5; i++)
            unique.Add(new MyItem { id = i });
    }
}

var unique = new Unique<MyItem>();
unique.Init(null, null, new MyItem());
var item = unique.Get();
```

---

## EventBusSystem

`namespace Weariness.Util`

Enum 키 기반 이벤트 버스. 타입 파라미터로 이벤트 채널을 구분합니다.

```csharp
public enum GameEvent { PlayerDied, ScoreChanged }

// 구독
EventBusSystem<GameEvent>.Subscribe(GameEvent.ScoreChanged, OnScoreChanged);
EventBusSystem<GameEvent>.Subscribe<int>(GameEvent.ScoreChanged, OnScoreChangedWithData);

// 발행
EventBusSystem<GameEvent>.Publish(GameEvent.PlayerDied);
EventBusSystem<GameEvent>.Publish<int>(GameEvent.ScoreChanged, 100);

// 구독 해제
EventBusSystem<GameEvent>.Unsubscribe(GameEvent.ScoreChanged, OnScoreChanged);
EventBusSystem<GameEvent>.Unsubscribe<int>(GameEvent.ScoreChanged, OnScoreChangedWithData);

// 전체 초기화
EventBusSystem<GameEvent>.Clear();
```

---

## PoolSystem

`namespace Weariness.Util`

`GameObject` 기반 오브젝트 풀. 씬 전환 시 자동으로 풀이 초기화됩니다.

```csharp
// 가져오기
GameObject obj = PoolSystem.Get(prefab);

// 컴포넌트 타입으로 가져오기
MyComponent comp = PoolSystem.Get<MyComponent>(prefab);

// 반환
PoolSystem.Release(obj);
PoolSystem.Release(comp);
```

> 풀에서 꺼낸 오브젝트는 자동으로 `SetActive(true)`, 반환 시 `SetActive(false)`됩니다.
> `Additive` 씬 로드 시에는 풀이 초기화되지 않습니다.

---

## CoroutineManager

`namespace Weariness.Util.Managers`

MonoBehaviour 없이도 코루틴을 실행/정지할 수 있는 싱글턴 매니저.
키(key) 기반으로 그룹 관리가 가능하며, 모든 코루틴이 끝나면 내부 GameObject가 자동 정리됩니다.

```csharp
// 기본 그룹으로 실행
CoroutineManager.Play("MyCoroutine", MyEnumerator());

// 키 그룹 지정
CoroutineManager.Play("BossGroup", "AttackRoutine", AttackEnumerator());

// 정지
CoroutineManager.Stop("MyCoroutine", MyEnumerator());
CoroutineManager.Stop("BossGroup", "AttackRoutine", AttackEnumerator());

// 실행 여부 확인
bool isRunning = CoroutineManager.HasCoroutine("MyCoroutine", MyEnumerator());
```

---

## Singleton\<T\>

`namespace Weariness.Util`

MonoBehaviour 기반 싱글턴 베이스 클래스.

```csharp
public class GameManager : Singleton<GameManager>
{
    protected override void Initialize()
    {
        // 최초 생성 시 초기화 로직
    }
}

// 사용
GameManager.Instance.DoSomething();
GameManager.Make(); // 미리 생성
bool exists = GameManager.HasInstance;
```

**옵션:**
- `Singleton<T>.IsDontDestroy = true` (기본값): 씬 전환 시 파괴 방지
- 씬에 이미 오브젝트가 있으면 그것을 사용, 없으면 자동 생성

---

## DataPrefs

`namespace Weariness.Util`

`PlayerPrefs` 대신 JSON 파일(`Assets/Resources/DataPrefs/DataPrefs.json`)로 데이터를 저장/불러오는 유틸리티.

> **저장(Save)은 Editor 전용**이며, 런타임에서는 불러오기만 가능합니다.

```csharp
// 저장
DataPrefs.SetString("PlayerName", "Alice");
DataPrefs.SetInt("Score", 9999);
DataPrefs.SetFloat("Volume", 0.8f);
DataPrefs.SetBool("IsTutorialDone", true);

// 불러오기
string name = DataPrefs.GetString("PlayerName", "Unknown");
int score = DataPrefs.GetInt("Score", 0);
float vol = DataPrefs.GetFloat("Volume", 1f);
bool done = DataPrefs.GetBool("IsTutorialDone", false);

// 키 존재 확인
bool has = DataPrefs.HasKey("Score");

// 강제 저장/불러오기
DataPrefs.SavePrefs();
DataPrefs.LoadPrefs();
```

---

## ObjectGrid

`namespace Weariness.Util`

자식 오브젝트를 3D 그리드 형태로 자동 배치하는 `MonoBehaviour`.

**주요 설정:**

| 프로퍼티 | 설명 |
|----------|------|
| `gridCount` | 격자 수 (X/Y/Z) |
| `cellBoxSize` | 셀 크기 |
| `spacing` | 셀 간격 |
| `cellAngle` | 셀 회전 각도 |
| `cellScale` | 셀 스케일 |
| `padding` | 그리드 전체 여백 (RectOffset) |
| `isUseX/Y/Z` | 해당 축 배치 활성화 여부 |
| `sortType` | 정렬 시작 방향 (`LeftUpForward`, `RightDownBack` 등 8가지) |
| `isUpdate` | FixedUpdate에서 매 프레임 갱신 여부 |

- 비활성화(`SetActive(false)`) 자식은 배치 대상에서 제외됩니다.
- Scene View 선택 시 그리드 Gizmo가 표시됩니다.

```csharp
// 코드에서 강제 갱신
GetComponent<ObjectGrid>().ForceUpdate();
```

---

## UI

### UIScaler

`namespace Weariness.Util.UI`

화면 크기 변화에 반응하는 반응형 UI 스케일러.

**요구 사항:**
- 루트 Canvas의 `Render Mode`: `Screen Space - Overlay`
- 루트 CanvasScaler의 `UI Scale Mode`: `Scale With Screen Size`

```
Inspector에서 Reset 버튼 클릭 시 자동으로 참조를 잡아줍니다.
```

- 화면 크기가 변경되면 다음 프레임 끝에 스케일을 자동 재계산합니다.
- `isUpdate = false`로 자동 업데이트를 끌 수 있습니다.
- Stretch 앵커(`anchorMin == anchorMax`가 아닌 경우)도 처리합니다.

---

### SafeAreaApplier

`namespace Weariness.Util.Mobile`

모바일 Safe Area에 맞춰 RectTransform을 자동 보정하는 컴포넌트.

**주요 설정:**

| 프로퍼티 | 설명 |
|----------|------|
| `affectX / affectY` | X/Y축 보정 활성화 여부 |
| `isNavigationBar` | 네비게이션 바 높이 보정 (Android) |
| `padding` | Safe Area 내부 추가 여백 |
| `areaSize` | 비 Stretch UI의 기준 크기 |

- Stretch UI(Full Screen 등)와 고정 크기 UI 모두 지원
- `OnRectTransformDimensionsChange`, `OnEnable` 시 자동 적용
- Android 네비게이션 바 대응: 풀스크린 전환 후 Safe Area 차이로 높이 계산

---

## Identifier\<T\>

`namespace Weariness.Util`

임의 타입 참조를 GameObject에 붙이기 위한 범용 컴포넌트.

```csharp
// PoolSystem 내부에서 사용 예시
var identifier = obj.GetOrAddComponent<GameObjectPoolIdentifier>();
identifier.target = poolingInstance;
```

---

## 네임스페이스 정리

| 네임스페이스 | 포함 내용 |
|--------------|-----------|
| `Weariness.Util` | Stat, Unique, EventBus, Pool, Singleton, DataPrefs, ObjectGrid, MinMax, MinMaxValue |
| `Weariness.Util.Extensions` | Math, String, Color, Transform, Vector3, Mesh 확장 메서드 |
| `Weariness.Util.Container` | KeyValueDictionary, Wrapping |
| `Weariness.Util.Managers` | CoroutineManager |
| `Weariness.Util.UI` | UIScaler |
| `Weariness.Util.Mobile` | SafeAreaApplier |
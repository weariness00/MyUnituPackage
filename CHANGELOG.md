# Changelog

이 프로젝트의 주요 변경 사항을 기록합니다.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)를 따르며,
버전 규칙은 [Semantic Versioning](https://semver.org/lang/ko/)을 따릅니다.

## [1.0.19] - 2026-06-30

### Added
- **Spawner**: `ObjectSpawner` / `ObjectPoolSpawner` 추가 및 `SpawnPlace` 추상화 도입
  - `ISpawnPlace`, `SpawnPlaceType`, `Circle/Line/Rect/Transform SpawnPlace` 구현
  - `ObjectSpawner` 커스텀 인스펙터(`ObjectSpawnerEditor`) 추가
- **Looting**: 루팅 시스템 추가 (`LootingResolver`, `LootingGroup`, `LootingEntry`, `LootingResult`, `LootingPickMode`, `LootingDuplicateMode`)
- **Threshold**: 임계값 추적 시스템 추가 (`ThresholdTracker`, `ThresholdEntry`, `ThresholdDirection`)
- **UniqueList**: 기존 `Unique<T>`를 대체하는 `UniqueList` 추가 (`IUniqueListMaker`, `UniqueExtensions`)
- **EnumSerialized\<TEnum\>** 컨테이너 및 전용 드로어 추가
- **Extensions**:
  - `ComponentExtensions.GetOrAddComponent<T>`
  - `LocalizeExtensions.LocalizeAsync` (Localization + UniTask)
  - `StringExtension`에 positional/named placeholder Format 헬퍼 추가
  - `Vector3Extension` 헬퍼 추가
- **PoolSystem**: `AssetReferenceGameObject` 기반 풀링 지원 (`Get` / `GetAsync`, Addressable 동기·비동기 로드 및 GUID 캐시)
- **Stat**: `PercentMultiply` 모디파이어 추가
- `SUBTREE.md` 사용 규칙 문서 추가

### Changed
- `Looting` / `Threshold` 네임스페이스를 `Weariness.Util`로 통일
- `ObjectPoolSpawner.PoolInstantiate()`를 `protected`로 변경
- `LootingResolver`를 static으로 처리
- `MeshExtension` 로직 개선
- 에디터 드로어 수정: `KeyValueDictionaryDrawer`, `MinMaxPropertyDrawer`, `MinMaxVectorBaseDrawer`, `StatDrawer`, `StatModifierDrawer`
- 패키지 의존성 추가: `com.unity.localization` 1.5.12, `com.unity.addressables` 2.4.6, `com.cysharp.unitask` 2.5.10
- `Weariness.Util.asmdef`에 Localization / UniTask / Addressables / ResourceManager 참조 추가
- README 갱신 (Spawner, PoolSystem, Looting, Threshold, UniqueList 등)

### Removed
- `Unique<T>` / `IUniqueMaker` 제거 (`UniqueList`로 대체)

[1.0.19]: #

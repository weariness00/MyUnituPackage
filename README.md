# MinMaxValue / MinMax

GitHub: [https://github.com/weariness00/MyUnituPackage/tree/MinMaxValue](https://github.com/weariness00/MyUnituPackage/tree/MinMaxValue)

---

## 개요 (Overview)

### 문제 정의

RPG 등 수치 기반 게임에서 HP, MP, 스태미나, 경험치처럼 **최소값(Min)**, **최대값(Max)**, **현재값(Current)** 을 각각 따로 선언하고 인스펙터에서 개별 필드로 관리하면 번거롭고, 값 동기화/검증 로직이 중복되며 실수 확률이 높습니다.

### 목표

* 자주 쓰는 패턴을 **재사용 가능한 데이터 구조**로 추상화
* Unity Inspector에서 **직관적**으로 확인·편집
* **제네릭**으로 `int`, `float` 등 다양한 수치 타입 지원

### 결과

* 변수 선언/검증/보정(Clamp) 로직의 **중복 제거**
* **커스텀 드로어**로 한 줄에 Min / Current / Max를 표시해 작업 효율 증가
* 범용 유틸 패키지로 **OpenUPM 배포** 고려한 설계

---

## 제공 컴포넌트

### 1) `MinMaxValue<T>`

* **역할**: `Min`, `Max`, `Current` 세 값을 한 번에 관리하는 통합 구조체(클래스 기반 구현).
* **용도**: HP/MP/스태미나/게이지처럼 “현재값”이 특정 범주 내에 존재하는 데이터에 사용.
* **특징**

    * `Current`가 범위를 벗어나면 **자동 보정(Clamp)**
    * `IsMin`, `IsMax`, 정규화(`NormalizeToRange()` 또는 동등 기능) 같은 편의 API
    * Inspector에서 한 줄로 **Min / Current / Max**를 직관적으로 편집(커스텀 드로어)

```csharp
using UnityEngine;
using Weariness.MinMax; // 실제 네임스페이스 기준으로 수정

public class PlayerStats : MonoBehaviour
{
    public MinMaxValue<int> health = new MinMaxValue<int>(0, 100, 50);
    public MinMaxValue<float> mana = new MinMaxValue<float>(0f, 50f, 25f);

    void Damage(int amount)
    {
        health.Current -= amount; // 자동으로 Min~Max 범위 내 보정
        Debug.Log($"HP ratio: {health.NormalizeToRange()}"); // 0~1 정규화
    }

    void RecoverMana(float amount)
    {
        mana.Current += amount; // 자동 보정
        if (mana.IsMax) Debug.Log("Mana full!");
    }
}
```

### 2) `MinMax<T>`

* **역할**: “범위(최소/최대)만” 필요할 때 쓰는 간결한 구조.
* **용도**: 난수 생성 범위, 에이밍/감도 설정, 데미지 변동폭, 슬라이더 한계치 등.
* **특징**

    * `Min`, `Max`만 유지 → 가벼움
    * 값 검증/보정 보조: 예) `IsInRange(value)`, `Clamp(value)` 등의 범위 체크(해당 API가 있다면 사용)

```csharp
public class VolumeSetting : MonoBehaviour
{
    public MinMax<float> volumeRange = new MinMax<float>(0f, 1f);

    public float ApplyVolume(float raw)
    {
        return volumeRange.Clamp(raw); // 필요 시 범위 보정
    }
}
```

> 요약
>
> * `MinMaxValue<T>`: **현재값을 가진 스탯**
> * `MinMax<T>`: **범위 한계만 필요한 설정/로직**

---

## Inspector & 에디터 경험

* **Inspector 친화적 UI**

    * `MinMaxValue<T>`: 한 줄에 **Min / Current / Max** 정렬, 즉시 입력 보정
    * `MinMax<T>`: **Min/Max**만 간결하게 표시
* **커스텀 드로어**

    * `[CustomPropertyDrawer]` 기반 깔끔한 라벨/필드 배치
    * 실수 최소화를 위해 **입력 즉시 Clamp** 적용
    * (선택) 값 표시 폭/소수점 자리수 자동 최적화 등 UX 개선 포인트 반영

---

## 설계 & 아키텍처

* **구조**

    * `MinMaxValue<T>`: **제네릭 클래스**

        * `where T : struct, IComparable<T>` 제약으로 범위 비교 보장
        * 내부 setter/연산 시 자동 **Clamp**
    * `MinMax<T>`: **제네릭 구조**로 범위만 관리(필요 타입 특화 가능)
    * 공통 컨셉: 값 \*\*일관성(불변식)\*\*을 타입 내부에서 강제 → 호출부 코드 단순화
* **핵심 의사결정**

    * 스탯별로 흩어진 `min/max/current` 변수를 **하나의 타입**으로 통합 → 가독성/유지보수성 향상
    * 보정(Clamp) 로직을 타입 내부로 캡슐화하여 **가드 로직 중복 제거**
    * 런타임/에디터 공용 타입 유지 + 에디터 전용 드로어 제공

---

## 사용 패턴

* **스탯/게이지** → `MinMaxValue<T>`

    * HP, MP, 게이지/쿨다운 등 “현재값”이 존재하는 데이터
* **설정/범주** → `MinMax<T>`

    * 난수 범위, 감도/속도 제한, 슬라이더 한계치, 밸런스 파라미터 구간 등

---

## 성능/품질 (Perf & Quality)

* **할당 특성**

    * `MinMaxValue<T>`는 **클래스**이므로 생성 시 **힙 할당**이 발생합니다.
    * 제네릭 매개변수 `T`는 **구조체 제약**이라 값 저장 시 **박싱 없이** 처리됩니다.
* **권장 사항**

    * 대량 생성/파괴가 잦으면 **객체 풀링** 또는 **필드 재사용**으로 GC 압력 완화
    * “현재값이 필요 없는” 지점은 **MinMax<T>** 로 대체해 불필요 필드/연산 최소화
    * 빈번한 수치 갱신 루프에서는 `Current` 변경이 **Clamp 1회**로 귀결되도록 상위 로직 단순화

---

## 에디터 커스텀 드로어 기능(요약)

* 한 줄 배치: **Min / Current / Max**
* 입력 즉시 Clamp 및 상태 플래그(`IsMin`, `IsMax`) 시각 보조(있다면)
* 숫자 폭/소수점 자리수 자동 최적화(있다면)로 UI 폭 낭비 최소화

---

## 라이선스 & 기여

* 라이선스 표기 및 컨트리뷰션 가이드는 저장소 `README` 기준
* 이슈/PR 환영: API 제안, 에디터 UX 개선 아이디어 등

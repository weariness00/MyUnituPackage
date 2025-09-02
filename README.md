# ImageTransition

GitHub: [https://github.com/weariness00/MyUnituPackage/tree/ImageTransition](https://github.com/weariness00/MyUnituPackage/tree/ImageTransition)

---

## 개요 (Overview)

### 문제 정의

UGUI에서 페이드/와이프/슬라이드/격자 분할 같은 **이미지 전환 효과**를 매 화면마다 별도 스크립트·애니메이터·마스크로 구현하면 **중복 코드**가 늘고 연출 **일관성**이 떨어집니다. 특히 **사각(Rectangle)/마름모(Rhombus)** 격자 단위로 블록을 쪼개 “시차(Stagger)” 전환을 만들려면, 버텍스/인덱스 생성과 그리드 인덱싱, 진행도에 따른 TRS(위치/회전/스케일)·알파 변형 관리가 필요합니다.

### 목표

* UGUI `Image`에 **컴포넌트 1개**만 붙여 **격자 기반 전환**을 즉시 사용
* **사각/마름모** 블록 생성과 **정렬(TextAnchor) 기준 순차 재생** 제공
* 코드/인스펙터에서 **진행도(normalizedTime)** 를 0\~1로 제어하여 미리보기
* **UniTask**로 간결한 비동기 연출 API 지원

### 결과

* 재사용 가능한 전환 파이프라인으로 **제작 효율**·**일관성** 향상
* 격자·정렬·이징만 바꿔 다양한 연출을 **코드 1\~2줄**로 구성
* **진행도(normalizedTime)** 를 0\~1로 제어하여 미리보기를 지원하려 했으나 미리보기를 지원할시 Vertex를 매번 계산하고 업데이트 해야되는 문제점이 있어 사용하지 않음 ( 추후에 에디터 전용으로만 만들 예정 )
---

## 제공 컴포넌트

### 1) `ImageTransition` (UGUI Image 파생)

* **역할**: 이미지 메시를 격자 블록으로 분해하고, 진행도에 따라 블록별 변형을 적용해 전환을 렌더링.
* **핵심 필드**

    * `TransitionBlockType blockType` : `Rectangle | Rhombus`
    * `TextAnchor childAlignment` : **재생 시작점/방향**(UpperLeft/UpperRight/LowerLeft/LowerRight/MiddleCenter …)
    * `Vector2Int grid` : 격자 크기 (가로 x, 세로 y)
    * `float normalizedTime` : 전환 진행도(0\~1).
* **주요 메서드**

    * `TransitionRotate(start, end, duration, delayInterval, ease)`
    * `TransitionFlash(duration, delayInterval, ease, isOnOff)`
    * `Transition(updateFunc, duration, delayInterval, ease)` — **사용자 정의 블록 업데이트**를 전달하는 일반화 API

### 2) 전환 핸들러 & 데이터

* `ITransitionHandler` — 블록 생성/인덱싱 인터페이스
* `RectangleTransitionHandler` / `RhombusTransitionHandler` — 사각/마름모 전용 구현
* `TransitionData` — 그리드/인덱스 등 공용 데이터
* `TransitionUIBlock` — 1개 블록(원본/변형 버텍스, UV, 인덱스)
* `UIVertexExtension` — UIVertex 유틸리티

### 3) 이징

* `TransitionEase` (`Linear`, `OutExpo`), `TransitionEaseCalculate` — 이징 계산기

### 4) 열거형

* `TransitionBlockType { Rectangle, Rhombus }`
* `TransitionAnimationType { OneByOne, OneLineWidth, OneLineHeight, AdjacentRegions }`
* `TransitionEase { Linear, OutExpo }`

---

## Inspector & 에디터 경험

* **ImageTransitionEditor**에서 한 번에 편집
    * `Block Type` / `Child Alignment` / `Grid (x,y)` / `Normalized Time`

---

## 설계 & 아키텍처

* **흐름**: *(ImageTransition 설정)* → *(ITransitionHandler가 블록 생성)* → *(정렬/격자 기반 순서 산출)* → *(이징/진행도로 블록 TRS/색상 보간)* → *(OnPopulateMesh 렌더링)*
* **핵심 포인트**

    * `OnPopulateMesh(VertexHelper)` 오버라이드로 **UI 버텍스/인덱스**를 동적 생성
    * `childAlignment`·`grid`에 따라 블록 **재생 순서**(좌상→우하, 중앙→외곽 등)를 결정
    * 핸들러 교체만으로 **사각↔마름모** 전환 구조를 재사용
    * `Normalilze Time(0 ~ 1)`값으로 Transition의 진행 상태를 파악
---

## 사용 패턴

* **화면/페이지 전환**: 동일 프리셋으로 UI 간 연출 일관성 확보
* **배너/카드 롤링**: 그리드 밀도·정렬만 변경해 다양한 맛 내기
* **컷신/팝업 인/아웃**: 회전/플래시·슬라이드 등 간단 조합으로 빠른 제작

---

## 성능/품질 (Perf & Quality)

* **버텍스 수**: `grid.x * grid.y`에 비례해 증가 — 고밀도 격자는 **드로우/버텍스 코스트** 상승
* **캔버스 배치**: Canvas 전체 리빌드를 줄이기 위해 **필요한 오브젝트에만** 전환 적용 권장
* **GC/할당**: 내부 배열 재사용으로 런타임 할당을 최소화(구현 기준). 대량 동시 재생 시엔 **프리셋 공유/필드 재사용**을 권장

---

## 사용법 (예시)

1. **컴포넌트 부착 & 실행**

    * Canvas 하위 `Image` 오브젝트에 **ImageTransition** 추가
    * 인스펙터에서 `Block Type/Alignment/Grid` 설정

2. **코드 예시**

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using Weariness.Transition;

public class ImageTransitionTest : MonoBehaviour
{
    [SerializeField] private ImageTransition transition;

    private async UniTaskVoid Start()
    {
        // 회전 전환
        transition.TransitionRotate(
            start: Vector3.zero,
            end:   new Vector3(0f, 0f, 90f),
            duration: 0.6f,
            delayInterval: 0.02f,
            ease: TransitionEase.OutExpo
        );

        await UniTask.Delay(700);

        // 플래시 전환
        transition.TransitionFlash(
            duration: 0.3f,
            delayInterval: 0.01f,
            ease: TransitionEase.Linear,
            isOnOff: true
        );
    }
}
```

---

## 라이선스 & 기여

* 라이선스/컨트리뷰션 가이드는 저장소 루트의 `LICENSE.md`, `CANGELOG.md`를 따릅니다.
* 버그 제보/기능 제안/PR 환영합니다.

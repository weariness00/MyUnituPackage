## FMOD Occlusion

GitHub: [https://github.com/weariness00/MyUnituPackage/tree/Fmod-Occlusion](https://github.com/weariness00/MyUnituPackage/tree/Fmod-Occlusion)

---

## 개요 (Overview)

### 문제 정의

3D 게임에서 **벽/문/지형**에 가려진 소리가 **현실적으로 감쇠**되지 않으면 이질감이 커집니다. Unity + FMOD 조합에서 이를 매번 수작업(파라미터 키프레임, 이벤트마다 스크립팅)으로 처리하면 **중복 코드**, **파라미터 불일치**, **씬 전환 시 설정 누락** 같은 문제가 잦습니다.

### 목표

* 간단한 **컴포넌트 하나**로 오클루전 값을 계산해 FMOD 파라미터에 전달
* 프로젝트 전반에 **동일한 파라미터 흐름**을 적용해 유지보수 비용을 낮춤
* 장애물에 따른 감쇠율을 수동으로 조정하여 **게임플레이에 최적화된 오클루전**을 구현

### 결과

* 오디오 이벤트마다 같은 로직 복붙 없이 **일관된 오클루전 처리** → 유지보수 비용 감소
* `Compute Shader`와 `Custom Shader`를 활용한 **GPU 기반 오클루전 마스크**로 성능 최적화
---

## 제공 컴포넌트

### 1) `FMOD_OcclusionListener`

* **역할**: FMOD Studio Listener를 대체/확장하여 **오클루전 값**을 계산·전달
* **사용**: 기본 `StudioListener` 대신 본 컴포넌트를 부착(카메라/Audio Listener 오브젝트)

### 2) `StudioEventEmitter` (패치/대체)

* **역할**: 별도의 "OcclusionEmitter" 컴포넌트를 만들지 않고, 기존 `StudioEventEmitter` 스크립트를 **수정/대체**하여 오클루전 반영이 자동으로 이루어지도록 함
* **의도**: 팀 전반에 흔히 쓰이는 API를 유지하면서 **사용 코스트를 0에 가깝게**

### 3) `OcclusionRay` / `Occlusion Camera` `OcclusionRay` / `Occlusion Camera`

* **역할**: 레이 기반 또는 **카메라(RenderTexture) 기반** 감지 보조
* **메모**: 카메라 기반 감지에서는 FMOD 내부 업데이트 지연을 보완하기 위해 `(1f - OcclusionValue)`를 **일시적으로 볼륨에 곱해 감쇠를 판정**하는 방식이 포함됩니다.

### 4) `Occlusion Sound Detecting`

* **역할**: **가상 Listener**를 활용해 사운드를 탐지하고, 감지 범위 내 이벤트의 위치를 추적

### 5) `FMOD_OcclusionMode` / `FMOD_OcclusionUtil`

* **역할**: 오클루전 모드 정의 및 헬퍼 유틸리티 제공
* **모드**: Ray 오클루전, Render Texture 오클루전 제공

---

## 사용 예시

### 리스너 기반: 전체 오디오에 오클루전 적용

1. 씬의 **FMODUnity StudioListener**를 제거합니다.
2. 동일 오브젝트(보통 **메인 카메라/Audio Listener**)에 `FMOD_OcclusionListener`를 부착합니다.

### AI/게임플레이: 사운드 탐지에 활용

1. 탐지 주체(예: **AI 캐릭터**)에 **`Occlusion Sound Detecting`** 컴포넌트를 부착합니다.
2. 프로젝트 요구에 맞게 **가상 리스너/탐지 거리/레이 전략** 등을 구성합니다.&#x20;
3. 컴포넌트가 제공하는 **감지 결과**를 읽어 경계/추적 등 게임플레이 로직에 연결합니다.

#### 코드: 수동 탐지(성능 예산 제어)

```csharp
using System;
using UnityEngine;
using Weariness.FMOD.Occlusion.Detecting;

namespace Test.FMOD_Occlusion
{
    public class Detector : MonoBehaviour
    {
        public FMOD_Occlusion_SoundDetectingController soundDetectingController;

        private Transform emitterTransform;

        public void FixedUpdate()
        {
            if (soundDetectingController.TryAnyDetecting(out emitterTransform))
            {
                Debug.Log($"사운드 감지 {emitterTransform.name}의 사운드의 위치 [{emitterTransform.position}]");
            }
        }
    }
}
```

> **왜 자동으로 돌리지 않나?** 컴포넌트가 내부에서 매 프레임 감지를 수행하면 프로젝트 전반의 **레이/업데이트 예산**을 통제하기 어렵습니다. 이 패턴은 **사용자(게임 로직)** 가 `Update`/`FixedUpdate`/Coroutine/Job 등 원하는 타이밍에 감지를 호출하여 **성능을 직접 관리**하도록 설계했습니다.
---
## 설계 & 아키텍처

* **데이터 흐름**: *(Listener 위치)* → *(레이/카메라 기반 감지)* → *(0\~1 오클루전 값)* → *(FMOD 파라미터/볼륨 반영)*
* **의존성**: `FMODUnity` 패키지(Studio Event Emitter/Listener), Unity Physics, (카메라 기반 시) RenderTexture

### 설계 노트: 사실적 음파 시뮬레이션 시도 → RenderTexture 대체

* **시도 배경**: 더 사실적인 결과를 위해 **음파 파동(웨이브) 시뮬레이션**을 검토했으나, 런타임 성능 비용이 커 실사용에 부적합했습니다.
* **대체 전략**: **GPU(RenderTexture)** 를 활용한 **카메라 기반 오클루전 마스크** 방식으로 전환하여, *근사값*을 빠르게 산출하고 이를 파라미터/볼륨에 반영하도록 설계
* **결과**: 유지보수와 성능 사이의 현실적인 균형을 선택—정확한 전파 물리 대신, 게임플레이에 충분한 품질의 근사치를 일관되게 제공

---

## 사용 패턴

* **FPS/TPP**: 캐릭터/적 개체의 발소리/총성/대사에 부착
* **문/창문 상호작용**: 여닫힘에 따라 감쇠 변화 반영
* **환경음**: 지역 앰비언트에 연결해 지형/건물 내부에 따른 자연스러운 감쇠

---

## 필수 환경 셋팅
- FMOD를 기반으로 동작함으로 필수적으로 FMOD를 프로젝트에 설치해주셔야합니다.
- https://assetstore.unity.com/packages/p/fmod-for-unity-2-02-161631 해당 에셋스토어에서 패키지를 추가해주세요
- 오직 PC 환경에서만 사용 가능합니다.

---

## 참고 자료 (References)

* FMOD for Unity (Asset Store): [https://assetstore.unity.com/packages/p/fmod-for-unity-2-02-161631](https://assetstore.unity.com/packages/p/fmod-for-unity-2-02-161631)
* Listener Occlusion 튜토리얼(YouTube): [https://www.youtube.com/watch?v=wTOHc803\_ys\&t=382s](https://www.youtube.com/watch?v=wTOHc803_ys&t=382s)

## 라이선스 & 기여

* 라이선스/컨트리뷰션 가이드는 저장소 기준을 따릅니다. 이슈/PR 환영합니다.

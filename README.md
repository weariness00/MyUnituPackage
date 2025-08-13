# Singleton

GitHub: [https://github.com/weariness00/MyUnituPackage/tree/Singleton](https://github.com/weariness00/MyUnituPackage/tree/Singleton)

---

## 개요 (Overview)

### 문제 정의

게임 전역에서 하나만 존재해야 하는 매니저(예: GameManager, AudioManager, SaveSystem)를 **안전하고 일관된 방식**으로 관리하기 어렵습니다. 수동으로 오브젝트를 배치하거나 씬 전환 시 중복 인스턴스가 생기는 문제, `DontDestroyOnLoad` 처리 누락 등으로 버그가 발생합니다.

### 목표

* **제네릭 기반**의 재사용 가능한 싱글턴 베이스 제공
* 씬 전환/중복 생성/라이프사이클을 **일관되게** 처리
* 사용자가 필요한 옵션만 오버라이드하여 **의도대로 동작**하도록 단순화

### 결과

* 전역 매니저의 **중복/수명 관리 코드 제거**
* `Instance` 접근 통일로 **가독성/유지보수성 향상**
* 샘플 패턴을 통해 팀 내 **코딩 컨벤션 표준화**

---

## 제공 컴포넌트

### `Singleton<T>` (MonoBehaviour 기반)

* **역할**: `T` 타입이 **프로젝트 내에서 단 하나만 존재**하도록 보장하는 베이스 클래스.
* **핵심 기능**

    * `Instance` 정적 프로퍼티로 전역 접근
    * `Awake()`에서 단일 인스턴스 보장 처리(파생 클래스에서 `base.Awake()` 호출 필요)
    * `IsDontDestroy` 플래그로 **씬 간 지속 여부** 제어 (기본값/동작은 구현에 따르며, 필요 시 파생 클래스에서 설정)
* **예시**

```csharp
using UnityEngine;

public class TestSingleton : Singleton<TestSingleton>
{
    public override void Awake()
    {
        // 파괴 불가 객체로 전환하지 않고, 현재 씬에 종속되게 유지
        IsDontDestroy = false;
        base.Awake();
    }

    public void DoSomething()
    {
        Debug.Log("Singleton instance is doing something!");
    }
}

public class Other : MonoBehaviour
{
    private void Start()
    {
        TestSingleton.Instance.DoSomething();
    }
}
```
---

## 설계 & 아키텍처

* **제네릭 패턴**: `Singleton<T>`는 `T : MonoBehaviour` 제약을 가정한 전형적인 유니티 싱글턴 패턴을 단일 베이스로 캡슐화
* **수명 관리**: 초기화는 `Awake()`에서 수행하고, 필요 시 `DontDestroyOnLoad`를 적용할 수 있도록 속성 노출
* **의존성 방향**: 호출부는 전역 상태를 `Singleton<T>.Instance`로만 참조 → 생성/중복/해제 정책은 베이스 클래스에서 통일

---

## 사용 패턴

* **전역 매니저**: `AudioManager`, `GameManager`, `InputManager`, `UIRoot` 등
* **서비스 접근자**: 저장/설정/로깅/원격 설정/네트워크 클라이언트 등 공용 서비스
* **씬 전환 정책**

    * 공용 전역 매니저: `IsDontDestroy = true` (전역 지속)
    * 씬 한정 컨트롤러: `IsDontDestroy = false` (씬 종속)

---

## 라이선스 & 기여

* 라이선스/컨트리뷰션 가이드는 저장소 기준을 따릅니다. 이슈/PR 환영합니다.

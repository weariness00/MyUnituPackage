# Extensions란
- 모든 확장형 함수 또는 컨테이너 또는 Util성 모듈들을 모아둔 라이브러리

# MathExtension
- 수학 관련 계산을 편리하게 하기 위한 확장형 함수
- 현재는 int, float만 지원

### Example
```utf8
// 0~10의 랜덤값 출력
// 기본 최소값은 0
int a = 10;
Debug.Log(a.Random());

// 현재 값을 확률이라고 보고 해당 확률에 당첨되었는지에 대한 bool 반환
float chance = 0.5f; // 50% 확률
bool isLucky = chance.IsLucky();
Debug.Log(isLucky);
```
---
# Unique
- Random하게 원소를 뽑는데 중복 뽑기를 방지하기 위해 사용
- int, Vector2Int, Vector3Int 만 기본 지원
- 커스텀 class, struct에 Unique 만들려면 IUniqueMaker<T> 를 상속


### 사용 예시 Unique\<int>
```utf8
public Unique<int> intUnique = new(0,3); // 0부터 3까지 0,1,2,3 총 4개의 원소가 셋팅
var value = intUnique.Get(); // 4개의 원소중 특정 원소 반환
intUnique.Add(4); // 4라는 새로운 원소 추가 (단, 중복 추가는 불가능)
intUnique.Remove(1); // 특정 원소 제거
```

### IUniqueMaker<T> 를 상속 받아 커스텀 Unique 생성
```utf8
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Weariness.Util;

public class UniqueTest : MonoBehaviour
{
    public Unique<CustomUnique> customUnique;

    public void Awake()
    {
        customUnique = new();
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (customUnique.Length > 0)
            {
                Debug.Log($"{customUnique.Get().obj.name}을 Int Unique로부터 뽑다.");
            }
            else
            {
                customUnique.Init(null, null, new CustomUnique());
            }
        }
    }
}

[Serializable]
public partial class CustomUnique
{
    public int id;
    public GameObject obj;
}

public partial class CustomUnique : IUniqueMaker<CustomUnique>
{
    // start, end를 사용해도되고 안해도 상관없다.
    public void UniqueMake(Unique<CustomUnique> unique, CustomUnique start, CustomUnique end)
    {
        var objs = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int id = 0;
        foreach (var obj in objs)
        {
            var cu = new CustomUnique()
            {
                id = id++,
                obj = obj,
            };
            unique.Add(cu);
        }
    }
}

```
---
# Stat, StatModifier
- Status를 사용할때 계산 공식을 구조화한 클래스
- Stat은 기본적으로 StatModifier를 가지고 있으며, StatModifier는 Stat에 영향을 주는 수정자이다.
- Stat의 값은 float으로 관리된다.
- StatModifier은 Flat(float 형 실수), Percent(float 형 실수), 
  Flat은 Value 에 +로 추가되는 수치이다.  
  StatModifier는 Stat에 영향을 주는 수정자이다.

### 사용 예시
```utf8
using System;
using UnityEngine;
using Weariness.Util;

public class StatTest : MonoBehaviour
{
    public Stat health;
    public Stat damage;
    public StatModifier flatModifier = new StatModifier(20, StatModifierType.Flat);
    public StatModifier percentModifier = new StatModifier(0.5f, StatModifierType.Percent);

    private void Awake()
    {
        health = new Stat(100);
        damage = new Stat(10);

        // Flat 수정자 추가
        health.AddModifier(flatModifier);
        // Percent 수정자 추가
        damage.AddModifier(percentModifier);

        Debug.Log($"Health: {health.Value}, Damage: {damage.Value}");
    }
}
``` 
---
# String Extension
## Text 태그 확장
- Text에 사용되는 태그(Color, Sprite Name)를 확장메서드로 제작
### 사용 예시
```utf8
using UnityEngine;
using UnityEngine.UI;
using Weariness.Util;
using TMPro;

public class TextExtensionTest : MonoBehaviour
{
    public TMP_Text t;

    private void Start()
    {
        // Color 태그 사용 예시
        t.text = "This is a <color=red>red</color> text.";
        
        // Sprite Name 태그 사용 예시
        t.text += " This is a <sprite name=\"MySprite\"/> sprite.";
        
        // 확장 메서드 사용
        t.text = "blue Color".ApplyColorTag("blue", Color.blue);
        t.text = "My Sprite : " + "".ApplySpriteNameTag("MySprite");
    }
}
```
---
### Example

```csharp 
using UnityEngine;

public TestSingleton : Singleton<TestSinglton>
{
    public override void Awake()
    {
        // 이러면 파괴 불가 객체로 생성하지 않고 생성된 씬에서 종속되게 생성됨
        IsDontDestroy = false;
        base.Awake();
    }
    
    public void DoSomething()
    {
        Debug.Log("Singleton instance is doing something!");
    }
}

// other.cs
using UnityEngine;

public class Other : MonoBehaviour
{
    private void Start()
    {
        TestSingleton.Instance.DoSomething();
    }
}
```

### Example
```csharp
// 0~10의 랜덤값 출력
// 기본 최소값은 0
int a = 10;
Debug.Log(a.Random());

// 현재 값을 확률이라고 보고 해당 확률에 당첨되었는지에 대한 bool 반환
float chance = 0.5f; // 50% 확률
bool isLucky = chance.IsLucky();
Debug.Log(isLucky);
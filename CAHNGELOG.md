# [1.0.12]
## 2025-09-18
### 추가 사항
- Stat자체를 Modifier로 사용가능하도록 하는 Stat.AsModifier(StatModifier.ModifierType modifierType) 함수를 구현
- TransformExtension을 제작, 2d 환경에서는 바라보는 방향으로 인한 회전이 z축에서만 일어나야하는 것에 대한 LookAt2D 함수를 구현
- PoolingSystem 구현
- DataPrefs 구현
- ColorExtension 구현

### 수정 사항
- Stat 클래스에 BaseValue 속성 추가
- Stat 클래스에 CachedValue를 추가하여 Value 속성 호출시 마다. 비용이 큰 GetValue 메서드 호출을 방지
- Stat 클래스의 Value 속성 접근자 수정
- Stat, StatModifier의 Inspector 표시를 좀더 보기 편하게 수정
- UI Scaler가 Anchor의 X,Y가 둘다 Stretch일때(확장형일때)에도 동작하도록 구현

### 버그 수정
- Stat, StatModifier의 Dispose를 호출할때 의존성에 의해 nullRerfernce 뜨는 문제점 수정

### 추가적인 전달 사항
- UI Scaler와 UI Safe Area는 같은 오브젝트 내에서 사용하지 않도록 하기
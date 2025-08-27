# [1.0.11]
## 2025-08-26
### 추가 사항
- Stat자체를 Modifier로 사용가능하도록 하는 Stat.AsModifier(StatModifier.ModifierType modifierType) 함수를 구현

### 수정 사항
- Stat 클래스에 BaseValue 속성 추가
- Stat 클래스에 CachedValue를 추가하여 Value 속성 호출시 마다. 비용이 큰 GetValue 메서드 호출을 방지
- Stat 클래스의 Value 속성 접근자 수정
- Stat, StatModifier의 Inspector 표시를 좀더 보기 편하게 수정

### 버그 수정
- Stat, StatModifier의 Dispose를 호출할때 의존성에 의해 nullRerfernce 뜨는 문제점 수정


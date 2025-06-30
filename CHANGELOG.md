# [1.0.2]
## 2025-06-30

### FMOD Occlsuion Detection 

- Ray 기반 Occlusion이 적용되어 Detecting 되도록 구현
- 가상 Listeners를 사용하여 Occlusion이 적용된 소리 감지
- 가상 Listeners는 사용하지 않으면 먼곳으로 위치 변경
- FMOD Occlusion SO를 사용해서 공유 데이터 관리
- DataPrefs를 의존 패키지로 추가
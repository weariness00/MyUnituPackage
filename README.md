# 설명
- 인식/지각/이해를 뜻하는 철학 용어로 인식에 관한 라이브러리

# Field Of View
## Detecting
- 시야로 Target을 감지하는 아키텍쳐
- Mesh 기반 Ray 탐지 구현
- Camera 기반 View 탐지 구현

### 필수적으로 해야할 것
- Field Of View/Detecting/Setting/Noesis Field Of View Detecting Renderer Data.asset의 Render Pass에 있는 Target의 Filter Layer를 FieldOfViewDetectingTarget으로 변경
- Field Of View/Detecting/Setting/Noesis Field Of View Detecting Target Only Renderer Data.asset의 Render Pass에 있는 Target의 Filter Layer를 FieldOfViewDetectingTarget으로 변경
- Render Pipline에 Noesis Field Of View Detecting Renderer Data, Noesis Field Of View Detecting Target Only Renderer Data 2개 추가

### Detecting Mesh Ray
- Mesh 객체가 있을 경우에만 탐지 하며 Mesh의 정점 데이터를 기반으로 탐지
- Mesh를 가진 객체가 Collider 또한 가지고 있어야 탐지가 가능

### Detecting Camera
- Data 클래스의 Redner Data Index를 Noesis Field Of View Detecting Renderer Data가 Pipline에 설정한 Index와 동일하게 변경
- Data 클래스의 Target Only Redner Data Index를 Noesis Field Of View Detecting Target Only Renderer Data Pipline에 설정한 Index와 동일하게 변경

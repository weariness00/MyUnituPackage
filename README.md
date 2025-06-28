# Description

- 미리 기본적인 데이터를 프로젝트에 포함하고 빌드해야할 경우 사용한다.
- 기본적으로는 PlayerPrefs, EditorPrefs와 같다.
  함수 또한 같다
- Set으로 저장되는 데이터들은 모두 Json으로 저장되며 프로젝트 내부에 저장된다
- 빌드된 게임에서는 Set 관련함수로 Key,Value가 저장되지 않는다.

## Example

```utf-8
using Weariness.Util

DataPrefs.SetInt("Key", 123);
// 에디터에서는 정상적으로 저장
// 빌드에서는 저장이 안된다.
```
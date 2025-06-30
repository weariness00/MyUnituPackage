# 필수 환경 셋팅
- FMOD를 기반으로 동작함으로 필수적으로 FMOD를 프로젝트에 설치해주셔야합니다.
- https://assetstore.unity.com/packages/p/fmod-for-unity-2-02-161631 해당 에셋스토어에서 패키지를 추가해주세요
- 오직 PC 환경에서만 사용 가능합니다.

# 패키지 제거시 필수 사항
- 패키지 제거 전에 FMOD>Occlusion>Remove FMOD Occlusion Package를 실행해야 한다.

# Occlusion Util
- FMOD Occlusion Util은 FMOD Studio에서 Occlusion을 구현하기 위한 유틸리티입니다.

# Listener Occlusion
- https://www.youtube.com/watch?v=wTOHc803_ys&t=382s 를 참고하여 구현
- 단순 Listener 변경일 경우 FMOD Studio Listener대신 FMOD_OcclusionListener를 사용합니다.

# Occlusion Sound Detecting
- 가상 Listener를 사용하여 기본적으로는 매우 먼 위치에 두었지만 x,y,z 가 100000f임으로 이 값에 근접하면 사운드가 mix될 수 있다.
- 감지 범위 내에서 사운드가 발생하면 해당 사운드의 위치를 출력합니다.
- 사운드 범위 + 감지 범위 만큼 탐지가 가능합니다.
- Ray기반 감지 가능

``` utf8
using System;
using UnityEngine;
using Weariness.FMOD.Detecting;

namespace Test.FMOD_Occlusion
{
    public class Detector : MonoBehaviour
    {
        public FMOD_Occlusion_SoundDetectingController soundDetectingController;

        private Transform emitterTransform;

        public void FixedUpdate()
        {
            if (soundDetectingController.GetTryAnyDetecting(out emitterTransform))
            {
                Debug.Log($"사운드 감지 {emitterTransform.name}의 사운드의 위치 [{emitterTransform.position}]");
            }
        }
    }
}
```
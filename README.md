# 필수 환경 셋팅
- FMOD를 기반으로 동작함으로 필수적으로 FMOD를 프로젝트에 설치해주셔야합니다.
- https://assetstore.unity.com/packages/p/fmod-for-unity-2-02-161631 해당 에셋스토어에서 패키지를 추가해주세요
- 오직 PC 환경에서만 사용 가능합니다.

# 패키지 제거시 필수 사항
- 패키지 제거 전에 FMOD>Occlusion>Remove FMOD Occlusion Package를 실행해야 한다.

# Listener Occlusion
- https://www.youtube.com/watch?v=wTOHc803_ys&t=382s 를 참고하여 구현
- 단순 Listener 변경일 경우 FMOD Studio Listener대신 FMOD_OcclusionListener를 사용합니다.

# Occlusion Sound Detecting


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
using System;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    [AddComponentMenu("FMOD Studio/Occlusion/Cube Map Camera")]
    public class FMOD_OcclusionCubeMapCamera : MonoBehaviour
    {
        public Camera cubeMapCamera;
        
        public void Awake()
        {
            // 큐브맵 RenderTexture 생성 (예: 128x128 해상도)
            var cubeRT = new RenderTexture(128, 128, 16);
            cubeRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            cubeRT.hideFlags = HideFlags.DontSave;
            cubeRT.Create();
            
            // 카메라 세팅
            cubeMapCamera.clearFlags = CameraClearFlags.SolidColor;
            cubeMapCamera.backgroundColor = Color.black; // 배경 = 완전 투명/무음
            cubeMapCamera.enabled = false; // 수동 렌더링만 사용
        }

        public void RenderOcclusionTexture()
        {
            
        }
    }
}
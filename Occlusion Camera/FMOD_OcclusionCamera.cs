using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FMOD;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Debug = UnityEngine.Debug;

namespace Weariness.FMOD.Occlusion
{
    /// <summary>
    /// Occlusion을 카메라의 View의 R값으로만 판독한다.
    /// 판독값으로는 OcclusionMaksUnlit.mat의 occlusionStrength를 사용한다.
    /// </summary>
    [AddComponentMenu("FMOD Studio/Occlusion/Occlusion Camera")]
    public class FMOD_OcclusionCamera : MonoBehaviour
    {
        public FMOD_OcclusionCameraData data = new();

        public void Reset()
        {
            data.occlusionCamera = GetComponent<Camera>();
            if (data.occlusionCamera != null)
            {
                data.occlusionCamera.orthographic = false;
                // data.occlusionCamera.orthographicSize = 10;
                data.occlusionCamera.fieldOfView = 150f;
                data.occlusionCamera.farClipPlane = 1000f;
                data.occlusionCamera.clearFlags = CameraClearFlags.SolidColor;
                data.occlusionCamera.backgroundColor = Color.black; // 배경 = 완전 투명/무음
            }
        }

        public void Awake()
        {
            RenderTextureFormat targetFormat = RenderTextureFormat.ARGB32;
            // RenderTexture 생성 (예: 128x128 해상도)
            data.occlusionTexture = new RenderTexture(256, 256, 16, targetFormat)
            {
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.DontSave,
            };
            data.occlusionTexture.Create();
            
            // 카메라 
            data.occlusionCamera.targetTexture = data.occlusionTexture;
            data.occlusionCamera.enabled = false; // 수동 렌더링만 사용
            
            var camData = data.occlusionCamera.GetComponent<UniversalAdditionalCameraData>();
            camData.renderType = CameraRenderType.Base;
        }

        public void SetFieldOfView(float fov)
        {
            data.occlusionCamera.fieldOfView = fov;
        }
    
        public RenderTexture RenderOcclusionTexture()
        {
            if (FMOD_OcclusionSO.IsLoad == false) return data.occlusionTexture;
            
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.EnableKeyword("_Occlusion_Enable");
            data.occlusionCamera.Render();
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.DisableKeyword("_Occlusion_Enable");
            return data.occlusionTexture;
        }

        public float OcclusionValueSampling(RenderTexture texture)
        {
            if (FMOD_OcclusionSO.IsLoad == false) return 0;
            
            var textureSize = texture.width * texture.height;
            var buffer = new ComputeBuffer(textureSize, sizeof(float) * 4);
            ComputeShader cs = FMOD_OcclusionSO.Instance.occlusionTextureSampling;

            { // Texture color값 가져오기
                int kernel = cs.FindKernel("CSMain_RGBA");
                cs.SetTexture(kernel, "_SourceTex", texture);
                cs.SetBuffer(kernel, "_SumBufferRGBA", buffer);
                cs.SetInt("_WidthRGBA", texture.width);
                cs.SetInt("_HeightRGBA", texture.height);
                cs.Dispatch(kernel, texture.width/16, texture.height/16, 1);
            }

            { // Sound의 거리에 대한 정보로 Distance Sampling
                int kernel = cs.FindKernel("CSMain_Distance_Sampling");
                cs.SetTexture(kernel, "_SourceTex", texture);
                cs.SetBuffer(kernel, "_SumBufferRGBA", buffer);
                cs.SetInt("_WidthRGBA", texture.width);
                cs.SetInt("_HeightRGBA", texture.height);
                cs.Dispatch(kernel, texture.width/16, texture.height/16, 1);
            }

            var values = new Vector4[textureSize];
            buffer.GetData(values);
            float sum = 0f;
            for (int i = 0; i < values.Length; ++i)
                sum += Mathf.Clamp(values[i].x + values[i].y, 0, 1);
            float avg = sum / values.Length;

            return avg < 0f ? 0f : avg; // 음향 차단 강도는 0 이상
        }

        public float GetOcclusionValue(StudioEventEmitter emitter)
        {
            // 3D 음향이 아니면 검사 안함
            emitter.EventDescription.is3D(out var is3D);
            if(!is3D) return 0;

            data.occlusionCamera.transform.LookAt(emitter.transform);
            data.occlusionCamera.farClipPlane = Vector3.Distance(transform.position, emitter.transform.position);
            
            // emitter의 위치를 머테리얼에 전달
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.SetVector("_Sound_Position", emitter.transform.position);
            emitter.EventInstance.getMinMaxDistance(out var min, out var max);
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.SetFloat("_Sound_Disatnce", max);

            var renderTexture = RenderOcclusionTexture();
            float occlusionValue = OcclusionValueSampling(renderTexture);
            return occlusionValue;
        }
    }
}
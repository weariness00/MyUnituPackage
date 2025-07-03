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
        [SerializeField] private Camera occlusionCamera;

        public RenderTexture OcclusionTexture { get; private set; }
        public void Awake()
        {
            RenderTextureFormat targetFormat = RenderTextureFormat.ARGB32;
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
                targetFormat = RenderTextureFormat.ARGB32; // 혹은 필요시 RFloat 등
            // RenderTexture 생성 (예: 128x128 해상도)
            OcclusionTexture = new RenderTexture(256, 256, 16, targetFormat)
            {
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.DontSave,
            };
            OcclusionTexture.Create();
            
            // 카메라 
            occlusionCamera.targetTexture = OcclusionTexture;
            occlusionCamera.farClipPlane = 1000f;
            occlusionCamera.clearFlags = CameraClearFlags.SolidColor;
            occlusionCamera.backgroundColor = Color.black; // 배경 = 완전 투명/무음
            occlusionCamera.enabled = false; // 수동 렌더링만 사용
            occlusionCamera.fieldOfView = 90f;
            
            var camData = occlusionCamera.GetComponent<UniversalAdditionalCameraData>();
            camData.renderType = CameraRenderType.Base;
        }

        public void SetFieldOfView(float fov)
        {
            occlusionCamera.fieldOfView = fov;
        }
    
        public RenderTexture RenderOcclusionTexture()
        {
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.EnableKeyword("_Occlusion_Enable");
            occlusionCamera.Render();
            FMOD_OcclusionSO.Instance.occlusionMaterialInstance.DisableKeyword("_Occlusion_Enable");
            return OcclusionTexture;
        }

        public float OcclusionValueSampling(RenderTexture texture, float volume)
        {
            var textureSize = texture.width * texture.height;
            var buffer = new ComputeBuffer(textureSize, sizeof(float) * 2);
            ComputeShader cs = FMOD_OcclusionSO.Instance.occlusionTextureSampling;

            if (texture.format == RenderTextureFormat.R8)
            {
                int kernel = cs.FindKernel("CSMain_R");
                cs.SetTexture(kernel, "_SourceTex", texture);
                cs.SetBuffer(kernel, "_SumBufferR", buffer);
                cs.SetInt("_WidthR", texture.width);
                cs.SetInt("_HeightR", texture.height);
                cs.Dispatch(kernel, texture.width/16, texture.height/16, 1);
            }
            else
            {
                int kernel = cs.FindKernel("CSMain_RGBA");
                cs.SetTexture(kernel, "_SourceTex", texture);
                cs.SetBuffer(kernel, "_SumBufferRGBA", buffer);
                cs.SetInt("_WidthRGBA", texture.width);
                cs.SetInt("_HeightRGBA", texture.height);
                cs.Dispatch(kernel, texture.width/16, texture.height/16, 1);
            }

            var values = new Vector2[textureSize];
            buffer.GetData(values);
            float sum = 0f;
            for (int i = 0; i < values.Length; ++i) {
                sum += values[i].x; // R값만 사용
                sum -= values[i].y; // G값을 빼서 차폐 강도 계산
            }
            sum -= volume * values.Length;
            float avg = sum / values.Length;

            return avg < 0f ? 0f : avg; // 음향 차단 강도는 0 이상
        }

        public float GetOcclusionValue(StudioEventEmitter emitter)
        {
            // 3D 음향이 아니면 검사 안함
            emitter.EventDescription.is3D(out var is3D);
            if(!is3D) return 0;

            occlusionCamera.transform.LookAt(emitter.transform);
            occlusionCamera.farClipPlane = Vector3.Distance(transform.position, emitter.transform.position);

            emitter.EventInstance.getVolume(out var volume);

            var renderTexture = RenderOcclusionTexture();
            float occlusionValue = OcclusionValueSampling(renderTexture, volume);
            return occlusionValue;
        }
    }
}
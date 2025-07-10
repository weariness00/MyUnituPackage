using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Jobs;
namespace Weariness.Noesis.FieldOfView
{
    public class FieldOfViewDetectingCamera : MonoBehaviour
    {
        public static readonly string LayerName = "Field Of View Detecting Target";
        public FieldOfViewDetectingCameraData data = new();

        public void Awake()
        {
            if (data.eyes == null)
                data.eyes = transform;
            data.Awake();
        }

        /// <summary>
        /// 타겟이 카메라에 얼마나 비추는지 반환
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="isChangedAllMeshLayer"> targetObject의 모든 Mesh Layer을 바꿀 것인지 안바꾼다면 이미 레이어가 지정되어있다는 것</param>
        /// <returns>0~1 사이값으로 반환하며 1에 가까울수록 잘 보이는 것</returns>
        public float DetectingTargetThresHold(GameObject targetObject, bool isChangedAllMeshLayer = true)
        {
            float Sampling()
            {
                float value1, value2;
                var camData = data.camera.GetUniversalAdditionalCameraData();
                camData.SetRenderer(data.renderDataIndex);
                data.camera.Render();
                
                var sum = new NativeArray<float>(1, Allocator.TempJob);

                ComputeBuffer DisapatchComputeShader(ComputeShader cs, RenderTexture texture)
                {
                    var textureSize = texture.width * texture.height;
                    var buffer = new ComputeBuffer(textureSize, sizeof(float) * 4);

                    int kernel = cs.FindKernel("CSMain");
                    cs.SetTexture(kernel, "_SourceTex", texture);
                    cs.SetBuffer(kernel, "_OutputTextureBuffer", buffer);
                    cs.SetInt("_Width", texture.width);
                    cs.SetInt("_Height", texture.height);
                    cs.Dispatch(kernel, texture.width / 16, texture.height / 16, 1);

                    return buffer;
                }

                var textureSize = data.renderTexture.width * data.renderTexture.height;
                var buffer = DisapatchComputeShader(FieldOfViewSO.Instance.detectingTextureSampling, data.renderTexture);
                var values = new Vector4[textureSize];
                buffer.GetData(values);

                var nativeValues = new NativeArray<Vector4>(values, Allocator.TempJob);

                var calJob = new DetectingTextureCalcurateJob()
                {
                    textureColorValues = nativeValues,
                    sum = sum
                };

                calJob.Schedule().Complete();
                nativeValues.Dispose();
                value1 = sum[0] / textureSize;

                // Target만 Render
                camData.SetRenderer(data.targetOnlyRenderDataIndex);
                data.camera.Render();

                // 다시 컴퓨트 쉐이더 targetonly로 실행
                buffer = DisapatchComputeShader(FieldOfViewSO.Instance.detectingTextureSampling, data.renderTexture);
                buffer.GetData(values);
                
                nativeValues = new NativeArray<Vector4>(values, Allocator.TempJob);
                sum[0] = 0;
                calJob.textureColorValues = nativeValues;
                calJob.sum = sum;

                calJob.Schedule().Complete();
                nativeValues.Dispose();
                value2 = sum[0] / textureSize;

                sum.Dispose();
                // TargetOnly와 아닌것 두개의 값을 나눈뒤 Threshold 보다 크면 탐지
                if (value1 == 0 || value2 == 0) return 0f;
                return value1 / value2;
            }

            if (isChangedAllMeshLayer)
            {
                var layer = LayerMask.NameToLayer(LayerName);
                var allMeshs = targetObject.GetComponentsInChildren<MeshFilter>();
                var prevLayers = new LayerMask[allMeshs.Length];
                for (int i = 0; i < allMeshs.Length; i++)
                {
                    prevLayers[i] = allMeshs[i].gameObject.layer;
                    allMeshs[i].gameObject.layer = layer;
                }

                var value = Sampling();

                for (int i = 0; i < allMeshs.Length; i++)
                {
                    allMeshs[i].gameObject.layer = prevLayers[i];
                }

                return value;
            }
            return Sampling();
        }
    }
}
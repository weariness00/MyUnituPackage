using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Weariness.Noesis.FieldOfView
{
    [CreateAssetMenu(fileName = "Noesis Field Of View Data", menuName = "Noesis/Field Of View")]
    public class FieldOfViewSO : ScriptableObject
    {
        public static FieldOfViewSO Instance => SettingProviderHelper.setting;

        public ScriptableRendererFeature detectingOtherRenderFeature; // 나중에 Render pass로 제어할 수 있게 하기
        public ComputeShader detectingTextureSampling;
    }
}

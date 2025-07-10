using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Weariness.Noesis.FieldOfView
{
    [CreateAssetMenu(fileName = "Noesis Field Of View Data", menuName = "Noesis/Field Of View")]
    public class FieldOfViewSO : ScriptableObject
    {
        public static FieldOfViewSO Instance => SettingProviderHelper.setting;

        public ScriptableRendererFeature detectingOtherRenderFeature; // ���߿� Render pass�� ������ �� �ְ� �ϱ�
        public ComputeShader detectingTextureSampling;
    }
}

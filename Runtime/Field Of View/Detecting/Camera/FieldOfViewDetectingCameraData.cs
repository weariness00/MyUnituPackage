using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Weariness.Noesis.FieldOfView
{
    [Serializable]
    public class FieldOfViewDetectingCameraData
    {
        [Tooltip("직접 Render Data Index를 지정해주어야함")] public int renderDataIndex = -1;
        [Tooltip("직접 Target Only Render Data Index를 지정해주어야함")] public int targetOnlyRenderDataIndex = -1;
        [Space]
        public Transform eyes;

        [NonSerialized] public Camera camera;
        [NonSerialized] public RenderTexture renderTexture;

        public void Awake()
        {
            CreateCamera();
            CreateCameraRenderTexture();
        }

        private void CreateCamera()
        {
            var obj = new GameObject("Field Of View Detecting Camera");
            obj.transform.SetParent(eyes, false);
            camera = obj.AddComponent<Camera>();
            camera.enabled = false; // 수동 렌더링만 함

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            var camData = camera.GetUniversalAdditionalCameraData();
            camData.renderType = CameraRenderType.Base;
            camData.renderShadows = false;
            camData.volumeLayerMask = 0;
        }

        public void CreateCameraRenderTexture()
        {
            int size = 256;
            renderTexture = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32)
            {
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.DontSave,
            };
            renderTexture.Create();

            camera.targetTexture = renderTexture;
        }
    }
}
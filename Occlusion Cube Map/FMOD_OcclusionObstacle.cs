using System;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    public partial class FMOD_OcclusionObstacle
    {
        private static Material SharedOcclusionMat;
        private static bool isInit = false;

        private static void Init()
        {
            if (SharedOcclusionMat == null)
            {
                // "OcclusionMaskUnlit"은 .mat 파일명 (확장자 제외)
                SharedOcclusionMat = Resources.Load<Material>("OcclusionMaskUnlit");
                if (SharedOcclusionMat == null)
                    Debug.LogError("OcclusionMaskUnlit 머티리얼을 Resources 폴더에 넣으세요!");
            }
        }
    }
    
    public partial class FMOD_OcclusionObstacle : MonoBehaviour
    {
        private static int OcclusionID = Shader.PropertyToID("_Occlusion");
        
        [Range(0,1)] public float occlusionStrength = 1f; // 장애물의 음향 차단 강도
        
        private Renderer renderer;
        private MaterialPropertyBlock mpb;

#if UNITY_EDITOR
        [NonSerialized] private float prevOcclusionStrength = 1f; // 이전 occlusionStrength 값, 에디터에서만 사용
#endif

        public void OnValidate()
        {
            if (EditorApplication.isPlaying)
            {
                if (Math.Abs(occlusionStrength - prevOcclusionStrength) > 0.001f)
                {
                    SetOcclusion(occlusionStrength);
                }
            }
        }

        void Awake()
        {
#if UNITY_EDITOR
            prevOcclusionStrength = occlusionStrength; // 이전 occlusionStrength 값, 에디터에서만 사용
#endif
            renderer = GetComponent<Renderer>();
            Debug.Assert(renderer != null, "Renderer가 존재 하지 않아 OcclusionObstacle이 동작하지 않습니다.");
            
            mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(OcclusionID, occlusionStrength);
            renderer.SetPropertyBlock(mpb);
        }

        // 값이 바뀔 때마다 갱신하고 싶으면 함수로 따로 빼서 호출
        public void SetOcclusion(float value)
        {
#if true
            prevOcclusionStrength = value;    
#endif
            
            occlusionStrength = value;
            mpb.SetFloat(OcclusionID, value);
            renderer.SetPropertyBlock(mpb);
        }
    }
}
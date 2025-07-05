using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    [AddComponentMenu("FMOD/Occlusion/Occlusion Obstacle")]
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

            // MaterialPropertyBlock을 사용하여 occlusionStrength 설정
            mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(OcclusionID, occlusionStrength);
            renderer.SetPropertyBlock(mpb);
            
            StartCoroutine(InitEnumerator());
        }

        private IEnumerator InitEnumerator()
        {
            yield return new WaitUntil(() => FMOD_OcclusionSO.IsLoad);
            
            // FMOD_OcclusionSO에서 occlusionMaterialInstance를 가져와서 공유 재질로 설정
            List<Material> sharedMaterials = new();
            renderer.GetSharedMaterials(sharedMaterials);
            sharedMaterials.Add(FMOD_OcclusionSO.Instance.occlusionMaterialInstance);
            renderer.SetSharedMaterials(sharedMaterials);
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
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD.Occlusion
{
    /// <summary>
    /// Occlusion Camera를 사용하면 Sound를 내는 객체에 부착해줘야하는 컴포넌트
    /// </summary>
    [AddComponentMenu("FMOD Studio/Occlusion/Occlusion Sound")]
    public class FMOD_OcclusionSound : MonoBehaviour
    {
        private static int OcclusionIsSoundID = Shader.PropertyToID("_Is_Sound");
        private static int OcclusionStrengthID = Shader.PropertyToID("_Occlusion");

        public bool isSound = true;

#if UNITY_EDITOR
        private bool prevIsSound = true; // 이전 isSound 값, 에디터에서만 사용
#endif
        
        private Renderer renderer;
        private MaterialPropertyBlock mpb;

        public void OnValidate()
        {
            if (EditorApplication.isPlaying)
            {
                if (prevIsSound != isSound)
                {
                    SetEnable(isSound);
                }
            }
        }

        void Awake()
        {
#if UNITY_EDITOR
            prevIsSound = isSound; // 이전 occlusionStrength 값, 에디터에서만 사용
#endif
            renderer = GetComponent<Renderer>();
            Debug.Assert(renderer != null, "Renderer가 존재 하지 않아 OcclusionObstacle이 동작하지 않습니다.");

            // FMOD_OcclusionSO에서 occlusionMaterialInstance를 가져와서 공유 재질로 설정
            List<Material> sharedMaterials = new();
            renderer.GetSharedMaterials(sharedMaterials);
            sharedMaterials.Add(FMOD_OcclusionSO.Instance.occlusionMaterialInstance);
            renderer.SetSharedMaterials(sharedMaterials);
            
            // MaterialPropertyBlock을 사용하여 occlusionStrength 설정
            mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(OcclusionIsSoundID, isSound ? 1f : 0f);
            mpb.SetFloat(OcclusionStrengthID, 0f);
            renderer.SetPropertyBlock(mpb);
        }

        // 값이 바뀔 때마다 갱신하고 싶으면 함수로 따로 빼서 호출
        public void SetEnable(bool value)
        {
#if true
            prevIsSound = value;
#endif
            isSound = value;
            mpb.SetFloat(OcclusionIsSoundID, isSound ? 1f : 0f);
            renderer.SetPropertyBlock(mpb);
        }
    }
}
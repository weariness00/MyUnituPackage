using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Weariness.Util
{
    public static class PoolSystem
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void EditorInitPool()
        {
            foreach (var (key, target) in pools)
            {
                target.pool.Clear();
            }
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void PoolClearSubscribe()
        {
            // 한 번만 등록 → 이후 씬이 로드될 때마다 호출됨
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if(mode == LoadSceneMode.Additive) return;
                foreach (var (key, target) in pools)
                {
                    target.pool.Clear();
                }
            };
        }
        
        private static readonly Dictionary<GameObject, GameObjectPooling> pools = new();

        public static T Get<T>(T target) where T : Object
        {
            var go = target.GameObject();
            if (go != null)
            {
                if (!pools.TryGetValue(go, out var pooling))
                {
                    pooling = new(go);
                    pools.Add(go, pooling);
                }

                var obj = pooling.pool.Get();
                
                return obj.GetComponent<T>();
            }

            return null;
        }

        public static void Release<T>(T obj) where T : Object
        {
            if(obj == null) return;
            var go = obj.GameObject();
            if (go.TryGetComponent(out GameObjectPoolIdentifier identifier))
            {
                identifier.target.pool.Release(go);
                return;
            }
            
            Debug.LogError("Pool이 존재하지 않습니다.");
        }
    }
}
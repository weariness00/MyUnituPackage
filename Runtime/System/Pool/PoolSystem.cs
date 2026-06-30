using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Weariness.Util
{
    public static class PoolSystem
    {
#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        static void EditorInitPool()
        {
            ClearAll();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void PoolClearSubscribe()
        {
            // 한 번만 등록 → 이후 씬이 로드될 때마다 호출됨
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if(mode == LoadSceneMode.Additive) return;
                ClearAll();
            };
        }

        private static readonly Dictionary<GameObject, GameObjectPooling> pools = new();

        // Addressable로 로드한 프리팹 캐시(AssetGUID → 로드된 프리팹). 씬 정리 시 핸들을 함께 해제한다.
        private static readonly Dictionary<string, GameObject> assetRefCache = new();

        // 진행 중인 비동기 로드(AssetGUID → 로드 태스크). 동일 GUID 동시 호출 시 중복 로드를 막는다.
        private static readonly Dictionary<string, UniTask<GameObject>> pendingLoads = new();
        
        public static GameObject Get(GameObject target)
        {
            if (target != null)
            {
                if (!pools.TryGetValue(target, out var pooling))
                {
                    pooling = new(target);
                    pools.Add(target, pooling);
                }

                return pooling.pool.Get();
            }

            return null;
        }
        
        /// <summary>
        /// AssetReferenceGameObject가 가리키는 프리팹을 동기 로드(WaitForCompletion)한 뒤 풀에서 인스턴스를 반환한다.
        /// 로드된 프리팹은 AssetGUID로 캐시되어 재호출 시 재로드하지 않으며, 씬 정리 시 핸들이 해제된다.
        /// </summary>
        public static GameObject Get(AssetReferenceGameObject reference)
        {
            var prefab = ResolvePrefab(reference);
            return prefab != null ? Get(prefab) : null;
        }

        /// <summary>
        /// <see cref="Get(AssetReferenceGameObject)"/>의 비동기 버전. 프리팹을 비동기 로드한 뒤 풀에서 인스턴스를 반환한다.
        /// 동일 GUID에 대한 동시 호출은 하나의 로드만 수행한다.
        /// </summary>
        public static async UniTask<GameObject> GetAsync(AssetReferenceGameObject reference)
        {
            var prefab = await ResolvePrefabAsync(reference);
            return prefab != null ? Get(prefab) : null;
        }

        private static GameObject ResolvePrefab(AssetReferenceGameObject reference)
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
                return null;

            var guid = reference.AssetGUID;
            if (assetRefCache.TryGetValue(guid, out var cached) && cached != null)
                return cached;

            var handle = Addressables.LoadAssetAsync<GameObject>(reference.RuntimeKey);
            var prefab = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null)
            {
                Debug.LogError($"PoolSystem: AssetReferenceGameObject 로드 실패 ({guid})");
                return null;
            }

            assetRefCache[guid] = prefab;
            return prefab;
        }

        private static UniTask<GameObject> ResolvePrefabAsync(AssetReferenceGameObject reference)
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
                return UniTask.FromResult<GameObject>(null);

            var guid = reference.AssetGUID;
            if (assetRefCache.TryGetValue(guid, out var cached) && cached != null)
                return UniTask.FromResult(cached);

            // 이미 진행 중인 로드면 그 태스크를 그대로 반환(중복 로드 방지). Preserve로 다중 await 허용.
            if (pendingLoads.TryGetValue(guid, out var pending))
                return pending;

            var task = LoadPrefabAsync(reference, guid).Preserve();
            pendingLoads[guid] = task;
            return task;
        }

        private static async UniTask<GameObject> LoadPrefabAsync(AssetReferenceGameObject reference, string guid)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(reference.RuntimeKey);
                await handle;
                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    Debug.LogError($"PoolSystem: AssetReferenceGameObject 비동기 로드 실패 ({guid})");
                    return null;
                }

                assetRefCache[guid] = handle.Result;
                return handle.Result;
            }
            finally
            {
                pendingLoads.Remove(guid);
            }
        }

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

        // 모든 풀 인스턴스를 비우고, Addressable로 로드한 프리팹은 풀 엔트리 제거 + 핸들 해제까지 수행한다.
        private static void ClearAll()
        {
            foreach (var (_, target) in pools)
                target.pool.Clear();

            foreach (var prefab in assetRefCache.Values)
            {
                if (prefab == null) continue;
                pools.Remove(prefab);
                Addressables.Release(prefab);
            }
            assetRefCache.Clear();
            pendingLoads.Clear();
        }
    }
}
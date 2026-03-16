using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Util
{
    public class ObjectPoolPrefabIdentifier : MonoBehaviour
    {
        [HideInInspector] public GameObject prefab;
    }
    
    public class ObjectPoolSpawner : ObjectPoolSpawner<GameObject>
    {
        public UnityEvent<GameObject> onGetObject;
        public UnityEvent<GameObject> onReleaseObject;
        public UnityEvent<GameObject> onDestroyObject;
        public override void OnGetObject(GameObject obj)
        {
            obj.SetActive(true);
            onGetObject?.Invoke(obj);
        }

        public override void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
            onReleaseObject?.Invoke(obj);
        }

        public override void OnDestroyObject(GameObject obj)
        {
            Destroy(obj);
            onDestroyObject?.Invoke(obj);
        }
    }
    
    public partial class ObjectPoolSpawner<TGameObject> : ObjectSpawner<TGameObject> where TGameObject : Object
    {
        public override void Spawn()
        {
            NextObject();
            NextPlace();
            NextInterval();
            
            var obj = PoolInstantiate();
            
            if (spawnPlaceType == SpawnPlaceType.Transform && isSameLayer && obj is GameObject go) go.layer = spawnPlaceList[_spawnPlaceCount].gameObject.layer;

            onSpawnSuccessAction.Invoke(obj);
        }
    }

    public abstract partial class ObjectPoolSpawner<TGameObject>
    {
        private Dictionary<GameObject, ObjectPool<TGameObject>> poolDictionary = new(); // 오브젝트 pool

        private TGameObject PoolInstantiate()
        {
            if (!poolDictionary.TryGetValue(_currentSpawnObject.GameObject(),out var pool))
            {
                pool = new(
                    OnCreateObject,
                    obj =>
                    {
                        spawnCount.Current++;
                        totalSpawnCount++;
                        OnGetObject(obj);
                    },
                    obj =>
                    {
                        spawnCount.Current--;
                        OnReleaseObject(obj);
                    },
                    OnDestroyObject,
                    false
                );
                
                poolDictionary.Add(_currentSpawnObject.GameObject(), pool);
            }

            var obj = pool.Get();
            if (obj is GameObject go)
            {
                go.transform.position = currentPosition;
                go.transform.rotation = currentRotate;
            }
            return obj;
        }

        public void Release(TGameObject obj)
        {
            var identifier = obj.GetComponent<ObjectPoolPrefabIdentifier>();
            if (identifier != null &&
                poolDictionary.TryGetValue(identifier.prefab, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                Debug.LogError("Spawner Pool의 Object가 아닙니다.");
            }
        }
        
        public virtual TGameObject OnCreateObject()
        {
            var obj = Instantiate(_currentSpawnObject, currentPosition, currentRotate, parentTransform);
            var identifier = obj.AddComponent<ObjectPoolPrefabIdentifier>();
            identifier.prefab = _currentSpawnObject.GameObject();
            return obj;
        }
        public abstract void OnGetObject(TGameObject obj);
        public abstract void OnReleaseObject(TGameObject obj);
        public abstract void OnDestroyObject(TGameObject obj);
    }

}


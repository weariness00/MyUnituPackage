using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Weariness.Util
{
    public class GameObjectPoolIdentifier : Identifier<GameObjectPooling>
    {
    }
    
    public class GameObjectPooling : Pooling<GameObject>
    {
        public GameObjectPooling(GameObject _prefab) : base(_prefab)
        {
        }
        
        protected override GameObject OnCreate()
        {
            var obj = base.OnCreate();
            var identifier = obj.GetOrAddComponent<GameObjectPoolIdentifier>();
            identifier.target = this;
            return obj;
        }
    }
    
    public class Pooling<T> where T : Object
    {
        public ObjectPool<T> pool;
        private T prefab;
        
        public Pooling(T _prefab)
        {
            prefab = _prefab;
            pool = new(
                OnCreate,
                OnGet,
                OnRelease,
                OnDestroy,
                true,
                5
            );
        }
        
        protected virtual T OnCreate()
        {
            var obj = Object.Instantiate(prefab);
            return obj;
        }

        protected virtual void OnGet(T obj)
        {
            obj.GameObject().SetActive(true);
        }
        
        protected virtual void OnRelease(T obj)
        {
            obj.GameObject().SetActive(false);
        }
        
        protected virtual void OnDestroy(T obj)
        {
            if(obj != null)
                Object.Destroy(obj.GameObject());
        }
    }
}
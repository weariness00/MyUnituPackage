using UnityEngine;
using Unity.VisualScripting;

namespace Weariness.Util
{
    public abstract class Singleton<T> : MonoBehaviour, ISingleton where T : Component, new()
    {
        public static T Instance
        {
            get
            {
                Init();
                return _instance;
            }
        }
        private static T _instance = null;
        public static bool IsDontDestroy = true;
        public static bool HasInstance => _instance != null;
        public static void Make() => Init();


        public virtual void Awake()
        {
            Init();
            if (_instance != this)
                Destroy(gameObject);
        }

        private static void Init()
        {
            if (_instance == null)
            {
#if UNITY_6000_0_OR_NEWER
                var component = FindAnyObjectByType<T>();
#else
                var component = FindObjectOfType<T>();
#endif
                if (component != null)
                {
                    _instance = component;
                    if(_instance is Singleton<T> s1)
                        s1.Initialize();
                    if(IsDontDestroy) DontDestroyOnLoad(_instance.gameObject);
                    return;
                }

                var singletonObject = new GameObject(typeof(T).Name);
                singletonObject.AddComponent<T>();
            }
        }

        protected virtual void Initialize() {}
    }
}
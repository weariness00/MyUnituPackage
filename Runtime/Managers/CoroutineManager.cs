using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Util.Managers
{
    public class CoroutineHandler : MonoBehaviour{}
    
    public partial class CoroutineManager
    {
        private static readonly string DefaultKey = "Default";
        
        public static void Play(string enumeratorName,IEnumerator enumerator)
        {
            Instance.Add(DefaultKey,enumeratorName, enumerator);
        }
        
        public static void Play(string key,string enumeratorName, IEnumerator enumerator)
        {
            Instance.Add(key,enumeratorName, enumerator);
        }
        
        public static void Stop(string enumeratorName, IEnumerator enumerator)
        {
            Instance.Remove(DefaultKey,enumeratorName, enumerator);
        }
        public static void Stop(string key,string enumeratorName, IEnumerator enumerator)
        {
            Instance.Remove(key,enumeratorName, enumerator);
        }
        
        public static bool HasCoroutine(string enumeratorName,IEnumerator enumerator)
        {
            return Instance.Contain(DefaultKey,enumeratorName, enumerator);
        }

    }
    public partial class CoroutineManager : Singleton<CoroutineManager>
    {
        private Dictionary<string, CoroutineHandler> handlerDict = new Dictionary<string, CoroutineHandler>();
        private Dictionary<string, int> runningRefDict = new Dictionary<string, int>();
        private Dictionary<string, List<Coroutine>> coroutineDict = new();
        
        private void Add(string key, string enumeratorName, IEnumerator enumerator)
        {
            if (!handlerDict.TryGetValue(key, out var handler))
            { 
                var obj = new GameObject($"Coroutine_{key}");
                handler = obj.AddComponent<CoroutineHandler>();
                handlerDict[key] = handler;
            }

            if (!coroutineDict.TryGetValue(enumeratorName, out var coroutines))
            {
                coroutines = new();
                coroutineDict[enumeratorName] = coroutines;
            }
            coroutines.Add(handler.StartCoroutine(Enumerator(key,enumeratorName, enumerator)));
        }
        
        private void Remove(string key, string enumeratorName, IEnumerator enumerator)
        {
            if (handlerDict.TryGetValue(key, out var handler))
            { 
                if (coroutineDict.TryGetValue(enumeratorName, out var coroutines))
                {
                    foreach (var coroutine in coroutines)
                        handler.StopCoroutine(coroutine);
                    coroutineDict.Remove(enumeratorName);
                }
            }
        }
        

        private bool Contain(string key, string enumeratorName, IEnumerator enumerator)
        {
            return coroutineDict.ContainsKey(enumeratorName);
        }
        
        private IEnumerator Enumerator(string key, string enumeratorName, IEnumerator enumerator)
        {
            if(!runningRefDict.TryGetValue(key, out var index))
                runningRefDict[key] = 0;

            var coroutines = coroutineDict[enumeratorName];
            var coroutineIndex = coroutines.Count;
            
            runningRefDict[key]++;

            try
            {
                yield return enumerator;
            }
            finally
            {
                runningRefDict[key]--;

                if (runningRefDict.TryGetValue(key, out index))
                {
                    if (index <= 0 && handlerDict.TryGetValue(key, out var handler))
                    {
                        handlerDict.Remove(key);
                        runningRefDict.Remove(key);
                        Destroy(handler.gameObject);
                    }
                }

                if (coroutines.Count > coroutineIndex)
                    coroutines.RemoveAt(coroutineIndex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weariness.Util
{
    [Serializable]
    public class UniqueList<T>
    {
        #region Static
        private static readonly Dictionary<Type, object> makerRegistry = new Dictionary<Type, object>();

        static UniqueList()
        {
            makerRegistry[typeof(int)] = new UniqueListMakerInt();
            makerRegistry[typeof(Vector2Int)] = new UniqueListMakerVector2Int();
            makerRegistry[typeof(Vector3Int)] = new UniqueListMakerVector3Int();
        }

        public static void RegisterMaker(IUniqueListMaker<T> maker)
        {
            makerRegistry[typeof(T)] = maker;
        }

        public static IUniqueListMaker<T> GetRegisteredMaker()
        {
            if (makerRegistry.TryGetValue(typeof(T), out var registered))
                return registered as IUniqueListMaker<T>;
            return null;
        }
        #endregion

        private HashSet<T> uniqueSet = new HashSet<T>();
        [SerializeField] private List<T> list = new List<T>();
        
        public int Count => list.Count;

        public void Add(T value)
        {
            if (uniqueSet.Contains(value))
                return;
            uniqueSet.Add(value);
            list.Add(value);
        }

        public void Remove(T value)
        {
            if (!uniqueSet.Contains(value))
                return;
            uniqueSet.Remove(value);
            list.Remove(value);
        }

        public void Clear()
        {
            uniqueSet.Clear();
            list.Clear();
        }

        public T Get()
        {
            if (list.Count == 0)
                throw new InvalidOperationException("UniqueList 컬렉션이 비어있습니다.");

            var index = Random.Range(0, list.Count);
            var value = list[index];

            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            uniqueSet.Remove(value);
            return value;
        }

        public bool TryGet(out T value)
        {
            value = default;
            if (list.Count == 0)
                return false;

            var index = Random.Range(0, list.Count);
            value = list[index];

            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            uniqueSet.Remove(value);
            return true;
        }
    }
}

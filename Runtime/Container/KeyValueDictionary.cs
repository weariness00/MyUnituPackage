
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Util.Container
{
    /// <summary>
    /// 딕셔너리가 직렬화가 안될때 사용하는 대체제
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class KeyValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        public List<TKey> Keys = new();
        public List<TValue> Values = new();

        public int Count => Keys.Count;

        public TValue this[TKey key]
        {
            get
            {
                int index = Keys.IndexOf(key);
                return Values[index];
            }
            set
            {
                int index = Keys.IndexOf(key);
                if (index < 0 || index >= Values.Count)
                {
                    Keys.Add(key);
                    Values.Add(value);
                }
                else
                {
                    Values[index] = value;
                }
            }
        }

        public KeyValueDictionary()
        {
            Keys = new();
            Values = new();
        }

        public KeyValueDictionary(TKey[] keys, TValue[] values)
        {
            if (keys.Length != values.Length)
            {
                Debug.LogError("Keys and Values arrays must have the same length.");
                return;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                Keys.Add(keys[i]);
                Values.Add(values[i]);
            }
        }

        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default)
        {
            int index = Keys.IndexOf(key);
            return index >= 0 && index < Values.Count ? Values[index] : defaultValue;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = Keys.IndexOf(key);
            if (index >= 0 && index < Values.Count)
            {
                value = Values[index];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!Keys.Contains(key))
            {
                Keys.Add(key);
                Values.Add(value);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (!Keys.Contains(key))
            {
                Keys.Add(key);
                Values.Add(value);
                return true;
            }

            return false;
        }

        public void Remove(TKey key)
        {
            int index = Keys.IndexOf(key);
            if (index < 0 || index >= Values.Count)
            {
                Debug.LogError($"Key {key} not found in Keys list.");
                return;
            }

            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public void Remove(TValue value)
        {
            int index = Values.IndexOf(value);
            if (index < 0 || index >= Keys.Count)
            {
                Debug.LogError($"Value {value} not found in Values list.");
                return;
            }

            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < Keys.Count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
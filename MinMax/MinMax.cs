using UnityEngine;

namespace Util
{
    [System.Serializable]
    public class MinMax<T>
    {
        [SerializeField] private T _min;
        [SerializeField] private T _max;
        
        public T Min
        {
            get => _min;
            set => _min = value;
        }

        public T Max
        {
            get => _max;
            set => _max = value;
        }
        
        public MinMax(T min, T max)
        {
            _min = min;
            _max = max;
        }
    }
    
    public static class MinMaxIntExtension
    {
        public static int Length(this MinMax<int> value)
        {
            return Mathf.Abs(value.Min) + Mathf.Abs(value.Max);
        }

        public static int Random(this MinMax<int> value, bool includeMax =false)
        {
            return UnityEngine.Random.Range(value.Min, value.Max + (includeMax ? 1 : 0));
        }
    }

    public static class MinMaxFloatExtension
    {
        public static int Length(this MinMax<float> value)
        {
            return (int)(Mathf.Abs(value.Min) + Mathf.Abs(value.Max));
        }
    }
}
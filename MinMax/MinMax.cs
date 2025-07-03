    using UnityEngine;

    namespace Weariness.Util
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
        
        // int
        public static partial class MinMaxExtension
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
        
        // float
        public static partial class MinMaxExtension
        {
            public static int Length(this MinMax<float> value)
            {
                return (int)(Mathf.Abs(value.Min) + Mathf.Abs(value.Max));
            }

            public static float Random(this MinMax<float> value)
            {
                return UnityEngine.Random.Range(value.Min, value.Max);
            }
        }
        
        // Vector2
        public static partial class MinMaxExtension
        {
            public static Vector2Int Length(this MinMax<Vector2> value)
            {
                Vector2Int length = Vector2Int.zero;
                length.x = Mathf.Abs((int)value.Min.x) + Mathf.Abs((int)value.Max.x);
                length.y = Mathf.Abs((int)value.Min.y) + Mathf.Abs((int)value.Max.y);
                return length;
            }

            public static Vector2 Random(this MinMax<Vector2> value)
            {
                Vector2 random = Vector2.zero;
                random.x = UnityEngine.Random.Range(value.Min.x, value.Max.x);
                random.y = UnityEngine.Random.Range(value.Min.x, value.Max.x);
                return random;
            }
        }
        
        // Vector3
        public static partial class MinMaxExtension
        {
            public static Vector3Int Length(this MinMax<Vector3> value)
            {
                Vector3Int length = Vector3Int.zero;
                length.x = Mathf.Abs((int)value.Min.x) + Mathf.Abs((int)value.Max.x);
                length.y = Mathf.Abs((int)value.Min.y) + Mathf.Abs((int)value.Max.y);
                return length;
            }

            public static Vector3 Random(this MinMax<Vector3> value)
            {
                Vector3 random = Vector3.zero;
                random.x = UnityEngine.Random.Range(value.Min.x, value.Max.x);
                random.y = UnityEngine.Random.Range(value.Min.x, value.Max.x);
                random.z = UnityEngine.Random.Range(value.Min.z, value.Max.z);
                return random;
            }
        }
    }
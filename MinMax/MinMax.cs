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

        public static int Random(this MinMax<int> value, bool includeMax = false)
        {
            return UnityEngine.Random.Range(value.Min, value.Max + (includeMax ? 1 : 0));
        }

        public static bool IsInRange(this MinMax<int> range, int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min <= value && value <= range.Max;
            else if (isIncludeLeft)
                return range.Min <= value && value <= range.Max;
            else if (isIncludeRight)
                return range.Min < value && value <= range.Max;
            else
                return range.Min < value && value < range.Max;
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

        public static bool IsInRange(this MinMax<float> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min <= value && value <= range.Max;
            else if (isIncludeLeft)
                return range.Min <= value && value <= range.Max;
            else if (isIncludeRight)
                return range.Min < value && value <= range.Max;
            else
                return range.Min < value && value < range.Max;
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
            random.y = UnityEngine.Random.Range(value.Min.y, value.Max.y);
            return random;
        }

        #region Is In Range

        public static bool IsInRange(this MinMax<Vector2> range, Vector2 value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y;
        }

        public static bool IsInRangeX(this MinMax<Vector2> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value && value <= range.Max.x;
            else if (isIncludeLeft)
                return range.Min.x <= value && value < range.Max.x;
            else if (isIncludeRight)
                return range.Min.x < value && value <= range.Max.x;
            else
                return range.Min.x < value && value < range.Max.x;
        }

        public static bool IsInRangeY(this MinMax<Vector2> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value && value <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.y <= value && value < range.Max.y;
            else if (isIncludeRight)
                return range.Min.y < value && value <= range.Max.y;
            else
                return range.Min.y < value && value < range.Max.y;
        }

        #endregion
    }

    // Vector3
    public static partial class MinMaxExtension
    {
        public static Vector3Int Length(this MinMax<Vector3> value)
        {
            Vector3Int length = Vector3Int.zero;
            length.x = Mathf.Abs((int)value.Min.x) + Mathf.Abs((int)value.Max.x);
            length.y = Mathf.Abs((int)value.Min.y) + Mathf.Abs((int)value.Max.y);
            length.z = Mathf.Abs((int)value.Min.z) + Mathf.Abs((int)value.Max.z);
            return length;
        }

        public static Vector3 Random(this MinMax<Vector3> value)
        {
            Vector3 random = Vector3.zero;
            random.x = UnityEngine.Random.Range(value.Min.x, value.Max.x);
            random.y = UnityEngine.Random.Range(value.Min.y, value.Max.y);
            random.z = UnityEngine.Random.Range(value.Min.z, value.Max.z);
            return random;
        }

        #region Is In Range

        public static bool IsInRange(this MinMax<Vector3> range, Vector3 value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y &&
                       range.Min.z <= value.z && value.z <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y &&
                       range.Min.z <= value.z && value.z < range.Max.z;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y &&
                       range.Min.z < value.z && value.z <= range.Max.z;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y &&
                       range.Min.z < value.z && value.z < range.Max.z;
        }

        public static bool IsInRangeX(this MinMax<Vector3> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value && value <= range.Max.x;
            else if (isIncludeLeft)
                return range.Min.x <= value && value < range.Max.x;
            else if (isIncludeRight)
                return range.Min.x < value && value <= range.Max.x;
            else
                return range.Min.x < value && value < range.Max.x;
        }

        public static bool IsInRangeY(this MinMax<Vector3> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value && value <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.y <= value && value < range.Max.y;
            else if (isIncludeRight)
                return range.Min.y < value && value <= range.Max.y;
            else
                return range.Min.y < value && value < range.Max.y;
        }

        public static bool IsInRangeZ(this MinMax<Vector3> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.z <= value && value <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.z <= value && value < range.Max.z;
            else if (isIncludeRight)
                return range.Min.z < value && value <= range.Max.z;
            else
                return range.Min.z < value && value < range.Max.z;
        }

        public static bool IsInRangeXY(this MinMax<Vector3> range, Vector2 value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y;
        }


        public static bool IsInRangeXZ(this MinMax<Vector3> range, Vector2 value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.z <= value.z && value.z <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.z <= value.z && value.z < range.Max.z;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.z < value.z && value.z <= range.Max.z;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.z < value.z && value.z < range.Max.z;
        }

        public static bool IsInRangeYZ(this MinMax<Vector3> range, Vector2 value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value.x && value.x <= range.Max.y &&
                       range.Min.z <= value.y && value.y <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.y <= value.x && value.x < range.Max.y &&
                       range.Min.z <= value.y && value.y < range.Max.z;
            else if (isIncludeRight)
                return range.Min.y < value.x && value.x <= range.Max.y &&
                       range.Min.z < value.y && value.y <= range.Max.z;
            else
                return range.Min.y < value.x && value.x < range.Max.y &&
                       range.Min.z < value.y && value.y < range.Max.z;
        }

        #endregion
    }

    // Vector2Int
    public static partial class MinMaxExtension
    {
        public static Vector2Int Length(this MinMax<Vector2Int> value)
        {
            Vector2Int length = Vector2Int.zero;
            length.x = Mathf.Abs((int)value.Min.x) + Mathf.Abs((int)value.Max.x);
            length.y = Mathf.Abs((int)value.Min.y) + Mathf.Abs((int)value.Max.y);
            return length;
        }

        public static Vector2Int Random(this MinMax<Vector2Int> value)
        {
            Vector2Int random = Vector2Int.zero;
            random.x = UnityEngine.Random.Range(value.Min.x, value.Max.x);
            random.y = UnityEngine.Random.Range(value.Min.y, value.Max.y);
            return random;
        }

        #region Is In Range

        public static bool IsInRange(this MinMax<Vector2Int> range, Vector2Int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y;
        }

        public static bool IsInRangeX(this MinMax<Vector2Int> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value && value <= range.Max.x;
            else if (isIncludeLeft)
                return range.Min.x <= value && value < range.Max.x;
            else if (isIncludeRight)
                return range.Min.x < value && value <= range.Max.x;
            else
                return range.Min.x < value && value < range.Max.x;
        }

        public static bool IsInRangeY(this MinMax<Vector2Int> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value && value <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.y <= value && value < range.Max.y;
            else if (isIncludeRight)
                return range.Min.y < value && value <= range.Max.y;
            else
                return range.Min.y < value && value < range.Max.y;
        }

        #endregion
    }

    // Vector3Int
    public static partial class MinMaxExtension
    {
        public static Vector3Int Length(this MinMax<Vector3Int> value)
        {
            Vector3Int length = Vector3Int.zero;
            length.x = Mathf.Abs((int)value.Min.x) + Mathf.Abs((int)value.Max.x);
            length.y = Mathf.Abs((int)value.Min.y) + Mathf.Abs((int)value.Max.y);
            length.z = Mathf.Abs((int)value.Min.z) + Mathf.Abs((int)value.Max.z);
            return length;
        }

        public static Vector3Int Random(this MinMax<Vector3Int> value)
        {
            Vector3Int random = Vector3Int.zero;
            random.x = UnityEngine.Random.Range(value.Min.x, value.Max.x);
            random.y = UnityEngine.Random.Range(value.Min.y, value.Max.y);
            random.z = UnityEngine.Random.Range(value.Min.z, value.Max.z);
            return random;
        }

        #region Is In Range

        public static bool IsInRange(this MinMax<Vector3Int> range, Vector3Int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y &&
                       range.Min.z <= value.z && value.z <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y &&
                       range.Min.z <= value.z && value.z < range.Max.z;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y &&
                       range.Min.z < value.z && value.z <= range.Max.z;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y &&
                       range.Min.z < value.z && value.z < range.Max.z;
        }

        public static bool IsInRangeX(this MinMax<Vector3Int> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value && value <= range.Max.x;
            else if (isIncludeLeft)
                return range.Min.x <= value && value < range.Max.x;
            else if (isIncludeRight)
                return range.Min.x < value && value <= range.Max.x;
            else
                return range.Min.x < value && value < range.Max.x;
        }

        public static bool IsInRangeY(this MinMax<Vector3Int> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value && value <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.y <= value && value < range.Max.y;
            else if (isIncludeRight)
                return range.Min.y < value && value <= range.Max.y;
            else
                return range.Min.y < value && value < range.Max.y;
        }

        public static bool IsInRangeZ(this MinMax<Vector3Int> range, float value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.z <= value && value <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.z <= value && value < range.Max.z;
            else if (isIncludeRight)
                return range.Min.z < value && value <= range.Max.z;
            else
                return range.Min.z < value && value < range.Max.z;
        }

        public static bool IsInRangeXY(this MinMax<Vector3Int> range, Vector3Int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.y <= value.y && value.y <= range.Max.y;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.y <= value.y && value.y < range.Max.y;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.y < value.y && value.y <= range.Max.y;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.y < value.y && value.y < range.Max.y;
        }


        public static bool IsInRangeXZ(this MinMax<Vector3Int> range, Vector3Int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.x <= value.x && value.x <= range.Max.x &&
                       range.Min.z <= value.z && value.z <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.x <= value.x && value.x < range.Max.x &&
                       range.Min.z <= value.z && value.z < range.Max.z;
            else if (isIncludeRight)
                return range.Min.x < value.x && value.x <= range.Max.x &&
                       range.Min.z < value.z && value.z <= range.Max.z;
            else
                return range.Min.x < value.x && value.x < range.Max.x &&
                       range.Min.z < value.z && value.z < range.Max.z;
        }

        public static bool IsInRangeYZ(this MinMax<Vector3Int> range, Vector3Int value, bool isIncludeLeft = true, bool isIncludeRight = true)
        {
            if (isIncludeLeft && isIncludeRight)
                return range.Min.y <= value.x && value.x <= range.Max.y &&
                       range.Min.z <= value.y && value.y <= range.Max.z;
            else if (isIncludeLeft)
                return range.Min.y <= value.x && value.x < range.Max.y &&
                       range.Min.z <= value.y && value.y < range.Max.z;
            else if (isIncludeRight)
                return range.Min.y < value.x && value.x <= range.Max.y &&
                       range.Min.z < value.y && value.y <= range.Max.z;
            else
                return range.Min.y < value.x && value.x < range.Max.y &&
                       range.Min.z < value.y && value.y < range.Max.z;
        }

        #endregion
    }
}
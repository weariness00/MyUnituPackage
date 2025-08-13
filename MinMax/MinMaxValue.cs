using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weariness.Util
{
    [System.Serializable]
    public enum MinMaxValueType
    {
        Min,
        Current,
        Max,
        
        CurrentAndMax,
        CurrentAndMin,
    }
    
    [System.Serializable]
    public class MinMaxValue<T> where T : struct
    {
        [SerializeField] private T _min;
        [SerializeField] private T _max;
        [SerializeField] private T _current;
        [SerializeField] private bool _isMin;
        [SerializeField] private bool _isMax;

        public bool isOverMax; // 기존의 Max보다 높은 값을 허용 할 것인지
        public bool isOverMin; // 기존의 Min보다 낮은 값을 허용 할 것인지
        
        private Comparer<T> cashedComparer;
        private Comparer<T> Comparer => cashedComparer ??= Comparer<T>.Default;

        public event Action<T> onChangeValueMin;
        public event Action<T> onChangeValueMax;
        public event Action<T> onChangeValueCurrent;
        
        public static implicit operator T(MinMaxValue<T> value)
        {
            return value.Current;
        }
        
        public T Current
        {
            get
            {
#if UNITY_EDITOR // Editor 상에서는 초기화가 안되고 나머지 값들이 그대로 남아있는 현상을 없애기 위해 사용
                CheckCurrent();
#endif
                return _current;
            }
            set
            {
                if (Comparer.Compare(_current, value) != 0)
                {
                    _current = value;
                    CheckCurrent();
                    onChangeValueCurrent?.Invoke(_current);
                }
            }
        }
        public T Min
        {
            get => _min;
            set
            {
                if(Comparer.Compare(_min, value) != 0)
                {
                    var prevCurrent = _current;
                    _min = value; 
                    CheckCurrent();
                    onChangeValueMin?.Invoke(_min);
                    if (Comparer.Compare(prevCurrent, _current) != 0)
                        onChangeValueCurrent?.Invoke(_current);
                }
            }
        }
        
        public T Max
        {
            get => _max;
            set
            {
                if (Comparer.Compare(_max, value) != 0)
                {
                    var prevCurrent = _current;
                    _max = value;
                    onChangeValueMax?.Invoke(_max);
                    CheckCurrent();
                    if (Comparer.Compare(prevCurrent, _current) != 0)
                        onChangeValueCurrent?.Invoke(_current);
                }
            }
        }
        
        public bool IsMin => _isMin;
        public bool IsMax => _isMax;

        public MinMaxValue(bool _isOverMin = false, bool _isOverMax = false)
        {
            _current = default;
            _min = default;
            _max = default;
            isOverMin = _isOverMin;
            isOverMax = _isOverMax;
            CheckCurrent();
        }

        public MinMaxValue(T current, T min, T max, bool _isOverMin = false, bool _isOverMax = false)
        {
            _current = current;
            _min = min;
            _max = max;
            isOverMin = _isOverMin;
            isOverMax = _isOverMax;
            CheckCurrent();
        }
        
        public MinMaxValue(T min, T max, bool _isOverMin = false, bool _isOverMax = false)
        {
            _current = max;
            _min = min;
            _max = max;
            isOverMin = _isOverMin;
            isOverMax = _isOverMax;
            CheckCurrent();
        }

        public override string ToString()
        {
            return $"[Current : {_current}] [Min : {_min}] ~ [Max : {_max}]";
        }

        void CheckCurrent()
        {
            _isMin = _isMax = false;
            if (Comparer.Compare(_min, _max) == 0)
            {
                _isMin = _isMax = true;
            }
            if (Comparer.Compare(_current, _min) <= 0)
            {
                if (isOverMin == false)
                {
                    _current = _min;
                }
                _isMin = true;
            }
            else if (Comparer.Compare(_current, _max) >= 0)
            {
                if (isOverMax == false)
                {
                    _current = _max;
                }
                _isMax = true;
            }
        }

        // IsOver 관련 값이 True 일때 사용
        // Min, Max 사이값을 반환
        public T GetClampCurrent()
        {
            if (_isMin)
                return _min;
            if (_isMax)
                return _max;
            return _current;
        }

        // Current의 값을 Min 변경
        public void SetMin() => Current = Min;
        // Current의 값을 Max로 변경
        public void SetMax() => Current = Max;
        public void SetMax(T value)
        {
            _max = value;
            Current = Max;
        }

        public virtual T MinMaxRandom()
        {
            if (this is MinMaxValue<int> value)
            {
                var randomInt = Random.Range(value._min, value._max);
                return (T)(object)randomInt; // int로 캐스팅
            }
            if (this is MinMaxValue<long> longValue)
            {
                var randomLong = Random.Range(longValue._min, longValue._max);
                return (T)(object)randomLong; // long으로 캐스팅   
            }
            if (this is MinMaxValue<float> floatValue)
            {
                var randomFloat = Random.Range(floatValue._min, floatValue._max);
                return (T)(object)randomFloat; // float로 캐스팅
            }
            if (this is MinMaxValue<double> doubleValue)
            {
                var randomDouble = Random.Range((float)doubleValue._min, (float)doubleValue._max);
                return (T)(object)randomDouble; // double로 캐스팅
            }
            if(this is MinMaxValue<Vector2> vector2Value)
            {
                var randomX = Random.Range(vector2Value._min.x, vector2Value._max.x);
                var randomY = Random.Range(vector2Value._min.y, vector2Value._max.y);
                return (T)(object)new Vector2(randomX, randomY); // Vector2로 캐스팅
            }
            if (this is MinMaxValue<Vector2Int> vector2IntValue)
            {
                var randomX = Random.Range(vector2IntValue._min.x, vector2IntValue._max.x);
                var randomY = Random.Range(vector2IntValue._min.y, vector2IntValue._max.y);
                return (T)(object)new Vector2Int(randomX, randomY); // Vector2Int로 캐스팅
            }
            if (this is MinMaxValue<Vector3> vector3Value)
            {
                var randomX = Random.Range(vector3Value._min.x, vector3Value._max.x);
                var randomY = Random.Range(vector3Value._min.y, vector3Value._max.y);
                var randomZ = Random.Range(vector3Value._min.z, vector3Value._max.z);
                return (T)(object)new Vector3(randomX, randomY, randomZ); // Vector3로 캐스팅
            }
            if (this is MinMaxValue<Vector3Int> vector3IntValue)
            {
                var randomX = Random.Range(vector3IntValue._min.x, vector3IntValue._max.x);
                var randomY = Random.Range(vector3IntValue._min.y, vector3IntValue._max.y);
                var randomZ = Random.Range(vector3IntValue._min.z, vector3IntValue._max.z);
                return (T)(object)new Vector3Int(randomX, randomY, randomZ); // Vector3Int로 캐스팅
            }

            Debug.LogError("Status Value의 Type이 랜덤 값을 생성 할 수 없는 타입입니다.");
            return default;
        }

        /// <summary>
        /// min, max 인자 값으로 현재 Current값을 정규화
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float NormalizeToRange(float min = 0f, float max = 1f)
        {
            float normalized = 0f;
            switch (this)
            {
                case MinMaxValue<int> intValue:
                    normalized = Mathf.InverseLerp(intValue._min, intValue._max, intValue._current);
                    break;
                case MinMaxValue<long> longValue:
                    normalized = Mathf.InverseLerp(longValue._min, longValue._max, longValue._current);
                    break;
                case MinMaxValue<float> floatValue:
                    normalized = Mathf.InverseLerp(floatValue._min, floatValue._max, floatValue._current);
                    break;
                case MinMaxValue<double> doubleValue:
                    normalized = Mathf.InverseLerp((float)doubleValue._min, (float)doubleValue._max, (float)doubleValue._current);
                    break;
                default:
                    Debug.LogError("Status Value의 Type이 정규화 할 수 없는 타입입니다.");
                    break;
            }

            return Mathf.Lerp(min, max, normalized);
        }

        public Vector2 NormalizeToRange(Vector2 min, Vector2 max)
        {
            var normalized = Vector2.zero;
            switch (this)
            {
                case MinMaxValue<Vector2> vector2Value:
                    normalized.x = Mathf.InverseLerp(vector2Value._min.x, vector2Value._max.x, vector2Value._current.x);
                    normalized.y = Mathf.InverseLerp(vector2Value._min.y, vector2Value._max.y, vector2Value._current.y);
                    break;
                case MinMaxValue<Vector2Int> vector2IntValue:
                    normalized.x = Mathf.InverseLerp(vector2IntValue._min.x, vector2IntValue._max.x, vector2IntValue._current.x);
                    normalized.y = Mathf.InverseLerp(vector2IntValue._min.y, vector2IntValue._max.y, vector2IntValue._current.y);
                    break;
            }

            normalized.x = Mathf.Lerp(min.x, max.x, normalized.x);
            normalized.y = Mathf.Lerp(min.y, max.y, normalized.y);
            return normalized;
        }

        public Vector3 NormalizeToRange(Vector3 min, Vector3 max)
        {
            var normalized = Vector3.zero;
            switch (this)
            {
                case MinMaxValue<Vector3> vector2Value:
                    normalized.x = Mathf.InverseLerp(vector2Value._min.x, vector2Value._max.x, vector2Value._current.x);
                    normalized.y = Mathf.InverseLerp(vector2Value._min.y, vector2Value._max.y, vector2Value._current.y);
                    normalized.z = Mathf.InverseLerp(vector2Value._min.z, vector2Value._max.z, vector2Value._current.z);
                    break;
                case MinMaxValue<Vector3Int> vector3IntValue:
                    normalized.x = Mathf.InverseLerp(vector3IntValue._min.x, vector3IntValue._max.x, vector3IntValue._current.x);
                    normalized.y = Mathf.InverseLerp(vector3IntValue._min.y, vector3IntValue._max.y, vector3IntValue._current.y);
                    normalized.z = Mathf.InverseLerp(vector3IntValue._min.z, vector3IntValue._max.z, vector3IntValue._current.z);
                    break;
            }

            normalized.x = Mathf.Lerp(min.x, max.x, normalized.x);
            normalized.y = Mathf.Lerp(min.y, max.y, normalized.y);
            normalized.z = Mathf.Lerp(min.z, max.z, normalized.z);
            return normalized;
        }
    }
}
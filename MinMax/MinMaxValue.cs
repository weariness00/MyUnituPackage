using System;
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
    public class MinMaxValue<T> where T : struct, IComparable
    {
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
                _current = value;
                CheckCurrent();
            }
        }
        public T Min
        {
            get => _min;
            set
            {
                _min = value; 
                CheckCurrent();
            }
        }
        
        public T Max
        {
            get => _max;
            set
            {
                _max = value;
                CheckCurrent();
            }
        }
        
        [SerializeField] private T _min;
        [SerializeField] private T _max;
        [SerializeField] private T _current;
        [SerializeField] private bool _isMin;
        [SerializeField] private bool _isMax;

        public bool isOverMax; // 기존의 Max보다 높은 값을 허용 할 것인지
        public bool isOverMin; // 기존의 Min보다 낮은 값을 허용 할 것인지
        public bool IsMin
        {
            get
            {
                CheckCurrent();
                return _isMin;
            }
        }

        public bool IsMax
        {
            get
            {
                CheckCurrent();
                return _isMax;
            }   
        }
        
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
        
        void CheckCurrent()
        {
            _isMin = _isMax = false;
            if (_min.CompareTo(_max) == 0)
            {
                _isMin = _isMax = true;
            }
            if (_current.CompareTo(_min) <= 0)
            {
                if(isOverMin == false) {_current = _min;}
                _isMin = true;
            }
            else if (_current.CompareTo(_max) >= 0)
            {
                if(isOverMax == false) {_current = _max;}
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

        public float MinMaxRandom()
        {
            if (this is MinMaxValue<int> value)
            {
                var randomInt = Random.Range(value._min, value._max);
                return randomInt;
            }
            if (this is MinMaxValue<float> floatValue)
            {
                var randomFloat = Random.Range(floatValue._min, floatValue._max);
                return randomFloat;
            }

            return 0f;
        }

        /// <summary>
        /// min, max 인자 값으로 현재 Current값을 정규화
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float NormalizeToRange(float min = 0f, float max = 1f)
        {
            if (this is MinMaxValue<int> intValue)
            {
                var normalized = Mathf.InverseLerp(intValue._min, intValue._max, intValue);
                return Mathf.Lerp(min, max, normalized);
            }
            if (this is MinMaxValue<float> floatValue)
            {
                var normalized = Mathf.InverseLerp(floatValue._min, floatValue._max, floatValue);
                return Mathf.Lerp(min, max, normalized);
            }

            Debug.LogError("Status Value의 Type이 정규화 할 수 없는 타입입니다.");

            return 0f;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Weariness.Util;
using Weariness.Util.Extensions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Util
{
    [System.Serializable]
    public enum SpawnPlaceType
    {
        Transform,
        Line,
    }
    
    public class ObjectSpawner : ObjectSpawner<GameObject> {}
    public partial class ObjectSpawner<TGameObject> : MonoBehaviour where TGameObject : Object
    {
        public bool isStartSpawn = false; // 이 컴포넌트가 생성되자마자 스폰하게 할 것인지
        public bool isEnableSpawn = true; // 비활성화 -> 활성화 될때 스폰하게 할 것인지
        [Tooltip("Count와 상관없이 계속 스폰 할 건지")] public bool isLoop = false;
        [Tooltip("생성 잠시 중단")] public bool isPause;
        [Tooltip("스폰 간격에 곱샘해준다.")] public float timeScale = 1f;
        
        // 해당 random은 아래의 리스트의 원소상에서의 랜덤임
        public bool isRandomObject = false; // 랜덤한 객체를 소환할 것인지
        public bool isRandomPlace = false; // 랜덤한 위치에 소환할 것인지
        public bool isRandomInterval = false; // 랜덤한 간격에 소환할 것인지
        public Transform parentTransform; // 소환된 객체가 갈 부모 객체 ( 하이어라키에서 관리 편의성을 위해 사용 )
        public MinMaxValue<int> spawnCount = new MinMaxValue<int>(); // 현재 스폰된 갯수
        [HideInInspector] public int totalSpawnCount = 0;

        public List<TGameObject> spawnObjectList = new();
        public int[] spawnObjectOrders; // 스폰할 객체, 1개일 경우 해당 객체만 스폰 여러개일 경우 순차적으로 스폰
        private int _spawnObjectOrderCount = -1; // 현재 스폰할 객체
        protected TGameObject _currentSpawnObject;

        public SpawnPlaceType spawnPlaceType;
        // 고정 위치
        [Tooltip("Transform의 레이어와 같은 레이어로 스폰 시킬지")] public bool isSameLayer = false;
        public List<Transform> spawnPlaceList = new();
        public int[] spawnPlaceOrders; // 스폰 위치 설정, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        protected int _spawnPlaceCount = -1;
        protected Vector3 currentPosition;
        protected Quaternion currentRotate;
        
        // 범위 위치
        // line
        public Vector3 firstPosition;
        public Vector3 lastPosition;
        
        public float[] spawnIntervals; // 스폰 간격, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        private int _spawnIntervalCount = -1;
        [HideInInspector] public MinMaxValue<float> intervalTimer = new(true);
    
        public UnityEvent<TGameObject> onSpawnSuccessAction; // 스폰 되면 실행하는 이벤트
        private bool _isPlay = false;
        private Coroutine _spawnCoroutine;

        public virtual void Awake()
        {
            intervalTimer.isOverMin = true;
            
            if (isStartSpawn)
            {
                Play();
            }
            
            if (spawnPlaceList.Count == 0)
            {
                currentPosition = gameObject.transform.position;
                currentRotate = gameObject.transform.rotation;
            }
        }

        public void OnEnable()
        {
            if(isEnableSpawn && _isPlay && gameObject.activeInHierarchy && _spawnCoroutine == null) _spawnCoroutine = StartCoroutine(SpawnerEnumerator(0f));
        }

        public void OnDisable()
        {
            _spawnCoroutine = null;
        }

        public void Init()
        {
            _spawnObjectOrderCount = -1;
            _spawnPlaceCount = -1;
            _spawnIntervalCount = -1;
            
            NextObject();
            NextPlace();
            NextInterval();
        }

        public void Play(float delay = 0f)
        {
            isPause = false;
            _isPlay = true;
            if (gameObject.activeInHierarchy && _spawnCoroutine == null) _spawnCoroutine = StartCoroutine(SpawnerEnumerator(delay));
        }

        public void Stop()
        {
            if(_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _isPlay = false;
            _spawnCoroutine = null;
        }

        public void Pause()
        {
            isPause = true;
        }

        private IEnumerator SpawnerEnumerator(float delay)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            while (_isPlay && (!spawnCount.IsMax || isLoop))
            {
                if (isPause == false)
                {
                    if(!spawnCount.IsMax) intervalTimer.Current -= Time.deltaTime * timeScale;
                    if (intervalTimer.IsMin && !spawnCount.IsMax)
                    {
                        Spawn();
                    }
                }
                yield return null;
            } 
        }

        public virtual void Spawn()
        {
            NextObject();
            NextPlace();
            NextInterval();
            var obj = Instantiate(_currentSpawnObject, currentPosition, currentRotate, parentTransform);
            spawnCount.Current++;

            if (spawnPlaceType == SpawnPlaceType.Transform && isSameLayer && obj is GameObject go) go.layer = spawnPlaceList[_spawnPlaceCount].gameObject.layer;
            
            onSpawnSuccessAction.Invoke(obj);
        }

        protected void NextObject()
        {
            if (isRandomObject)
                _spawnObjectOrderCount = spawnObjectOrders.Length == 0 ? Random.Range(0, spawnObjectList.Count) : Random.Range(0, spawnObjectOrders.Length);
            else
                _spawnObjectOrderCount++;

            if (spawnObjectOrders.Length > 0)
            {
                if (spawnObjectOrders.Length <= _spawnObjectOrderCount) _spawnObjectOrderCount = 0;
                _currentSpawnObject = spawnObjectList[spawnObjectOrders[_spawnObjectOrderCount]];
            }
            else
            {
                if (spawnObjectList.Count - 1 < _spawnObjectOrderCount) _spawnObjectOrderCount = 0;
                _currentSpawnObject = spawnObjectList[_spawnObjectOrderCount];
            }
        }
    
        protected void NextPlace()
        {
            void PlaceToTransform()
            {
                if (spawnPlaceList.Count == 0)
                {
                    currentPosition = transform.position;
                    currentRotate = transform.rotation;
                    return;
                }
            
                // 길이 할당
                int length = spawnPlaceOrders.Length != 0 ? spawnPlaceOrders.Length : spawnPlaceList.Count;

                Transform t;
                // 인덱스 설정
                _spawnPlaceCount++;
                if (isRandomPlace) _spawnPlaceCount = Random.Range(0, length);
                if (spawnPlaceOrders.Length == 0)
                {
                    if (_spawnPlaceCount >= length) _spawnPlaceCount = 0;
                    t = spawnPlaceList[_spawnPlaceCount];
                }
                else
                {
                    if (_spawnPlaceCount >= spawnPlaceOrders.Length) _spawnPlaceCount = 0;
                    t = spawnPlaceList[spawnPlaceOrders[_spawnPlaceCount]];
                }

                // 위치 할당
                currentPosition = t.position;
                currentRotate = t.rotation;
            }

            void PlaceToLine()
            {
                currentPosition = Vector3Extension.Random(firstPosition, lastPosition);
                currentRotate = Quaternion.identity;
            }
            
            switch (spawnPlaceType)
            {
                case SpawnPlaceType.Transform:
                    PlaceToTransform();
                    break;
                case SpawnPlaceType.Line:
                    PlaceToLine();
                    break;
            }
        }
    
        protected void NextInterval()
        {
            if (spawnIntervals.Length == 0)
            {
                intervalTimer.SetMax(1);
                return;
            }

            float interval = 0;
            
            if (isRandomInterval)
                _spawnIntervalCount =  Random.Range(0, spawnIntervals.Length);
            else
                _spawnIntervalCount++;

            if (_spawnIntervalCount >= spawnIntervals.Length) _spawnIntervalCount = 0;
            
            if (spawnIntervals.Length == 0) interval = 0;
            else interval = spawnIntervals[_spawnIntervalCount];

            var dis = intervalTimer.Current;
            intervalTimer.SetMax(interval);
            intervalTimer.Current += dis;
        }
    }
}
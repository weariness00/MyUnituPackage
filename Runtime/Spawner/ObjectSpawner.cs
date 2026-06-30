using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Weariness.Util;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Util
{
    public abstract class ObjectSpawnerBase : MonoBehaviour {}
    public class ObjectSpawner : ObjectSpawner<GameObject> {}
    public partial class ObjectSpawner<TGameObject> : ObjectSpawnerBase where TGameObject : Object
    {
        public bool isStartSpawn = false;
        public bool isEnableSpawn = false;
        [Tooltip("Count와 상관없이 계속 스폰 할 건지")] public bool isLoop = false;
        [Tooltip("생성 잠시 중단")] public bool isPause;
        [Tooltip("스폰 간격에 곱샘해준다.")] public float timeScale = 1f;

        public bool isRandomObject = false;
        public bool isRandomInterval = false;
        public Transform parentTransform;
        public MinMaxValue<int> spawnCount = new MinMaxValue<int>();
        [HideInInspector] public int totalSpawnCount = 0;

        public List<TGameObject> spawnObjectList = new();
        public int[] spawnObjectOrders = Array.Empty<int>();
        private int _spawnObjectOrderCount = -1;
        protected TGameObject _currentSpawnObject;

        [SerializeReference] public ISpawnPlace spawnPlace = new TransformSpawnPlace();
        protected Vector3 currentPosition;
        protected Quaternion currentRotate;

        public List<float> spawnIntervals = new();
        private int _spawnIntervalCount = -1;
        [HideInInspector] public MinMaxValue<float> intervalTimer = new(true);

        public UnityEvent<TGameObject> onSpawnSuccessAction;
        private bool _isPlay = false;
        private Coroutine _spawnCoroutine;

        public virtual void Awake()
        {
            intervalTimer.isOverMin = true;

            if (isStartSpawn)
            {
                Play();
            }
        }

        public void OnEnable()
        {
            if(isEnableSpawn && _isPlay && gameObject.activeInHierarchy && _spawnCoroutine == null) _spawnCoroutine = StartCoroutine(SpawnEnumerator(0f));
        }

        public void OnDisable()
        {
            _spawnCoroutine = null;
        }

        public void Init()
        {
            _spawnObjectOrderCount = -1;
            _spawnIntervalCount = -1;

            if (spawnPlace is TransformSpawnPlace transformPlace)
                transformPlace.Reset();

            NextObject();
            NextPlace();
            NextInterval();
        }

        public void Play(float delay = 0f)
        {
            isPause = false;
            _isPlay = true;
            if (gameObject.activeInHierarchy && _spawnCoroutine == null) _spawnCoroutine = StartCoroutine(SpawnEnumerator(delay));
        }

        public void Stop(float delay = 0f)
        {
            if (gameObject.activeInHierarchy && _spawnCoroutine != null) StartCoroutine(StopEnumerator(delay));
        }

        public void Pause()
        {
            isPause = true;
        }

        public void ChangePlaceType(SpawnPlaceType type)
        {
            spawnPlace = type switch
            {
                SpawnPlaceType.Transform => new TransformSpawnPlace(),
                SpawnPlaceType.Line => new LineSpawnPlace(),
                SpawnPlaceType.Circle => new CircleSpawnPlace(),
                SpawnPlaceType.Rect => new RectSpawnPlace(),
                _ => spawnPlace
            };
        }

        public void ChangePlaceType(ISpawnPlace newPlace)
        {
            spawnPlace = newPlace;
        }

        public T GetSpawnPlace<T>() where T : class, ISpawnPlace
        {
            return spawnPlace as T;
        }

        private IEnumerator SpawnEnumerator(float delay)
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

        private IEnumerator StopEnumerator(float delay)
        {
            if(delay > 0)
                yield return new WaitForSeconds(delay);
            if(_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _isPlay = false;
            _spawnCoroutine = null;
        }

        public virtual void Spawn()
        {
            NextObject();
            NextPlace();
            NextInterval();
            var obj = Instantiate(_currentSpawnObject, currentPosition, currentRotate, parentTransform);
            spawnCount.Current++;

            if (spawnPlace is TransformSpawnPlace tp && tp.isSameLayer && obj is GameObject go)
                go.layer = tp.GetCurrentLayer();

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
            var (pos, rot) = spawnPlace.GetSpawnPosition(transform);
            currentPosition = pos;
            currentRotate = rot;
        }

        protected void NextInterval()
        {
            if (spawnIntervals.Count == 0)
            {
                intervalTimer.SetMax(1);
                return;
            }

            float interval = 0;

            if (isRandomInterval)
                _spawnIntervalCount =  Random.Range(0, spawnIntervals.Count);
            else
                _spawnIntervalCount++;

            if (_spawnIntervalCount >= spawnIntervals.Count) _spawnIntervalCount = 0;

            if (spawnIntervals.Count == 0) interval = 0;
            else interval = spawnIntervals[_spawnIntervalCount];

            var dis = intervalTimer.Current;
            intervalTimer.SetMax(interval);
            intervalTimer.Current += dis;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            spawnPlace?.DrawGizmos(transform);
        }
    }
}

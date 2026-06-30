using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Util
{
    [Serializable]
    public class TransformSpawnPlace : ISpawnPlace
    {
        [Tooltip("Transform의 레이어와 같은 레이어로 스폰 시킬지")]
        public bool isSameLayer = false;
        public bool isRandomPlace = false;
        public List<Transform> spawnPlaceList = new();
        public int[] spawnPlaceOrders = Array.Empty<int>();

        private int _spawnPlaceCount = -1;

        public int CurrentPlaceIndex => _spawnPlaceCount;

        public void Reset()
        {
            _spawnPlaceCount = -1;
        }

        public (Vector3 position, Quaternion rotation) GetSpawnPosition(Transform pivot)
        {
            if (spawnPlaceList.Count == 0)
            {
                return (pivot.position, pivot.rotation);
            }

            int length = spawnPlaceOrders.Length != 0 ? spawnPlaceOrders.Length : spawnPlaceList.Count;

            _spawnPlaceCount++;
            if (isRandomPlace) _spawnPlaceCount = Random.Range(0, length);

            Transform t;
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

            return (t.position, t.rotation);
        }

        public int GetCurrentLayer()
        {
            if (_spawnPlaceCount < 0 || spawnPlaceList.Count == 0) return 0;
            int index = spawnPlaceOrders.Length > 0
                ? spawnPlaceOrders[_spawnPlaceCount]
                : _spawnPlaceCount;
            return spawnPlaceList[index].gameObject.layer;
        }

        public void DrawGizmos(Transform pivot)
        {
            if (spawnPlaceList == null) return;
            Gizmos.color = Color.cyan;
            foreach (var t in spawnPlaceList)
            {
                if (t != null)
                    Gizmos.DrawWireSphere(t.position, 0.3f);
            }
        }
    }
}

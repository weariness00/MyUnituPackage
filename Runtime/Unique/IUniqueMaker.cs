using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Weariness.Util
{
    public interface IUniqueMaker<T>
    {
        public void UniqueMake(Unique<T> unique, T start, T end);
    }

    public struct IntUniqueMaker : IUniqueMaker<int>
    {
        public void UniqueMake(Unique<int> unique, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                unique.Add(i);
            }
        }
    }

    public struct Vector2IntUniqueMaker : IUniqueMaker<Vector2Int>
    {

        public void UniqueMake(Unique<Vector2Int> unique, Vector2Int start, Vector2Int end)
        {
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    var value = new Vector2Int(x, y);
                    unique.Add(value);
                }
            }
        }
    }

    public struct Vector3IntUniqueMaker : IUniqueMaker<Vector3Int>
    {

        public void UniqueMake(Unique<Vector3Int> unique, Vector3Int start, Vector3Int end)
        {
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    for (int z = start.z; z <= end.z; z++)
                    {
                        var value = new Vector3Int(x, y, z);
                        unique.Add(value);
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weariness.Util
{
    [Serializable]
    public class Unique<T>
    {
        private HashSet<T> uniqueSet = new HashSet<T>(); // ���ϼ� �Ǵܿ�
        [SerializeField] private List<T> list = new List<T>();

        public int Length => list.Count;

        public Unique()
        {

        }

        public Unique(T start, T end)
        {
            Init(start, end);
        }

        public void Init(T start, T end, IUniqueMaker<T> maker = null)
        {
            if (typeof(T) == typeof(int))
            {
                maker ??= new IntUniqueMaker() as IUniqueMaker<T>;
            }
            else if (typeof(T) == typeof(Vector2Int))
            {
                maker ??= new Vector2IntUniqueMaker() as IUniqueMaker<T>;
            }
            else if (typeof(T) == typeof(Vector3Int))
            {
                maker ??= new Vector3IntUniqueMaker() as IUniqueMaker<T>;
            }
            else if(maker == null)
            {
                if(start is IUniqueMaker<T> s)
                {
                    maker = s;
                }
                else if(end is IUniqueMaker<T> e)
                {
                    maker = e;
                }
                else
                {
                    throw new System.Exception("Unique�� ������� Maker�� �����ϴ�. IUniqueMaker�� ��ӹ޾� �������ּ���");
                }
            }
            maker.UniqueMake(this, start, end);
        }

        public T Get()
        {
            var index = Random.Range(0, list.Count);
            var value = list[index];
            list.RemoveAt(index);
            uniqueSet.Remove(value);
            return value;
        }

        public void Add(T value)
        {
            if (uniqueSet.Contains(value))
                return;
            uniqueSet.Add(value);
            list.Add(value);
        }


        public void Remove(T value)
        {
            if (uniqueSet.Contains(value))
            {
                uniqueSet.Remove(value);
                list.Remove(value);
            }
        }
    }
}
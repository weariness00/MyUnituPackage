using UnityEngine;

namespace Weariness.Util
{
    public interface IUniqueListMaker<T>
    {
        public void UniqueMake(UniqueList<T> uniqueList, T start, T end);
    }

    public struct UniqueListMakerInt : IUniqueListMaker<int>
    {
        public void UniqueMake(UniqueList<int> uniqueList, int start, int end)
        {
            uniqueList.Clear();
            for (int i = start; i <= end; i++)
            {
                uniqueList.Add(i);
            }
        }
    }

    public struct UniqueListMakerVector2Int : IUniqueListMaker<Vector2Int>
    {
        public void UniqueMake(UniqueList<Vector2Int> uniqueList, Vector2Int start, Vector2Int end)
        {
            uniqueList.Clear();
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    uniqueList.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    public struct UniqueListMakerVector3Int : IUniqueListMaker<Vector3Int>
    {
        public void UniqueMake(UniqueList<Vector3Int> uniqueList, Vector3Int start, Vector3Int end)
        {
            uniqueList.Clear();
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    for (int z = start.z; z <= end.z; z++)
                    {
                        uniqueList.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}

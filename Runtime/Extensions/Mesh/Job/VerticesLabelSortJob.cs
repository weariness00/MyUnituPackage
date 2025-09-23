using Unity.Collections;
using Unity.Jobs;

namespace Weariness.Util.Extensions
{
    public struct VerticesLabelSortJob 
#if UNITY_2022_2_OR_NEWER
        : IJobParallelForBatch
#else
        : IJobParallelFor
#endif
    {
        public NativeArray<NativeList<int>> sortLabels;
        public NativeArray<int> labelArray; // 각 정점의 라벨
        
#if UNITY_2022_2_OR_NEWER
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                int label = labelArray[i];
                sortLabels[label].Add(i);
            }
        }
#else
        public void Execute(int index)
        {
            int label = labelArray[index];
            sortLabels[label].Add(index);
        }
#endif
    }
}
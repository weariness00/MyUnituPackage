using Unity.Collections;
using Unity.Jobs;

namespace Weariness.Util.Extensions
{
    public struct VerticesLabelSortJob : IJobParallelForBatch
    {
        public NativeArray<NativeList<int>> sortLabels;
        public NativeArray<int> labelArray; // 각 정점의 라벨
        
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < count; i++)
            {
                int label = labelArray[i];
                sortLabels[label].Add(i);
            }
        }
    }
}
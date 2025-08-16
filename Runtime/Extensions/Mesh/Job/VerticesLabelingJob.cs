using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Weariness.Util.Extensions
{
    // 폴리곤으로 연결되있지 않고 불안정한 메쉬들을 연결해주는 역할 ( 단순 길 찾기에 사용 )
    // 예를들면 삼각 폴리곤 A, B가 있는데 여기에 사용된 6개점 그 어떤 것도 위치가 같은게 없을 경우 사용
    public struct VerticesLabelingJob : IJob
    {
        public NativeArray<NativeList<int>> adjacency;
        public NativeArray<int> labelArray;
        public NativeArray<int> labelsCount; // [0] = 라벨 개수
        
        public int vertexCount; // vertices.Length
        
        public void Execute()
        {
            var componentCount = 0;

            for (int i = 0; i < vertexCount; ++i)
            {
                if (labelArray[i] != 0) continue; // 이미 라벨링된 점은 패스

                componentCount++;
                // BFS 혹은 DFS로 현재 조각 탐색 시작
                var stack = new Stack<int>();
                stack.Push(i);
                labelArray[i] = componentCount;

                while (stack.Count > 0)
                {
                    int v = stack.Pop();
                    var neighbors = adjacency[v];
                    for (int j = 0; j < neighbors.Length; ++j)
                    {
                        int u = neighbors[j];
                        if (labelArray[u] == 0)
                        {
                            labelArray[u] = componentCount;
                            stack.Push(u);
                        }
                    }
                }
            }
            
            labelsCount[0] = componentCount;
        }
    }
}
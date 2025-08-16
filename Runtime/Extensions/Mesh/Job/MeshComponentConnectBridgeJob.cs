using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    public struct MeshComponentConnectBridgeJob : IJobParallelFor
    {
        public NativeArray<NativeArray<NativeList<int>>> adjacencyList; // 각 컴포넌트의 인접 리스트
        public NativeArray<NativeList<int>> sortLabel;
        public NativeArray<Vector3> vertices; // 정점 정보

        public void Execute(int index)
        {
            NativeArray<NativeList<int>> adjacency = new NativeArray<NativeList<int>>(vertices.Length, Allocator.TempJob); // 인접 리스트
            for (int i = 0; i < adjacency.Length; i++)
                adjacency[i] = new NativeList<int>(Allocator.Temp);
            
            float minDist = float.MaxValue;
            var setA = sortLabel[index];
            for (int i = 0; i < sortLabel.Length; i++)
            {
                if (i == index) continue;
                var setB = sortLabel[index + 1];

                foreach (var a in setA)
                foreach (var b in setB)
                    minDist = Mathf.Min(minDist, Vector3.Distance(vertices[a], vertices[b]));

                //허용 오차(예: maxDelta = 0.2f 등) 설정
                float maxDelta = 0.2f;

                //모든 쌍에서 (거리 - minDist) <= maxDelta 인 쌍 모두 연결
                foreach (var a in setA)
                {
                    foreach (var b in setB)
                    {
                        float d = Vector3.Distance(vertices[a], vertices[b]);
                        if (d - minDist <= maxDelta)
                        {
                            adjacency[a].Add(b);
                            adjacency[b].Add(a);
                            // (가중치 정보 저장 필요시 별도 관리)
                        }
                    }
                }
                adjacencyList[index] = adjacency;
            }
        }
    }

    public struct ComponentAdjacencyMergeJob : IJob
    {
        public NativeArray<NativeList<int>> resultAdjacency; // 결과 인접 리스트
        public NativeArray<NativeArray<NativeList<int>>> adjacencyList; // 각 컴포넌트의 인접 리스트
        
        public void Execute()
        {
            foreach (var adjacency in adjacencyList)
            {
                for (int i = 0; i < adjacency.Length; i++)
                {
                    for (int j = 0; j < adjacency[i].Length; j++)
                    {
                        resultAdjacency[i].Add(j);
                    }
                }
            }
        }
    }
}
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    [BurstCompile]
    public struct DijkstraJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<NativeList<int>> adjacency;
        public int startIndex;
        public int endIndex;

        [WriteOnly] public NativeList<int> outPath; // 경로(정점 인덱스)
        [WriteOnly] public NativeArray<float> outDistance;

        public void Execute()
        {
            int n = vertices.Length;
            var dist = new NativeArray<float>(n, Allocator.Temp);
            var prev = new NativeArray<int>(n, Allocator.Temp);
            var visited = new NativeArray<bool>(n, Allocator.Temp);

            for (int i = 0; i < n; ++i)
            {
                dist[i] = float.MaxValue;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[startIndex] = 0f;

            // 간단한 최소 거리 탐색(Heap 대신 for문, 작은 mesh 기준)
            for (int iter = 0; iter < n; ++iter)
            {
                float minDist = float.MaxValue;
                int u = -1;
                for (int i = 0; i < n; ++i)
                    if (!visited[i] && dist[i] < minDist)
                    {
                        minDist = dist[i];
                        u = i;
                    }

                if (u == -1) break;
                if (u == endIndex) break;
                visited[u] = true;

                var adj = adjacency[u];
                for (int j = 0; j < adj.Length; ++j)
                {
                    int v = adj[j];
                    float alt = dist[u] + Vector3.Distance(vertices[u], vertices[v]);
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }

            // 경로 복원
            outPath.Clear();
            int idx = endIndex;
            if (prev[idx] != -1 || idx == startIndex)
            {
                while (idx != -1)
                {
                    outPath.Add(idx);
                    idx = prev[idx];
                }

                for (int i = 0, j = outPath.Length - 1; i < j; ++i, --j)
                {
                    (outPath[i], outPath[j]) = (outPath[j], outPath[i]);
                }
            }

            outDistance[0] = dist[endIndex];

            dist.Dispose();
            prev.Dispose();
            visited.Dispose();
        }
    }
}
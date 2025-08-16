using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class MeshExtension
    {
        public static float GetMeshGeodesicDistance(this Mesh mesh, Vector3 origin, Vector3 direction, float maxDistance, float threshold = 0.1f)
        {
            // Origin, Direction, MaxDistance으로 선분을 정의하고,
            // 해당 선분에 제일 가까운 정점과 제일 먼 정점을 찾는다.
            var result = new NativeArray<int>(2, Allocator.TempJob);
            var vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.TempJob);
            var findNearAndFarVertexJob = new FindNearAndFarVertexJob()
            {
                resultIndices = result,
                vertices = vertices,
                origin = origin,
                direction = direction.normalized,
                maxDistance = maxDistance,
                threshold = threshold
            };
            
            findNearAndFarVertexJob.Schedule().Complete();
            
            // A 정점에서 B 정점까지의 지오데식 거리를 구한다.
            var adjacency = mesh.CreateAdjacencyGraph(Allocator.TempJob);
            var distance = new NativeArray<float>(1, Allocator.TempJob);
            var pathFindJob = new DijkstraJob
            {
                vertices = vertices,
                adjacency = adjacency,
                startIndex = result[0],
                endIndex = result[1],
                outPath = new NativeList<int>(Allocator.TempJob),
                outDistance = distance,
            };
            pathFindJob.Schedule().Complete();
            
            // 두 정점 사이의 지오데식 거리를 구하지 못했다면
            if (distance[0] == float.MaxValue)
            {
                // 라벨링 작업을 수행하여 메쉬들이 붙어 있지 않고 나누어져 있는 것을 구분한다.
                var labels = new NativeArray<int>(mesh.vertexCount, Allocator.TempJob);
                var labelCount = new NativeArray<int>(1, Allocator.TempJob);
                var verticesLabelJob = new VerticesLabelingJob
                {
                    adjacency = adjacency,
                    labelArray = labels,
                    labelsCount = labelCount,

                    vertexCount = mesh.vertexCount,
                };
                
                verticesLabelJob.Schedule().Complete();
                
                // 라벨링 정렬
                var sortLabels = new NativeArray<NativeList<int>>(labelCount[0], Allocator.TempJob);
                var labelSortJob = new VerticesLabelSortJob
                {
                    sortLabels = sortLabels,
                    labelArray = labels
                };
                
                labelSortJob.Schedule(labelCount[0], 32).Complete();
                
                // 라벨링을 통해 분리된 메쉬를 유클리드 거리를 통해 가까운 것들은 그래프에 정점을 연결
                var adjacencyList = new NativeArray<NativeArray<NativeList<int>>>(labelCount[0], Allocator.TempJob);
                var connectBridgeJob = new MeshComponentConnectBridgeJob
                {
                    adjacencyList = adjacencyList,
                    sortLabel = sortLabels,
                    vertices = vertices
                };
                connectBridgeJob.Schedule(labelCount[0], 32).Complete();
                
                // 컴포넌트 간의 인접 리스트를 병합
                var mergeJob = new ComponentAdjacencyMergeJob
                {
                    resultAdjacency = adjacency,
                    adjacencyList = adjacencyList
                };
                mergeJob.Schedule().Complete();
            }

            pathFindJob.adjacency = adjacency;
            pathFindJob.Schedule().Complete();
            return distance[0];
        }
        
        public static NativeArray<NativeList<int>> CreateAdjacencyGraph(this Mesh mesh, Allocator allocator)
        {
            var adjacency = new NativeArray<NativeList<int>>(mesh.vertexCount, allocator);
            for (int i = 0; i < mesh.vertexCount; i++)
                adjacency[i] = new NativeList<int>(4, allocator);
            BuildAdjacencyGraph(
                mesh.triangles,
                ref adjacency);
            return adjacency;
        }
        // vertexCount: mesh.vertexCount
        // triangles: mesh.triangles (NativeArray<int>)
        public static void BuildAdjacencyGraph(
            int[] triangles,
            ref NativeArray<NativeList<int>> adjacency)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];

                if (!adjacency[a].Contains(b)) adjacency[a].Add(b);
                if (!adjacency[a].Contains(c)) adjacency[a].Add(c);

                if (!adjacency[b].Contains(a)) adjacency[b].Add(a);
                if (!adjacency[b].Contains(c)) adjacency[b].Add(c);

                if (!adjacency[c].Contains(a)) adjacency[c].Add(a);
                if (!adjacency[c].Contains(b)) adjacency[c].Add(b);
            }
        }
    }
}
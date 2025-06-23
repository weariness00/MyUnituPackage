using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    /// <summary>
    /// 선분을 구한다.
    /// 메쉬의 vertices중 선분에 가까운 NearVertices 구한다.
    /// NearVertices들 중 origin과 가장 가까운 Vertex과 가장 먼 Vertex를 구한다.
    /// 두 점을 반환한다.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="dir"></param>
    /// <param name="maxDistance"></param>
    [BurstCompile]
    public struct FindNearAndFarVertexJob : IJob
    {
        [WriteOnly] public NativeArray<int> resultIndices; // [0]=가장 가까운, [1]=가장 먼
        [ReadOnly] public NativeArray<Vector3> vertices; // 메쉬의 vertex 배열 (월드 좌표)
        public Vector3 origin; // 선분 시작점
        public Vector3 direction; // 선분 방향(정규화)
        public float maxDistance; // 선분 길이
        public float threshold; // 1차 임계값

        public void Execute()
        {
            int minIdx = -1, maxIdx = -1;
            float minOriginDist = float.MaxValue, maxOriginDist = float.MinValue;
            bool found = false;
            float usedThreshold = threshold;

            while (true)
            {
                minIdx = -1;
                maxIdx = -1;
                minOriginDist = float.MaxValue;
                maxOriginDist = float.MinValue;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = vertices[i];
                    Vector3 toV = v - origin;
                    float t = Vector3.Dot(toV, direction); // 투영값
                    if (t < 0f || t > maxDistance) continue; // 선분 범위 밖

                    Vector3 closest = origin + direction * t;
                    float distToLine = (v - closest).magnitude;
                    if (distToLine > usedThreshold) continue; // 너무 멀면 제외

                    found = true;

                    float originDist = toV.magnitude;
                    if (originDist < minOriginDist)
                    {
                        minOriginDist = originDist;
                        minIdx = i;
                    }

                    if (originDist > maxOriginDist)
                    {
                        maxOriginDist = originDist;
                        maxIdx = i;
                    }
                }

                if (found) break; // NearVertices가 하나라도 있으면 반복 종료
                usedThreshold *= 10f; // 임계값 10배로 증가
            }

            resultIndices[0] = minIdx; // 가장 가까운
            resultIndices[1] = maxIdx; // 가장 먼
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Weariness.Transition
{
    public class RhombusTransitionHandler : ITransitionHandler
    {
        public ImageTransition ImageTransition { get; set; }

        public Vector2Int GetIndexLength(Vector2Int grid)
        {
            return new Vector2Int(grid.x + 1, grid.y * 2 + 1);
        }

        public int GetIndex(int x, int y)
        {
            int index = 0;
            for (int i = 0; i < y; i++)
                index += (i % 2 == 0) ? ImageTransition.grid.x + 1 : ImageTransition.grid.x; // 짝수 줄은 4칸, 홀수 줄은 3칸

            if (y % 2 == 1 && x == ImageTransition.grid.x + 1 - 1) return -1;
            if (index + x >= ImageTransition.originBlocks.Length)
                return -1;
            return index + x;
        }

        public void UpdateVert(out TransitionUIBlock[] originBlocks)
        {
            var _uvs = ImageTransition.sprite
                ? ImageTransition.sprite.uv
                : new Vector2[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f)
                };
            float uMin = 0;
            float uMax = 1;
            float vMin = 0;
            float vMax = 1;

            var color = ImageTransition.color;
            var grid = ImageTransition.grid;
            Rect rect = ImageTransition.rectTransform.rect;
            float width = rect.width;
            float height = rect.height;
            Vector2 origin = new Vector2(-width / 2f, -height / 2f);

            var blockList = new List<TransitionUIBlock>();

            float stepX = width / grid.x;
            float stepY = height / grid.y;

            var index = 0;
            Vector2Int intGrid = new Vector2Int((int)grid.x, (int)grid.y);
            for (int y = 0; y < grid.y * 2 + 1; y++)
            {
                for (int x = 0; x < grid.x + 1; x++)
                {
                    var isEvenNum = y % 2 == 0;
                    if (isEvenNum == false && x + 1 == grid.x + 1) continue;
                    var vX = x;
                    var vY = y / 2;
                    var lerp = isEvenNum ? 0f : 0.5f;

                    // 중심점 기준 마름모 정점 구성
                    Vector2 center = new Vector2((vX + lerp) * stepX, (vY + lerp) * stepY) + origin;

                    Vector2 p0 = center + new Vector2(-stepX * 0.5f, 0f); // 왼쪽
                    Vector2 p1 = center + new Vector2(0f, stepY * 0.5f); // 위
                    Vector2 p2 = center + new Vector2(stepX * 0.5f, 0f); // 오른쪽
                    Vector2 p3 = center + new Vector2(0f, -stepY * 0.5f); // 아래

                    // UV 중앙 기준 비율로 보간
                    float u = Mathf.Lerp(uMin, uMax, (vX + lerp) / grid.x);
                    float v = Mathf.Lerp(vMin, vMax, (vY + lerp) / grid.y);
                    float uvSpanU = (uMax - uMin) / grid.x * 0.5f;
                    float uvSpanV = (vMax - vMin) / grid.y * 0.5f;

                    Vector2 uv0 = new Vector2(u - uvSpanU, v); // 좌
                    Vector2 uv1 = new Vector2(u, v + uvSpanV); // 상
                    Vector2 uv2 = new Vector2(u + uvSpanU, v); // 우
                    Vector2 uv3 = new Vector2(u, v - uvSpanV); // 하

                    // 정점 추가
                    var block = new TransitionUIBlock()
                    {
                        position = Vector3.zero,
                        rotation = Vector3.zero,
                        scale = Vector3.one
                    };

                    var vertList = new List<UIVertex>();
                    var triangleList = new List<int>();
                    vertList.Add(UIVertexExtension.MakeUIVertex(p0, uv0, color));
                    vertList.Add(UIVertexExtension.MakeUIVertex(p1, uv1, color));
                    vertList.Add(UIVertexExtension.MakeUIVertex(p2, uv2, color));
                    vertList.Add(UIVertexExtension.MakeUIVertex(p3, uv3, color));

                    triangleList.Add(index + 0);
                    triangleList.Add(index + 1);
                    triangleList.Add(index + 2);

                    triangleList.Add(index + 2);
                    triangleList.Add(index + 3);
                    triangleList.Add(index + 0);

                    index += vertList.Count;
                    block.vertices = vertList.ToArray();
                    block.triangles = triangleList.ToArray();

                    blockList.Add(block);
                }
            }

            originBlocks = blockList.ToArray();
        }
    }
}


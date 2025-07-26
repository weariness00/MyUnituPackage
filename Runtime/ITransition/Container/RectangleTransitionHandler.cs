using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Weariness.Transition
{
    public class RectangleTransitionHandler : ITransitionHandler
    {
        public ImageTransition ImageTransition { get; set; }

        public int GetIndex(int x, int y)
        {
            if (y * x + x >= ImageTransition.originBlocks.Length) return -1;
            return y * x + x;
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

            var index = 0;
            var blockList = new List<TransitionUIBlock>();
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    // 위치 계산
                    float x0 = (float)x / grid.x * width;
                    float x1 = (float)(x + 1) / grid.x * width;
                    float y0 = (float)y / grid.y * height;
                    float y1 = (float)(y + 1) / grid.y * height;

                    Vector2 p0 = new Vector2(x0, y0) + origin;
                    Vector2 p1 = new Vector2(x0, y1) + origin;
                    Vector2 p2 = new Vector2(x1, y1) + origin;
                    Vector2 p3 = new Vector2(x1, y0) + origin;

                    // UV 계산
                    float u0 = Mathf.Lerp(uMin, uMax, (float)x / grid.x);
                    float u1 = Mathf.Lerp(uMin, uMax, (float)(x + 1) / grid.x);
                    float v0 = Mathf.Lerp(vMin, vMax, (float)y / grid.y);
                    float v1 = Mathf.Lerp(vMin, vMax, (float)(y + 1) / grid.y);

                    Vector2 uv0 = new Vector2(u0, v0);
                    Vector2 uv1 = new Vector2(u0, v1);
                    Vector2 uv2 = new Vector2(u1, v1);
                    Vector2 uv3 = new Vector2(u1, v0);

                    // 두 삼각형 추가
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

        public void Transition(TextAnchor childAlignment, float delay, TransitionEase ease, CancellationTokenSource cts, Func<int, float, TransitionEase, CancellationToken, UniTask> UpdateBlockAsync)
        {
            
        }
    }
}
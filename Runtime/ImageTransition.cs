using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Weariness.Transition
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Image Transition", 11)]
    public partial class ImageTransition : Image
    {
        [HideInInspector] public TransitionUIBlock[] originBlocks;

        public ImageTransition()
        {
            handler = new RectangleTransitionHandler
            {
                ImageTransition = this
            };
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (handler == null) handler = new RectangleTransitionHandler();
            QuadType = blockType;
        }
#endif

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // 정점 추가
            vh.Clear();
            for (var i = 0; i < originBlocks.Length; i++)
            {
                var block = originBlocks[i];
                // 회전 적용
                foreach (var vertex in UpdateBlockTransform(block))
                    vh.AddVert(vertex);

                for (var index = 0; index < block.triangles.Length / 3; index++)
                    vh.AddTriangle(block.triangles[index * 3 + 0], block.triangles[index * 3 + 1], block.triangles[index * 3 + 2]);
            }
        }
        protected override void OnRectTransformDimensionsChange()
        {
            handler.UpdateVert(out originBlocks);
            base.OnRectTransformDimensionsChange();
        }
    }

    public partial class ImageTransition
    {
        [SerializeField] private TransitionBlockType blockType;
        public TextAnchor childAlignment; // 정렬 방식
        public Vector2Int grid = new Vector2Int(4, 4); // 그리드 크기
        public float normalizedTime = 1; // 트랜지션 진행 시간을 0~1사이값으로 정규화
        private CancellationTokenSource cts;

        public TransitionBlockType QuadType
        {
            get => blockType;
            set
            {
                blockType = value;
                switch (blockType)
                {
                    case TransitionBlockType.Rectangle:
                        Handler = new RectangleTransitionHandler();
                        break;
                    case TransitionBlockType.Rhombus:
                        Handler = new RhombusTransitionHandler();
                        break;
                    default:
                        throw new AggregateException();
                }
            }
        }



        private ITransitionHandler handler;

        public ITransitionHandler Handler
        {
            get => handler;
            set
            {
                value.ImageTransition = this;
                handler = value;
                handler.UpdateVert(out originBlocks);
                SetVerticesDirty();
            }
        }

        public override Texture mainTexture => sprite ? sprite.texture : s_WhiteTexture;

        public CancellationTokenSource TransitionRotate(Vector3 start, Vector3 end, float duration, float delay = 0f, TransitionEase ease = TransitionEase.Linear)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new();

            for (int i = 0; i < originBlocks.Length; i++)
            {
                RotateTransitionBlockAsync(i, i * delay, cts.Token).Forget();
            }

            UpdateDrawVerticesAsync(duration, cts.Token).Forget();

            return cts;

            async UniTask RotateTransitionBlockAsync(
                int index,
                float realDelay,
                CancellationToken token
            )
            {
                try
                {
                    if (delay > 0f)
                        await UniTask.WaitForSeconds(realDelay, cancellationToken: token);
                    // 전체 트랜지션 시간 동안 SetVerticesDirty() 반복 호출
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);
                        originBlocks[index].rotation = Vector3.Lerp(start, end, ease.Normalize(t));
                    }
                }
                finally
                {
                    originBlocks[index].rotation = end;
                }
            }
        }

        // 갑자기 나타나는 듯한 연출
        // isOnOff - True : ON , False : Off
        // Delay Interval : Block 1개당 딜레이 간격
        public CancellationTokenSource TransitionFlash(float duration, float delay, TransitionEase ease = TransitionEase.Linear, bool isOnOff = true)
        {
            var ro = new Vector3(0, 0, 45);
            var sc = Vector3.zero;
            var co = new Color32(255, 255, 255, 0);

            var roEnd = Vector3.zero;
            var scEnd = Vector3.one;
            var coEnd = new Color32(255, 255, 255,255 );

            if (isOnOff == false)
            {
                (ro, roEnd) = (roEnd, ro);
                (sc, scEnd) = (scEnd, sc);
                (co, coEnd) = (coEnd, co);
            }

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new();
            
            var delayInterval = delay / originBlocks.Length;
            var index = handler.GetIndexLength(grid);
            int i = 0;
            switch (childAlignment)
            {
                // Upeer
                case TextAnchor.UpperLeft:
                    for (int sum = 0; sum <= index.x + index.y - 2; sum++)
                    {
                        for (int x = Math.Min(index.x - 1, sum); x >= 0; x--)
                        {
                            int y = sum - x;
                            var blockIndex = handler.GetIndex(x, index.y - y - 1);
                            if (blockIndex == -1) continue;
                            if (y >= 0 && y < index.y)
                            {
                                UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                                ++i;
                            }
                        }
                    }
                    break;
                case TextAnchor.UpperCenter:
                    foreach (var posList in GetLayeredPositions(index, index.x / 2, index.y))
                    {
                        foreach (var (x, y) in posList)
                        {
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                        }

                        i++;
                    }

                    break;
                case TextAnchor.UpperRight:
                    for (int sum = index.x + index.y - 2 + 0; sum >= 0; sum--)
                    {
                        for (int x = Math.Min(index.x - 1, sum); x >= 0; x--)
                        {
                            int y = sum - x;
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            if (y >= 0 && y < index.y)
                            {
                                UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                                ++i;
                            }
                        }
                    }

                    break;

                // Middle
                case TextAnchor.MiddleLeft:
                    break;
                case TextAnchor.MiddleCenter:
                    foreach (var posList in GetLayeredPositions(index, index.x / 2, index.y / 2))
                    {
                        foreach (var (x, y) in posList)
                        {
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                        }

                        i++;
                    }
                    break;
                case TextAnchor.MiddleRight:
                    for (int x = index.x - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < index.y; y++)
                        {
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                            i++;
                        }
                    }

                    break;

                // Lower
                case TextAnchor.LowerLeft:
                    for (int sum = 0; sum <= index.x + index.y - 2; sum++) // sum = x + y
                    {
                        for (int x = Math.Min(index.x - 1, sum); x >= 0; x--)
                        {
                            int y = sum - x;
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            if (y >= 0 && y < index.y)
                            {
                                UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                                ++i;
                            }
                        }
                    }

                    break;
                case TextAnchor.LowerCenter:
                    foreach (var posList in GetLayeredPositions(index, index.x / 2, 0))
                    {
                        foreach (var (x, y) in posList)
                        {
                            var blockIndex = handler.GetIndex(x, y);
                            if (blockIndex == -1) continue;
                            UpdateBlockAsync(blockIndex, i * delayInterval, ease, cts.Token).Forget();
                        }

                        i++;
                    }
                    break;
                case TextAnchor.LowerRight:
                    for (int sum = index.x + index.y - 2 + 0; sum >= 0; sum--)
                    {
                        for (int x = Math.Min(index.x - 1, sum); x >= 0; x--)
                        {
                            int y = sum - x;
                            var blockIndex = handler.GetIndex(x, index.y - y - 1);
                            if (blockIndex == -1) continue;
                            if (y >= 0 && y < index.y)
                            {
                                UpdateBlockAsync(blockIndex, i * delayInterval,ease, cts.Token).Forget();
                                ++i;
                            }
                        }
                    }

                    break;
            }
            UpdateDrawVerticesAsync(duration + delay, cts.Token).Forget();

            return cts;
            async UniTask UpdateBlockAsync(int blockIndex, float realDelay, TransitionEase _ease, CancellationToken token)
            {
                // 초기값 직접 설정
                originBlocks[blockIndex].rotation = ro;
                originBlocks[blockIndex].scale = sc;
                originBlocks[blockIndex].vertices[0].color = co;
                originBlocks[blockIndex].vertices[1].color = co;
                originBlocks[blockIndex].vertices[2].color = co;
                originBlocks[blockIndex].vertices[3].color = co;

                if (realDelay > 0f)
                    await UniTask.WaitForSeconds(realDelay, cancellationToken: token);

                float t = 0f;
                try
                {
                    while (t <= duration)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                        t += Time.deltaTime;
                        var norT = _ease.Normalize(t);
                        originBlocks[blockIndex].rotation = Vector3.Lerp(ro, roEnd, norT);
                        originBlocks[blockIndex].scale = Vector3.Lerp(sc, scEnd, norT);
                        originBlocks[blockIndex].vertices[0].color = Color32.Lerp(co, coEnd, norT);
                        originBlocks[blockIndex].vertices[1].color = Color32.Lerp(co, coEnd, norT);
                        originBlocks[blockIndex].vertices[2].color = Color32.Lerp(co, coEnd, norT);
                        originBlocks[blockIndex].vertices[3].color = Color32.Lerp(co, coEnd, norT);
                    }
                }
                finally
                {
                    originBlocks[blockIndex].rotation = roEnd;
                    originBlocks[blockIndex].scale = scEnd;
                    originBlocks[blockIndex].vertices[0].color = coEnd;
                    originBlocks[blockIndex].vertices[1].color = coEnd;
                    originBlocks[blockIndex].vertices[2].color = coEnd;
                    originBlocks[blockIndex].vertices[3].color = coEnd;
                }
            }
            
            IEnumerable<List<(int x, int y)>> GetLayeredPositions(Vector2Int arrIndex, int cx, int cy)
            {
                var layers = new Dictionary<int, List<(int x, int y)>>();

                for (int x = 0; x < arrIndex.x; x++)
                {
                    for (int y = 0; y < arrIndex.y; y++)
                    {
                        int dist = Math.Abs(x - cx) + Math.Abs(y - cy); // Manhattan distance
                        if (!layers.ContainsKey(dist))
                            layers[dist] = new List<(int x, int y)>();
                        layers[dist].Add((x, y));
                    }
                }

                delayInterval = delay / (float)layers.Count;

                int maxDistance = layers.Keys.Max();
                for (int i = 0; i <= maxDistance; i++)
                {
                    if (layers.TryGetValue(i, out var layer))
                        yield return layer;
                }
            }
        }
        
        public void Transition(TransitionData data, CancellationTokenSource cts, Func<int, float, TransitionEase, CancellationToken, UniTask> UpdateBlockAsync)
        {
            
        }

        private async UniTask UpdateDrawVerticesAsync(float duration, CancellationToken token)
        {
            normalizedTime = 0f;
            float elapsed = 0;

            try
            {
                while (elapsed <= duration)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    elapsed += Time.deltaTime;
                    normalizedTime = Mathf.Clamp01(elapsed / duration);
                    SetVerticesDirty();
                }
            }
            finally
            {
                normalizedTime = 1f;
            }
        }
    }

    public partial class ImageTransition
    {
        // 회전 함수
        public static UIVertex[] UpdateBlockTransform(TransitionUIBlock block)
        {
            var sum = Vector3.zero;
            foreach (var vertex in block.vertices)
                sum += vertex.position;

            Vector3 center = sum / block.vertices.Length;
            Matrix4x4 matrix = Matrix4x4.TRS(block.position, Quaternion.Euler(block.rotation), block.scale);

            var list = new UIVertex[block.vertices.Length];
            for (var i = 0; i < block.vertices.Length; i++)
            {
                var temp = block.vertices[i];
                temp.position = matrix.MultiplyPoint3x4(temp.position - center) + center;
                list[i] = temp;
            }

            return list;
        }
    }
}
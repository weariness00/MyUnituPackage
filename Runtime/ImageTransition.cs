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
                // Matrix 변화 적용
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
        public BlockAnchor childAlignment; // 정렬 방식
        public Vector2Int grid = new Vector2Int(4, 4); // 그리드 크기
        [SerializeField] private TransitionEase ease;
        [SerializeField] private GridGroupMode gridGroupMode = GridGroupMode.Single; 

        [NonSerialized] public float normalizedTime = 1; // 트랜지션 진행 시간을 0~1사이값으로 정규화
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

        public ImageTransition Transition(TransitionStart startData, TransitionEnd endData, float duration, float delay)
        {
            return Transition(startData, endData, duration, delay, gameObject.GetCancellationTokenOnDestroy());
        }
        public ImageTransition Transition(TransitionStart startData, TransitionEnd endData, float duration, float delay, CancellationToken token)
        {
            Stop();
            cts = new();

            var delayInterval = delay / originBlocks.Length;
            var index = handler.GetIndexLength(grid);
            int i = 0;
            int startX = 0, startY = 0;
            switch (childAlignment)
            {
                // Upeer
                case BlockAnchor.UpperLeft:
                    startX = 0;
                    startY = index.y;
                    break;
                case BlockAnchor.UpperCenter:
                    startX = index.x / 2;
                    startY = index.y;
                    break;
                case BlockAnchor.UpperRight:
                    startX = index.x;
                    startY = index.y;
                    break;

                // Middle
                case BlockAnchor.MiddleLeft:
                    startX = 0;
                    startY = index.y / 2;
                    break;
                case BlockAnchor.MiddleCenter:
                    startX = index.x / 2;
                    startY = index.y / 2;
                    break;
                case BlockAnchor.MiddleRight:
                    startX = index.x;
                    startY = index.y / 2;
                    break;

                // Lower
                case BlockAnchor.LowerLeft:
                    startX = 0;
                    startY = 0;
                    break;
                case BlockAnchor.LowerCenter:
                    startX = index.x / 2;
                    startY = 0;
                    break;
                case BlockAnchor.LowerRight:
                    startX = index.x;
                    startY = 0;
                    break;
            }

            if(gridGroupMode != GridGroupMode.Single)
            {
                int layerCount = 0;
                foreach (var posList in handler.GetLayeredIndex(index, startX, startY))
                    layerCount++;
                delayInterval = delay / layerCount;
            }
            
            foreach (var posList in handler.GetLayeredIndex(index, startX, startY))
            {
                foreach (var (x, y) in posList)
                {
                    var blockIndex = handler.GetIndex(x, y);
                    if (blockIndex == -1) continue;
                    UpdateBlockAsync(blockIndex, i * delayInterval, token).Forget();
                    
                    if(gridGroupMode == GridGroupMode.Single)
                        i++;
                }
                if(gridGroupMode != GridGroupMode.Single)
                    i++;
            }
            
            UpdateDrawVerticesAsync(duration + delay, cts.Token).Forget();

            return this;

            async UniTask UpdateBlockAsync(int blockIndex, float realDelay, CancellationToken token)
            {
                // 초기값 직접 설정
                originBlocks[blockIndex].rotation = startData.rotateOffset;
                originBlocks[blockIndex].scale = startData.scaleOffset;
                originBlocks[blockIndex].vertices[0].color = startData.colorOffset;
                originBlocks[blockIndex].vertices[1].color = startData.colorOffset;
                originBlocks[blockIndex].vertices[2].color = startData.colorOffset;
                originBlocks[blockIndex].vertices[3].color = startData.colorOffset;

                if (realDelay > 0f)
                    await UniTask.WaitForSeconds(realDelay, cancellationToken: token);

                float t = 0f;
                try
                {
                    while (t <= duration)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                        t += Time.deltaTime;
                        var norT = ease.Normalize(t);
                        originBlocks[blockIndex].rotation = Vector3.Lerp(startData.rotateOffset, endData.rotateOffset, norT);
                        originBlocks[blockIndex].scale = Vector3.Lerp(startData.scaleOffset, endData.scaleOffset, norT);
                        originBlocks[blockIndex].vertices[0].color = Color32.Lerp(startData.colorOffset, endData.colorOffset, norT);
                        originBlocks[blockIndex].vertices[1].color = Color32.Lerp(startData.colorOffset, endData.colorOffset, norT);
                        originBlocks[blockIndex].vertices[2].color = Color32.Lerp(startData.colorOffset, endData.colorOffset, norT);
                        originBlocks[blockIndex].vertices[3].color = Color32.Lerp(startData.colorOffset, endData.colorOffset, norT);
                    }
                }
                finally
                {
                    originBlocks[blockIndex].rotation = endData.rotateOffset;
                    originBlocks[blockIndex].scale = endData.scaleOffset;
                    originBlocks[blockIndex].vertices[0].color = endData.colorOffset;
                    originBlocks[blockIndex].vertices[1].color = endData.colorOffset;
                    originBlocks[blockIndex].vertices[2].color = endData.colorOffset;
                    originBlocks[blockIndex].vertices[3].color = endData.colorOffset;
                }
            }
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
        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }
        
        // Matrix 변화 적용
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

        public ImageTransition SetEase(TransitionEase _ease)
        {
            ease = _ease;
            return this;
        }

        public ImageTransition SetGroup(ImageTransition.GridGroupMode groupMode)
        {
            this.gridGroupMode = groupMode;
            return this;
        }
    }
}
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Weariness.Transition
{
    public interface ITransitionHandler
    {
        public ImageTransition ImageTransition { get; set; }

        public int GetIndex(int x, int y);
        public void UpdateVert(out TransitionUIBlock[] originBlocks);
        public void Transition(TextAnchor childAlignment, float delay, TransitionEase ease, CancellationTokenSource cts, Func<int, float, TransitionEase, CancellationToken, UniTask> UpdateBlockAsync);
    }
}
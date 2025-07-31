using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Weariness.Transition
{
    public interface ITransitionHandler
    {
        public ImageTransition ImageTransition { get; set; }

        public Vector2Int GetIndexLength(Vector2Int grid);
        public int GetIndex(int x, int y);
        public void UpdateVert(out TransitionUIBlock[] originBlocks);
    }
}
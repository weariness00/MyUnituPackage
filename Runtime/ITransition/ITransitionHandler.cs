using System.Collections.Generic;
using UnityEngine;

namespace Weariness.UI.Transition
{
    public interface ITransitionHandler
    {
        public ImageTransition ImageTransition { get; set; }

        public Vector2Int GetIndexLength(Vector2Int grid);
        public int GetIndex(int x, int y);
        public void UpdateVert(out TransitionUIBlock[] originBlocks);
        public IEnumerable<List<(int x, int y)>> GetLayeredIndex(Vector2Int index, int x, int y);
    }
}
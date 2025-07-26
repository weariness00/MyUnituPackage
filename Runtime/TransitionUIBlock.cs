using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Transition
{
    public struct TransitionUIBlock
    {
        public UIVertex[] vertices;
        public int[] triangles;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
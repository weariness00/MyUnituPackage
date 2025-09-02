using System;
using UnityEngine;

namespace Weariness.Transition
{
    public partial class ImageTransition
    {
        public enum GridGroupMode
        {
            Single,        // 한개씩
            HorizontalLine,    // Width로 한줄씩
            VerticalLine,   // Heigth로 한줄씩
            AdjacentRegions, // 인접한 영역순으로
        }
        
        [Serializable]
        public struct TransitionStart
        {
            public Vector3 positionOffset;
            public Vector3 rotateOffset;
            public Vector3 scaleOffset;
            public Color32 colorOffset;

            public TransitionStart(Vector3 p, Vector3 r, Vector3 s,Color32 c)
            {
                positionOffset = p;
                rotateOffset = r;
                scaleOffset = s;
                colorOffset = c;
            }
        }
        
        [Serializable]
        public struct TransitionEnd
        {
            public Vector3 positionOffset;
            public Vector3 rotateOffset;
            public Vector3 scaleOffset;
            public Color32 colorOffset;
            
            public TransitionEnd(Vector3 p, Vector3 r, Vector3 s,Color32 c)
            {
                positionOffset = p;
                rotateOffset = r;
                scaleOffset = s;
                colorOffset = c;
            }
        }
        
        public enum BlockAnchor
        {
            UpperLeft,
            UpperCenter,
            UpperRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            LowerLeft,
            LowerCenter,
            LowerRight,
            Random,
        }
    }
}
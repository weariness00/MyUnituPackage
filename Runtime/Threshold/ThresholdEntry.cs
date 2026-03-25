using System;

namespace Project.Core
{
    [Serializable]
    public struct ThresholdEntry
    {
        public int   Index;      // 구간 번호 (0-based)
        public float Value;      // 임계값 (절댓값)
        public bool  IsCrossed;  // 이미 돌파했는지
    }
}

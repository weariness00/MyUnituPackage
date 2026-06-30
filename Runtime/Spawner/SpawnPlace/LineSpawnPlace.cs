using System;
using UnityEngine;
using Weariness.Util.Extensions;

namespace Util
{
    [Serializable]
    public class LineSpawnPlace : ISpawnPlace
    {
        public Vector3 firstPosition;
        public Vector3 lastPosition;

        public (Vector3 position, Quaternion rotation) GetSpawnPosition(Transform pivot)
        {
            var offset = Vector3Extension.Random(firstPosition, lastPosition);
            return (pivot.position + offset, Quaternion.identity);
        }

        public void DrawGizmos(Transform pivot)
        {
            Gizmos.color = Color.green;
            var start = pivot.position + firstPosition;
            var end = pivot.position + lastPosition;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, 0.15f);
            Gizmos.DrawWireSphere(end, 0.15f);
        }
    }
}

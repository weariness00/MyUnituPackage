using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Util
{
    [Serializable]
    public class CircleSpawnPlace : ISpawnPlace
    {
        public float radius = 1f;
        public bool edgeOnly;
        public SpawnAxis axis = SpawnAxis.XY;

        public (Vector3 position, Quaternion rotation) GetSpawnPosition(Transform pivot)
        {
            Vector2 point2D;
            if (edgeOnly)
            {
                var raw = Random.insideUnitCircle;
                if (raw == Vector2.zero) raw = Vector2.right;
                point2D = raw.normalized * radius;
            }
            else
            {
                point2D = Random.insideUnitCircle * radius;
            }

            var offset = axis.ToVector3(point2D);
            return (pivot.position + offset, Quaternion.identity);
        }

        public void DrawGizmos(Transform pivot)
        {
            Gizmos.color = edgeOnly ? Color.yellow : new Color(1f, 1f, 0f, 0.3f);
            var pos = pivot.position;

            int segments = 64;
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float a1 = Mathf.Deg2Rad * (angleStep * i);
                float a2 = Mathf.Deg2Rad * (angleStep * (i + 1));
                var p1 = pos + axis.ToVector3(new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius);
                var p2 = pos + axis.ToVector3(new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius);
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}

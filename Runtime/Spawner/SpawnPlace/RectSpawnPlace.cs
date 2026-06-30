using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Util
{
    [Serializable]
    public class RectSpawnPlace : ISpawnPlace
    {
        public Vector2 size = Vector2.one;
        public bool edgeOnly;
        public SpawnAxis axis = SpawnAxis.XY;

        public (Vector3 position, Quaternion rotation) GetSpawnPosition(Transform pivot)
        {
            Vector2 halfSize = size * 0.5f;
            Vector2 point2D;

            if (edgeOnly)
            {
                float w = size.x;
                float h = size.y;
                float perimeter = 2f * (w + h);
                float r = Random.Range(0f, perimeter);

                if (r < w)
                    point2D = new Vector2(r - halfSize.x, -halfSize.y);
                else if (r < w + h)
                    point2D = new Vector2(halfSize.x, (r - w) - halfSize.y);
                else if (r < 2f * w + h)
                    point2D = new Vector2(halfSize.x - (r - w - h), halfSize.y);
                else
                    point2D = new Vector2(-halfSize.x, halfSize.y - (r - 2f * w - h));
            }
            else
            {
                point2D = new Vector2(
                    Random.Range(-halfSize.x, halfSize.x),
                    Random.Range(-halfSize.y, halfSize.y)
                );
            }

            var offset = axis.ToVector3(point2D);
            return (pivot.position + offset, Quaternion.identity);
        }

        public void DrawGizmos(Transform pivot)
        {
            Gizmos.color = edgeOnly ? Color.magenta : new Color(1f, 0f, 1f, 0.3f);
            var pos = pivot.position;
            var halfSize = size * 0.5f;

            var p0 = pos + axis.ToVector3(new Vector2(-halfSize.x, -halfSize.y));
            var p1 = pos + axis.ToVector3(new Vector2(halfSize.x, -halfSize.y));
            var p2 = pos + axis.ToVector3(new Vector2(halfSize.x, halfSize.y));
            var p3 = pos + axis.ToVector3(new Vector2(-halfSize.x, halfSize.y));

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }
    }
}

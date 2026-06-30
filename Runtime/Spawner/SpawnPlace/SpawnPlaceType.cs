using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public enum SpawnPlaceType
    {
        Transform,
        Line,
        Circle,
        Rect,
    }

    [Serializable]
    public enum SpawnAxis
    {
        XY,
        XZ,
        YZ,
    }

    public static class SpawnAxisExtensions
    {
        public static Vector3 ToVector3(this SpawnAxis axis, Vector2 point)
        {
            return axis switch
            {
                SpawnAxis.XY => new Vector3(point.x, point.y, 0f),
                SpawnAxis.XZ => new Vector3(point.x, 0f, point.y),
                SpawnAxis.YZ => new Vector3(0f, point.x, point.y),
                _ => new Vector3(point.x, point.y, 0f),
            };
        }
    }
}

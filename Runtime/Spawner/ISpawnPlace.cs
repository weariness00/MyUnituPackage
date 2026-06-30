using UnityEngine;

namespace Util
{
    public interface ISpawnPlace
    {
        (Vector3 position, Quaternion rotation) GetSpawnPosition(Transform pivot);
        void DrawGizmos(Transform pivot);
    }
}

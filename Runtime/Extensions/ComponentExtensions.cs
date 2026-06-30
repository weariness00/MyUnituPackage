using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent(out T component))
                return component;
            component = go.AddComponent<T>();
            return component;
        }
    }
}
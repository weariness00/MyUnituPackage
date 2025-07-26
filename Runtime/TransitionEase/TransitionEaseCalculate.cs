using UnityEngine;

namespace Weariness.Transition
{
    public static class TransitionEaseCalculate
    {
        public static float Normalize(this TransitionEase ease, float t)
        {
            switch (ease)
            {
                case TransitionEase.OutExpo:
                    return t >= 1f ? 1f : 1 - Mathf.Pow(2, -10 * t);
            }

            return t;
        }
    }
}
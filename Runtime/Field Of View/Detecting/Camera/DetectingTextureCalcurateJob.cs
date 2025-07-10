
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Weariness.Noesis.FieldOfView
{
    public struct DetectingTextureCalcurateJob : IJob
    {
        public NativeArray<Vector4> textureColorValues;
        public NativeArray<float> sum; 

        public void Execute()
        {
            foreach (var color in textureColorValues)
            {
                sum[0] += color.x;
            }
        }
    }
}

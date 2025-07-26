using UnityEngine;

namespace Weariness.Transition
{
    public static class UIVertexExtension
    {
        public static UIVertex MakeUIVertex(Vector2 pos, Vector2 uv, Color color)
        {
            // UIVertex 생성
            var vert = UIVertex.simpleVert;
            vert.position = pos;
            vert.uv0 = uv;
            vert.color = color;
            return vert;
        }
    }
}
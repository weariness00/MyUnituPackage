Shader "FMOD/Occlusion/OcclusionMaskUnlit"
{
    Properties
    {
        _Occlusion("Occlusion Strength", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 50

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Occlusion;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 단순 R=방음력, G/B=0, A=1
                return fixed4(_Occlusion, 0, 0, 1);
            }
            ENDCG
        }
    }
}

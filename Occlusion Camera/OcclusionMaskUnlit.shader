Shader "FMOD/Occlusion/OcclusionMaskUnlit"
{
    Properties
    {
        _ListenerPos("Listener Pos", Vector) = (0,0,0,0)
        _SoundPos("Sound Pos", Vector) = (0,0,0,0)
        _Occlusion("Occlusion Strength", Range(0,1)) = 1
        _Is_Sound("Is Sound", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 50

        Pass
        {
            Blend One One // 색상값끼리 합산
            ZWrite Off    // Z버퍼 기록 비활성화
            ZTest Always  // 항상 렌더(거리 무시)
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _Occlusion_Enable
            #include "UnityCG.cginc"

            float _Occlusion;
            float _Is_Sound;

            struct appdata { float4 vertex : POSITION; };
struct v2f {
    float4 pos : SV_POSITION;
    float3 worldPos : TEXCOORD1;
};

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // 월드 공간 변환
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef _Occlusion_Enable
                // 단순 R=방음력, G/B=0, A=1
                return fixed4(_Occlusion, _Is_Sound, 0, 0);
                
                #else
                return fixed4(0,0,0,0);
                #endif

            }
            ENDCG
        }
    }
}

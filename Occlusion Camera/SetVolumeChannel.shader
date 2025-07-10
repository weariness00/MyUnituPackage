Shader "Hidden/SetVoloumeChannel"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_Volume("Volume", Range(0, 1)) = 1
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc" // ✅ 꼭 포함해야 함

			sampler2D _MainTex;
			float _Volume;

			fixed4 frag(v2f_img i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.uv);
				c.g = _Volume;
				return c;
			}
			ENDCG
		}
	}
}

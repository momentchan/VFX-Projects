Shader "Unlit/PostProcess"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_ST;

	fixed4 frag(v2f_img i) : SV_Target
	{
		float s = (tex2D(_MainTex, i.uv).r - 0.5) * 32;
		return 1 / (1 + exp(-s));
	}
	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}

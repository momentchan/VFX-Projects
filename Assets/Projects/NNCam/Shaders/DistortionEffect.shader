Shader "NNCam/DistortionEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	CGINCLUDE

	#include "UnityCG.cginc"
	#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

	sampler2D _MainTex;
	sampler2D _FeedbackTex;
	sampler2D _MaskTex;

	float2 _Feedback; // length, decay
	float3 _Noise; // freq, speed, amount

	float2 DFNoise(float2 uv, float3 freq)
	{
		float3 np = float3(uv, _Time.y) * freq;
		float2 n1 = snoise_grad(np).xy;
		return cross(float3(n1, 0), float3(0, 0, 1)).xy;
	}

	float2 Displacement(float2 uv)
	{
		float aspect = _ScreenParams.x / _ScreenParams.y;
		float2 p = uv * float2(aspect, 1);
		float2 n = DFNoise(p, _Noise.xxy * -1) * _Noise.z +
			DFNoise(p, _Noise.xxy * +2) * _Noise.z * 0.5;
		return n * float2(1, aspect);
	}

	float2 Mirror(float2 uv) {
		uv.x -= 0.5f;
		uv.x *= -1;
		uv.x += 0.5f;
		return uv;
	}

	float4 frag(v2f_img i) : SV_Target
	{
		float2 uv =  Mirror(i.uv);
		float3 camera = tex2D(_MainTex, uv); 
		float4 feedback = tex2D(_FeedbackTex, i.uv+ Displacement(i.uv));
		float mask = smoothstep(0.9, 1, dot(tex2D(_MaskTex, uv), float4(1,1,1,1)));
		float alpha = lerp(feedback.a * (1 - _Feedback.y), _Feedback.x, mask);
		float3 rgb = lerp(camera, feedback.rgb, saturate(alpha) * (1 - mask));

		return float4(rgb, alpha);
	}
	ENDCG

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}

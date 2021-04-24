Shader "Common/Fractal" {
	Properties{
		_TimeScale("Time scale", Float) = 1
		_FracScale("Fractal Scale", Vector) = (1, 1, 1, 1)
		_Velocity("Velocity XY", Vector) = (-0.2, -0.2, 1, 1)
		_MaxStrength("Max Sstrength", Float) = 1
		_Threshold("Threshold", Float) = 1
	}
	CGINCLUDE
	#include "../ShaderUtil/Noise.cginc"
	#include "UnityCG.cginc"

	float3 _FracScale;
	float _TimeScale;
	float3 _Velocity;
	float _MaxStrength;
	float _Threshold;

	float CalculateFractalFast(float2 uv)
	{
		float2 uvFractal = uv * _FracScale.xy;
		uvFractal.y *= 2;
		float t = _TimeScale * _Time.y;
		float3 p = (float3(uvFractal, 0) + t * float3(-1.0 * _Velocity.x, -1.0 * _Velocity.y, -1.0 * _Velocity.z));

		float3 q = p;
		float f;
		f = 0.50000 * noise(q); q = q * 2.02;
		f += 0.25000 * noise(q); q = q * 2.03;
		f += 0.12500 * noise(q); q = q * 2.01;
		f += 0.06250 * noise(q); q = q * 2.02;
		f += 0.03125 * noise(q);
		f = pow(saturate((f - _Threshold)) * _MaxStrength, _FracScale.z);
		return saturate(f);
	}

	half4 frag(v2f_img i) : SV_Target {

		return CalculateFractalFast(i.uv);
	}
	ENDCG

	SubShader {
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		pass {
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl
			ENDCG
		}
	}
}
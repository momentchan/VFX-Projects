Shader "Hidden/BlazePoseVisualizer"
{
	CGINCLUDE
	#include "UnityCG.cginc"

	StructuredBuffer<float4> _KeyPoints;
	StructuredBuffer<float4> _WorldKeyPoints;

	float _HumanExistThreshold;
	uint _KeypointCount;
	float2 _UiScale;

	float2 _LinePair[35];

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
	};

	v2f VertLine(uint vid : SV_VertexID, uint iid : SV_InstanceID)
	{
		uint2 index = _LinePair[iid];
		float4 p1 = _KeyPoints[index.x];
		float4 p2 = _KeyPoints[index.y];

		float2 dir = normalize((p1 - p2).xy);
		float2 orth = float2(dir.y, -dir.x);

		const float size = 0.005f;

		float2 p = lerp(p1, p2, vid & 1);
		p.xy += orth * size * lerp(-0.5, 0.5, vid < 2 || vid == 5);         //   2 4-3
		p = (2 * p - 1) * _UiScale.x / _ScreenParams.xy;

		float score = lerp(p1.w, p2.w, vid & 1);
		float humanExist = _KeyPoints[_KeypointCount].x;

		v2f o;
		o.vertex = float4(p, 0, 1);
		o.color = humanExist >= _HumanExistThreshold ? float4(0, 1, 0, score) : float4(0, 0, 1, score);
		return o;
	}

	v2f VertPoint(uint vid : SV_VertexID, uint iid : SV_InstanceID)
	{
		float4 p = _KeyPoints[iid];

		const float size = 0.01f;

		float x = p.x + size * lerp(-0.5, 0.5, vid & 1);                     //   0-1 5
		float y = p.y + size * lerp(-0.5, 0.5, vid < 2 || vid == 5);         //   2 4-3

		x = (2 * x - 1) * _UiScale.x / _ScreenParams.x;
		y = (2 * y - 1) * _UiScale.y / _ScreenParams.y;

		float score = p.w;

		v2f o;
		o.vertex = float4(x, y, 0, 1);
		o.color = float4(1, 0, 0, score);
		return o;
	}

	v2f VertLine3D(uint vid : SV_VertexID, uint iid : SV_InstanceID)
	{
		uint2 index = _LinePair[iid];
		float4 p1 = _WorldKeyPoints[index.x];
		float4 p2 = _WorldKeyPoints[index.y];

		float3 camPos = _WorldSpaceCameraPos;
		float3 dir = p2.xyz - p1.xyz;
		float3 toCam = normalize(camPos - p1);
		float3 sideDir = normalize(cross(toCam, dir));

		float len = length(dir);

		const float size = 0.01f;

		float3 p = lerp(p1, p2, vid & 1);
		p += sideDir * size * lerp(-0.5, 0.5, vid < 2 || vid == 5);

		float score = lerp(p1.w, p2.w, vid & 1);
		float humanExist = _WorldKeyPoints[_KeypointCount].x;

		v2f o;
		o.vertex = UnityWorldToClipPos(p);
		o.color = humanExist >= _HumanExistThreshold ? float4(0, 1, 0, score) : float4(0, 0, 1, score);
		return o;
	}

	v2f VertPoint3D(uint vid : SV_VertexID, uint iid : SV_InstanceID)
	{
		float3 p = _WorldKeyPoints[iid].xyz;
		float score = _WorldKeyPoints[iid].w;

		const float size = 0.03f;

		float x = size * lerp(-0.5, 0.5, vid & 1);                     //   0-1 5
		float y = size * lerp(-0.5, 0.5, vid < 2 || vid == 5);         //   2 4-3
		p.xyz += mul(unity_CameraToWorld, float3(x, y, 0));

		v2f o;
		o.vertex = UnityWorldToClipPos(float4(p, 1));
		o.color = float4(1, 0, 0, score);
		return o;
	}


	fixed4 Fragment(v2f i) : SV_Target{
		return i.color;
	}
		ENDCG

		SubShader
	{
		ZWrite Off ZTest Always Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
		{
			CGPROGRAM
			#pragma vertex VertLine
			#pragma fragment Fragment
			ENDCG
		}

			Pass
		{
			CGPROGRAM
			#pragma vertex VertPoint
			#pragma fragment Fragment
			ENDCG
		}

			Pass
		{
			CGPROGRAM
			#pragma vertex VertLine3D
			#pragma fragment Fragment
			ENDCG
		}

			Pass
		{
			CGPROGRAM
			#pragma vertex VertPoint3D
			#pragma fragment Fragment
			ENDCG
		}
	}
}

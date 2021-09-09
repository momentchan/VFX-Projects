Shader "Hidden/PoseLandMarkVisualizer"
{
	CGINCLUDE
	#include "UnityCG.cginc"

	StructuredBuffer<float4> _KeyPoints;
	float2 _UiScale;

	struct v2f {
		float4 vertex : SV_POSITION;
		float4 color :COLOR;
	};

	v2f vert(uint vid : SV_VertexID, uint iid : SV_InstanceID)
	{
		float4 p = _KeyPoints[iid];

		const float size = 0.02f;
																	//   3
		float x = p.x + size * lerp(-1, 1, vid == 1) * (vid < 2);	//0 --- 1
		float y = p.y + size * lerp(-1, 1, vid == 3) * (vid >= 2);	//   2

		x = (2 * x - 1) * _UiScale.x / _ScreenParams.x;
		y = (2 * y - 1) * _UiScale.y / _ScreenParams.y;

		float score = p.w;

 		v2f o;
		o.vertex = float4(x, y, 0, 1);
		o.color = float4(1, 0, 0, score);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
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
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}

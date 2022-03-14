Shader "Unlit/MozaicEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Ratio("Ratio", float) = 1
        _Segments("Segments", int) = 1
        _Border("Border", range(0,1)) = 0
        _Range("Range", Vector) = (0, 1, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderQueue" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Segments;
            float _Ratio;
            float _Border;
            float4 _Range;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 segments = max(_Segments, 1) * float2(1, _Ratio);
                float2 uv = i.uv * segments;
                float2 index = (floor(uv) + 0.5) / segments;
                float2 f = frac(uv);
                fixed4 col = tex2D(_MainTex, index);

                float border = saturate(step(1 - _Border, f.x) + step(f.x,  _Border) + step(1 - _Border, f.y) + step(f.y, _Border));
                col = lerp(col, 1, border);
                col.a *= step(_Range.x, i.uv.x) * step(i.uv.x, _Range.y) * step(_Range.z, i.uv.y) * step(i.uv.y, _Range.w);
                return col;
            }
            ENDCG
        }
    }
}

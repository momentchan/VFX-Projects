Shader "TestSolver"
{
    Properties{
        _MaskTex("MaskTex", 2D) = "white" {}
        _SoverTex("SolverTex", 2D) = "white" {}
        _GradTex("GradTex", 2D) = "white" {}
        _PosDir("PosDirection", Vector) = (0,0,0,0)
    }
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 SampleCustomColor(float2 uv);
    // float4 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    sampler2D _GradTex;
    sampler2D _SolverTex;
    sampler2D _MaskTex;
    float4 _SolverTex_ST;
    float4 _PosDir;
    float _Show;
    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);

        float2 uv = TRANSFORM_TEX(posInput.positionNDC.xy, _SolverTex);

        float mask = length(tex2D(_MaskTex, uv));
        float3 solver = tex2D(_SolverTex, uv);// *mask;
        float3 color = solver;// tex2D(_GradTex, float2(saturate(1 - solver.z * 5), 0.5));

        float w = 0.001;

        float2 dir = normalize(_PosDir.zw);
        float2 p0 = _PosDir.xy;
        float2 p1 = p0 + dir * 0.05;
        float m = -dir.y / dir.x;
        float c = -p0.y - m * p0.x;

        float d = abs(m * uv.x + uv.y + c);
        float l = step(d, w) * step(min(p0.y, p1.y), uv.y) * step(uv.y, max(p0.y, p1.y));
        float p = step(abs(uv.x - _PosDir.x), 0.01) * step(abs(uv.y - _PosDir.y), 0.01);
        l += p;

        return float4(color,1) + l * _Show;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}

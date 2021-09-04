Shader "Hidden/PoseDetectionVisualizer"
{
    CGINCLUDE
    #include "UnityCG.cginc"
    #include "PoseData.cginc"

    #define PI 3.14159265359

    uint _UpperBodyOnly;
    StructuredBuffer<PoseData> _Detections;

    float2x2 rot2D(float angle) {
        return float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
    }

    float4 VertexFaceBox(uint vid : SV_VertexID, uint iid : SV_InstanceID) : SV_POSITION {
        PoseData pd = _Detections[iid];
        
        // Bounding box
        float x = pd.center.x + pd.extent.x * lerp(-0.5, 0.5, vid & 1);                     //   0-1 5
        float y = pd.center.y + pd.extent.y * lerp(-0.5, 0.5, vid < 2 || vid == 5);         //   2 4-3
        y = 1.0 - y;

        // Clip space to screen space (0 , 1) -> (-1, 1)
        x = (2 * x - 1) * _ScreenParams.y / _ScreenParams.x;
        y = 2 * y - 1;

        return float4(x, y, 0, 1);
    }

    float4 FragmentFaceBox(float4 position : SV_Position) : SV_Target {
        return float4(1, 0, 0, 0.5);
    }

    float4 VertexBodyBox(uint vid : SV_VertexID, uint iid : SV_InstanceID) : SV_POSITION {

        PoseData pd = _Detections[iid];
        float2 hip = pd.keyPoints[0];
        float2 shoulder = pd.keyPoints[2];

        float2 center = _UpperBodyOnly ? shoulder : hip;
        float2 roi = _UpperBodyOnly ? pd.keyPoints[3] : pd.keyPoints[1];

        float sizeX = abs(roi.x - center.x);
        float sizeY = abs(roi.y - center.y);
        float size = max(sizeX, sizeY) ;

        float dx = size * lerp(-0.5, 0.5, vid & 1);                     //   0-1 5
        float dy = size * lerp(-0.5, 0.5, vid < 2 || vid == 5);         //   2 4-3

        float target = PI * 0.5f;
        float angle = target - atan2(-(shoulder.y - hip.y), shoulder.x - hip.x);
        angle = angle - 2 * PI * floor((angle + PI) / (2 * PI));

        float2 rotPos = mul(rot2D(angle), float2(dx, dy));
        float x = center.x + rotPos.x;
        float y = center.y + rotPos.y;
        y = 1.0 - y;

        x = (2 * x - 1) * _ScreenParams.y / _ScreenParams.x;
        y = 2 * y - 1;

        return float4(x, y, 0, 1);
    }

    float4 FragmentBodyBox(float4 position : SV_Position) : SV_Target
    {
        return float4(0, 1, 0, 0.5);
    }

    float4 VertexBodyLine(uint vid : SV_VertexID, uint iid : SV_InstanceID) : SV_POSITION {

        PoseData pd = _Detections[iid];
        float2 hip = pd.keyPoints[0];
        float2 shoulder = pd.keyPoints[2];

        float x = lerp(hip.x, shoulder.x, vid);
        float y = lerp(hip.y, shoulder.y, vid);
        y = 1.0 - y;

        x = (2 * x - 1) * _ScreenParams.y / _ScreenParams.x;
        y = 2 * y - 1;

        return float4(x, y, 0, 1);
    }

    float4 FragmentBodyLine(float4 position : SV_Position) : SV_Target
    {
        return float4(0, 0, 1, 0.5);
    }


    ENDCG



    SubShader
    {
        ZWrite Off ZTest Always Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexFaceBox
            #pragma fragment FragmentFaceBox
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexBodyBox
            #pragma fragment FragmentBodyBox
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexBodyLine
            #pragma fragment FragmentBodyLine
            ENDCG
        }
    }
}

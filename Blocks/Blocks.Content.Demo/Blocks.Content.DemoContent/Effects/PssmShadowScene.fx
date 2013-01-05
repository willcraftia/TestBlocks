#include "Common.fxh"
#include "Shadow.fxh"

//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;

float DepthBias;

#define MAX_SPLIT_COUNT 3

int SplitCount = MAX_SPLIT_COUNT;
float SplitDistances[MAX_SPLIT_COUNT + 1];
float4x4 SplitLightViewProjections[MAX_SPLIT_COUNT];

texture ShadowMap0;
#if MAX_SPLIT_COUNT > 1
texture ShadowMap1;
#endif
#if MAX_SPLIT_COUNT > 2
texture ShadowMap2;
#endif

sampler ShadowMapSampler[MAX_SPLIT_COUNT] =
{
    sampler_state
    {
        Texture = <ShadowMap0>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#if MAX_SPLIT_COUNT > 1
    sampler_state
    {
        Texture = <ShadowMap1>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SPLIT_COUNT > 2
    sampler_state
    {
        Texture = <ShadowMap2>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
};

//-----------------------------------------------------------------------------
// PCF specifiec

float TapCount;
float2 Offsets[MAX_PCF_TAP_COUNT];

//=============================================================================
// Structures
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION;
};

struct VSOutput
{
    float4 Position                             : POSITION;
    float4 ViewPosition                         : TEXCOORD0;
    float4 LightingPosition[MAX_SPLIT_COUNT]    : TEXCOORD1;
};

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.ViewPosition = viewPosition;

    for (int i = 0; i < SplitCount; i++)
    {
        output.LightingPosition[i] = mul(worldPosition, SplitLightViewProjections[i]);
    }

    return output;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 ClassicPS(VSOutput input) : COLOR0
{
    float distance = abs(input.ViewPosition.z);

    float splitIndex = -1;
    float shadow = 1;
    for (int i = 0; i < SplitCount; i++)
    {
        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 lightingPosition = input.LightingPosition[i];
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestClassicShadowMap(ShadowMapSampler[i], shadowTexCoord, lightingPosition, DepthBias);
            splitIndex = i;
        }
    }

    float r = shadow;
    float g = (splitIndex == 1) ? shadow : 0;
    float b = (splitIndex == 2) ? shadow : 0;
    return float4(r, g, b, 1);
}

/*
float4 PcfPS(VSOutput input) : COLOR
{
    float distance = abs(input.ViewPosition.z);

    float splitIndex = -1;
    float shadow = 1;
    for (int i = 0; i < SplitCount; i++)
    {
        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 lightingPosition = input.LightingPosition[i];
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestPcfShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                DepthBias,
                TapCount,
                Offsets);
            splitIndex = i;
        }
    }

    float r = shadow;
    float g = (splitIndex == 1) ? shadow : 0;
    float b = (splitIndex == 2) ? shadow : 0;
    return float4(r, g, b, 1);
}
*/

// TODO
float4 PcfPS(VSOutput input) : COLOR
{
    return float4(1, 1, 1, 1);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    float distance = abs(input.ViewPosition.z);

    float splitIndex = -1;
    float shadow = 1;

    for (int i = 0; i < SplitCount; i++)
    {
        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 lightingPosition = input.LightingPosition[i];
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestVarianceShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                DepthBias);
            splitIndex = i;
        }
    }

    float r = shadow;
    float g = (splitIndex == 1) ? shadow : 0;
    float b = (splitIndex == 2) ? shadow : 0;
    return float4(r, g, b, 1);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Classic
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 ClassicPS();
    }
}

technique Pcf
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PcfPS();
    }
}

technique Vsm
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 VsmPS();
    }
}

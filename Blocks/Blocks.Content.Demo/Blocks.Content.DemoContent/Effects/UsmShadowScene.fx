#include "Common.fxh"
#include "Shadow.fxh"

//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 LightViewProjection;

float DepthBias;

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
// TODO: ê¸å`ëŒâûÅB
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
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
    float4 Position         : POSITION;
    float4 LightingPosition : TEXCOORD0;
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
    output.LightingPosition = mul(worldPosition, LightViewProjection);

    return output;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 ClassicPS(VSOutput input) : COLOR0
{
    float4 lightingPosition = input.LightingPosition;

    float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
    float shadow = TestClassicShadowMap(
        ShadowMapSampler,
        shadowTexCoord,
        lightingPosition,
        DepthBias);

    return float4(shadow, shadow, shadow, 1);
}

float4 TODO_PcfPS(VSOutput input) : COLOR
{
    float4 lightingPosition = input.LightingPosition;
    float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);

    float shadow = TestPcfShadowMap(ShadowMapSampler, shadowTexCoord, lightingPosition, DepthBias, TapCount, Offsets);
    return float4(shadow, shadow, shadow, 1);
}

// TODO
float4 PcfPS(VSOutput input) : COLOR
{
    return float4(1, 1, 1, 1);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    float4 lightingPosition = input.LightingPosition;
    float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);

    float4 result = float4(1, 0, 0, 1);
    float shadow = TestVarianceShadowMap(ShadowMapSampler, shadowTexCoord, lightingPosition, DepthBias);
    return float4(shadow, shadow, shadow, 1);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Classic
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 ClassicPS();
    }
}

technique Pcf
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PcfPS();
    }
}

technique Vsm
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VsmPS();
    }
}

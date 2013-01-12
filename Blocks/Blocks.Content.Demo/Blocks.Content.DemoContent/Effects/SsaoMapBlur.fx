#include "Common.fxh"
#include "SpriteBatch3.fxh"

//=============================================================================
//
// 定数
//
//-----------------------------------------------------------------------------
#define MAX_RADIUS 4
#define KERNEL_SIZE (MAX_RADIUS * 2 + 1)

float Sigma2 = 25;
float KernelSize = KERNEL_SIZE;
float Weights[KERNEL_SIZE];
float2 OffsetsH[KERNEL_SIZE];
float2 OffsetsV[KERNEL_SIZE];

texture ColorMap;
sampler2D ColorMapSampler : register(s0) = sampler_state
{
    Texture = <ColorMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

texture NormalDepthMap;
sampler2D NormalDepthMapSampler = sampler_state
{
    Texture = <NormalDepthMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float SampleColor(float3 centerNormal, float centerDepth, float2 sampleTexCoord, float baseWeight, inout float totalWeight)
{
    //------------------------------------------------------------------------
    // サンプル位置での法線と深度を取得

    float4 sampleNormalDepth = tex2D(NormalDepthMapSampler, sampleTexCoord);
    float3 sampleNormal = DecodeNormal(sampleNormalDepth.xyz);
    float sampleDepth = sampleNormalDepth.w;

    //------------------------------------------------------------------------
    // 深度差についての重み付けの度合い

    float deltaDepth = abs(centerDepth - sampleDepth);
    float w = exp(- deltaDepth * deltaDepth / 2 * Sigma2 * Sigma2);

    //------------------------------------------------------------------------
    // 重みの決定

    float weight = baseWeight * w;

    // 重みの和を記録
    totalWeight += weight;

    //------------------------------------------------------------------------
    // 重み付けされた色

    float color = tex2D(ColorMapSampler, sampleTexCoord).x;
    return color * weight;
}

float4 HorizontalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    float3 normal = DecodeNormal(normalDepth.xyz);
    float depth = normalDepth.w;

    float4 totalColor = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsH[i];
        totalColor += SampleColor(normal, depth, sampleTexCoord, Weights[i], totalWeight);
    }

    return totalColor / totalWeight;
}

float4 VerticalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    float3 normal = DecodeNormal(normalDepth.xyz);
    float depth = normalDepth.w;

    float4 totalColor = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsV[i];
        totalColor += SampleColor(normal, depth, sampleTexCoord, Weights[i], totalWeight);
    }

    return totalColor / totalWeight;
}

//=============================================================================
//
// テクニック
//
//-----------------------------------------------------------------------------
technique HorizontalBlur
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 HorizontalBlurPS();
    }
}

technique VerticalBlur
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 VerticalBlurPS();
    }
}

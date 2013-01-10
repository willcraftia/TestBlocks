#include "Common.fxh"

//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
#define MAX_RADIUS 4
#define KERNEL_SIZE (MAX_RADIUS * 2 + 1)

float2 HalfPixel;

float KernelSize = KERNEL_SIZE;
float Weights[KERNEL_SIZE];
float2 OffsetsH[KERNEL_SIZE];
float2 OffsetsV[KERNEL_SIZE];

texture ColorMap;
sampler2D ColorMapSampler = sampler_state
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
// 構造体宣言
//
//-----------------------------------------------------------------------------
struct NormalDepth
{
    float3 Normal;
    float Depth;
};

//=============================================================================
//
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
void VS(inout float4 position : POSITION0, inout float2 texCoord : TEXCOORD0)
{
    position.w = 1;
    texCoord -= HalfPixel;
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
NormalDepth GetNormalDepth(float2 texCoord)
{
    NormalDepth output;

    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    output.Normal = DecodeNormal(normalDepth.xyz);
    output.Depth = normalDepth.w;

    return output;
}

float SampleColor(NormalDepth center, float2 sampleTexCoord, float baseWeight, inout float totalWeight)
{
    //------------------------------------------------------------------------
    // サンプル位置での法線と深度を取得

    NormalDepth sample = GetNormalDepth(sampleTexCoord);

    //------------------------------------------------------------------------
    // 深度差についての重み付けの度合い

    // 深度差が小さい程、影響が大きくなるようにする。
    float deltaDepth = abs(center.Depth - sample.Depth);
    float depthCoeff = (1 - saturate(deltaDepth));
    depthCoeff *= depthCoeff;
    depthCoeff = 1;

    //------------------------------------------------------------------------
    // 法線のなす角についての重み付けの度合い

    // なす角が平行に近い程、影響が大きくなるようにする。
    float normalCoeff = abs(dot(center.Normal, sample.Normal));
    normalCoeff *= normalCoeff;
    normalCoeff = 1;

    //------------------------------------------------------------------------
    // 重みの決定

    float weight = baseWeight * depthCoeff * normalCoeff;

    // 重みの和を記録
    totalWeight += weight;

    //------------------------------------------------------------------------
    // 重み付けされた色

    float color = tex2D(ColorMapSampler, sampleTexCoord).x;
    return color * weight;
}

float4 HorizontalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    NormalDepth center = GetNormalDepth(texCoord);

    float4 totalColor = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsH[i];
        totalColor += SampleColor(center, sampleTexCoord, Weights[i], totalWeight);
    }

    return totalColor / totalWeight;
}

float4 VerticalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    NormalDepth center = GetNormalDepth(texCoord);

    float4 totalColor = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsV[i];
        totalColor += SampleColor(center, sampleTexCoord, Weights[i], totalWeight);
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

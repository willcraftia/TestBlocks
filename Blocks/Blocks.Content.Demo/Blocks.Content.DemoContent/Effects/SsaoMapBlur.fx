//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
#define MAX_RADIUS 2
#define KERNEL_SIZE (MAX_RADIUS * 2 + 1)

float KernelSize = KERNEL_SIZE;
float Weights[KERNEL_SIZE];
float2 OffsetsH[KERNEL_SIZE];
float2 OffsetsV[KERNEL_SIZE];

texture SsaoMap;
sampler2D SsaoMapSampler : register(s0) = sampler_state
{
    Texture = <SsaoMap>;
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
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
void VS(inout float4 position : POSITION0, inout float2 texCoord : TEXCOORD0)
{
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float3 DecodeNormal(float3 encodedNormal)
{
    return encodedNormal * 2 - 1;
}

struct NormalDepth
{
    float3 Normal;
    float Depth;
};

NormalDepth GetNormalDepth(float2 texCoord)
{
    NormalDepth output;

    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    output.Normal = normalize(DecodeNormal(normalDepth.xyz));
    output.Depth = normalDepth.w;

    return output;
}

float SampleAo(NormalDepth center, float2 sampleTexCoord, float baseWeight, inout float totalWeight)
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

    //------------------------------------------------------------------------
    // 法線のなす角についての重み付けの度合い

    // なす角が平行に近い程、影響が大きくなるようにする。
    float normalCoeff = abs(dot(center.Normal, sample.Normal));
    normalCoeff *= normalCoeff;

    //------------------------------------------------------------------------
    // 重みの決定

    float weight = baseWeight * depthCoeff * normalCoeff;

    // 重みの和を記録
    totalWeight += weight;

    //------------------------------------------------------------------------
    // 重み付けされた AO 値

    float sampleAo = tex2D(SsaoMapSampler, sampleTexCoord).x;
    return sampleAo * weight;
}

float4 HorizontalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    NormalDepth center = GetNormalDepth(texCoord);

    float4 totalAo = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsH[i];
        totalAo += SampleAo(center, sampleTexCoord, Weights[i], totalWeight);
    }

    return totalAo / totalWeight;
}

float4 VerticalBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    NormalDepth center = GetNormalDepth(texCoord);

    float4 totalAo = 0;
    float totalWeight = 0;

    for (int i = 0; i < KernelSize; i++)
    {
        float2 sampleTexCoord = texCoord + OffsetsV[i];
        totalAo += SampleAo(center, sampleTexCoord, Weights[i], totalWeight);
    }

    return totalAo / totalWeight;
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
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 HorizontalBlurPS();
    }
}

technique VerticalBlur
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 VerticalBlurPS();
    }
}

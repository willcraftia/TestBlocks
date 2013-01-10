#include "Common.fxh"

//=============================================================================
//
// �ϐ��錾
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
// �\���̐錾
//
//-----------------------------------------------------------------------------
struct NormalDepth
{
    float3 Normal;
    float Depth;
};

//=============================================================================
//
// ���_�V�F�[�_
//
//-----------------------------------------------------------------------------
void VS(inout float4 position : POSITION0, inout float2 texCoord : TEXCOORD0)
{
    position.w = 1;
    texCoord -= HalfPixel;
}

//=============================================================================
//
// �s�N�Z�� �V�F�[�_
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
    // �T���v���ʒu�ł̖@���Ɛ[�x���擾

    NormalDepth sample = GetNormalDepth(sampleTexCoord);

    //------------------------------------------------------------------------
    // �[�x���ɂ��Ă̏d�ݕt���̓x����

    // �[�x�������������A�e�����傫���Ȃ�悤�ɂ���B
    float deltaDepth = abs(center.Depth - sample.Depth);
    float depthCoeff = (1 - saturate(deltaDepth));
    depthCoeff *= depthCoeff;
    depthCoeff = 1;

    //------------------------------------------------------------------------
    // �@���̂Ȃ��p�ɂ��Ă̏d�ݕt���̓x����

    // �Ȃ��p�����s�ɋ߂����A�e�����傫���Ȃ�悤�ɂ���B
    float normalCoeff = abs(dot(center.Normal, sample.Normal));
    normalCoeff *= normalCoeff;
    normalCoeff = 1;

    //------------------------------------------------------------------------
    // �d�݂̌���

    float weight = baseWeight * depthCoeff * normalCoeff;

    // �d�݂̘a���L�^
    totalWeight += weight;

    //------------------------------------------------------------------------
    // �d�ݕt�����ꂽ�F

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
// �e�N�j�b�N
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

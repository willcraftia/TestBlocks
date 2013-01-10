//=============================================================================
//
// �ϐ��錾
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
// ���_�V�F�[�_
//
//-----------------------------------------------------------------------------
void VS(inout float4 position : POSITION0, inout float2 texCoord : TEXCOORD0)
{
}

//=============================================================================
//
// �s�N�Z�� �V�F�[�_
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
    // �T���v���ʒu�ł̖@���Ɛ[�x���擾

    NormalDepth sample = GetNormalDepth(sampleTexCoord);

    //------------------------------------------------------------------------
    // �[�x���ɂ��Ă̏d�ݕt���̓x����

    // �[�x�������������A�e�����傫���Ȃ�悤�ɂ���B
    float deltaDepth = abs(center.Depth - sample.Depth);
    float depthCoeff = (1 - saturate(deltaDepth));
    depthCoeff *= depthCoeff;

    //------------------------------------------------------------------------
    // �@���̂Ȃ��p�ɂ��Ă̏d�ݕt���̓x����

    // �Ȃ��p�����s�ɋ߂����A�e�����傫���Ȃ�悤�ɂ���B
    float normalCoeff = abs(dot(center.Normal, sample.Normal));
    normalCoeff *= normalCoeff;

    //------------------------------------------------------------------------
    // �d�݂̌���

    float weight = baseWeight * depthCoeff * normalCoeff;

    // �d�݂̘a���L�^
    totalWeight += weight;

    //------------------------------------------------------------------------
    // �d�ݕt�����ꂽ AO �l

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
// �e�N�j�b�N
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

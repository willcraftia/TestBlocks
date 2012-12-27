//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 WorldViewProjection;

// ���_���猩�����z�̕���
float3 SunDirection;
float3 SunDiffuseColor;
// ���z�̏ꏊ�𔻒肷�邽�߂�臒l (0.999 �ȏオ�Ó�)
float SunThreshold;
// 0: ���z��`�悵�Ȃ�
// 1: ���z��`�悷��
float SunVisible;

float Time;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
    AddressU = Mirror;
    AddressV = Clamp;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

//=============================================================================
// Structures
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float3 Normal   : TEXCOORD1;
};

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = input.Normal;

    return output;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 PS(VSOutput input) : COLOR0
{
    float2 texCoord = float2(Time, 1);

    float4 color = tex2D(TextureSampler, texCoord);

    // �@�����ǂ̒��x���z�̌����Ɉ�v���Ă��邩���Z�o
    // ���z�̋t������ 0 �Ƃ��Ĕj��
    float amount = saturate(dot(normalize(input.Normal), SunDirection)) * SunVisible;

    // (1 - SunThreshold) �ȏ��v���Ă���͈͂Ȃ�Α��z�͈̔�
    amount -= SunThreshold;
    amount = saturate(amount);
    amount *= 1 / (1 - SunThreshold);

    // ���z�̐F���u�����h
    color.rgb += SunDiffuseColor * amount;

    return color;
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}

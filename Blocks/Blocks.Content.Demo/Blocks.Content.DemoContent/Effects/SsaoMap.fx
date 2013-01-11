#include "Common.fxh"

//=============================================================================
//
// �ϐ��錾
//
//-----------------------------------------------------------------------------
float TotalStrength = 1;
float Strength = 1;
float RandomOffset = 18;
float Falloff = 0.00001f;
float Radius = 1;

#define SAMPLE_COUNT 10
float invSamples = 1 / (float) SAMPLE_COUNT;

#if SAMPLE_COUNT == 8
const float3 SampleSphere[8] =
{
    float3(0.24710192, 0.6445882, 0.033550154),
    float3(0.00991752, -0.21947019, 0.7196721),
    float3(0.25109035, -0.1787317, -0.011580509),
    float3(-0.08781511, 0.44514698, 0.56647956),
    float3(-0.011737816, -0.0643377, 0.16030222),
    float3(0.035941467, 0.04990871, -0.46533614),
    float3(-0.058801126, 0.7347013, -0.25399926),
    float3(-0.24799341, -0.022052078, -0.13399573)
};
#elif SAMPLE_COUNT == 10
const float3 SampleSphere[10] =
{
    float3(-0.010735935, 0.01647018, 0.0062425877),
    float3(-0.06533369, 0.3647007, -0.13746321),
    float3(-0.6539235, -0.016726388, -0.53000957),
    float3(0.40958285, 0.0052428036, -0.5591124),
    float3(-0.1465366, 0.09899267, 0.15571679),
    float3(-0.44122112, -0.5458797, 0.04912532),
    float3(0.03755566, -0.10961345, -0.33040273),
    float3(0.019100213, 0.29652783, 0.066237666),
    float3(0.8765323, 0.011236004, 0.28265962),
    float3(0.29264435, -0.40794238, 0.15964167)
};
#endif

texture NormalDepthMap;
sampler NormalDepthMapSampler = sampler_state
{
    Texture = <NormalDepthMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

texture RandomNormalMap;
sampler RandomNormalMapSampler = sampler_state
{
    Texture = <RandomNormalMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Wrap;
    AddressV = Wrap;
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
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // �����_���ȃ��C���Z�o���邽�߂̖@���B
    float3 randomNormal = DecodeNormal(tex2D(RandomNormalMapSampler, texCoord * RandomOffset).xyz);

    // ���ݑΏۂƂ���ʒu�ł̖@���Ɛ[�x�B
    float4 normalDepth = tex2D(NormalDepthMapSampler, texCoord);
    float3 normal = DecodeNormal(normalDepth.xyz);
    float depth = normalDepth.w;

    // �����ł�����ɃT���v�����O�̔��a������������B
    float adjustedRadius = Radius * (1 - depth);

    // �Ŕw�ʂ͏������Ȃ��B
    // �Ŕw�ʂ̖@���̓V�[���̖@���ł͂Ȃ��A������p���ĉ��Z���s���ƁA
    // �Ŕw�ʂɑ΂��Č�����Ǐ����o�͂��Ă��܂��B
    // �܂��A���Z���Ȃ��Ӗ�������B
    float occlusion = 0;
    if (depth < 0.999999f)
    {
        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            // �T���v���̍��W�����肷�邽�߂̃��C�B
            float3 ray = adjustedRadius * reflect(SampleSphere[i], randomNormal);

            // �T���v���̍��W�B
            float2 occluderTexCoord = texCoord + sign(dot(ray, normal)) * ray;

            // �T���v���̖@���Ɛ[�x�B
            float4 occluderNormalDepth = tex2D(NormalDepthMapSampler, occluderTexCoord);
            float3 occluderNormal = DecodeNormal(occluderNormalDepth.xyz);
            float occluderDepth = occluderNormalDepth.w;

            // �[�x���B
            // deltaDepth < 0 �́A�T���v������艜�ɂ����ԁB
            float deltaDepth = depth - occluderDepth;

            // �@���̂Ȃ��p���Z�o�B
            float dotNormals = dot(occluderNormal, normal);

            // �@���̂Ȃ��p�����s�ł͂Ȃ����ɉe�����傫���Ȃ�悤�ɂ���B
            float deltaNormal = 1 - (dotNormals * 0.5 + 0.5);

            // �T���v�������ɂ���ꍇ�͓ʏ�Ԃł���A
            // step �ɂ��@���̉e���� 0 �ɂ��Ă��܂��B
            //
            // TODO
            // �K�����������ł͂Ȃ��̂ł́H
            deltaNormal *= step(Falloff, deltaDepth);

            // [Falloff, Strength] �̊ԂŐ[�x���ɂ��e���̓x������ς���B
            // ���[�x�������������ɉe�����傫���A���[�x�����傫�����ɉe�����������B
//            deltaNormal *= (1 - smoothstep(Falloff, Strength, deltaDepth));

            // �@���̉e���̓x�����𑫂��B
            occlusion += deltaNormal;
        }

        occlusion *= invSamples * TotalStrength;
    }

    float ao = 1 - occlusion;
    return float4(ao, 0, 0, 0);
}

//=============================================================================
//
// �e�N�j�b�N
//
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        // ps_3_0 ���g���̂� vs_3_0 �𖾎��B
        // ����𖾎����Ȃ���Ύ��s���G���[�ƂȂ�B
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}

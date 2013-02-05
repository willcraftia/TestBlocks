#include "Common.fxh"
#include "Shadow.fxh"

//=============================================================================
//
// �萔
//
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 EyePosition;

// ����
//
// �w�������� (directional light) �́A
// ���A���u���b�N�֕s���R�ȃ��C�e�B���O���s�����߁A�����Ȃ��B
// ����āA�w���������Ɉˑ�����g�U�F (diffuse) �����
// ���ːF (specular) ��p���Ȃ��B
// ���ːF (emissive) �͌����Ɉˑ����Ȃ����ߗp����B
// ���� (ambient) �͌����̕����Ɉˑ����Ȃ����ߗp����B
float3 AmbientLightColor;

float FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

texture TileMap;
sampler TileMapSampler = sampler_state
{
    Texture = <TileMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

texture EmissiveMap;
sampler EmissiveMapSampler = sampler_state
{
    Texture = <EmissiveMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

//-----------------------------------------------------------------------------
// �V���h�E �}�b�s���O�p

// ����
//
// ���I�V���h�E�̓u���b�N���E�Ƃ̑������������߁A
// �@�\�Ƃ��Ă͎c�����A��{�I�ɂ͗p���Ȃ��l���B

float ShadowMapDepthBias;

#define MAX_SHADOW_MAP_COUNT 3

int ShadowMapCount = MAX_SHADOW_MAP_COUNT;
float ShadowMapDistances[MAX_SHADOW_MAP_COUNT + 1];
float4x4 ShadowMapLightViewProjections[MAX_SHADOW_MAP_COUNT];

texture ShadowMap0;
#if MAX_SHADOW_MAP_COUNT > 1
texture ShadowMap1;
#endif
#if MAX_SHADOW_MAP_COUNT > 2
texture ShadowMap2;
#endif

sampler ShadowMapSampler[MAX_SHADOW_MAP_COUNT] =
{
    sampler_state
    {
        Texture = <ShadowMap0>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#if MAX_SHADOW_MAP_COUNT > 1
    sampler_state
    {
        Texture = <ShadowMap1>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
#if MAX_SHADOW_MAP_COUNT > 2
    sampler_state
    {
        Texture = <ShadowMap2>;
        MinFilter = Point;
        MagFilter = Point;
        MipFilter = None;
    },
#endif
};

// Classic specific
float ShadowMapSize;
static float ShadowMapTexelSize = 1 / ShadowMapSize;

// PCF specific
float2 PcfOffsets[MAX_PCF_TAP_COUNT];

//=============================================================================
//
// �\����
//
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION0;
};

struct VSInputNmTxVc
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION0;
};

struct VSOutputNmTxVc
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutputNmTxVcFog
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float FogFactor : TEXCOORD1;
};

struct VSOutputNmTxVcShadow
{
    float4 Position         : POSITION0;
    float3 Normal           : NORMAL0;
    float4 Color            : COLOR0;
    float2 TexCoord         : TEXCOORD0;
    float4 WorldPosition    : TEXCOORD2;
    float4 ViewPosition     : TEXCOORD3;
};

struct VSOutputNmTxVcFogShadow
{
    float4 Position         : POSITION0;
    float3 Normal           : NORMAL0;
    float4 Color            : COLOR0;
    float2 TexCoord         : TEXCOORD0;
    float FogFactor         : TEXCOORD1;
    float4 WorldPosition    : TEXCOORD2;
    float4 ViewPosition     : TEXCOORD3;
};

//=============================================================================
//
// ���_�V�F�[�_
//
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

    return output;
}

VSOutputNmTxVc VSNmTxVc(VSInputNmTxVc input)
{
    VSOutputNmTxVc output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}

VSOutputNmTxVcFog VSNmTxVcFog(VSInputNmTxVc input)
{
    VSOutputNmTxVcFog output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    float eyeDistance = distance(worldPosition, EyePosition);
    output.FogFactor = CalculateFogFactor(FogStart, FogEnd, eyeDistance, FogEnabled);

    return output;
}

VSOutputNmTxVcShadow VSNmTxVcShadow(VSInputNmTxVc input)
{
    VSOutputNmTxVcShadow output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    output.WorldPosition = worldPosition;
    output.ViewPosition = viewPosition;

    return output;
}

VSOutputNmTxVcFogShadow VSNmTxVcFogShadow(VSInputNmTxVc input)
{
    VSOutputNmTxVcFogShadow output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    float eyeDistance = distance(worldPosition, EyePosition);
    output.FogFactor = CalculateFogFactor(FogStart, FogEnd, eyeDistance, FogEnabled);

    output.WorldPosition = worldPosition;
    output.ViewPosition = viewPosition;

    return output;
}

//=============================================================================
//
// �s�N�Z�� �V�F�[�_
//
//-----------------------------------------------------------------------------
float4 PSNmTxVc(VSOutputNmTxVc input) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    return color;
}

float4 PSNmTxVcFog(VSOutputNmTxVcFog input) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // ���B
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 PSOcclusionQuery(VSOutput input) : COLOR0
{
    // �I�N���[�W���� �N�G���ł̓e�N�X�`���⃉�C�e�B���O�͕s�v�B
    // �Օ������s�����΂���ŗǂ��B
    return float4(0, 0, 0, 1);
}

float4 PSWireframe(VSOutput input) : COLOR0
{
    return float4(0, 0, 0, 1);
}

float4 PSShadowBasic(VSOutputNmTxVcShadow input) : COLOR0
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestClassicShadowMap(
                ShadowMapSampler[i],
                ShadowMapSize,
                ShadowMapTexelSize,
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    return color;
}

float4 PSFogShadowBasic(VSOutputNmTxVcFogShadow input) : COLOR0
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestClassicShadowMap(
                ShadowMapSampler[i],
                ShadowMapSize,
                ShadowMapTexelSize,
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    // ���B
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 PSShadowPcf2x2(VSOutputNmTxVcShadow input) : COLOR
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestPcf2x2ShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias,
                PcfOffsets);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    return color;
}

float4 PSFogShadowPcf2x2(VSOutputNmTxVcFogShadow input) : COLOR
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestPcf2x2ShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias,
                PcfOffsets);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e
    color.rgb *= shadow;

    // ���B
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 PSShadowPcf3x3(VSOutputNmTxVcShadow input) : COLOR
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestPcf3x3ShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias,
                PcfOffsets);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e
    color.rgb *= shadow;

    return color;
}

float4 PSFogShadowPcf3x3(VSOutputNmTxVcFogShadow input) : COLOR
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestPcf3x3ShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias,
                PcfOffsets);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    // ���B
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 PSShadowVsm(VSOutputNmTxVcShadow input) : COLOR0
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestVarianceShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    return color;
}

float4 PSFogShadowVsm(VSOutputNmTxVcFogShadow input) : COLOR0
{
    // �e�B
    float distance = abs(input.ViewPosition.z);
    float shadow = 1;
    for (int i = 0; i < ShadowMapCount; i++)
    {
        if (ShadowMapDistances[i] <= distance && distance < ShadowMapDistances[i + 1])
        {
            float4 lightingPosition = mul(input.WorldPosition, ShadowMapLightViewProjections[i]);
            float2 shadowTexCoord = ProjectionToTexCoord(lightingPosition);
            shadow = TestVarianceShadowMap(
                ShadowMapSampler[i],
                shadowTexCoord,
                lightingPosition,
                ShadowMapDepthBias);
        }
    }

    float4 color = float4(0, 0, 0, 1);

    // �e�N�X�`���B
    color += tex2D(TileMapSampler, input.TexCoord);
    color *= input.Color;

    // ��������ѕ��ˌ��B
    color.rgb *= AmbientLightColor + tex2D(EmissiveMapSampler, input.TexCoord);

    // �e�B
    color.rgb *= shadow;

    // ���B
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
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
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcFog();
        PixelShader = compile ps_3_0 PSNmTxVcFog();
    }
}

technique NoFog
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVc();
        PixelShader = compile ps_3_0 PSNmTxVc();
    }
}

technique OcclusionQuery
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PSOcclusionQuery();
    }
}

technique Wireframe
{
    pass P0
    {
        FillMode = WIREFRAME;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PSWireframe();
    }
}

technique BasicShadow
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcFogShadow();
        PixelShader = compile ps_3_0 PSFogShadowBasic();
    }
}

technique BasicShadowNoFog
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcShadow();
        PixelShader = compile ps_3_0 PSShadowBasic();
    }
}

technique Pcf2x2Shadow
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcFogShadow();
        PixelShader = compile ps_3_0 PSFogShadowPcf2x2();
    }
}

technique Pcf2x2ShadowNoFog
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcShadow();
        PixelShader = compile ps_3_0 PSShadowPcf2x2();
    }
}

technique Pcf3x3Shadow
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcFogShadow();
        PixelShader = compile ps_3_0 PSFogShadowPcf3x3();
    }
}

technique Pcf3x3ShadowNoFog
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcShadow();
        PixelShader = compile ps_3_0 PSShadowPcf3x3();
    }
}

technique VsmShadow
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcFogShadow();
        PixelShader = compile ps_3_0 PSFogShadowVsm();
    }
}

technique VsmShadowNoFog
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VSNmTxVcShadow();
        PixelShader = compile ps_3_0 PSShadowVsm();
    }
}

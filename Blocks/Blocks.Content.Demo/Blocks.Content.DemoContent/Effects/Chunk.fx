//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 ViewProjection;

float3 EyePosition;

float3 AmbientLightColor;
float3 LightDirection;
float3 LightDiffuseColor;
float3 LightSpecularColor;

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

texture DiffuseMap;
sampler DiffuseMapSampler = sampler_state
{
    Texture = <DiffuseMap>;
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

texture SpecularMap;
sampler SpecularMapSampler = sampler_state
{
    Texture = <SpecularMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

//=============================================================================
// Structures
//-----------------------------------------------------------------------------
struct ColorPair
{
    float3 Diffuse;
    float3 Specular;
};

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float FogFactor : TEXCOORD1;
};

//=============================================================================
// Vertex shader helper
//-----------------------------------------------------------------------------
float CalculateFogFactor(float d)
{
    return saturate((d - FogStart) / (FogEnd - FogStart)) * FogEnabled;
}

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);

    output.Position = mul(worldPosition, ViewProjection);
    output.Normal = input.Normal;
    output.TexCoord = input.TexCoord;

    float eyeDistance = distance(worldPosition, EyePosition);
    output.FogFactor = CalculateFogFactor(eyeDistance);

    return output;
}

//=============================================================================
// Pixel shader helper
//-----------------------------------------------------------------------------
ColorPair CalculateLight(float3 E, float3 N, float2 texCoord)
{
    ColorPair result;

    result.Diffuse = AmbientLightColor;
    result.Specular = 0;

    float4 specular = tex2D(SpecularMapSampler, texCoord);

    float3 L = -LightDirection;
    float3 H = normalize(E + L);
    float dt = max(0, dot(L, N));
    result.Diffuse += LightDiffuseColor * dt;
    if (dt != 0)
        result.Specular += LightSpecularColor * pow(max(0.00001f, dot(H, N)), specular.a);
// XNA 4.0 Release �r���h�ł́A�V�F�[�_�� pow(0,e) �� exp(log(0) * e) �֓W�J���A
// ���ꂪ exp(-inf * e) �ƂȂ邽�߁A�R���p�C�� �G���[�ƂȂ邱�Ƃ����m�̖��炵���B
// �䂦�ɁA0 �̕���������Ȃ� 0 �ɋ߂��l�ɂ��ĉ������炵���B
//        result.Specular += LightSpecularColor * pow(max(0, dot(H, N)), specular.a);

    result.Diffuse *= tex2D(DiffuseMapSampler, texCoord);
    result.Diffuse += tex2D(EmissiveMapSampler, texCoord);
    result.Specular *= specular.rgb;

    return result;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 DefaultPS(VSOutput input) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);

    // Base color
    color += tex2D(TileMapSampler, input.TexCoord);

    // Lighting
    float3 E = normalize(-EyePosition);
    float3 N = normalize(input.Normal);
    ColorPair light = CalculateLight(E, N, input.TexCoord);
    color.rgb *= light.Diffuse;
    color.rgb += light.Specular;

    // Fog
    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 WireframePS(VSOutput input) : COLOR0
{
    return float4(0, 0, 0, 1);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 DefaultPS();
    }
}

technique Wireframe
{
    pass P0
    {
        FillMode = WIREFRAME;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 WireframePS();
    }
}
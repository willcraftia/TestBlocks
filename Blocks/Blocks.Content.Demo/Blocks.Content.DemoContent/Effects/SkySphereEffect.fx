//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 WorldViewProjection;

// ‹“_‚©‚çŒ©‚½‘¾—z‚Ì•ûŒü
float3 SunDirection;
float3 SunDiffuseColor;
// ‘¾—z‚ÌêŠ‚ğ”»’è‚·‚é‚½‚ß‚Ìè‡’l (0.999 ˆÈã‚ª‘Ã“–)
float SunThreshold;
// 0: ‘¾—z‚ğ•`‰æ‚µ‚È‚¢
// 1: ‘¾—z‚ğ•`‰æ‚·‚é
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

    // –@ü‚ª‚Ç‚Ì’ö“x‘¾—z‚ÌŒü‚«‚Éˆê’v‚µ‚Ä‚¢‚é‚©‚ğZo
    // ‘¾—z‚Ì‹t•ûŒü‚Í 0 ‚Æ‚µ‚Ä”jŠü
    float amount = saturate(dot(normalize(input.Normal), SunDirection)) * SunVisible;

    // (1 - SunThreshold) ˆÈãˆê’v‚µ‚Ä‚¢‚é”ÍˆÍ‚È‚ç‚Î‘¾—z‚Ì”ÍˆÍ
    amount -= SunThreshold;
    amount = saturate(amount);
    amount *= 1 / (1 - SunThreshold);

    // ‘¾—z‚ÌF‚ğƒuƒŒƒ“ƒh
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

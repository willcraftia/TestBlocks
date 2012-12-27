//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 WorldViewProjection;

// 視点から見た太陽の方向
float3 SunDirection;
float3 SunDiffuseColor;
// 太陽の場所を判定するための閾値 (0.999 以上が妥当)
float SunThreshold;
// 0: 太陽を描画しない
// 1: 太陽を描画する
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

    // 法線がどの程度太陽の向きに一致しているかを算出
    // 太陽の逆方向は 0 として破棄
    float amount = saturate(dot(normalize(input.Normal), SunDirection)) * SunVisible;

    // (1 - SunThreshold) 以上一致している範囲ならば太陽の範囲
    amount -= SunThreshold;
    amount = saturate(amount);
    amount *= 1 / (1 - SunThreshold);

    // 太陽の色をブレンド
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

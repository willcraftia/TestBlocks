//=============================================================================
//
// 定数
//
//-----------------------------------------------------------------------------
float4x4 ViewProjection;

float3 SkyColor;

// 視点から見た太陽の方向
float3 SunDirection;
float3 SunDiffuseColor;
// 太陽の場所を判定するための閾値 (0.999 以上が妥当)
float SunThreshold;
// 0: 太陽を描画しない
// 1: 太陽を描画する
float SunVisible;

//=============================================================================
//
// 構造体
//
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
//
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, ViewProjection);
    output.Normal = input.Normal;

    return output;
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(VSOutput input) : COLOR0
{
    float4 color = float4(SkyColor, 1);

    // 法線がどの程度太陽の向きに一致しているかを算出
    // 太陽の逆方向は 0 として破棄
    float amount = saturate(dot(normalize(input.Normal), SunDirection)) * SunVisible;

    // SunThreshold から太陽の範囲を算出
    amount -= SunThreshold;
    amount = saturate(amount);
    amount *= 1 / (1 - SunThreshold);

    // 太陽の色をブレンド
    color.rgb += SunDiffuseColor * amount;

    return color;
}

//=============================================================================
//
// テクニック
//
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

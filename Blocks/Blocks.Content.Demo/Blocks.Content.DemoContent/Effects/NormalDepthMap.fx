//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;

//=============================================================================
//
// 構造体宣言
//
//-----------------------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
};

struct VSOutput
{
    float4 Position     : POSITION;
    float4 PositionWVP  : TEXCOORD0;
    float3 Normal       : TEXCOORD1;
};

//=============================================================================
//
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.PositionWVP = output.Position;
    output.Normal = mul(input.Normal, World);

    return output;
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(VSOutput input) : COLOR0
{
    float4 color;
    // 法線は [-1, 1] から [0, 1] へ変換して設定。
    color.rgb = normalize(input.Normal) * 0.5f + 0.5f;
    color.a = input.PositionWVP.z / input.PositionWVP.w;
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
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}

//=============================================================================
//
// 定数
//
//-----------------------------------------------------------------------------
float4x4 World;
float4x4 LightViewProjection;

struct VSOutput
{
    float4 Position     : POSITION;
    float4 PositionWVP  : TEXCOORD0;
};

//=============================================================================
//
// 頂点シェーダ
//
//-----------------------------------------------------------------------------
VSOutput VS(float4 position : POSITION)
{
    VSOutput output;

    output.Position = mul(position, mul(World, LightViewProjection));
    output.PositionWVP = output.Position;

    return output;
}

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 DefaultPS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, 0.0f, 0.0f, 0.0f);
}

float4 VsmPS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, depth * depth, 0.0f, 0.0f);
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
        // CW では異常にピーターパン現象が発生してしまうため、
        // CCW にしている。
        // CCW は深度バイアスを必要とするが、ピーターパン現象を消すよりも楽。
        //
        // http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx
        // 『背面と全面』参照。
        //
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 DefaultPS();
    }
}

technique Vsm
{
    pass P0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VsmPS();
    }
}

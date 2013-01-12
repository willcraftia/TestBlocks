//=============================================================================
//
// 定数宣言
//
//-----------------------------------------------------------------------------
float Density;
float Brightness;

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Y 座標に応じて sin で輝度を下げる。
    float amount = (sin(texCoord.y * Density) + 1) * 0.5 * (1 - Brightness) + Brightness;

    return tex2D(TextureSampler, texCoord) * amount;
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
        PixelShader = compile ps_2_0 PS();
    }
}

//=============================================================================
//
// 変数宣言
//
//-----------------------------------------------------------------------------
//
// メモ
//
// Y  =  0.29900 * R + 0.58700 * G + 0.11400 * B
// Cb = -0.16874 * R - 0.33126 * G + 0.50000 * B
// Cr =  0.50000 * R - 0.41869 * G - 0.08131 * B
//
// Y is luminance component
// Cb is the blue-defference chroma component
// Cr is the red-difference chroma component
//
const float4 RGBToY = { 0.299f,  0.587f,  0.114f, 0.0f };

//
// メモ
//
// R = Y                + 1.40200 * Cr
// G = Y - 0.34414 * Cb - 0.71414 * Cr
// B = Y + 1.77200 * Cb
//
const float4 CbToRGB = { 0.0f, -0.344f, 1.772f, 0.0f };
const float4 CrToRGB = { 1.402f, -0.714, 0.0f, 0.0f };

//
// メモ
//
// Grayscale:  Cb = 0, Cr = 0
// Sepia Tone: Cb = -0.1, Cr = 0.1
//
float Cb = 0.0f;
float Cr = 0.0f;

texture SceneMap;
sampler SceneMapSampler : register(s0) = sampler_state
{
    Texture = <SceneMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

//=============================================================================
//
// ピクセル シェーダ
//
//-----------------------------------------------------------------------------
float GetYFromRGB(float4 RGB)
{
    return dot(RGBToY, RGB);
}

float4 GetRGBFromCb(float Cb)
{
    return CbToRGB * Cb;
}

float4 GetRGBFromCr(float Cr)
{
    return CrToRGB * Cr;
}

float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 RGB = tex2D(SceneMapSampler, texCoord);
    float4 Y = (float4) GetYFromRGB(RGB);

    float4 result = Y + GetRGBFromCb(Cb) + GetRGBFromCr(Cr);
    result.a = RGB.a;

    return result;
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

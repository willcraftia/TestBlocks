//=============================================================================
//
// 共通定義
//
//=============================================================================
#if !defined (COMMON_FXH)
#define COMMON_FXH

//-----------------------------------------------------------------------------
//
// [0, 1] でエンコードされた法線を [-1, 1] へ変換します。
//
//-----------------------------------------------------------------------------
float3 DecodeNormal(float3 encodedNormal)
{
    return encodedNormal * 2 - 1;
}

//-----------------------------------------------------------------------------
//
// 射影空間座標からテクスチャ座標へ変換。
//
//-----------------------------------------------------------------------------
float2 ProjectionToTexCoord(float4 position)
{
    return position.xy / position.w * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
}

//-----------------------------------------------------------------------------
//
// 距離フォグのファクタを計算。
//
//-----------------------------------------------------------------------------
float CalculateFogFactor(float fogStart, float fogEnd, float distance, float fogEnabled)
{
    return saturate((distance - fogStart) / (fogEnd - fogStart)) * fogEnabled;
}

#endif // !defined (COMMON_FXH)

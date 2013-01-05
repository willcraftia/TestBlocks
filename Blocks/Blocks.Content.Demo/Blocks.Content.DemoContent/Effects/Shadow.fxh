//-----------------------------------------------------------------------------
//
// Shadow Common Definitions
//
//-----------------------------------------------------------------------------
#if !defined (SHADOW_FXH)
#define SHADOW_FXH

#define MAX_PCF_KERNEL_SIZE 3
#define MAX_PCF_TAP_COUNT (MAX_PCF_KERNEL_SIZE * MAX_PCF_KERNEL_SIZE)

//-----------------------------------------------------------------------------
//
// Returns the result of classic shadow test.
//
//-----------------------------------------------------------------------------
float TestClassicShadowMap(
    sampler2D shadowMap,
    float shadowMapSize,
    float shadowMapTexelSize,
    float2 shadowTexCoord,
    float4 position,
    float depthBias)
{
    // Bilinear interpolation manually.
    float tl = tex2D(shadowMap, shadowTexCoord).x;
    float tr = tex2D(shadowMap, shadowTexCoord + float2(shadowMapTexelSize, 0)).x;
    float bl = tex2D(shadowMap, shadowTexCoord + float2(0, shadowMapTexelSize)).x;
    float br = tex2D(shadowMap, shadowTexCoord + float2(shadowMapTexelSize, shadowMapTexelSize)).x;
    float2 f = frac(shadowTexCoord * shadowMapSize);
    float t = lerp(tl, tr, f.x);
    float b = lerp(bl, br, f.x);
    float depth = lerp(t, b, f.y);

    // 除算を回避して乗算へ
    // REFERENCE: position.z / position.w - depthBias <= depth
    return position.z <= position.w * (depth + depthBias);
}

//-----------------------------------------------------------------------------
//
// Returns the result of pcf shadow test.
//
//-----------------------------------------------------------------------------

float divide4 = 1 / 4.0f;

float TestPcf2x2ShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float2 offsets[MAX_PCF_TAP_COUNT])
{
    float depth = 0;
    for (int i = 0; i < 4; i++)
    {
        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
//    depth /= 4.0f;
    depth *= divide4;

    // 除算を回避して乗算へ
    // REFERENCE: position.z / position.w - depthBias <= depth
    return position.z <= position.w * (depth + depthBias);
}

float divide9 = 1 / 9.0f;

float TestPcf3x3ShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float2 offsets[MAX_PCF_TAP_COUNT])
{
    float depth = 0;
    for (int i = 0; i < 9; i++)
    {
        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
//    depth /= 9.0f;
    depth *= divide9;

    // 除算を回避して乗算へ
    // REFERENCE: position.z / position.w - depthBias <= depth
    return position.z <= position.w * (depth + depthBias);
}

//-----------------------------------------------------------------------------
//
// Returns the result of vsm shadow test.
//
//-----------------------------------------------------------------------------
float TestVarianceShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias)
{
    float4 moments = tex2D(shadowMap, shadowTexCoord);

    float Ex = moments.x;
    float E_x2 = moments.y;
    float Vx = E_x2 - Ex * Ex;
    Vx = min(1, max(0, Vx + 0.00001f));
    float t = position.z / position.w - depthBias;
    float tMinusM = t - Ex;
    float p = Vx / (Vx + tMinusM * tMinusM);

    // チェビシェフの不等式により t > Ex で p が有効。
    // t <= Ex では p = 1、つまり、影がない。
    return saturate(max(p, t <= Ex));
}

#endif // !defined (SHADOW_FXH)

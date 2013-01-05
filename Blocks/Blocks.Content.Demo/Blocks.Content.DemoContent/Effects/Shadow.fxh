//-----------------------------------------------------------------------------
//
// Shadow Common Definitions
//
//-----------------------------------------------------------------------------
#if !defined (SHADOW_FXH)
#define SHADOW_FXH

#define MAX_PCF_KERNEL_SIZE 5
#define MAX_PCF_TAP_COUNT (MAX_PCF_KERNEL_SIZE * MAX_PCF_KERNEL_SIZE)

//-----------------------------------------------------------------------------
//
// Returns the result of classic shadow test.
//
//-----------------------------------------------------------------------------
float TestClassicShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias)
{
    float depth = tex2D(shadowMap, shadowTexCoord).x;

    // 除算を回避して乗算へ
    // REFERENCE: depth < position.z / position.w - depthBias
    return position.z <= position.w * (depth + depthBias);
}

//-----------------------------------------------------------------------------
//
// Returns the result of pcf shadow test.
//
//-----------------------------------------------------------------------------
float TestPcfShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float tapCount,
    float2 offsets[MAX_PCF_TAP_COUNT])
{
    float depth = 0;
    for (int i = 0; i < tapCount && i < MAX_PCF_TAP_COUNT; i++)
    {
//        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
// TODO: correct?
        depth += tex2Dlod(shadowMap, float4(shadowTexCoord + offsets[i], 0, 1)).x;
    }
    depth /= tapCount;

    // optimaization by avoiding division and using multiplication
    //
    // depth < position.z / position.w - depthBias
    //
    if (position.w * (depth + depthBias) < position.z)
    {
        return 0;
    }
    else
    {
        return 1;
    }
}

float TestPcf3x3ShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float2 offsets[9])
{
    float depth = 0;
    for (int i = 0; i < 9; i++)
    {
        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    depth /= 9.0f;

    // optimaization by avoiding division and using multiplication
    //
    // depth < position.z / position.w - depthBias
    //
    if (position.w * (depth + depthBias) < position.z)
    {
        return 0;
    }
    else
    {
        return 1;
    }
}

float TestPcf4x4ShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float2 offsets[16])
{
    float depth = 0;
    for (int i = 0; i < 16; i++)
    {
        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    depth /= 16.0f;

    // optimaization by avoiding division and using multiplication
    //
    // depth < position.z / position.w - depthBias
    //
    if (position.w * (depth + depthBias) < position.z)
    {
        return 0;
    }
    else
    {
        return 1;
    }
}

float TestPcf5x5ShadowMap(
    sampler2D shadowMap,
    float2 shadowTexCoord,
    float4 position,
    float depthBias,
    float2 offsets[25])
{
    float depth = 0;
    for (int i = 0; i < 25; i++)
    {
        depth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    depth /= 25.0f;

    // optimaization by avoiding division and using multiplication
    //
    // depth < position.z / position.w - depthBias
    //
    if (position.w * (depth + depthBias) < position.z)
    {
        return 0;
    }
    else
    {
        return 1;
    }
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
    float t = position.z / position.w - depthBias;
    float tMinusM = t - Ex;
    float p = Vx / (Vx + tMinusM * tMinusM);

    // チェビシェフの不等式により t > Ex で p が有効。
    // t <= Ex では p = 1、つまり、影がない。
    return saturate(max(p, t <= Ex));
}

#endif // !defined (SHADOW_FXH)

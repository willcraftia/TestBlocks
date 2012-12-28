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
    float lightingDepth = tex2D(shadowMap, shadowTexCoord).x;

    // optimaization by avoiding division and using multiplication
    //
    // lightingDepth < position.z / position.w - depthBias
    //
    if (position.w * (lightingDepth + depthBias) < position.z)
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
    float lightingDepth = 0;
    for (int i = 0; i < tapCount && i < MAX_PCF_TAP_COUNT; i++)
    {
//        lightingDepth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
// TODO: correct?
        lightingDepth += tex2Dlod(shadowMap, float4(shadowTexCoord + offsets[i], 0, 1)).x;
    }
    lightingDepth /= tapCount;

    // optimaization by avoiding division and using multiplication
    //
    // lightingDepth < position.z / position.w - depthBias
    //
    if (position.w * (lightingDepth + depthBias) < position.z)
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
    float lightingDepth = 0;
    for (int i = 0; i < 9; i++)
    {
        lightingDepth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    lightingDepth /= 9.0f;

    // optimaization by avoiding division and using multiplication
    //
    // lightingDepth < position.z / position.w - depthBias
    //
    if (position.w * (lightingDepth + depthBias) < position.z)
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
    float lightingDepth = 0;
    for (int i = 0; i < 16; i++)
    {
        lightingDepth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    lightingDepth /= 16.0f;

    // optimaization by avoiding division and using multiplication
    //
    // lightingDepth < position.z / position.w - depthBias
    //
    if (position.w * (lightingDepth + depthBias) < position.z)
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
    float lightingDepth = 0;
    for (int i = 0; i < 25; i++)
    {
        lightingDepth += tex2D(shadowMap, shadowTexCoord + offsets[i]).x;
    }
    lightingDepth /= 25.0f;

    // optimaization by avoiding division and using multiplication
    //
    // lightingDepth < position.z / position.w - depthBias
    //
    if (position.w * (lightingDepth + depthBias) < position.z)
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

    return saturate(max(p, t <= Ex));
}

#endif // !defined (SHADOW_FXH)

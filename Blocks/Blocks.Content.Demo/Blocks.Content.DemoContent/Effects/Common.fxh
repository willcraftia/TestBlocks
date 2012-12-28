//-----------------------------------------------------------------------------
//
// Common Definitions
//
//-----------------------------------------------------------------------------
#if !defined (COMMON_FXH)
#define COMMON_FXH

#define HALF_LAMBERT 0

struct Light
{
    float3 AmbientColor;
    float3 DiffuseColor;
    float3 SpecularColor;
    float3 ShadowColor;
    float3 Direction;
    float Attenuation;
};

struct Material
{
    float3 AmbientColor;
    float3 DiffuseColor;
    float3 SpecularColor;
    float SpecularPower;
};

//-----------------------------------------------------------------------------
//
// NOTE: Direct3D 9 Ambient Lighting
//
// Ambient Lighting = Ca * [Ga + sum(Atten * Spot * La)]
//
// Ca:      Material ambient color.
// Ga:      Global ambient color.
// Atten:   Light attenuation.
// Spot:    Spotlight factor.
// sum:     Summation of each light's ambient component.
// L:       Light ambient color.
//
//-----------------------------------------------------------------------------
float3 AmbientComponent(Light light)
{
    return light.AmbientColor * light.Attenuation;
}

//-----------------------------------------------------------------------------
//
// NOTE: Direct3D 9 Diffuse Lighting
//
// Diffuse Lighting = sum[Cd * Ld * (N.Ldir) * Atten * Spot]
//
// sum:     Summation of each light's diffuse component.
// Cd:      Diffuse color.
// Ld:      Light diffuse color.
// N:       Vertex normal.
// Ldir:    Direction vector from object to the light.
// Atten:   Light attenuation.
// Spot:    Spotlight factor.
//
//-----------------------------------------------------------------------------
float3 DiffuseComponent(float3 normal, Material material, Light light)
{
    if (light.Attenuation == 0)
    {
        // optimazation
        return material.DiffuseColor * light.ShadowColor;
    }
    else
    {
        float nDotL = dot(normal, light.Direction);
#if HALF_LAMBERT == 1
        nDotL = nDotL * 0.5f + 0.5f;
        nDotL = nDotL * nDotL;
#endif
        if (nDotL <= 0)
        {
            // optimazation
            return material.DiffuseColor * light.ShadowColor;
        }
        else
        {
            float coefficient = nDotL * light.Attenuation;
            float3 mixedLightColor = lerp(light.ShadowColor, light.DiffuseColor, coefficient);
            return material.DiffuseColor * light.DiffuseColor * mixedLightColor;
        }
    }
}

//-----------------------------------------------------------------------------
//
// NOTE: Direct3D 9 Specular Lighting
//
// Specular Lighting = Cs * sum[Ls * (N.H)^P * Atten * Spot]
//
// Cs:      Specular color.
// sum:     Summation of each light's specular component.
// Ls:      Light specular color.
// N:       Vertex normal.
// H:       Halfway vector.
// P:       Specular reflection power.
// Atten:   Light attenuation.
// Spot:    Spotlight factor.
//
//-----------------------------------------------------------------------------
float3 SpecularComponent(float3 normal, float3 cameraDirection, Material material, Light light)
{
    float3 halfway = normalize(cameraDirection + light.Direction);
    float nDotH = dot(normal, halfway);
    if (nDotH <= 0)
    {
        // optimazation
        return light.ShadowColor;
    }
    else
    {
        float coefficient = pow(nDotH, material.SpecularPower) * light.Attenuation;
        float3 mixedLightColor = lerp(light.ShadowColor, light.SpecularColor, coefficient);
        return mixedLightColor;
    }
//
// NOTE
//
// i dont use reflection because its specular looks like pretty strenge lighting.
// maybe it is too more correct reflection, i feel.
//

//    float3 reflection = -reflect(lightDirection, normal);
}

//-----------------------------------------------------------------------------
//
// NOTE: Direct3D 9 Attenuation
//
// Attenuation = 1 / (Atten0 + Atten1 * d + Atten2 * d^2)
//
// Atten0:  Constant attenuation factor.
// Atten0:  Linear attenuation factor.
// Atten0:  Quadratic attenuation factor.
// d:       Distance from vertex position to light position.
//
//-----------------------------------------------------------------------------
float Attenuation(
    float constantFactor,
    float linearFactor,
    float quadraticFactor,
    float distance,
    float maxDistance)
{
    float result = 0;
    if (distance < maxDistance)
    {
        result = 1.0f / (constantFactor + linearFactor * distance + quadraticFactor * distance * distance);
    }
    return result;
}

//-----------------------------------------------------------------------------
//
// Ambient, diffuse and specular with shadow color
//
//-----------------------------------------------------------------------------
float3 AmbientDiffuseSpecularShadow(
    float3 normal,
    float3 cameraDirection,
    float3 globalAmbientColor,
    Material material,
    Light light)
{
    float3 color;
    color = material.AmbientColor * (globalAmbientColor + AmbientComponent(light));
    color += DiffuseComponent(normal, material, light);
    color += material.SpecularColor * SpecularComponent(normal, cameraDirection, material, light);
    return color;
}

// test code for shadow color from GPU Gems - Chapter 10
float3 BACKUP_AmbientDiffuseSpecularShadow(
    float3 normal,
    float3 cameraDirection,
    float3 globalAmbientColor,
    Material material,
    Light light,
    float shadow)
{
    float nDotL = dot(normal, light.Direction);
    float3 halfway = normalize(cameraDirection + light.Direction);
    float nDotH = dot(normal, halfway);

    float3 lightingCoefficients = lit(nDotL, nDotH, material.SpecularPower).xyz;

    float3 mixedLightColor = lerp(light.ShadowColor, light.DiffuseColor, shadow);

    float3 color;

    color = material.AmbientColor * (globalAmbientColor + light.AmbientColor * lightingCoefficients.x);
    color += mixedLightColor * material.DiffuseColor * lightingCoefficients.y;
    color += mixedLightColor * shadow * material.SpecularColor * lightingCoefficients.z;
    color *= light.Attenuation;
    return color;
}

//-----------------------------------------------------------------------------
//
// Creates matrix transforming world coodinate to tangent coodinate.
//
//-----------------------------------------------------------------------------
float3x3 CreateWorldToTangentSpace(float3 normal, float3 binormal, float3 tangent, float4x4 world)
{
    float3x3 result;
    result[0] = mul(tangent, world);
    result[1] = mul(binormal, world);
    result[2] = mul(normal, world);
    return result;
}

//-----------------------------------------------------------------------------
//
// Converts the position [-w, w] of projective space to texture coodinate [0, 1].
//
//-----------------------------------------------------------------------------
float2 ProjectionToTexCoord(float4 position)
{
    return position.xy / position.w * float2(0.5f, -0.5f) + 0.5f;
}

//-----------------------------------------------------------------------------
//
// Converts color [0, 1] to vector [-1, 1].
//
//-----------------------------------------------------------------------------
float4 ColorToVector(float4 color)
{
    return color * 2 - 1;
}

//-----------------------------------------------------------------------------
//
// Checks if the specified texture coodinate is between 0 and 1.
//
//-----------------------------------------------------------------------------
bool IsInTexCoord(float2 texCoord)
{
    return (saturate(texCoord.x) == texCoord.x) &&
        (saturate(texCoord.y) == texCoord.y);
}

//-----------------------------------------------------------------------------
//
// Caluculates the degree of linear fog.
//
//-----------------------------------------------------------------------------
float CalculateDistanceFog(float fogStart, float fogEnd, float distance)
{
    float fog;
    if (fogEnd <= distance)
    {
        fog = 0;
    }
    else if (fogStart < distance)
    {
        fog = (fogEnd - distance) / (fogEnd - fogStart);
    }
    else
    {
        fog = 1;
    }
    return fog;
}

#endif // !defined (COMMON_FXH)

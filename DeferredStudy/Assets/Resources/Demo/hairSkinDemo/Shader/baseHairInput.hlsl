#ifndef BASEHAIRINPUT
#define BASEHAIRINPUT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"   


CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float4 _MainColor;
float _Cutoff;
CBUFFER_END

TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

static int DITHER_SIZE_UE = 5;
static half DITHER_THRESHOLDS_UE[25] =
{
    2.0/6,  4.0/6,  1.0/6,  3.0/6,  5.0/6,
    1.0/6,  3.0/6,  5.0/6,  2.0/6,  4.0/6,
    5.0/6,  2.0/6,  4.0/6,  1.0/6,  3.0/6,
    4.0/6,  1.0/6,  3.0/6,  5.0/6,  2.0/6,
    3.0/6,  5.0/6,  2.0/6,  4.0/6,  1.0/6
};

float random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}
half Dither_UE(half opacity, float2 screenUV)
{
    half2 uv = screenUV.xy * _ScreenParams.xy;
    float rand = random(uv);
    int index = (int(uv.x) % DITHER_SIZE_UE) * DITHER_SIZE_UE + int(uv.y) % DITHER_SIZE_UE;
    
    return opacity - DITHER_THRESHOLDS_UE[index] + (rand*2-1)*1.0/14;
}
half Dither(half opacity, float2 screenUV)
{
    return Dither_UE(opacity, screenUV);
}

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 uv            : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;        
        
};
struct Varyings
{
    float4 positionHCS  : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 posWS                    : TEXCOORD2;    // xyz: posWS
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
    float4 screenPos                : TEXCOORD8;
};     

#endif
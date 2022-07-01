#ifndef WATERINPUT
#define WATERINPUT   

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 PositionOS: POSITION;
    float2 uv: TEXCOORD0;
    float3 normalOS: NORMAL;
    float4 tangentOS: TANGENT;
};
struct Varyings
{
    float4 PositionHCS: SV_POSITION;
    float2 uv: TEXCOORD0;
    float fogCoord: TEXCOORD1;
    float3 PositionVS: TEXCOORD2;  // view Space
    float3 PositionWS: TEXCOORD3;
    float3 TW1: TEXCOORD4;   // Normal
    float3 TW2: TEXCOORD5;
    float3 TW3: TEXCOORD6;
};


CBUFFER_START(UnityPerMaterial)
float4 _WaterColor, _WaterDepthColor, _SpecularColor,_CartoonSpecular;
float _DepthIntensity;
half4 _FoamSpeed;
half4 _FoamTex_ST, _DistortionTex_ST, _BumpTex_ST;
half _FoamRange;
half4 _FoamTint;
half _DistortionIntensity;
half4 _DistortionSpeed, _NormalSpeed;
half _Specular, _Smoothness;
float _BumpScale;
half _WaterAlpha;
half4 _CausticTex_ST;
half _CausticIntensity;
half4 _ShadowColor;
half _CausticScale;
half _CausticFacade;
half _DepthForCol;
half4 _WaterDepthWSColor;
half _fresnelScale;
half4 _fresnelColor;
CBUFFER_END

TEXTURE2D(_BumpTex);    SAMPLER(sampler_BumpTex);
TEXTURE2D(_FoamTex);    SAMPLER(sampler_FoamTex);
TEXTURE2D(_DistortionTex);          SAMPLER(sampler_DistortionTex);
TEXTURE2D(_CameraDepthTexture);     SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_CameraOpaqueTexture);    SAMPLER(sampler_CameraOpaqueTexture);
TEXTURECUBE(_RefectionTex);         SAMPLER(sampler_RefectionTex);
TEXTURE2D(_CausticTex);             SAMPLER(sampler_CausticTex);

float3 DecodeNormalWS(float3x3 TW, float4 normalTex)
{
    float3 bumpWS = UnpackNormal(normalTex);                              //解包，也就是将法线从-1,1重新映射回0,1
    bumpWS.xy *= _BumpScale;
    bumpWS.z = sqrt(1.0 - saturate(dot(bumpWS.xy, bumpWS.xy)));
    bumpWS = mul(TW, bumpWS);             //将切线空间中的法线转换到世界空间中
    return bumpWS;
}

float2 TexNoTile(float2 InputUV)
{
    float2 center = floor(InputUV) + 0.5;
    float Rotation = frac(sin(dot(floor(InputUV),float2(12.9898,78.233)))*43758.5453);
    Rotation = floor(saturate(Rotation) * 1060 / 90) * 90 * 1 * 57.3;
    InputUV -= center;
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2x2 rMatrix = float2x2(c,-s,s,c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2-1;
    InputUV.xy = mul(InputUV.xy,rMatrix);
    InputUV.xy += center;
    return InputUV;
}
// TEXTURE2D(tex); SAMPLER(samplerTex);
// #define UNITY_DECLARE_TEX2D(tex) Texture2D tex; SamplerState sampler##tex
// real4 TexNoTileY(TEXTURE2D_ARGS(tex, samplerTex),  float2 uv) {
//     half w1;
//     half w2;
//     half w3;
//     half2 vertex1;
//     half2 vertex2;
//     half2 vertex3;
//     float2 uv1;
//     float2 uv2;
//     float2 uv3;
// 	const float2x2 gridToSkewedGrid = float2x2(1,0,0,-0.57735,1.1547);
//     float2 skewedCoord = mul(gridToSkewedGrid,uv);
//     in2 baseID = in2(floor(skewedCoord));
//     half3 temp = half3(frac(skewedCoord),0);
//     temp.z = 1 - temp.x - temp.y;
//     if(temp.z>0)
//     {
//         w1 = temp.z;
//         w2 = temp.y;
//         w3 = temp.x;
//         vertex1 = baseID;
//         vertex2 = baseID + int2(0,1);
//         vertex3 = baseID + int2(1,0);
//     }
//     else
//     {
//         w1 = -temp.z;
//         w2 = 1 - temp.y;
//         w3 = 1 - temp.x;
//         vertex1 = baseID + int2(1,1);
//         vertex2 = baseID + int2(0,1);
//         vertex3 = baseID + int2(1,0);
//     }
//     uv1 = uv + frac(sin(mul(half2x2(127.1,311.7,269.5,183.3),vertex1)) * 43758.5453);
//     uv2 = uv + frac(sin(mul(half2x2(127.1,311.7,269.5,183.3),vertex2)) * 43758.5453);
//     uv3 = uv + frac(sin(mul(half2x2(127.1,311.7,269.5,183.3),vertex3)) * 43758.5453);
//     float4 color1 = SAMPLE_TEXTURE2D(tex,samplerTex,uv1);
//     float4 color2 = SAMPLE_TEXTURE2D(tex,samplerTex,uv2);
//     float4 color3 = SAMPLE_TEXTURE2D(tex,samplerTex,uv3);
//     return color1 + color2 + color3;
// }

#endif
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

real2 Rotate2D(float2 uv, float2 center, float rotation){
    uv -= center;
    float s = sin(rotation);
    float c = cos(rotation);
    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix*2 - 1;
    //multiply the UVs by the rotation matrix
    uv.xy = mul(uv.xy, rMatrix);
    uv += center;
    return uv;
}

// 注意:用这个函数，uv不能frac
real TexNoTileY(TEXTURE2D_PARAM(tex, samplerTex),  float2 uv, float4 _ST ,float rotation) {
    float2 a = uv * _ST.xy;
    float2 aa = floor(a) * 0.6;
    float aT = SAMPLE_TEXTURE2D(tex, samplerTex, aa + _ST.zw).r; // add
    float2 b = frac(a);
    float2 rot = Rotate2D(b, float2(0.5,0.5), aT * rotation);// add
    float2 finalUVa = aT + rot;
    float A = SAMPLE_TEXTURE2D(tex, samplerTex, finalUVa).r;
    float T = saturate(smoothstep(0.8,1,length((b-0.5)*2)));
    float B = SAMPLE_TEXTURE2D(tex, samplerTex, a).r;
    return lerp(A,B,T); //float4(float2(a.x,a.y),1,1);//

}

#endif
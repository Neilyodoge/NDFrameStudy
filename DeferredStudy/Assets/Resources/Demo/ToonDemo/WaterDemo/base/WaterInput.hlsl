#ifndef WATERINPUT
#define WATERINPUT

#define UNITY_PI 3.1415

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"


CBUFFER_START(UnityPerMaterial)
float4 _WaterSideTint;
float _waveSpeed,_flatNormal;
float4 _WaveA,_WaveB;
float _SHIntensity;
float _CartoonSpecularRoughness;
float4 _highLightDir;
float _debug1, _Debug;
float _VertexIntensity, _CartoonSpecularScale;
float _FoamSide, _FoamHeight,_DampSide;
float4 _WaterColor, _WaterSideColor, _SpecularColor, _CartoonSpecular;
float _DepthIntensity;
half4 _FoamSpeed;
half4 _FoamTex_ST, _DistortionTex_ST, _BumpTex_ST, _VertexAnim_ST,_DetailBumpTex_ST;
half _FoamRange;
half4 _FoamTint;
half _DistortionIntensity;
half4 _DistortionSpeed, _NormalSpeed, _VertexAnimSpeed;
half _Specular, _HeightScale;
float _WaterBumpScale,_DetailBumpScale;
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
//feature
int _NoTiling,_UseRamp,_UseBlend,_UseWaterSide;
float4 _SS;
CBUFFER_END

TEXTURE2D(_ramp);     SAMPLER(sampler_ramp);
TEXTURE2D(_VertexAnim);     SAMPLER(sampler_VertexAnim);
TEXTURE2D(_BumpTex);        SAMPLER(sampler_BumpTex);
TEXTURE2D(_DetailBumpTex);        SAMPLER(sampler_DetailBumpTex);
TEXTURE2D(_FoamTex);        SAMPLER(sampler_FoamTex);
TEXTURE2D(_DistortionTex);          SAMPLER(sampler_DistortionTex);
TEXTURE2D(_CameraDepthTexture);     SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_CameraOpaqueTexture);    SAMPLER(sampler_CameraOpaqueTexture);
TEXTURECUBE(_RefectionTex);         SAMPLER(sampler_RefectionTex);
TEXTURE2D(_CausticTex);             SAMPLER(sampler_CausticTex);

float3 TransformTangentToWorldNormal(float3x3 TBN, float4 normalTex ,float NormalScale)
{
    // Texture Type = NormalMap
    //float3 normalTS = UnpackNormalScale(normalTex,NormalScale);         //解包，也就是将法线从[0,1]重新映射回[-1,1]
    // Texture Type = Default
    float3 normalTS = normalTex;
    normalTS.xy = (normalTS * 2 - 1) * NormalScale;
    normalTS.z = sqrt(1.0 - saturate(dot(normalTS.xy, normalTS.xy)));
    float3 bumpWS = TransformTangentToWorld(normalTS,TBN);                       //将切线空间中的法线转换到世界空间中
    return normalize(bumpWS);
}

// 法线混合方式 UDN
// https://blog.selfshadow.com/publications/blending-in-detail/?tdsourcetag=s_pcqq_aiomsg
float3 UNDNormal(float3 n1, float3 n2)
{
    // UNDNormal
    float3 r = float3(n1.xy + n2.xy,n1.z);
    return normalize(r);
}

float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
{
    float steepness = wave.z;
    float wavelength = wave.w;
    float k = 2 * UNITY_PI / wavelength;
    float c = sqrt(_waveSpeed / k);               
    float2 d = normalize(wave.xy);
    float f = k * (dot(d, p.xz) - c * _Time.y);
    float a = steepness / k;                // _Amplitude

    tangent += float3(
        - d.x * d.x * (steepness * sin(f)),
        d.x * (steepness * cos(f)),
        - d.x * d.y * (steepness * sin(f))
    );
    binormal += float3(
        - d.x * d.y * (steepness * sin(f)),
        d.y * (steepness * cos(f)),
        - d.y * d.y * (steepness * sin(f))
    );
    return float3(// 输出顶点偏移量
    d.x * (a * cos(f)),
    a * sin(f),
    d.y * (a * cos(f))
    );
}

// GGX高光
half GGX_HeighLight(half r,half NoH,half3 LoH)
{
	half r2 = r * r;
	half part = (r2 - 1) * NoH * NoH + 1;
	half spe = r2 / ((4*r + 2) * part * part * LoH * LoH);
	return spe;
}

#if _NOTILING
    real2 Rotate2D(float2 uv, float2 center, float rotation)
    {
        uv -= center;
        float s = sin(rotation);
        float c = cos(rotation);
        //center rotation matrix
        float2x2 rMatrix = float2x2(c, -s, s, c);
        rMatrix *= 0.5;
        rMatrix += 0.5;
        rMatrix = rMatrix * 2 - 1;
        //multiply the UVs by the rotation matrix
        uv.xy = mul(uv.xy, rMatrix);
        uv += center;
        return uv;
    }

    // 注意:用这个函数，uv不能frac
    real noTiling(TEXTURE2D_PARAM(tex, samplerTex), float2 uv, float4 _ST, float rotation)
    {
        float2 a = uv * _ST.xy;
        float2 aa = floor(a) * 0.6;
        float aT = SAMPLE_TEXTURE2D(tex, samplerTex, aa + _ST.zw).r; // add
        float2 b = frac(a);
        float2 rot = Rotate2D(b, float2(0.5, 0.5), aT * rotation);// add
        float2 finalUVa = aT + rot;
        float A = SAMPLE_TEXTURE2D(tex, samplerTex, finalUVa).r;
        float T = saturate(smoothstep(0.8, 1, length((b - 0.5) * 2)));
        float B = SAMPLE_TEXTURE2D(tex, samplerTex, a).r;
        return lerp(A, B, T) ; 

    }
#endif // _NOTILING

#endif
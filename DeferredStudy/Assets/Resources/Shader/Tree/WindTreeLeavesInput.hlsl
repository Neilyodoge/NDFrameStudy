#ifndef WINDTREELEAVESINPUT
#define WINDTREELEAVESINPUT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//#if HEIGHT_FOG | _ENABLE_BILINEAR_FOG | ATMOSPHERIC_FOG_DAY | ATMOSPHERIC_FOG_NIGHT
//#include "Packages/com.pwrd.time-of-day/Resources/Shader/Include/FogCore.hlsl"
//#endif

#define UNITY_PI            3.14159265359f

CBUFFER_START(UnityPerMaterial)
// base
float4 _MainTex_ST;
float4 _BaseMap_ST;
float4 _WindDirection;
float4 _WindTex_ST;
float _TreeLerpIntensity;
float _Cutoff;
float _CutIntensity;
float _LocalShadowDepthBias;
//NPR
float _saturate;
float _FaceLightGrayIntensity;
float _FaceLightGrayScale;
float _ToonCutPos;
float _SHDarkPart;
//heightLight
float _heightLightSmooth;
//ref
float _refIntensity;
float _refDis;
float _refScale;
//ao
float _AORange;
//Subsurface
float _SubSurfaceGain;
float _SubSurfaceScale;
//HardRim
float _HardRimDistanceIntensity;
float _HardRimWidth;
float _LodMask;
//wind
float _Magnitude;
float _Frequency;
float _WindSineIntensity;
float _WindTexScale;
float _WindTexIntensity;
float _WindTexMoveSpeed;
float _ModelScaleCorrection;
//dither
float _DitherAmountMax;
float _DitherAmountMin;
float _CustomBloomIntensity;
float _CustomBloomAlphaOffset;

int _Debug,_FlatClip;
half4 _heightLightColor;
half4 _AOTint;
half4 _HardRimTint;
half4 _BaseColor;
half4 _LerpColor;
half4 _DarkColor;
half _LightIntensity;
half _TreeLerpTop;
half _TreeLerpRoot;
CBUFFER_END
float _SHIntensity;  // Gloab Property

TEXTURE2D(_WindTex);
SAMPLER(sampler_WindTex);
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

#include "WindOfFoliage.hlsl"

struct appdata
{
    float4 positionOS: POSITION;
    float3 normalOS: NORMAL;
    float3 tangentOS: TANGENT;
    float4 color: COLOR;
    float2 uv: TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 positionHCS: SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 positionWS: TEXCOORD1;
    float3 normalWS: TEXCOORD2;
    float treeParam: TEXCOORD3;
    float4 ambient: TEXCOORD4;
    float3 baseNormal: TEXCOORD5;        // 目前是把模型平滑前的法线存进了切线中
    //HEIGHT_FOG_COORDS(7)
    float fogCoord: TEXCOORD6;
    #if _NeedScreenPos
        float2 screenUV: TEXCOORD7;
    #endif
    #if _DEBUGMODE
        float debugWind: TEXCOORD8;
        float4 VertexColor: TEXCOORD9;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};
struct appdataDepth
{
    float4 positionOS: POSITION;
    float3 normalOS: NORMAL;
    float3 tangentOS: TANGENT;
    float4 color: COLOR;
    float2 uv: TEXCOORD0;
};

struct v2fDepth
{
    float4 positionHCS: SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 positionWS: TEXCOORD1;
    float3 baseNormal: TEXCOORD2;        // 目前是把模型平滑前的法线存进了切线中
};


void Unity_Dither(float In, float2 ScreenPosition, out float Out)
{
	float2 SCREEN_PARAM = float2(1, 1);
	float2 uv = ScreenPosition.xy* SCREEN_PARAM;
	const float4x4 DITHER_THRESHOLDS =
	{
		1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
		13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
		4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
		16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
	};
	Out = In - DITHER_THRESHOLDS[uint(uv.x) % 4][uint(uv.y) % 4];
}

half GGX_HeighLight(half r, half NoH, half LoH)
{
    half r2 = r * r;
    half part = (r2 - 1) * NoH * NoH + 1;
    half spe = r2 / ((4 * r + 2) * part * part * LoH * LoH);
    return spe;
}

half3 WardSpecularModel(half3 lightDir, half3 viewDir, half3 normal, half roughness)
{
    half3 halfVector = normalize(lightDir + viewDir);
    half NdotH = saturate(dot(normal, halfVector));
    half alphaX = roughness * roughness;
    half alphaY = roughness * roughness;
    half2 alpha = half2(alphaX * NdotH, alphaY * NdotH);
    half D = exp(-(1 / (dot(alpha, alpha) * pow(NdotH, 4))));
    half3 fresnel = 0.04 + (1 - 0.04) * pow(1 - dot(viewDir, halfVector), 5);
    half3 specular = D * fresnel / (4 * dot(normal, viewDir) * dot(normal, lightDir));

    return specular;
}

#endif

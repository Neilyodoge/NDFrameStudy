#ifndef HAIR_DITHER_INPUT_INCLUDED
#define HAIR_DITHER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    half _PixelDepthOffset;
    half _Scatter;
    half _SpecularR_Intensity;
    half _SpecularTRT_Intensity;
    half _SpecularTT_Intensity;
    half _Brightness;
    half _Roughness;
    half4 _Test;

    half3 _Tangent;
    half3 _TangentA;
    half3 _TangentB;

    half _EdgeMaskContrast;
    half _EdgeMaskMin;

    half3 _RootColor;
    half3 _TipColor;
    half3 _Tip2Color;

CBUFFER_END

TEXTURE2D(_DepthMap);       SAMPLER(sampler_DepthMap);
TEXTURE2D(_OpacityMap);       SAMPLER(sampler_OpacityMap);
TEXTURE2D(_ColorMap);       SAMPLER(sampler_ColorMap);
TEXTURE2D(_IdMap);       SAMPLER(sampler_IdMap);
TEXTURE2D(_RootMap);       SAMPLER(sampler_RootMap);
TEXTURE2D(_RootMap2);       SAMPLER(sampler_RootMap2);

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;

    outSurfaceData.albedo = albedoAlpha.rgb;

    half4 specularSmoothness = 1;
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = specularSmoothness.rgb;
    outSurfaceData.smoothness = specularSmoothness.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = 0;
}

#endif

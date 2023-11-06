Shader "Athena/Foliage/StylizedWindTreeLeavesNew_PreZ"
{
    Properties
    {
        [HideInInspector]_MainTex ("", 2D) = "white" { }
        [HideInInspector]_BaseColor ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_LerpColor ("垂直染色", Color) = (1, 1, 1, 1)
        [HideInInspector]_TreeLerpTop ("树冠染色顶端高度", Range(0, 40)) = 20
        [HideInInspector]_TreeLerpRoot ("树冠染色底端高度", Range(0, 40)) = 1
        [HideInInspector]_TreeLerpIntensity ("树冠染色强度", range(1, 100)) = 20
        [MainTexture]_BaseMap ("Albedo", 2D) = "white" { }
        [HideInInspector]_Cutoff ("Alpha Cutoff(阴影)", Range(0.0, 1.0)) = 0.5
        [Toggle]_FlatClip ("平面剔除开关", int) = 1
        _CutIntensity ("立面剔除强度", Range(0, 1)) = 0.95
        [HideInInspector]_LocalShadowDepthBias ("影子偏移值", range(-0.3, 0.3)) = 0.25

        //[Header(NPR)]
        [HideInInspector]_saturate ("饱和度", range(0, 1)) = 1
        [HideInInspector]_LightIntensity ("亮部强度", range(1, 5)) = 2
        [HideInInspector]_FaceLightGrayScale ("亮部灰阶范围", range(0, 7)) = 5
        [HideInInspector]_FaceLightGrayIntensity ("亮部灰阶强度", range(0, 7)) = 0
        [HideInInspector]_DarkColor ("暗部颜色", Color) = (0, 0, 0, 1)
        [HideInInspector]_ToonCutPos ("明暗交界线位置", range(-1, 1)) = 0
        [HideInInspector]_SHDarkPart ("阴影中环境光强度", range(0, 10)) = 1

        //[Header(HeightLight)]
        [HideInInspector]_heightLightSmooth ("高光范围", range(1, 100)) = 20
        [HideInInspector]_heightLightColor ("高光颜色", Color) = (0, 0, 0, 1)
        
        // [Header(RefPart)]
        [HideInInspector]_refIntensity ("反射阶强度", range(0, 1)) = 0
        [HideInInspector]_refDis ("反射阶有效距离", range(0, 500)) = 500
        [HideInInspector]_refScale ("反射阶范围", range(0.1, 5)) = 0.2

        //[Header(AO)]
        [HideInInspector]_AOTint ("AO颜色", color) = (1, 1, 1, 1)
        [HideInInspector]_AORange ("AO范围", Range(0.1, 1)) = 1

        //[Header(Subsurface)]
        [HideInInspector]_SubSurfaceGain ("透光强度", Range(0, 5)) = 1
        [HideInInspector]_SubSurfaceScale ("透光范围", Range(-1, 1)) = -1

        //[Header(Debug)]
        [HideInInspector][Toggle(_DEBUGMODE)]_DebugMode ("Debug Mode", Float) = 0
        [HideInInspector]_Debug ("Debug AO", Float) = 1

        //[Space(20)]
        //[Header(HardRim)]
        [HideInInspector] _UsingHardRim ("使用HardRim", int) = 0
        [HideInInspector][HDR]_HardRimTint ("HardRimColor", Color) = (1, 1, 1, 1)
        [HideInInspector]_LodMask ("LodMask", range(0, 1)) = 0.5       // 主要由于颜色渐变导致的灰度值域需要剔除
        [HideInInspector]_HardRimDistanceIntensity ("HardRim距离颜色变化 ", float) = 2000  // 主要实现颜色渐变
        [HideInInspector]_HardRimWidth ("HardRim宽度", range(0, 0.02)) = 0.01

        [Header(Wind)]
        _Magnitude ("随风漂移强度", float) = .5
        _Frequency ("随风飘动频率", float) = .5
        _WindSineIntensity ("风的规律波动强度", Range(0, 2)) = 0.5
        _WindDirection ("风向 (x,y,z))", Vector) = (1, 0, 1, 0)
        _WindTex ("风的波动贴图", 2D) = "black" { }
        _WindTexScale ("风的波动图的大小", float) = 1
        _WindTexIntensity ("风的波动贴图的强度", Range(0, 2)) = 1
        _WindTexMoveSpeed ("风的波动贴图的速度", float) = 0.1
        [HideInInspector]_ModelScaleCorrection ("ModelScaleCorrection ", Float) = 0.2

        //[Space(20)]
        [HideInInspector]_CustomBloomIntensity ("_CustomBloomIntensity", Range(0, 2)) = 1.0
        [HideInInspector]_CustomBloomAlphaOffset ("_CustomBloomAlphaOffset", Range(-1, 1)) = 0

        [Space(20)]
        [Header(Dither)]
        _DitherAmountMax ("DitherMax", Range(0, 25)) = 0
        _DitherAmountMin ("DitherMin", Range(0, 25)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest-1" "IgnoreProjector" = "True" } // 这里改队列才影响
        Pass
        {
            //* wanghaoyu 直接把DepthOnly拿过来用了，队列不太好用，要手动调一下
            // Name "DepthOnly"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            ColorMask 0
            ZWrite On

            HLSLPROGRAM

            // PC和移动端作区分
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12) || defined(SHADER_API_D3D11_9X) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL)
                #define IS_PC 1
            #endif
            #pragma vertex vertDepth
            #pragma fragment fragDepth
            #pragma target 3.0
            //#pragma multi_compile_instancing  /// 树固定开 instancing,但是这里不能精简否则 bundle 里有问题（星空遗留的情况，不清楚现在是否还这样）
            //#pragma multi_compile_fwdbase
            #pragma multi_compile _ _MRTEnable
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #define _MAIN_LIGHT_SHADOWS_CASCADE 1
            
            //#pragma shader_feature_local _ _DEBUGMODE
            // Shader分档
            #pragma multi_compile _TierHigh _TierMedium _TierLow
            #if defined(_TierHigh)
                // #define _LIGHTPROBE 1
                // #define _SUBSURFACE 1
                #define _VERTEXANIMTION 1
                // #pragma shader_feature_local _HARDRIM
                #if defined(IS_PC)
                    // #pragma multi_compile _ _ADDITIONAL_LIGHTS
                #endif
            #elif defined(_TierMedium)
                // #define _LIGHTPROBE 1
                // #define _SUBSURFACE 1
                #define _VERTEXANIMTION 1
            #elif defined(_TierLow)
                #define _LIGHTPROBE 1
            #endif
            // Shader分档
            
            #include "WindTreeLeavesPassNew.hlsl"
            ENDHLSL

        }
    }
}
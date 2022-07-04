Shader "Neilyodog/Water"
{
    // Neilyodog 2021.12.13
    // 混合部分有一些问题
    // 1.透明度控制并不是很理想
    // 2.泡沫和深度没分开
    // cube反射暂时没网上叠
    // 深度的时候多少会有一丢丢误差
    Properties
    {
        _WaterAlpha ("Alpha", range(0, 1)) = 0.5    // 没用
        _WaterColor ("水颜色", color) = (1, 1, 1, 1)
        _WaterDepthColor ("边缘深度颜色", color) = (1, 1, 1, 1)
        _WaterDepthWSColor ("深度颜色", color) = (1, 1, 1, 1)
        _DepthForCol ("深度颜色范围", float) = 1   

        _BumpTex ("Normal", 2D) = "white" { }
        _BumpScale ("法线强度", range(0, 2)) = 1
        _NormalSpeed ("法线速度", Vector) = (0, 0, 0, 0)

        [Space(10)]
        [Header(fresnel)]
        _fresnelScale("菲尼尔范围",float) = 5   
        _fresnelColor("菲尼尔颜色",color) = (1,1,1,1)

        [Space(10)]
        [Header(Shadow)]
        _ShadowColor ("阴影颜色", color) = (1, 1, 1, 1)

        [Space(10)]
        [Header(Refection))]
        _RefectionIntensity ("反射强度", range(0, 1)) = 1
        _RefectionTex ("反射贴图", Cube) = "white" { }

        [Space(10)]
        [Header(HighLight)]
        [HDR]_CartoonSpecular ("Toon高光颜色",color) = (1, 1, 1, 1)
        [HDR]_SpecularColor ("高光颜色", color) = (1, 1, 1, 1)
        _Specular ("高光强度", float) = 1
        _Smoothness ("高光范围", float) = 1

        [Space(10)]
        [Header(Distortion)]
        _DistortionTex ("扭曲", 2D) = "white" { }
        _DistortionIntensity ("扭曲强度", range(0, 0.3)) = 1
        _DistortionSpeed ("扭曲速度", Vector) = (0, 0, 0, 0)

        [Space(10)]
        [Header(Caustic)]
        _CausticTex ("焦散贴图", 2D) = "white" { }
        _CausticIntensity ("焦散强度", float) = 1
        _CausticScale ("焦散范围", float) = 1
        _CausticFacade ("焦散立面", range(0, 0.5)) = 0.15

        [Space(10)]
        [Header(Foam)]
        _FoamSpeed ("泡沫速度/焦散速度", Vector) = (0, 0, 0, 0)
        _FoamTex ("泡沫纹理", 2D) = "white" { }
        _FoamTint ("泡沫颜色", color) = (1, 1, 1, 1)
        _FoamRange ("泡沫范围", range(0, 1)) = 1
        _DepthIntensity ("深度强度", range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            // receive shadow
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT // 软阴影

            #include "WaterPass.hlsl"
            
            ENDHLSL

        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL

        }
    }
}

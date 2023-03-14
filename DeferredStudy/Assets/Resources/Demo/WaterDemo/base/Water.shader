Shader "Neilyodog/Water"
{
    //* 水下还有小问题，采样不太对
    // Neilyodog 2022.8.31
        // 焦散的uv是DepthVS,剩下基本都是WS
        // 转片的方向会导致ToonHeightLight效果出问题，没查原因
        // 焦散其实应该跟着呼吸动
    // 顶点色R控制扭曲强度和顶点动画强度。建议岸边画点
    Properties
    {
        _SHIntensity("环境光比例",range(0,1)) = 1
        _waveSpeed("wave速度",float) = 9.8
        _WaveA("_WaveA",Vector) = (0, 0, 0, 0)  // xy方向，z强度，w tiling
        _WaveB("_WaveB",Vector) = (0, 0, 0, 0)
        [Toggle]_CustomSunPosON("开启自定义sun位置",int) = 0
        _CustSunPos("自定义sun位置",Vector) = (0, 0, 0, 0)

        [Toggle]_UseBlend("开启混色",int) = 1
        [Toggle]_UseRamp("UseRamp",int) = 1
        _ramp("ramp",2d) = "black" {}
        [Toggle(_NOTILING)]_NoTiling("NoTilingOn",int) = 1
        _WaterAlpha ("水整体透明度", range(0, 1)) = 0.8    // 其实是和copyColor做lerp
        _WaterSideColor ("RGB边缘深度颜色A强度", color) = (1, 1, 1, 1)
        _WaterColor ("水颜色", color) = (1, 1, 1, 1)
        _WaterDepthWSColor ("深水颜色", color) = (1, 1, 1, 1)
        _DepthForCol ("深水颜色范围", float) = 1 

        [Space(10)]
        [Header(vertexAnim)]
        _VertexAnim("顶点动画贴图", 2D) = "black" { }
        _VertexAnimSpeed("顶点动画速度", vector) = (0,0,0,0)
        _VertexIntensity("顶点动画强度", range(0,20)) = 0


        [Space(10)]
        [Header(Normal)]
        _flatNormal("法线平整距离",range(0,0.05)) = 0
        _BumpTex ("Normal", 2D) = "white" { }
        _WaterBumpScale ("法线强度", range(0, 2)) = 0.5
        _NormalSpeed ("法线速度", Vector) = (0, 0, 0, 0)    // xy:Normal  zw:DetailNormal
        _DetailBumpTex ("DetailNormal", 2D) = "white" { }
        _DetailBumpScale ("Detail法线强度", range(0, 2)) = 0.5

        [Space(10)]
        [Header(fresnel)]
        _fresnelScale("菲尼尔范围",float) = 5   
        _fresnelColor("菲尼尔颜色",color) = (1,1,1,1)

        [Space(10)]
        [Header(Shadow)]
        _ShadowColor ("RGB阴影颜色A阴影内DF强度", color) = (1, 1, 1, 1) // D高光项 F菲尼尔项

        [Space(10)]
        [Header(Refection))]
        _RefectionIntensity ("反射强度", range(0, 1)) = 1
        _RefectionTex ("反射贴图", Cube) = "white" { }

        [Space(10)]
        [Header(HighLight)]
        [HDR]_CartoonSpecular ("Toon高光颜色",color) = (1, 1, 1, 1)
        _CartoonSpecularRoughness ("Toon高光粗糙度",float) = 0.5
        _CartoonSpecularScale ("Toon高光范围Min",range(0,1)) = 0.5
        [HDR]_SpecularColor ("高光颜色", color) = (1, 1, 1, 1)
        _Specular ("高光强度", float) = 1
        _HeightScale ("高光范围", float) = 1

        [Space(10)]
        [Header(Distortion)]
        _DistortionTex ("扭曲", 2D) = "black" { }
        _DistortionIntensity ("扭曲强度", range(0, 0.3)) = 1
        _DistortionSpeed ("扭曲速度", Vector) = (0, 0, 0, 0)

        [Space(10)]
        [Header(Caustic)]
        _CausticTex ("焦散贴图", 2D) = "black" { }
        _CausticIntensity ("焦散强度", float) = 1
        _CausticScale ("焦散范围", float) = 1
        _CausticFacade ("焦散立面", range(0, 0.5)) = 0.15

        [Space(10)]
        [Toggle(_WATERSIDE)] _UseWaterSide("使用WaterSide",int) = 0
        [Header(Foam)]
        _FoamSpeed ("泡沫速度/Y水边速度/焦散速度", Vector) = (0, 0, 0, 0)
        _FoamTex ("泡沫纹理", 2D) = "white" { }
        _FoamTint ("泡沫颜色", color) = (1, 1, 1, 1)
        _FoamRange ("泡沫范围", range(0, 1)) = 1
        _DepthIntensity ("深度强度", range(0, 10)) = 1
        [Header(WaterSide)]
        _WaterSideTint("岸边潮湿颜色",Color) = (1,1,1,1)
        _FoamSide ("水边范围", float) = 0.8
        _DampSide ("潮湿范围", float) = 0.32
        _FoamHeight ("水边高度修正值", float) = 1  // 用来锁定waterside的范围

        [HideInInspector]_Debug("Debug", Float) = 1
        
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE // P5要把_CASCADE去掉
            #pragma multi_compile _ _SHADOWS_SOFT // 软阴影

            // Debug
            #pragma shader_feature _ _DEBUGMODE
            #pragma shader_feature_local _NOTILING  // 去贴图重复度用的,但只能用在灰度图上
            #pragma shader_feature_local _WATERSIDE // Foam和WaterSide效果

            #include "WaterPass.hlsl"
            
            ENDHLSL

        }
        //* wanghaoyu 加完发现效果并不好
        // Pass
        // {
        //     Name "ShadowCaster"
        //     Tags { "LightMode" = "ShadowCaster" }

        //     ZWrite On
        //     ZTest LEqual
        //     ColorMask 0
        //     Cull[_Cull]

        //     HLSLPROGRAM

        //     // Required to compile gles 2.0 with standard srp library
        //     #pragma prefer_hlslcc gles
        //     #pragma exclude_renderers d3d11_9x
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Material Keywords
        //     // #pragma shader_feature _ALPHATEST_ON

        //     //--------------------------------------
        //     // GPU Instancing
        //     #pragma multi_compile_instancing
        //     // #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

        //     #pragma vertex ShadowPassVertex
        //     #pragma fragment ShadowPassFragment

        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
        //     // #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        //     #include "WaterInput.hlsl"

        //     float3 _LightDirection;

        //     struct Attributes
        //     {
        //         float4 positionOS   : POSITION;
        //         float3 normalOS     : NORMAL;
        //         float4 tangentOS    : TANGENT;
        //         float2 texcoord     : TEXCOORD0;
        //         UNITY_VERTEX_INPUT_INSTANCE_ID
        //     };

        //     struct Varyings
        //     {
        //         float2 uv           : TEXCOORD0;
        //         float4 positionCS   : SV_POSITION;
        //     };

        //     Varyings ShadowPassVertex(Attributes input)
        //     {
        //         Varyings output;
        //         UNITY_SETUP_INSTANCE_ID(input);

        //         output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
        //         // 扩展影子
        //         VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS, input.tangentOS);
        //         float3 tangentWS = normal.tangentWS;
        //         float3 bitangentWS = normal.bitangentWS;
        //         float3 normalWS = normal.normalWS;

        //         float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
        //         float2 animUV = float2(frac(_VertexAnimSpeed.x * _Time.y), frac(_VertexAnimSpeed.y * _Time.y))        // 法线偏移
        //                         + (worldPos.xz * _VertexAnim_ST.xy + _VertexAnim_ST.zw);
        //         half animTex = SAMPLE_TEXTURE2D_LOD(_VertexAnim, sampler_VertexAnim,animUV,0).r;
        //         float VertexAnim = GerstnerWave(_WaveA, worldPos, tangentWS, bitangentWS); // 这里有个问题,法线是贴图前的
        //         VertexAnim += GerstnerWave(_WaveB, worldPos, tangentWS, bitangentWS);
        //         VertexAnim = (VertexAnim + pow(animTex,4) * _VertexIntensity)/2;
        //         worldPos += VertexAnim;
        //         float4 clipPos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
        //         #if UNITY_REVERSED_Z
        //             clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        //         #else
        //             clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        //         #endif
        //         output.positionCS = clipPos;
        //         return output;
        //     }

        //     half4 ShadowPassFragment(Varyings input) : SV_TARGET
        //     {
        //         Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
        //         return 0;
        //     }
        //     ENDHLSL
        // }
    }
    CustomEditor "WaterShaderGUI"
}

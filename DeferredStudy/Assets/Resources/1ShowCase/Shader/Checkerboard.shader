Shader "Neilyodog/Checkerboard"
{
    Properties
    {
        _MainTex("MainTex",2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull Mode",int) = 0

        [Space(20)]
        [Toggle]_UsingCheckerboard("Using Checkerboard?",float) = 0
        _Repeat("Repeat",float) = 5
        
        [Space(20)]
        [Toggle(_PANARREF_ON)] _PanarRef_ON("Use PlanarReflection",float) = 0
        _RefIntensity("反射强度",range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull [_Cull]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog   //FOG_LINEAR FOG_EXP FOG_EXP2

            #pragma shader_feature_local _PANARREF_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD1;
                float fogCoord  : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
            half _Repeat;
            half4 _Color;
            #if defined(_PANARREF_ON)
                float4 _PlanarReflectionTexture_TexelSize;
                float _RefIntensity;
            #endif
            half _UsingCheckerboard;

            half4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            // _PlanarReflectionTexture
            TEXTURE2D(_PlanarReflectionTexture);
            SAMPLER(sampler_PlanarReflectionTexture);

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.positionOS = v.positionOS;
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 c;
                float2 uv = floor(i.uv * 2) * 0.5 * _Repeat;
                float checker = frac(uv.x + uv.y) * 2;

                half mask = i.positionOS.y + 0.55;
                c = checker * mask;
                c *= _Color;

                // 不用棋盘格
                half4 MainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * _Color;
                c = lerp(MainTex,c,_UsingCheckerboard);

                // PanarRef
                #if defined(_PANARREF_ON)
                float2 ScrPos = i.positionHCS.xy / _ScreenParams.xy;
                float2 screenUV = ScrPos.xy * _ScreenParams.xy * _PlanarReflectionTexture_TexelSize;
                half4 RefTex = SAMPLE_TEXTURE2D(_PlanarReflectionTexture,sampler_PlanarReflectionTexture,ScrPos);
                c.rgb = lerp(c.rgb,RefTex.rgb,_RefIntensity);
                #endif

                c.rgb = MixFog(c, i.fogCoord);
                return c;
            }
            ENDHLSL
        }
    }
}

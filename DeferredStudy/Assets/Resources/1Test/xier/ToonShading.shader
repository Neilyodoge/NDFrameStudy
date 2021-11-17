Shader "Neilyodog/ToonShading"
{
    Properties
    { 
        _MainTex("主贴图",2D) = "white"{}
        _MainColor("主颜色",color) = (1,1,1,1)
        _DarkColor("暗部颜色",color) = (1,1,1,1)
        _ToonCutSharpness ("Toon 明暗交界锋利度", Range(1, 20)) = 10
        [Space(20)][Header(Rim)]
        _RimlightCutPosition ("向阳边缘光 边界位置", float) = 5
        _RimlightSharpness ("边缘光 边界锋利度", range(0, 1)) = 1
        [HDR]_RimlightColor ("边缘光 颜色，A强度", Color) = (1, 1, 1, 0)
        _SHIntensity ("环境光影响强度", range(0, 1)) = 1
    }
    SubShader
    {
        Tags {"Queue"="Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"   

            struct Attributes
            {
                float4 positionOS   : POSITION;  
                float4 color: COLOR; 
                float3 normalOS: NORMAL;
                float4 tangentOS: TANGENT;
                float2 uv : TEXCOORD0;     

            };
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 vertexColor : COLOR;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 normalWS: NORMAL;
                float3 tangentWS: TEXCOORD3;
                float3 bitangentWS: TEXCOORD4;
                float3 positionWS : TEXCOORD5;
            };            
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor,_DarkColor;
            float _ToonCutSharpness;
            float _SHIntensity;
            // rim
            half _RimlightSharpness, _RimlightCutPosition; 
            half4 _RimlightColor;                          
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.normalWS = normalInput.normalWS;
                o.tangentWS = normalInput.tangentWS;
                o.bitangentWS = normalInput.bitangentWS;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);
                return o;
            }    
            half4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * _MainColor;

                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 viewDirWS = GetCameraPositionWS() - i.positionWS;
                half VoL = saturate(dot(viewDirWS, mainLight.direction));
                half NoL = saturate(dot(i.normalWS, mainLight.direction));
                half NoV = saturate(dot(i.normalWS, viewDirWS));
                half3 VertexSH = SampleSH(i.normalWS);
                half adjustedNoL = saturate(NoL * _ToonCutSharpness);
                half fresnel = pow(1 - NoV, _RimlightCutPosition);
                fresnel = smoothstep(0, _RimlightSharpness, fresnel);

                col.rgb *= lerp(1, mainLight.color, adjustedNoL);                               // 平行光叠加
                col.rgb = lerp(col.rgb * _DarkColor.rgb, col.rgb, adjustedNoL);                     // 颜色二分
                col.rgb += VertexSH * _SHIntensity;                                             // sh
                col.rgb = lerp(col.rgb, _RimlightColor.rgb, fresnel * _RimlightColor.a);        // rim最后叠加,现在是逐顶点的

                col.rgb = MixFog(col.rgb,i.fogCoord);
                return col;
            }
            ENDHLSL
        }
    }
}

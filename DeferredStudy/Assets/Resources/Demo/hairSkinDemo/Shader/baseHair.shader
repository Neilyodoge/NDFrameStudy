Shader "Neilyodog/baseHair"
{
    Properties
    { 
        _MainTex("主贴图",2D) = "white"{}
        _MainColor("主颜色",color) = (1,1,1,1)
        _Cutoff("_Cutoff",range(0,1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            cull off
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "baseHairInput.hlsl"

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                o.posWS = vertexInput.positionWS;
                o.positionHCS = vertexInput.positionCS;
                o.uv = v.uv;

                o.normal = half4(normalInput.normalWS, viewDirWS.x);
                o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
                o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

                return o;
            }    
            half4 frag(Varyings i) : SV_Target
            {
                //init base data
                float2 uv = i.uv;
                float2 screenUV = i.screenPos.xy/i.screenPos.w;
                float3 viewDir = normalize(i.posWS - _WorldSpaceCameraPos);
                half nov = dot(i.normal, viewDir);
                //sample clipping maps
                half alpha = SAMPLE_TEXTURE2D_BIAS(_MainTex, sampler_MainTex, uv, -1).r;//  * _Test.r;
                clip(Dither(alpha, screenUV));

                half4 c = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                c *= _MainColor;
                return c;
            }
            ENDHLSL
        }
    }
}
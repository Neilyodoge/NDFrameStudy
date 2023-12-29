Shader "CC/VertexVisualization"
{
    Properties
    {
        [Enum(Color, 0, Alpha, 1, uv1, 2, uv2, 3, uv3, 4, uv4, 5, tangentWS, 6)] _ViewMode ("_ViewMode", float) = 0
        [Enum(RGB, 0, R, 1, G, 2, B, 3)] _ViewChannel ("_ViewChannel", float) = 0
        [Toggle(_Multiply_Diffuse)] _Multiply_Diffuse("_Multiply_Diffuse", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 300
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _Multiply_Diffuse
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            float _ViewMode;
            float _ViewChannel;
            CBUFFER_END
            
            struct VertexInput
            {
                float4 posOS   : POSITION;
                float3 normal : NORMAL;
                float4 vertexColor : COLOR;
                float3 uv1 : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
                float3 uv3 : TEXCOORD2;
                float3 uv4 : TEXCOORD3;
                float4 tangent : TANGENT;
            };

            struct VertexOutput
            {
                float4 posCS : SV_POSITION;
                float4 vertexColor : COLOR;
                
                float3 uv1 : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
                float3 uv3 : TEXCOORD2;
                float3 uv4 : TEXCOORD3;
                
                float3 posWS : TEXCOORD4;
                float3 normalWS : TEXCOORD5;
                float3 worldTangent : TEXCOORD6;
                
            };            

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                
                o.posCS = TransformObjectToHClip(v.posOS.xyz);
                o.vertexColor = v.vertexColor;

                o.uv1 = v.uv1;
                o.uv2 = v.uv2;
                o.uv3 = v.uv3;
                o.uv4 = v.uv4;
                
                o.posWS = TransformObjectToWorld(v.posOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.worldTangent = TransformObjectToWorldDir(v.tangent);
                return o;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                float3 normal = normalize(i.normalWS);
                float3 lightDir = GetMainLight().direction;
                half3 col = 0;


                switch (_ViewMode)
                {
                    case 0: //color
                        col = i.vertexColor.xyz;
                        break;
                    case 1: //alpha
                        col = i.vertexColor.aaa;
                        break;
                    case 2: //uv1
                        col = i.uv1;
                        break;
                    case 3: //uv2
                        col = i.uv2;
                        break;
                    case 4: //uv3
                        col = i.uv3;
                        break;
                    case 5: //uv4
                        col = i.uv4;
                        break;
                    case 6: //tangent
                        col = normalize(i.worldTangent);
                        break;
                    default:
                        break;
                }
                switch (_ViewChannel)
                {
                    case 0: //rgb
                        break;
                    case 1: //r
                        col = col.rrr;
                        break;
                    case 2: //g
                        col = col.ggg;
                        break;
                    case 3: //b
                        col = col.bbb;
                        break;
                    default:
                        break;
                }
                
                #ifdef _Multiply_Diffuse
                    half ndotl = dot(normal , lightDir);
                    //ndotl = ndotl*0.5+0.5;
                    col = col*ndotl;
                #endif

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
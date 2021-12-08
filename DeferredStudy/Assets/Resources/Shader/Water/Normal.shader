Shader "URPNotes/BumpMap"{
    Properties{
        _BaseColor("Base Color",Color) = (1.0,1.0,1.0,1.0)
        _MainTex("Main Texture",2D) = "white"{}
        _BumpTex("Bump Texture",2D) = "white"{}
        _BumpScale("Bump Scale",Float) = 1.0
    }
    SubShader{
        Tags{
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        pass{
            HLSLPROGRAM
                #pragma vertex Vertex
                #pragma fragment Pixel
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                TEXTURE2D(_BumpTex);
                SAMPLER(sampler_BumpTex);

                CBUFFER_START(UnityPerMaterial)
                    float _BumpScale;
                    float4 _BumpTex_ST;
                    half4 _BaseColor;
                    float _Gloss;
                CBUFFER_END

                struct vertexInput{

                    float4 vertex:POSITION;
                    float3 normalOS : NORMAL;
                    float2 uv:TEXCOORD0;
                    float4 tangentOS : TANGENT;
                    //注意tangent是float4类型，因为其w分量是用于控制切线方向的。
                };

                struct vertexOutput{

                    float4 pos:SV_POSITION;
                    float2 uv:TEXCOORD0;
                    float3 TW1:TEXCOORD1;
                    float3 TW2:TEXCOORD2;
                    float3 TW3:TEXCOORD3;
                };

                vertexOutput Vertex(vertexInput v){

                    vertexOutput o;
                    o.pos = TransformObjectToHClip(v.vertex.xyz);
                    VertexNormalInputs normal = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                    float3 worldNormal = normal.normalWS;
                    float3 worldTangent = normal.tangentWS;
                    float3 worldBinormal = normal.bitangentWS;
                    
                    float3 worldPos = TransformObjectToWorld(v.vertex.xyz);

                    //计算世界法线，世界切线和世界副切线

                    o.uv = v.uv;
                    //计算采样坐标

                    o.TW1 = float3(worldTangent.x,worldBinormal.x,worldNormal.x);
                    o.TW2 = float3(worldTangent.y,worldBinormal.y,worldNormal.y);
                    o.TW3 = float3(worldTangent.z,worldBinormal.z,worldNormal.z);
                    return o;
                }

                half4 Pixel(vertexOutput i):SV_TARGET{

                    /* 提取在顶点着色器中的数据 */
                    float3x3 TW = float3x3(i.TW1.xyz,i.TW2.xyz,i.TW3.xyz);
                    
                    /* 先计算最终的世界法线 */
                    float4 normalTex = SAMPLE_TEXTURE2D(_BumpTex,sampler_BumpTex,i.uv * _BumpTex_ST.xy + _BumpTex_ST.zw);        //对法线纹理采样
                    float3 bump = UnpackNormal(normalTex);                              //解包，也就是将法线从-1,1重新映射回0,1
                    bump.xy *= _BumpScale;
                    bump.z = sqrt(1.0 - saturate(dot(bump.xy,bump.xy)));
                    //这个z的计算是因为法线仅存储x和y信息，而z可以由x^2 + y^2 + z^2 = 1反推出来。（法线是单位矢量）
                    bump = mul(TW,bump);             //将切线空间中的法线转换到世界空间中

                    Light light = GetMainLight();
                    half3 normalOS = saturate(dot(light.direction,bump));
                    return half4(normalOS,1);

                }
            ENDHLSL
        }
    }
}
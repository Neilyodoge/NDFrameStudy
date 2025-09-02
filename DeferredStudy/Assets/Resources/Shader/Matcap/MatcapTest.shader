Shader "Custom/MatcapURP"
{
    Properties
    {
        _MatcapTex ("Matcap Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
            };

            sampler2D _MatcapTex;
            float4 _MatcapTex_ST;
            float4 _Color;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.positionOS));
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                // 构造相机相关的切线空间
                float3 up = float3(0,1,0);
                float3 X = normalize(cross(V, up));
                float3 Y = cross(V, X);

                // 用 dot 投影到切线空间
                float2 uv;
                uv.x = dot(N, X);
                uv.y = dot(N, Y);
                uv = uv * 0.5 + 0.5;

                // 采样 matcap 贴图
                float3 matcapCol = tex2D(_MatcapTex, uv).rgb;

                return half4(matcapCol * _Color.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}

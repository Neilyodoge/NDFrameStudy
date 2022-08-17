Shader "PostProcess/Spread"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }

    SubShader
    {

        Tags { "RenderType" = "Transparent" }
        ZTest Always
        ZWrite on
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"  

            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float _Distance;
            float _Offset;
            float _SpreadWidth;
            float _DissolveAmount;
            float _DissolveMapSize;
            float4 _Color;
            float _FullMaxDistance;
            float _ObjMaxDistance;
            //* wanghaoyu  动态分辨率
            float _DynamicScaleValue;
            CBUFFER_END
            

            TEXTURE2D(_CamColorTex);
            SAMPLER(sampler_CamColorTex);
            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_TransparentsDepthBlend);
            SAMPLER(sampler_TransparentsDepthBlend);
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                //* wanghaoyu  动态分辨率
                o.uv = v.uv * _DynamicScaleValue;
                return o;
            }

            float4 GetWorldPositionFromDepthValue(float2 uv, float linearDepth)
            {
                float camPosZ = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * linearDepth;
                float height = 2 * camPosZ / unity_CameraProjection._m11;
                float width = _ScreenParams.x / _ScreenParams.y * height;
                float camPosX = width * uv.x - width / 2;
                float camPosY = height * uv.y - height / 2;
                float4 camPos = float4(camPosX, camPosY, camPosZ, 1.0);
                return mul(unity_CameraToWorld, camPos);
            }
            // 灰度处理从单个pass移到这里了
            float4 luminosityProcess(float4 col)
            {
                float luminosity = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
                luminosity /= 2;
                return float4(luminosity, luminosity, luminosity, col.a);
            }
            
            half4 frag(v2f i): SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_CamColorTex, sampler_CamColorTex, i.uv);
                float TransDepth = SAMPLE_TEXTURE2D(_TransparentsDepthBlend,sampler_TransparentsDepthBlend,i.uv).r;  // blend depthTex
                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.uv).r;
                depth = max(depth,TransDepth);
                float4 iuminosityCol = luminosityProcess(col);   
                if (_Distance > _ObjMaxDistance)
                {
                    if(depth != 0)
                        return col;
                    float size = (_Distance - _ObjMaxDistance) / (_FullMaxDistance - _ObjMaxDistance + 0.001);
                    if(size > 1)size = 1;
                    return lerp(iuminosityCol, col, size);
                }
                float linearDepth = Linear01Depth(depth,_ZBufferParams);
                float4 worldPos = GetWorldPositionFromDepthValue(i.uv, linearDepth);
                float distance = length(worldPos.xyz - GetCameraPositionWS());
                float displayDistance = _Distance - _SpreadWidth;
                if(displayDistance < 0)
                {
                    return iuminosityCol;
                }
                // 因为是这样的，判断好圈内圈外之后还要再判断个圈内的距离还原颜色
                if(distance < _Distance)    // 如果在圈内的话
                {
                    if (distance > displayDistance)
                    {
                        float scale = (distance - displayDistance) / _SpreadWidth;
                        float2 uvPos = i.uv + scale * float2(0, _Offset);
                        float depthOffset = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvPos).r;
                        float linearDepthOffset = Linear01Depth(depthOffset,_ZBufferParams);
                        float4 worldPosOffset = GetWorldPositionFromDepthValue(uvPos, linearDepthOffset);
                        float distanceOffset = length(worldPosOffset.xyz - GetCameraPositionWS());
                        if(distanceOffset < _Distance)
                        {
                            // 这里是圈内出去顶点偏移的部分
                            col = SAMPLE_TEXTURE2D(_CamColorTex, sampler_CamColorTex, uvPos);
                            float trans = scale * 2 - 1;
                            uvPos *= _DissolveMapSize;
                            half3 dissolve = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, uvPos).rgb;
                            if(trans < 0)
                            {
                                //圆环内侧
                                _DissolveAmount = (1 - (distance - displayDistance) / (0.5 * _SpreadWidth));
                                half t = 1 - smoothstep(0.0, 2, dissolve.r - _DissolveAmount);
                                if (dissolve.r < _DissolveAmount)
                                {
                                    t = 0;
                                }
                                float size = 0.5 * (1 - _DissolveAmount) * (1 - _DissolveAmount);
                                col += (t * size * _Color);
                                return col ;
                            }
                            else
                            {
                                //圆环外侧
                                _DissolveAmount = (distance - displayDistance) / (0.5 * _SpreadWidth + 0.001) - 1;
                                half t = 1 - smoothstep(0.0, 2, dissolve.r - _DissolveAmount);
                                if (dissolve.r < _DissolveAmount)
                                {
                                    t = 0;
                                }
                                scale = pow((trans - 1), 2);
                                col = lerp(iuminosityCol, col, scale);
                                float size = (0.5 + (1 - (0.5 + (0.5 * (1 - _DissolveAmount) * (1 - _DissolveAmount)))));
                                col += (t * size * _Color);
                                return col ;
                            }
                        }
                        else
                        {
                            //未遮挡的背景
                            col = iuminosityCol;
                            return col;
                        }
                    }
                    else
                    {
                        //内侧 就是扫过正常颜色的部分
                        return col;
                    }
                }
                else
                {
                    //在圈外，就是灰色
                    half4 col = iuminosityCol;
                    return col;
                }
            }
            ENDHLSL
            
        }
    }
}

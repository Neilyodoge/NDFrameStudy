Shader "Custom/VoronoiClamp01Tiling" {
    Properties {
        _MaxDistance ("Max Distance", Float) = 0.3 // 最大距离，控制单元大小
        _CenterCount ("Center Count", Int) = 5 // 圆心数量
        _Centers ("Centers", Vector) = (0.5, 0.5, 0, 0) // 圆心数组，扩展为 Vector[]
        _TileSize ("Tile Size", Float) = 1.0 // UV 平铺大小
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _MaxDistance;
            int _CenterCount;
            float4 _Centers[10]; // 最大支持 10 个圆心
            float _TileSize;

            float clamp01(float value) {
                return clamp(value, 0.0, 1.0);
            }

            float smoothstep01(float edge0, float edge1, float x) {
                x = clamp01((x - edge0) / (edge1 - edge0));
                return x * x * (3.0 - 2.0 * x);
            }

            // 找到最近的圆心，考虑平铺
            float2 getClosestCenter(float2 uv) {
                float minDistance = 1e5; // 较大初始值
                float2 closestCenter = float2(0, 0);

                // 考虑平铺，检查周围的 tile
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        float2 tileOffset = float2(i, j) * _TileSize;
                        for (int k = 0; k < _CenterCount; k++) {
                            float2 center = _Centers[k].xy + tileOffset;
                            float d = distance(uv, center);
                            if (d < minDistance) {
                                minDistance = d;
                                closestCenter = center;
                            }
                        }
                    }
                }
                return closestCenter;
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _TileSize; // 应用平铺
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv; // 平铺后的 UV
                float2 closestCenter = getClosestCenter(uv);

                // 计算局部坐标和距离
                float2 localPos = uv - closestCenter;
                float distanceToCenter = length(localPos);

                // 归一化距离
                float normalizedDistance = clamp01(distanceToCenter / _MaxDistance);

                // 可选：平滑处理
                float value = smoothstep01(0.0, 1.0, normalizedDistance);

                // 输出 clamp01 贴图值
                return fixed4(value, value, value, 1.0); // 灰度贴图
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
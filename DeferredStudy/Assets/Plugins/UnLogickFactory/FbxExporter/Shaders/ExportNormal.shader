// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/UnLogickFactory/FbxExporter/ExportNormal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags{ "RenderType" = "Opaque" }
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			//float4x4 _WorldToCamera;

	uniform sampler2D _CameraGBufferTexture0;	// Diffuse color (RGB), unused (A)
	uniform sampler2D _CameraGBufferTexture1;	// Specular color (RGB), roughness (A)
	uniform sampler2D _CameraGBufferTexture2;	// World space normal (RGB), unused (A)
	uniform sampler2D _CameraGBufferTexture3;	// ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 result = tex2D(_CameraGBufferTexture2, i.uv);
				result.r = 1-result.r;
				return float4(lerp(float3(0.5, 0.5, 1), result, saturate((result.b - 0.499) * 100000)).rgb,1);
			}
			ENDCG
		}
	}
	Fallback off
}



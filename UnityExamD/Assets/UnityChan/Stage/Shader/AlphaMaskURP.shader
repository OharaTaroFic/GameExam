// Custom/AlphaMask（ビルトイン用）を URP で動くように書き直したもの。
// 色は _MainTex、透明度は _AlphaMask のアルファから取る。ライトはメインライトの拡散のみ（環境光なし）。
Shader "Custom/AlphaMaskURP" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaMask ("Mask (A)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
		LOD 200

		Pass {
			Tags { "LightMode"="UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_AlphaMask); SAMPLER(sampler_AlphaMask);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _AlphaMask_ST;
			CBUFFER_END

			struct Attributes {
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				float2 uvMain : TEXCOORD1;
				float2 uvMask : TEXCOORD2;
			};

			Varyings vert (Attributes v) {
				Varyings o;
				o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvMask = TRANSFORM_TEX(v.uv, _AlphaMask);
				return o;
			}

			half4 frag (Varyings i) : SV_Target {
				half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvMain).rgb;
				half alpha = SAMPLE_TEXTURE2D(_AlphaMask, sampler_AlphaMask, i.uvMask).a;
				Light light = GetMainLight();
				half ndotl = saturate(dot(normalize(i.normalWS), light.direction));
				return half4(albedo * light.color * ndotl, alpha);
			}
			ENDHLSL
		}
	}
}

// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------


Shader "Magic Leap/BRDF/DetailAddShadow" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_DetailTex ("Detail (RGB) Trans (A)", 2D) = "white" {}
	_PatternTex ("Pattern (RGB) Mask (A)", 2D) = "white" {}
	_ShadowColor ("Shadow Color", Color) = (1,1,1,1)
	_ShadowFade ("Shadow Fade", Range(0,1)) = 1
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	_Fade ("Fade", Range(0,1)) = 1
	_PatFade ("Pattern Fade", Range(0,1)) = 1

	 _Ramp ("Ramp", 2D) = "white" {}
     _LightVector ("LightVector", Vector) = (0,0,0,0)
}
SubShader {
	Tags {"Queue"="geometry" "RenderType"="Opaque"}
	LOD 200

	//Lighting Off

	Pass {  

	        Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_fwdadd_fullshadows

			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"


			struct appdata {
			    float4 pos : POSITION;
			    float3 normal : NORMAL;
			    float2 texcoord : TEXCOORD0;
			    float2 texcoord2 : TEXCOORD1;
			    float2 texcoord3 : TEXCOORD2;
			    float3 viewDir : TEXCOORD5;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			struct v2f {
				float4 pos : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord2 : TEXCOORD1;
				half2 texcoord3 : TEXCOORD2;
				half3 worldNormal : TEXCOORD4;
                half3 viewDir : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex; 
			float4 _MainTex_ST;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;
			sampler2D _PatternTex;
			float4 _PatternTex_ST;
			fixed _Cutoff;
			fixed _Fade;
			fixed _ShadowFade;
			fixed _PatFade;
			fixed4 _ShadowColor;
			sampler2D _Ramp;
            float4 _LightVector;

			v2f vert (appdata v)
			{
				v2f o;
				
				UNITY_SETUP_INSTANCE_ID(v); //Insert
                UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
				
				o.pos = UnityObjectToClipPos (v.pos);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord2, _DetailTex);
				o.texcoord3 = TRANSFORM_TEX(v.texcoord3, _PatternTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.pos));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			    float light = dot(i.worldNormal,_LightVector);
			    float rim = dot(i.worldNormal,i.viewDir);
			    float diff = (light*.5)+.5;
			    float2 brdfdiff = float2(rim, diff);
			    fixed4 BRDFLight = tex2D(_Ramp, brdfdiff.xy).rgba;


				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 detail = tex2D(_DetailTex, i.texcoord2);
				fixed4 pattern = tex2D(_PatternTex, i.texcoord3);
				fixed4 c = 1;
				clip(col.a - _Cutoff);
				fixed4 colorcomp = lerp(col,col*saturate(pattern+_PatFade),pattern.a);
				fixed4 detailcomp=colorcomp*lerp(colorcomp,1,saturate((detail.r)+_Fade));

				c.rgb*=detailcomp.rgb;
				return (detailcomp)*BRDFLight;
				//return BRDFLight;
			}
		ENDCG
	}
	Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend DstColor Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// Include shadowing support for point/spot
			#pragma multi_compile_fwdadd_fullshadows
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD6;
				SHADOW_COORDS(1)
			};

			fixed _ShadowFade;
			fixed4 _ShadowColor;
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, IN, IN.worldPos)
				fixed4 c = lerp(_ShadowColor,1,saturate(saturate(atten)+_ShadowFade));
				// might want to take light color into account?
				//c.rgb *= _LightColor0.rgb;
				return c;
			}

			ENDCG
		}
	UsePass "VertexLit/SHADOWCASTER"
}

}
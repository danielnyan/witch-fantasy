Shader "Custom/NoFlyZoneShader"
{
    Properties
    {
        _MainTex ("Outer Texture", 2D) = "white" {}
		_Texture1 ("Inner Texture", 2D) = "white" {}
        _Alpha ("Opacity", Range(0,1)) = 1
		_ClipDistance ("Clip Distance", Range(0.1, 1000)) = 100
    }
    SubShader
    {
		Tags {"Queue" = "Transparent"}
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Alpha;
			float _ClipDistance;
			
			struct appdata {
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
			};

			// Credits to DMGregory, https://gamedev.stackexchange.com/questions/136652/uv-world-mapping-in-shader-with-unity
			float2 worldToUV(appdata v) {
				float3 n = normalize(mul(unity_ObjectToWorld, v.normal).xyz);
				float3 uDir, vDir;
				if (all(n == float3(0,1,0))) {
					vDir = float3(0,0,-1);
					uDir = float3(-1,0,0);
				} else if (all(n == float3(0,-1,0))) {
					vDir = float3(0,0,-1);
					uDir = float3(1,0,0);
				} else {
					vDir = normalize(float3(0,1,0) - n.y * n);
					uDir = normalize(cross(n, vDir));
				}
				float3 worldSpace = mul(unity_ObjectToWorld, v.pos).xyz;
				return float2(dot(worldSpace, uDir), dot(worldSpace, vDir));
			}

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.uv = worldToUV(v);
				o.uv = TRANSFORM_TEX(o.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.pos);
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float4 value = tex2D(_MainTex, i.uv);
				return float4(value.rgb, value.a * _Alpha);
			}
			ENDCG
		}

		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _Texture1;
			float4 _Texture1_ST;
			float _Alpha;
			float _ClipDistance;
			
			struct appdata {
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
			};
			
			float2 worldToUV(appdata v) {
				float3 n = normalize(mul(unity_ObjectToWorld, v.normal).xyz);
				float3 uDir, vDir;
				if (all(n == float3(0,1,0))) {
					vDir = float3(0,0,-1);
					uDir = float3(-1,0,0);
				} else if (all(n == float3(0,-1,0))) {
					vDir = float3(0,0,-1);
					uDir = float3(1,0,0);
				} else {
					vDir = normalize(float3(0,1,0) - n.y * n);
					uDir = normalize(cross(n, vDir));
				}
				float3 worldSpace = mul(unity_ObjectToWorld, v.pos).xyz;
				return float2(-dot(worldSpace, uDir), dot(worldSpace, vDir));
			}

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.uv = worldToUV(v);
				o.uv = TRANSFORM_TEX(o.uv, _Texture1);
				o.worldPos = mul(unity_ObjectToWorld, v.pos);
				o.normal = normalize(mul(unity_ObjectToWorld, v.normal).xyz);
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float distance = abs(dot((i.worldPos - _WorldSpaceCameraPos), i.normal));
				float delta = (_ClipDistance - distance) * 10 / _ClipDistance;
				float alphaMul = (delta / sqrt(1 + pow(delta, 2)) + 1) / 2;
				float4 value = tex2D(_Texture1, i.uv);
				return float4(value.rgb, value.a * _Alpha * alphaMul);
			}
			ENDCG
		}
    }
}

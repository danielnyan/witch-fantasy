Shader "Custom/TranslucentShader"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
		_Luminosity ("Luminosity", Color) = (0,0,0,0)
    }
    SubShader {
        Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200
        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
			fixed4 _Luminosity;

            struct appdata {
                fixed4 pos : POSITION;
            };

            struct v2f {
                fixed4 pos : POSITION;
                fixed4 grabPos : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            sampler2D _BackgroundTexture;

            float4 frag(v2f i) : SV_Target {
                fixed4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                fixed4 finalColor = lerp(bgcolor, _Color, _Color.a);
                return finalColor + _Luminosity;
            }
            ENDCG
        }
    }
}
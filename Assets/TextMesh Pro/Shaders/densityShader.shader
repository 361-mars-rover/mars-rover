Shader "Custom/CloudDensity" {
    Properties {
        _MainTex ("Density Texture", 2D) = "white" {}
        _RampTex ("Color Ramp", 2D) = "white" {}
        _Transparency ("Transparency", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            sampler2D _MainTex;
            sampler2D _RampTex;
            float4 _MainTex_ST;
            float _Transparency;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Get density value from the main texture
                float density = tex2D(_MainTex, i.uv).r;
                // Map density to color using the ramp texture
                fixed4 col = tex2D(_RampTex, float2(density, 0));
                col.a *= _Transparency; // Apply transparency
                return col;
            }
            ENDCG
        }
    }
}
Shader "Wikitude/Transparency + Rim Lighting" {

    Properties {
        _MainTex ("Albedo (RGBA)", 2D) = "white" {}
        _SpecTex ("Specular (RGB)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0.3, 0.4, 0.6, 0.2)
        _RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 0.0)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 0.5
    }

    SubShader {
        Tags { "Queue" = "Transparent" }

        Pass {
            ZWrite On
            ColorMask 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct v2f {
                float4 pos : SV_POSITION;

            };
            v2f vert(float4 vertex : POSITION) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_TARGET {
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }

        Pass {
            ZWrite On
            ZTest On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float3 worldPos : TEXCOORD0;
                    float3 worldNormal : TEXCOORD1;
                    float2 uv : TEXCOORD2;

                    float4 pos : SV_POSITION;
                };

                uniform float4 _MainTex_ST;
                uniform float4 _SpecTex_ST;
                uniform float4 _RimColor;
                uniform float _RimPower;

                v2f vert (appdata v) {
                    v2f output;

                    output.pos = UnityObjectToClipPos(v.vertex);
                    output.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    output.worldNormal = UnityObjectToWorldNormal(v.normal);
                    output.uv = v.uv;

                    return output;
                }

                uniform sampler2D _MainTex;
                uniform sampler2D _SpecTex;
                uniform float4 _Color;

                float4 frag(v2f input) : COLOR {
                    half3 worldViewDir = normalize(UnityWorldSpaceViewDir(input.worldPos));
                    float4 rim = abs(1.0 - dot(worldViewDir, input.worldNormal)) * _RimColor * _RimPower;

                    float3 specular;
                    if (dot(input.worldNormal, worldViewDir) < 0.0) {
                       specular = float3(0.0, 0.0, 0.0);
                    } else {
                       specular = pow(max(0.0, dot( reflect(-worldViewDir, input.worldNormal), worldViewDir)), 16.0);
                    }

                    specular *= tex2D(_SpecTex, input.uv).rgb;

                    float4 albedo = tex2D(_MainTex, input.uv) * _Color + float4(specular, 0.0) + rim;
                    return albedo;
                }

            ENDCG
        }
    }
}

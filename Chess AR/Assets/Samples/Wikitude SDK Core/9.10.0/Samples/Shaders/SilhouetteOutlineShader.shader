Shader "Wikitude/Silhouette Outline" {

    Properties { }

    SubShader {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

        Pass {
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float normal : NORMAL;
            };

            v2f vert(appdata v) {
                v.vertex.xyz *= 1.2;
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) :COLOR {
                return float4(1,1,1,0.9);
            }
            ENDCG
        }

    }

    Fallback "Diffuse"
}
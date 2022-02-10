/*
 * AR Foundation Samples copyright © 2020 Unity Technologies ApS
 * Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
 * Unless expressly provided otherwise, the Software under this license is made available strictly on an "AS IS" BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.
 */

Shader "Wikitude/Camera Grain"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex("Noise Texture", 3D) = "white" {}
        _NoiseIntensity ("Depth", Range(0,1)) = 0.0
        _NoiseSpeed("Noise Speed", VECTOR) = (30.0, 20.0, 0, 0)
        _EstimatedLightColor("Estimated Light Color", COLOR) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler3D _NoiseTex;
            float4 _NoiseTex_ST;

            float _NoiseIntensity;
            float4 _NoiseSpeed;
            float4 _EstimatedLightColor;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float scale = 3;
                float2 nuv = scale * (i.screenPos.xy/i.screenPos.w);
                nuv.xy += float2(sin(_Time.y * _NoiseSpeed.x), cos(_Time.y * _NoiseSpeed.y));
                float3 nuv3d = float3(nuv, _NoiseIntensity);

                float4 mainColor = tex2D(_MainTex, i.uv);
                float4 noiseColor = tex3D(_NoiseTex, nuv3d);
                float4 finalColor = lerp(mainColor, noiseColor, 0.25);
                return lerp(finalColor, finalColor * 1.5 * _EstimatedLightColor, 0.5);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
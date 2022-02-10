Shader "Wikitude/FluidShader" {
    Properties {
        _FluidColor("FluidColor", Color) = (0.0, 1.0, 0.0, 0.5)
        _LocalPosition("LocalPosition", Vector) = (0.0, 0.0, 0.0, 0.0)
        _HeightCutoff("HeightCutoff", float) = 0
        _ObjectRotation("ObjectRotation", float) = 0
        _OscillationIndex("OscillationIndex", float) = 0
        _OscillationMaxHeightDivider("OscillationMaxHeightDivider", float) = 0
        _OscillationMaxTimes("OscillationMaxTimes", float) = 0
        _OscillationMaxTimesCount("OscillationMaxTimesCount", float) = 0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite On

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 worldPosition : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _FluidColor;
            float4 _LocalPosition;
            float _HeightCutoff;
            float _ObjectRotation;
            float _OscillationIndex;
            float _OscillationMaxHeightDivider;
            float _OscillationMaxTimes;
            float _OscillationMaxTimesCount;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                bool isClipped = false;
                float yPlanePosition = 0;

                if (_OscillationIndex != 0) {
                    float radius = _LocalPosition - i.worldPosition.x;

                    // The fluid will move over the time, depending on the number of the index
                    // which will be from +MaxHeightDivider to -MaxHeightDivider
                    float oscillation = 10 / (_OscillationIndex / _OscillationMaxHeightDivider);

                    // Make the fluid to move slowly when the times are being increased
                    if (oscillation >= 0) {
                        oscillation += 5 * (_OscillationMaxTimesCount / _OscillationMaxTimes);
                    } else {
                        oscillation -= 10 * (_OscillationMaxTimesCount / _OscillationMaxTimes);
                    }
                    yPlanePosition = radius * sin(_ObjectRotation) / oscillation;
                }

                isClipped = i.worldPosition.y > ((_LocalPosition.y + _HeightCutoff) + yPlanePosition);

                if (isClipped) {
                    discard;
                    return float4(0.0, 0.0, 0.0, 0.0);
                } else {
                    return _FluidColor;
                }
            }
            ENDCG
        }
    }
}

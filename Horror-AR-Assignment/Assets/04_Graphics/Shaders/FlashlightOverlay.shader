Shader "UI/FlashlightOverlay"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0, 0, 0, 1)
        _Radius ("Flashlight Radius", Range(0.05, 0.5)) = 0.22
        _Softness ("Edge Softness", Range(0.001, 0.3)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Radius;
                float _Softness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);

                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 offset = IN.uv - center;

                // Correct the circle for portrait/wide screens
                offset.x *= _ScreenParams.x / _ScreenParams.y;

                float distanceFromCenter = length(offset);

                // Brightest part of the flashlight
                float centerRadius = _Radius * 0.35;

                // Gradual falloff across the whole beam
                float beamFalloff = smoothstep(
                    centerRadius,
                    _Radius,
                    distanceFromCenter
                );

                // Camera is mostly visible in the center,
                // but becomes darker toward the flashlight edge.
                float flashlightDarkness = lerp(
                    0.50,
                    0.80,
                    beamFalloff
                );

                // Transition from flashlight into the dark room
                float outsideTransition = smoothstep(
                    _Radius,
                    _Radius + _Softness,
                    distanceFromCenter
                );

                // IMPORTANT:
                // Outside is dark, but not completely black.
                float roomDarkness = 0.99;

                float finalDarkness = lerp(
                    flashlightDarkness,
                    roomDarkness,
                    outsideTransition
                );

                half4 color = _Color;
                color.a *= finalDarkness;

                return color;
            }

            ENDHLSL
        }
    }
}
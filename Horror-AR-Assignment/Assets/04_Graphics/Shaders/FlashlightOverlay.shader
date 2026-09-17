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
                // Center of the screen
                float2 center = float2(0.5, 0.5);

                // Distance from the center
                float2 offset = IN.uv - center;

                // Prevent the flashlight from becoming oval
                // on different phone aspect ratios
                offset.x *= _ScreenParams.x / _ScreenParams.y;

                float distanceFromCenter = length(offset);

                // Transparent inside the flashlight,
                // black outside, with a soft transition.
                float darkness = smoothstep(
                    _Radius,
                    _Radius + _Softness,
                    distanceFromCenter
                );

                half4 color = _Color;
                
                    float flashlightDarkness = 0.9;

                    color.a *= lerp(flashlightDarkness, 1.0, darkness);

                    return color;
            }

            ENDHLSL
        }
    }
}
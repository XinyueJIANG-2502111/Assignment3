Shader "Custom/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _IntroProgress ("Intro Progress", Range(0, 1)) = 0.0
        _EdgeWidth ("Burn Edge Width", Range(0, 0.2)) = 0.04
        _EdgeColor ("Burn Edge Color", Color) = (0, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _IntroProgress;
            float _EdgeWidth;
            fixed4 _EdgeColor;

            // Procedural noise generation function, scaled up for more intense dissolve effect
            float GenerateNoise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Generate detailed fullscreen procedural noise
                float noise = GenerateNoise(IN.texcoord * 25.0);

                // [Dissolve Clip] Burn holes through the UI mask as progress increases
                clip(_IntroProgress - noise);

                // Calculate burning neon borders
                float edgeCheck = _IntroProgress - noise;
                float isEdge = 0.0;
                if (edgeCheck < _EdgeWidth)
                {
                    isEdge = 1.0;
                }
                
                // Disable edge glow at the start and end to prevent visual artifacts
                isEdge *= step(0.01, 1.0 - _IntroProgress) * step(0.01, _IntroProgress);

                // Blend mask color with high-intensity burning edge
                fixed3 finalRGB = lerp(_Color.rgb, _EdgeColor.rgb * 3.0, isEdge);
                
                return fixed4(finalRGB, _Color.a);
            }
            ENDCG
        }
    }
}
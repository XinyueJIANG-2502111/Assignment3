Shader "Custom/CyberRippleBlast_Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Explosion Progress", Range(0, 1)) = 0.0
        _GlowIntensity ("Glow Intensity", Float) = 4.0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

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
            float _Progress;
            float _GlowIntensity;
            float _DissolveEdgeWidth;

            // Procedural noise generation function
            float GenerateNoise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // vertex shader
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                // Add a small epsilon to avoid division by zero when normalizing a zero vector
                float2 vertexXY = IN.vertex.xy;
                if (length(vertexXY) < 0.0001)
                {
                    vertexXY = float2(0.0, 1.0); // Assign default upward vector
                }
                float2 pushDirection = normalize(vertexXY);
                
                float blastAmplitude = pow(_Progress, 1.5) * 1.5; 
                IN.vertex.xy += pushDirection * blastAmplitude;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // fragment shader
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
    
                // Generate base procedural noise
                float noise = GenerateNoise(IN.texcoord * 10.0);

                // Dynamically calculate a protective threshold to ensure threshold = 0 when _Progress = 0
                float threshold = pow(_Progress, 1.2); 

                // Use the adjusted threshold for clipping
                clip(texColor.a * (noise - threshold));

                // Calculate the dissolve edge
                float edgeCheck = noise - threshold;
                
                // [Optimization] Use smooth range checks for burning edge effects
                float isEdge = 0.0;
                if (edgeCheck > 0.0 && edgeCheck < _DissolveEdgeWidth)
                {
                    isEdge = 1.0;
                }

                // Only allow edge glow to appear when the explosion has actually started (_Progress > 0)
                isEdge *= step(0.01, _Progress); 

                // Fade out as the explosion progresses
                float fade = 1.0 - _Progress;
                fixed3 baseColor = texColor.rgb * IN.color.rgb * _GlowIntensity;

                // Edge glow effect for the dissolve edge
                fixed3 edgeGlow = IN.color.rgb * _GlowIntensity * 3.0; 
                fixed3 finalRGB = lerp(baseColor, edgeGlow, isEdge) * fade;
    
                float finalAlpha = texColor.a * fade;

                return fixed4(finalRGB * finalAlpha, finalAlpha);
            }
            ENDCG
        }
    }
}
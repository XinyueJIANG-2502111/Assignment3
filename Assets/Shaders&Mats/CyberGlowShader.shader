Shader "Custom/CyberGlowSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Glow Settings)]
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1) // 发光颜色 / Neon Glow Color
        _GlowSize ("Glow Size", Range(0, 0.5)) = 0.1     // 发光内缩范围 / Glow Size Internal
        _GlowPower ("Glow Power", Range(1, 5)) = 2.0    // 发光强度 / Glow Intensity
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha // 支持透明度通道 / Support Alpha Blending

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
            fixed4 _GlowColor;
            float _GlowSize;
            float _GlowPower;

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
                // 采样原始 Sprite 纹理 / Sample original sprite texture
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                // 计算 UV 坐标到边缘的距离，用于制造内发光
                // Calculate distance from UV to borders to generate inner glow
                float minX = IN.texcoord.x;
                float maxX = 1.0 - IN.texcoord.x;
                float minY = IN.texcoord.y;
                float maxY = 1.0 - IN.texcoord.y;
                
                // 找到距离四边最近的距离 / Find shortest distance to any edge
                float edgeDist = min(min(minX, maxX), min(minY, maxY));
                
                // 计算发光衰减 / Calculate glow attenuation
                float glowFactor = smoothstep(0.0, _GlowSize, edgeDist);
                glowFactor = 1.0 - glowFactor; // 反转，让边缘最亮 / Invert to make edges brightest
                glowFactor = pow(glowFactor, _GlowPower); // 增强指数强度 / Power intensity

                // 将发光颜色混合到原本的颜色上
                // Combine the glow color with the base color
                fixed4 finalColor = texColor * IN.color;
                
                // 只在 Sprite 有像素的地方显示发光 / Apply glow only within sprite alpha mask
                finalColor.rgb += _GlowColor.rgb * glowFactor * finalColor.a;

                // 保持透明度预乘 / Pre-multiplied alpha
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
        ENDCG
        }
    }
}
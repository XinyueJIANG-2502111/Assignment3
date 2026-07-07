Shader "Custom/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1) // 默认纯黑遮罩
        _IntroProgress ("Intro Progress", Range(0, 1)) = 0.0 // 0 = 全黑, 1 = 完全烧完显露游戏
        _EdgeWidth ("Burn Edge Width", Range(0, 0.2)) = 0.04
        _EdgeColor ("Burn Edge Color", Color) = (0, 1, 1, 1) // 炫酷的赛博青色或霓虹红
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

            // 程序化高频噪声，这次把缩放拉大，让全屏溶解的孔洞更密集狂暴
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
                // 1. 获取全屏噪声（乘以 25 增加全屏噪点的密集度）
                // Generate detailed fullscreen procedural noise
                float noise = GenerateNoise(IN.texcoord * 25.0);

                // 2. 核心裁剪：随着进度推进，噪点低的像素率先被剔除（镂空）
                // [Dissolve Clip] Burn holes through the UI mask as progress increases
                clip(_IntroProgress - noise);

                // 3. 计算霓虹烧灼边缘
                // Calculate burning neon borders
                float edgeCheck = _IntroProgress - noise;
                float isEdge = 0.0;
                if (edgeCheck < _EdgeWidth)
                {
                    isEdge = 1.0;
                }
                
                // 开局和完全结束时关闭边缘发光，防止穿帮
                isEdge *= step(0.01, 1.0 - _IntroProgress) * step(0.01, _IntroProgress);

                // 4. 混合色彩：如果是边缘则涂上炽热的霓虹色，否则是原本的纯黑遮罩
                // Blend mask color with high-intensity burning edge
                fixed3 finalRGB = lerp(_Color.rgb, _EdgeColor.rgb * 3.0, isEdge);
                
                return fixed4(finalRGB, _Color.a);
            }
            ENDCG
        }
    }
}
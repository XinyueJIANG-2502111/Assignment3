Shader "Custom/CyberRippleBlast_Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Explosion Progress", Range(0, 1)) = 0.0 // 0 = 正常, 1 = 完全烧毁
        _GlowIntensity ("Glow Intensity", Float) = 4.0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05 // 溶解边缘霓虹火花的宽度
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

            // 程序化纯计算噪声生成函数
            float GenerateNoise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // 顶点着色器
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                // 【核心修正】加入 0.0001f 的保底偏移，防止顶点坐标为 (0,0) 时 normalize 发生除以 0 导致 NaN 崩溃
                // [Fix] Add a small epsilon to avoid division by zero when normalizing a zero vector
                float2 vertexXY = IN.vertex.xy;
                if (length(vertexXY) < 0.0001)
                {
                    vertexXY = float2(0.0, 1.0); // 赋予默认向上的方向 / Assign default upward vector
                }
                float2 pushDirection = normalize(vertexXY);
                
                float blastAmplitude = pow(_Progress, 1.5) * 1.5; 
                IN.vertex.xy += pushDirection * blastAmplitude;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // 片元着色器
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
    
                // 生成基础程序噪声
                float noise = GenerateNoise(IN.texcoord * 10.0);

                // 动态计算一个保护阈值，确保 _Progress = 0 时 threshold = 0
                float threshold = pow(_Progress, 1.2); 

                // 使用修正后的阈值进行剪裁
                clip(texColor.a * (noise - threshold));

                // 计算消融边缘
                float edgeCheck = noise - threshold;
                
                // 【优化】通过对 edgeCheck 的范围限制计算出发光边缘，比单纯使用 step 渐变过渡更平滑自然
                // [Optimization] Use smooth range checks for burning edge effects
                float isEdge = 0.0;
                if (edgeCheck > 0.0 && edgeCheck < _DissolveEdgeWidth)
                {
                    isEdge = 1.0;
                }

                // 只有当真正开始爆炸时（_Progress > 0），边缘强光才允许显现
                isEdge *= step(0.01, _Progress); 

                // 随着进度淡出
                float fade = 1.0 - _Progress;
                fixed3 baseColor = texColor.rgb * IN.color.rgb * _GlowIntensity;

                // 边缘极度加亮的霓虹火花
                fixed3 edgeGlow = IN.color.rgb * _GlowIntensity * 3.0; 
                fixed3 finalRGB = lerp(baseColor, edgeGlow, isEdge) * fade;
    
                float finalAlpha = texColor.a * fade;

                return fixed4(finalRGB * finalAlpha, finalAlpha);
            }
            ENDCG
        }
    }
}
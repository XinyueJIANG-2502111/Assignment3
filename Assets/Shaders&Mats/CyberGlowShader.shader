Shader "Custom/CyberGlowSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Explosion Progress", Range(0, 1)) = 0.0 
        _GlowIntensity ("Glow Intensity", Float) = 4.0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05 
        
        // 【新属性】描边的宽度与颜色
        // [New Properties] Control outline thickness and shade
        _OutlineThickness ("Outline Thickness", Range(0, 0.02)) = 0.005
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // 默认纯黑防粘连
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
            float4 _MainTex_TexelSize; // Unity 自动填充的贴图像素大小，用于精准计算偏移
            fixed4 _Color;
            float _Progress;
            float _GlowIntensity;
            float _DissolveEdgeWidth;
            float _OutlineThickness;
            fixed4 _OutlineColor;

            float GenerateNoise(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float2 vertexXY = IN.vertex.xy;
                if (length(vertexXY) < 0.0001)
                {
                    vertexXY = float2(0.0, 1.0);
                }
                float2 pushDirection = normalize(vertexXY);
                
                float blastAmplitude = pow(_Progress, 1.5) * 1.5; 
                IN.vertex.xy += pushDirection * blastAmplitude;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 基础采样与消融判定 / Base sampling and dissolve
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                float noise = GenerateNoise(IN.texcoord * 10.0);
                float threshold = pow(_Progress, 1.2); 
                clip(texColor.a * (noise - threshold));

                // 2. 【核心新增】像素级上下左右四手采样判定法，强行捕捉图形边缘
                // [Outline Calculation] Multi-sample neighboring pixels to detect contours
                float2 upAlpha    = tex2D(_MainTex, IN.texcoord + float2(0, _OutlineThickness)).a;
                float2 downAlpha  = tex2D(_MainTex, IN.texcoord - float2(0, _OutlineThickness)).a;
                float2 leftAlpha  = tex2D(_MainTex, IN.texcoord - float2(_OutlineThickness, 0)).a;
                float2 rightAlpha = tex2D(_MainTex, IN.texcoord + float2(_OutlineThickness, 0)).a;
                
                // 如果当前像素透明度低，但四周有像素，说明这里是“外描边”区域
                // If current pixel is fading but neighbors are solid, mark as outline
                float outlineFactor = max(max(upAlpha, downAlpha), max(leftAlpha, rightAlpha)) - texColor.a;
                outlineFactor = saturate(outlineFactor);

                // 3. 计算消融烧灼火花
                float edgeCheck = noise - threshold;
                float isEdge = 0.0;
                if (edgeCheck > 0.0 && edgeCheck < _DissolveEdgeWidth)
                {
                    isEdge = 1.0;
                }
                isEdge *= step(0.01, _Progress); 

                // 4. 颜色混合体系 / Color blending pipeline
                float fade = 1.0 - _Progress;
                fixed3 baseColor = texColor.rgb * IN.color.rgb * _GlowIntensity;
                fixed3 edgeGlow = IN.color.rgb * _GlowIntensity * 3.0; 
                
                // 优先渲染内部色块，在边缘混合深色描边
                // Lerp inside color with our protective dark outline
                fixed3 finalRGB = lerp(baseColor, edgeGlow, isEdge);
                finalRGB = lerp(finalRGB, _OutlineColor.rgb, outlineFactor) * fade;
                
                // 确保描边部分也能被渲染出来
                float finalAlpha = max(texColor.a, outlineFactor) * fade;

                return fixed4(finalRGB * finalAlpha, finalAlpha);
            }
            ENDCG
        }
    }
}
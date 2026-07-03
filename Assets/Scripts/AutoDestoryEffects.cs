using UnityEngine;
using UnityEngine.UI; // 【核心新增】必须引入 UI 命名空间才能控制 Image 组件 / Required to control Image component

public class AutoDestroyEffect : MonoBehaviour
{
    // 【修改】将 SpriteRenderer 替换为 Image / Replace SpriteRenderer with Image
    private Image uiImage;
    
    public float expandSpeed = 5f;  // 波纹扩散速度 / Expansion speed
    public float fadeSpeed = 4f;    // 淡出速度 / Fade speed
    private float alpha = 1f;

    void Awake()
    {
        // 【修改】获取 UI Image 组件 / Get UI Image component
        uiImage = GetComponent<Image>();
        
        // 随机给点击特效一个高亮的霓虹色 / Assign a random vivid neon color
        Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.9f, 1f);
        
        if (uiImage != null)
        {
            uiImage.color = randomColor;
        }
    }

    void Update()
    {
        // 1. 让波纹体积不断变大 / Smoothly scale up the effect
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // 2. 让波纹不断变透明 / Fade out the opacity
        alpha -= fadeSpeed * Time.deltaTime;
        
        // 【修改】对 UI Image 的颜色进行平滑淡出处理 / Fade out the UI Image color
        if (uiImage != null)
        {
            Color c = uiImage.color;
            uiImage.color = new Color(c.r, c.g, c.b, alpha);
        }

        // 3. 完全透明后自毁，防止内存泄漏 / Destroy when invisible to save memory
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
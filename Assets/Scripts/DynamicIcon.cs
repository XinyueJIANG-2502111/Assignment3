using UnityEngine;
using System.Collections;

public class DynamicIconBlock : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propBlock; // 使用 PropertyBlock 性能最高，防止材质实例化内存泄漏
    private bool isBlasting = false;

    public float blastDuration = 0.15f; // 涟漪炸裂持续时间，越短越有打击感 / Flash duration

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void Setup(Sprite iconSprite, Color neonColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = iconSprite;
            
            // 初始时将材质的爆炸进度归零
            // Reset progress to 0 on spawn
            spriteRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", neonColor);
            propBlock.SetFloat("_Progress", 0f);
            spriteRenderer.SetPropertyBlock(propBlock);
        }
    }

    // 【核心改变】公开一个被点击时触发的“自爆”方法，取代外部直接 Destroy
    // [Core Change] Public trigger for the click explosion sequence
    public void TriggerClickBlast()
    {
        if (isBlasting) return;
        isBlasting = true;

        // 关掉碰撞体，防止玩家在一口气的消亡时间内重复点击它计数
        // Kill the collider instantly so it won't be registered twice
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 开启协程，通知 GPU 冲锋！
        StartCoroutine(BlastRoutine());
    }

    IEnumerator BlastRoutine()
    {
        float elapsed = 0f;

        while (elapsed < blastDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / blastDuration);

            // 将当前进度传递给 Shader 的 _Progress 变量
            // Drive the shader's progress float smoothly from 0 to 1
            if (spriteRenderer != null)
            {
                spriteRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Progress", progress);
                spriteRenderer.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        // 动画播完了，此时图标已经完全扩大并淡出了，可以安心升天了
        // Animation finished, safely destroy the GameObject now
        Destroy(gameObject);
    }
}
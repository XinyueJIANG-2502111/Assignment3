using UnityEngine;
using System.Collections;

/// <summary>
/// This script is attached to a dynamic icon block prefab. It handles the setup of the icon's sprite and color, and manages the click blast effect that plays when the icon is clicked.
/// </summary>
public class DynamicIconBlock : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propBlock; 
    // PropertyBlock を使用してパフォーマンスを最大化し、マテリアルのインスタンス化のメモリリークを防止する

    private bool isBlasting = false;

    public float blastDuration = 0.15f; // Flash duration

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
            
            // Reset progress to 0 on spawn
            spriteRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", neonColor);
            propBlock.SetFloat("_Progress", 0f);
            spriteRenderer.SetPropertyBlock(propBlock);
        }
    }

    // エフェクト再生するためのトリガー
    // Public trigger for the click explosion sequence
    public void TriggerClickBlast()
    {
        if (isBlasting) return;
        isBlasting = true;

        // 重複クリックを防ぐためにコライダーを無効化する
        // Kill the collider instantly so it won't be registered twice
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Start the blast animation coroutine
        StartCoroutine(BlastRoutine());
    }

    IEnumerator BlastRoutine()
    {
        float elapsed = 0f;

        while (elapsed < blastDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / blastDuration);

            // 現在の進行状況をシェーダーに渡す
            // Drive the shader's progress float smoothly from 0 to 1
            if (spriteRenderer != null)
            {
                spriteRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Progress", progress);
                spriteRenderer.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        // Animation finished, safely destroy the GameObject now
        Destroy(gameObject);
    }
}
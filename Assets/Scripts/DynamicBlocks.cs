using UnityEngine;
using TMPro; // 必须引入 TMP 命名空间 / Required for TextMeshPro

public class DynamicWordBlock : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textComponent;      // 拖入子物体的 TMP 组件 / Child TMP component
    public SpriteRenderer backgroundSprite; // 拖入子物体的 Square 精灵 / Child SpriteRenderer

    [Header("Sizing Adjustments")]
    // 文字两边的留白缓冲，防止太挤 / Extra padding on left and right sides
    public float paddingX = 0.5f;
    // 方块的固定高度 / Fixed height of the block
    public float blockHeight = 1.0f;

    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // 初始化方块文本，并动态调整大小
    // Initialize block text and dynamically adjust its size
    public void Setup(string textContent)
    {
        if (textComponent == null || backgroundSprite == null || boxCollider == null) return;

        // 1. 赋值文字 / Assign the text
        textComponent.text = textContent;

        // 【核心】强制 TextMeshPro 立即计算文字的网格和实际渲染宽高
        // [Core] Force TMP to immediately calculate text mesh and rendered dimensions
        textComponent.ForceMeshUpdate();

        // 2. 获取文字实际渲染的世界宽度
        // Get the actual rendered width of the text in world units
        float textWidth = textComponent.renderedWidth;

        // 3. 计算最终方块应该具备的宽度 (文字宽 + 两边留白)
        // Calculate final block width (text width + padding)
        float finalWidth = textWidth + paddingX;

        // 4. 动态调整背景精灵的缩放 / Dynamically scale the background sprite
        backgroundSprite.transform.localScale = new Vector3(finalWidth, blockHeight, 1f);

        // 5. 动态调整物理碰撞盒的大小 / Dynamically resize the Box Collider 2D
        boxCollider.size = new Vector2(finalWidth, blockHeight);
    }

    // 基础点击消除 / Basic click destruction
    private void OnMouseDown()
    {
        // TODO: 触发爆炸粒子
        Destroy(gameObject);
    }
}
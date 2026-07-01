using UnityEngine;
using TMPro; // 必须引入 TMP 命名空间 / Required for TextMeshPro

public class DynamicWordBlock : MonoBehaviour
{
    private GameManager gameManager;

    [Header("References")]
    public TextMeshPro textComponent;      // 拖入子物体的 TMP 组件 / Child TMP component
    public SpriteRenderer backgroundSprite; // 拖入子物体的 Square 精灵 / Child SpriteRenderer

    [Header("Sizing Adjustments")]
    // 文字两边的留白缓冲，防止太挤 / Extra padding on left and right sides
    public float paddingX = 0.3f;
    // 方块的固定高度 / Fixed height of the block
    public float blockHeight = 1.0f;

    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Start()
    {
        // 【核心修改】一出生，就检查场上是不是垃圾太多了
        // [Core Change] Upon spawning, check if there's too much trash on screen
        CheckScreenOverload();
    }

    // 检查屏幕是否过载 / Check if the screen has too many blocks
    private void CheckScreenOverload()
    {
        // 寻找场景中所有挂载了当前脚本的方块数量
        // Find how many blocks currently exist in the scene
        int currentBlockCount = FindObjectsOfType<DynamicWordBlock>().Length;

        // Debug.Log($"当前垃圾数量: {currentBlockCount} / {maxAllowedBlocks}");

        // 如果超过了最大允许数量，触发游戏失败！
        // If count exceeds max allowed, trigger game over
        if (currentBlockCount > gameManager.maxAllowedBlocks)
        {
            if (gameManager != null)
            {
                gameManager.TriggerGameOver(false);
            }
        }
    }

    // 初始化方块文本，并动态调整大小
    public void Setup(string textContent)
    {
        if (textComponent == null || backgroundSprite == null || boxCollider == null) return;

        // 1. 赋值文字
        textComponent.text = textContent;

        // 随机字号
        textComponent.fontSize = Random.Range(2f, 6f);

        // 强制 TextMeshPro 立即计算文字的网格和实际渲染宽高
        textComponent.ForceMeshUpdate();

        // 2. 获取文字实际渲染的世界宽度
        float textWidth = textComponent.renderedWidth;

        // 随机留白
        float currentPaddingX = Random.Range(0.3f, 1.2f); 

        // 根据字号动态算出高度
        float finalHeight = textComponent.bounds.size.y + 0.4f;

        // 3. 计算最终方块应该具备的宽度
        float finalWidth = textWidth + currentPaddingX;

        // 4. 动态调整背景精灵的缩放
        backgroundSprite.transform.localScale = new Vector3(finalWidth, blockHeight, 1f);

        // 5. 动态调整物理碰撞盒的大小
        boxCollider.size = new Vector2(finalWidth, blockHeight);

        // 【新增】给方块材质赋予随机的霓虹发光色
        // [New] Assign random neon colors to the sprite material
        if (backgroundSprite != null)
        {
            // 创建一个随机的高饱和度霓虹色 / Generate a random vivid neon color
            Color randomNeonColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.8f, 1f);
    
            // 改变方块本身的底色（这里用稍微暗一点的颜色衬托发光）
            backgroundSprite.color = randomNeonColor * 0.4f; 
    
            // 通过 MaterialPropertyBlock 或者直接修改 Material 改变 Shader 的发光颜色
            // 修改我们在 Shader 里定义的 "_GlowColor" 属性
            backgroundSprite.material.SetColor("_GlowColor", randomNeonColor);
        }
    }

    // 基础点击消除
    // private void OnMouseDown()
    // {
    //     // TODO: 触发爆炸粒子
    //     Destroy(gameObject);
    // }
}
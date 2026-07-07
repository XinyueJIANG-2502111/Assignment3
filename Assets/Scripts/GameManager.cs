using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI System")]
    public GameUIManager uiManager;
    private int score; 
    public int maxAllowedBlocks = 15;

    public static bool isVictory = false;

    [Header("Spawning & Boundary Settings (Upgraded)")]
    // 之前拖 WordBlockPrefab 的槽位，现在用来拖你的随机图标方块预制件
    // Slot for your icon block prefab
    public GameObject blockPrefab;
    public float spawnInterval = 1.0f;

    // 虚拟网格行列数，用于将图标完美错开，防止扎堆
    // Grid configuration for pseudo-random discrete layout
    private int gridRows = 4;
    private int gridCols = 3;
    private List<Vector2> availableGridPositions = new List<Vector2>();
    //private int currentGridIndex = 0;

    // 动态计算出的绝对安全屏幕边界（世界坐标）
    // Calculated solid boundaries in world space
    private float minX, maxX, minY, maxY;

    [Header("Other Settings")]
    public float gameDuration = 30f; 
    private float timeRemaining;     
    private bool isGameOver = false; 

    [Header("Input Optimization")]
    public float clickBufferRadius = 0.3f; 

    [Header("Icon Pool Settings (New)")]
    // 【新增】取代原本的纯文本词库资产，在这里直接放入你准备好的尺寸一致的图标
    // [New] Drag and drop your square icon sprites here via the Inspector
    public Sprite[] iconPool;

    [Header("Glitch Transition Settings")]
    public RectTransform glitchMaskRect;
    public float transitionDuration = 0.25f;

    void Start()
    {
        // 1. 彻底解决出界：根据当前摄像机和视口动态计算绝对安全边界
        // Calculate secure boundaries using the current main camera
        CalculateSecureBoundaries();

        // 2. 彻底解决扎堆：在安全范围内划分虚拟网格，并完成初始洗牌
        // Partition virtual grid cells and shuffle them initially
        GenerateGridPositions();

        // 3. 初始化游戏状态 / Initialize game state
        timeRemaining = gameDuration;
        isGameOver = false;
        isVictory = false;
        score = 0;

        if (uiManager != null)
        {
            uiManager.InitUI(gameDuration, maxAllowedBlocks);
        }

        // 4. 开启开局倒计时，然后再生成图标
        // Start opening sequence before releasing icons
        StartCoroutine(OpeningSequenceRoutine());
    }

    // 计算安全的屏幕边界（已扣除图标预估大小的内边距）
    // Secure boundary calculation considering block width and height cushions
    void CalculateSecureBoundaries()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        // 因为图标是 1:1 的正方形，尺寸十分稳定，左右上下预留 1.2f 的内边距绝对不会出界
        // Consistent 1.2f padding keeps the square icons completely on-screen
        float paddingX = 1.2f; 
        float paddingY = 1.2f; 

        minX = bottomLeft.x + paddingX;
        maxX = topRight.x - paddingX;
        minY = bottomLeft.y + paddingY;
        maxY = topRight.y - paddingY;
    }

    // 在安全边界内划分虚拟网格，从根本上消灭堆叠卡死
    // Construct grid cells and shuffle their sequence
    void GenerateGridPositions()
    {
        availableGridPositions.Clear();

        float stepX = (maxX - minX) / gridCols;
        float stepY = (maxY - minY) / gridRows;

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                float centerX = minX + (c + 0.5f) * stepX;
                float centerY = minY + (r + 0.5f) * stepY;
                availableGridPositions.Add(new Vector2(centerX, centerY));
            }
        }

        ShuffleGrid(availableGridPositions);
        //currentGridIndex = 0;
    }

    // 开局 3, 2, 1 倒计时协程，压住主游戏时钟
    // Opening countdown locks core timers until completion
    IEnumerator OpeningSequenceRoutine()
    {
        // 临时锁住 Update 里的倒计时和垃圾统计跑路
        isGameOver = true; 

        if (uiManager != null && uiManager.timerText != null)
        {
            uiManager.timerText.gameObject.SetActive(true);

            uiManager.timerText.text = "3";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "2";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "1";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "<color=red>GO!</color>"; 
            yield return new WaitForSeconds(0.8f);
        }

        // 倒计时结束，释放限制
        isGameOver = false; 

        // 正式开始生成随机图标
        StartCoroutine(SpawnRoutine());
    }

    // 升级后的网格抖动生成协程
    // Upgraded spawn routine featuring grid jittering and color randomizations
    IEnumerator SpawnRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 场上图标满了就挂起 / Pause if screen is full
            if (GetCurrentBlockCount() >= maxAllowedBlocks) continue;

            if (blockPrefab != null && iconPool != null && iconPool.Length > 0)
            {
                // 【真·纯随机】直接在动态计算的绝对安全边界（minX到maxX，minY到maxY）里狂野抽点！
                // [True Random] Wildly pitch coordinates directly within secure screen boundaries
                float pureRandomX = Random.Range(minX, maxX);
                float pureRandomY = Random.Range(minY, maxY);
                Vector3 finalSpawnPos = new Vector3(pureRandomX, pureRandomY, 0f);

                // 实例化图标 / Instantiate the icon
                GameObject newBlock = Instantiate(blockPrefab, finalSpawnPos, Quaternion.identity);
                newBlock.transform.SetParent(this.transform); 

                // 随机抽图标与色彩 / Randomize look and feel
                Sprite randomIcon = iconPool[Random.Range(0, iconPool.Length)];
                Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.8f, 1f);

                DynamicIconBlock iconScript = newBlock.GetComponent<DynamicIconBlock>();
                if (iconScript != null)
                {
                    iconScript.Setup(randomIcon, randomColor);
                }
            }
        }
    }

    void Update()
    {
        if (isGameOver) return;
        if (Time.timeScale == 0f) return;

        // 倒计时逻辑：调用全新的平滑环形进度条 UI
        // Timer logic: hooks to the newly implemented circular progress bar interface
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (uiManager != null) uiManager.UpdateTimer(timeRemaining);
        }
        else
        {
            TriggerGameOver(true);
        }

        // 检测点触
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }

        // 动态统计场上的图标总数，实时回传至垃圾计数器
        int liveBlocks = GetCurrentBlockCount();
        if (uiManager != null)
        {
            uiManager.UpdateTrashCount(liveBlocks, maxAllowedBlocks);
        }

        // 触发爆血管的失败条件：如果场上图标积压到15个以上，当场输掉
        // Game Over condition: if the garbage screen overflows past the limits
        if (liveBlocks >= maxAllowedBlocks)
        {
            TriggerGameOver(false);
        }
    }

    void HandleClick(Vector3 screenPosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hitCollider = Physics2D.OverlapCircle(touchPos, clickBufferRadius);

        if (hitCollider != null)
        {
            // 确保点中的是带有全新 DynamicIconBlock 脚本的图标
            // Verify that the raycast catches the rewritten icon component
            DynamicIconBlock block = hitCollider.GetComponent<DynamicIconBlock>();
            if (block != null)
            {
                score++;
                if (uiManager != null) uiManager.UpdateScore(score);

                // 物理销毁
                //Destroy(block.gameObject);
                block.TriggerClickBlast();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(mouseWorld.x, mouseWorld.y, 0), clickBufferRadius);
        }
    }

    // 辅助工具：获取当前作为子物体存活在 GameManager 下的图标数
    // Helper to extract active block count directly via transform hierachy
    int GetCurrentBlockCount()
    {
        return transform.childCount;
    }

    // 经典费舍尔-耶茨二维向量洗牌算法 / Fisher-Yates Shuffle for Grid Positions
    void ShuffleGrid(List<Vector2> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Vector2 value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // 升级后的触发游戏结束方法
    // Upgraded Game Over trigger that intercepts direct scene loading
    public void TriggerGameOver(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;
        isVictory = isWin;

        // 1. 立即封锁所有图标的点击输入，不准玩家再点
        // Lock out all player inputs instantly
        var allBlocks = FindObjectsOfType<DynamicIconBlock>();
        foreach (var block in allBlocks)
        {
            Collider2D col = block.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // 2. 停止主循环更新，开启暴躁的死机转场协程
        // Fire the screen-off transition sequence
        StartCoroutine(GlitchScreenOffRoutine(isWin));
    }

    IEnumerator GlitchScreenOffRoutine(bool isWin)
    {
        if (glitchMaskRect != null)
        {
            float elapsed = 0f;

            // 【第一阶段：电涌凝聚】
            // 先把遮罩的 Y 轴缩放从 0 挤压到 0.02，形成一条横切屏幕的刺眼高亮霓虹线！
            // Phase 1: Condense into a sharp horizontal pixel line
            while (elapsed < transitionDuration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.4f);
                
                // X轴满，Y轴微微张开一条缝
                glitchMaskRect.localScale = new Vector3(1f, Mathf.Lerp(0f, 0.02f, t), 1f);
                yield return null;
            }

            // 稍微在死线状态定格 0.03 秒，配合一声音效最佳，传达断电卡死感
            // Micro-freeze to emphasize system failure
            yield return new WaitForSeconds(0.03f);

            elapsed = 0f;
            // 【第二阶段：全面黑屏坍塌】
            // 霓虹线瞬间纵向爆开（Y轴从 0.02 飙升到 1f），彻底吞噬整个游戏世界！
            // Phase 2: Snap expansion to swallow the entire screen in total darkness
            while (elapsed < transitionDuration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.6f);
                
                // 使用 pow(t, 3) 产生一个非线性的突变式加速，啪的一下全黑
                float curve = Mathf.Pow(t, 3);
                glitchMaskRect.localScale = new Vector3(1f, Mathf.Lerp(0.02f, 1f, curve), 1f);
                yield return null;
            }
        }
        else
        {
            // 防御性保底：如果没有挂载 UI，就直接等 0.2 秒
            yield return new WaitForSeconds(transitionDuration);
        }

        // 3. 此时整个屏幕已经是一片死黑，无缝载入新场景
        // Screen is now pitch black. Load the results safely.
        SceneManager.LoadScene("ResultScene");
    }
}